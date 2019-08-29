using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using beltexam.Models;

namespace beltexam.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;
 
        public HomeController(MyContext context){
            dbContext = context;
        }
        [HttpGet("")]
        public IActionResult Index()
        {
            IndexViewModel viewModel = new IndexViewModel(){
                User = new User(),
                Login = new Login()
            };
            return View(viewModel);
        }

        // [HttpPost("register")]
        public IActionResult Register(IndexViewModel modelData) {
            User user = modelData.User;
            if(ModelState.IsValid) {
                if(dbContext.Users.Any(u => u.Email == user.Email)) {
                    ModelState.AddModelError("User.Email", "Email already in use!");
                    return View("Index");
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, user.Password);
                dbContext.Users.Add(user);
                dbContext.SaveChanges();
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == user.Email);
                HttpContext.Session.SetString("Name", userInDb.FirstName);
                HttpContext.Session.SetInt32("Id", userInDb.id);

                return RedirectToAction("Dashboard");
            }
            return View("Index");
        }

        // [HttpPost("login")]
        public IActionResult Login(IndexViewModel modelData) {
            Login user = modelData.Login;
            if(ModelState.IsValid) {
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == user.Email);
                if(userInDb == null) {
                    ModelState.AddModelError("Login.Email", "Invalid Email/Password");
                    return View("Index");
                }
                var hasher = new PasswordHasher<Login>();
                var result = hasher.VerifyHashedPassword(user, userInDb.Password, user.Password);
                if(result == 0) {
                    ModelState.AddModelError("Login.Email", "Invalid Email/Password");
                    return View("Index");
                }
                HttpContext.Session.SetString("Name", userInDb.FirstName);
                ViewBag.Name = HttpContext.Session.GetString("Name");
                HttpContext.Session.SetInt32("Id", userInDb.id);
                ViewBag.Id = HttpContext.Session.GetInt32("Id");
                return RedirectToAction("Dashboard");
            }
            return View("Index");
        }

        [HttpGet("logout")]
         public IActionResult Logout() {
             HttpContext.Session.Clear();
             return RedirectToAction("Index");
         }

        // public IActionResult Privacy()
        // {
        //     return View();
        // }

        // [HttpGet("")]
        [HttpGet("dashboard")]
        public IActionResult Dashboard() {
            if(HttpContext.Session.GetInt32("Id")==null) {
                return RedirectToAction("Index");
            }
            ViewBag.Name = HttpContext.Session.GetString("Name");
            ViewBag.Id = HttpContext.Session.GetInt32("Id");
            DateTime Now = DateTime.Now;

            List<Activitee> AllActivities = dbContext.Activities
                // .Where(act => act.Date > Now)
                .Include(act => act.Participants)
                .ThenInclude(part => part.User)
                .OrderBy(act => act.Date).ToList();

            List<Activitee> FurtureActivities = new List<Activitee>();
            foreach(var act in AllActivities) {
                if(act.Date > Now) {
                    FurtureActivities.Add(act);
                }
            }

            System.Console.WriteLine("***************************************");
            foreach(var act in AllActivities) {
                System.Console.WriteLine("Activitee Id: " + act.id + " Activitee.Participants.Count:" + act.Participants.Count);
                foreach(var part in act.Participants) {
                    System.Console.WriteLine("Activitee Id: " + act.id + " Participant Id: " + part.id);
                }
            }

            ViewBag.AllActivities = FurtureActivities;

            return View();
        }

        [HttpGet("new")]
        public IActionResult New() {
            if(HttpContext.Session.GetInt32("Id") == null){
                System.Console.WriteLine("Session Id = null");
                return RedirectToAction("Index");
            }
            ViewBag.Name = HttpContext.Session.GetString("Name");
            ViewBag.Id = HttpContext.Session.GetInt32("Id");
            return View();
        }

        public IActionResult CreateAct(Activitee act) {
            System.Console.WriteLine("********************** in CreateAct act.Title: " + act.Title);
            int Year = new int();
            int Month = new int();
            int Day = new int();
            int Hour = new int();
            int Minutes = new int();

            Year = Int32.Parse(act.Date.ToString("yyy"));
            Month = Int32.Parse(act.Date.ToString("MM"));
            Day = Int32.Parse(act.Date.ToString("dd"));
            Hour = Int32.Parse(act.Time.ToString("HH"));
            // System.Console.WriteLine("act.Time.ToString(HH)" + act.Time.ToString("HH"));
            Minutes = Int32.Parse(act.Time.ToString("mm"));
            DateTime Now = DateTime.Now;
            DateTime StartDate = new DateTime(Year, Month, Day, Hour, Minutes, 0);
            if(StartDate < Now) {
                ModelState.AddModelError("Date", "You Cannot Creaate an Event that Starts in the Past!");
                ViewBag.Name = HttpContext.Session.GetString("Name");
                ViewBag.Id = HttpContext.Session.GetInt32("Id");
                return View("New");
            }
            if(HttpContext.Session.GetInt32("Id") == null){
                System.Console.WriteLine("Session Id = null");
                return RedirectToAction("Index");
            }
            if(act.Date < Now) {

            }
            if(ModelState.IsValid) {
                System.Console.WriteLine("******************* ModelState.IsValid");

                DateTime EndDate = new DateTime();

                if (act.DurationType == "Minute") {
                    EndDate = StartDate.AddMinutes(act.Duration);
                } else if (act.DurationType == "Hour") {
                    EndDate = StartDate.AddHours(act.Duration);
                } else if (act.DurationType == "Day") {
                    EndDate = StartDate.AddDays(act.Duration);
                }

                act.StartDate = StartDate;
                act.EndDate = EndDate;

                dbContext.Activities.Add(act);
                dbContext.SaveChanges();
                var actInDB = dbContext.Activities.Last();
                return RedirectToAction("Activity", new {id = actInDB.id});
            }
            System.Console.WriteLine("******************* ModelState.Is NOT Valid");
            ViewBag.Name = HttpContext.Session.GetString("Name");
            ViewBag.Id = HttpContext.Session.GetInt32("Id");
            return View("New");
        }

        [HttpGet("activity/{id}")]
        public IActionResult Activity(int id) {
            if(HttpContext.Session.GetInt32("Id") == null){
                System.Console.WriteLine("Session Id = null");
                return RedirectToAction("Index");
            }
            var actInDb = dbContext.Activities
                .Include(activity => activity.Participants)
                .ThenInclude(participant => participant.User)
                .FirstOrDefault(act => act.id == id);

            ViewBag.actInDb = actInDb;
            ViewBag.Name = HttpContext.Session.GetString("Name");
            ViewBag.Id = HttpContext.Session.GetInt32("Id");
            return View();
        }

        [HttpGet("addpart/{id}")]
        public IActionResult AddPart(int id) {
            System.Console.WriteLine("In addpart/{id} id: " + id);
            if(HttpContext.Session.GetInt32("Id") == null){
                System.Console.WriteLine("Session Id = null");
                return RedirectToAction("Index");
            }

            Participant newPart = new Participant();
            newPart.UserId = (int)HttpContext.Session.GetInt32("Id");
            newPart.ActiviteeId = id;
            // Activitee existing = dbContext.Activities.FirstOrDefault(a => a.id == id);
            // existing.Participants.Add(dbContext.Users.FirstOrDefault(u => u.id == (int)HttpContext.Session.GetInt32("Id")));

            Activitee actInDb = dbContext.Activities
                .Include(act => act.Participants)
                .ThenInclude(part => part.User)
                .FirstOrDefault(act => act.id == id);
            List<int> guestIds = new List<int>();
            foreach(var guestId in actInDb.Participants) {
                guestIds.Add(guestId.UserId);
            }
            if (guestIds.Any(x => x == newPart.UserId)) {
                DateTime isRNow = DateTime.Now;

                List<Activitee> Acs = dbContext.Activities
                // .Where(act => act.Date > Now)
                .Include(ac => ac.Participants)
                .ThenInclude(pa => pa.User)
                .OrderBy(ac => ac.Date).ToList();

                List<Activitee> FuActivities = new List<Activitee>();
                foreach(var ac in Acs) {
                    if(ac.Date > isRNow) {
                        FuActivities.Add(ac);
                    }
                }
                ViewBag.AllActivities = FuActivities;
                ViewBag.Name = HttpContext.Session.GetString("Name");
                ViewBag.Id = HttpContext.Session.GetInt32("Id");

                System.Console.WriteLine("You are already a participant of this event!");
                ViewBag.Error = "You are already a participant of this event!";
                return View("Dashboard");
            }
            System.Console.WriteLine("#%#%#%#%#%#%#%#%#%#%#%#%#%#%#");

            actInDb = dbContext.Activities.FirstOrDefault(act => act.id == id);

            // var StartDate = actInDb.Date;
            // System.Console.WriteLine("StartDate: " + StartDate);
            // var StartTime = actInDb.Time;
            // System.Console.WriteLine("StartTime: " + StartTime);
            // var StartDur = actInDb.Duration;
            // System.Console.WriteLine("StartDur: " + StartDur);
            // var StartDurType = actInDb.DurationType;
            // System.Console.WriteLine("StartDurType: " + StartDurType);

            var usrInDb = dbContext.Users
                .Include(user => user.Participants)
                .ThenInclude(part => part.Activitee)
                .FirstOrDefault(user => user.id == newPart.UserId);

            System.Console.WriteLine("@@@@@@@@@@@@@@@@@ actInDb.StartDate: " + actInDb.StartDate + " actInDb.EndDate: " + actInDb.EndDate);

            if(usrInDb.Participants != null) {
                // System.Console.WriteLine("@@@@@@@@@@@@@@@@@ usrInDb.Participants[0].Title: " + usrInDb.Participants[0].Activitee.Title);
                foreach(var act in usrInDb.Participants) {
                    System.Console.WriteLine("################# act.StartDate: " + act.Activitee.StartDate + "act.Activitee.EndDate: " + act.Activitee.EndDate);
                    if(act.Activitee.EndDate > actInDb.StartDate){
                        System.Console.WriteLine("act.Activitee.EndDate < actInDb.StartDate = true");
                    }
                    System.Console.WriteLine("act.Activitee.EndDate < actInDb.StartDate = false");
                    if(act.Activitee.StartDate < actInDb.EndDate){
                        System.Console.WriteLine("act.Activitee.StartDate > actInDb.StartDate = true");
                    }
                    System.Console.WriteLine("act.Activitee.StartDate > actInDb.StartDate = true");
 
                    if(act.Activitee.EndDate > actInDb.StartDate && act.Activitee.StartDate < actInDb.EndDate) {
                        System.Console.WriteLine("You already have an event at this Time!");
                        ViewBag.Error = "You already have an event at this Time!";

                        DateTime isNow = DateTime.Now;

                        List<Activitee> Activities = dbContext.Activities
                        // .Where(act => act.Date > Now)
                        .Include(a => a.Participants)
                        .ThenInclude(p => p.User)
                        .OrderBy(a => a.Date).ToList();

                        List<Activitee> FActivities = new List<Activitee>();
                        foreach(var a in Activities) {
                            if(a.Date > isNow) {
                                FActivities.Add(a);
                            }
                        }
                        ViewBag.AllActivities = FActivities;
                        ViewBag.Name = HttpContext.Session.GetString("Name");
                        ViewBag.Id = HttpContext.Session.GetInt32("Id");

                        return View("Dashboard");
                    }
                }
            }
            System.Console.WriteLine("usrInDb.Participants == null");

            dbContext.Participants.Add(newPart);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("delete/{id}")]
        public IActionResult Delete(int id) {
            List<Activitee> AllActivities = dbContext.Activities
                // .Where(act => act.Date > Now)
                .Include(act => act.Participants)
                .ThenInclude(part => part.User)
                .OrderBy(act => act.Date).ToList();

            List<Activitee> FurtureActivities = new List<Activitee>();
            Activitee actInDB = dbContext.Activities.SingleOrDefault(act => act.id == id);
            DateTime Now = DateTime.Now;
            if(actInDB == null) {
                foreach(var act in AllActivities) {
                    if(act.Date > Now) {
                        FurtureActivities.Add(act);
                    }
                }
            ViewBag.Name = HttpContext.Session.GetString("Name");
            ViewBag.Id = HttpContext.Session.GetInt32("Id");
            ViewBag.AllActivities = FurtureActivities;
            ViewBag.error = "You cannot Delet an Event that Dosen't Exist";
            return View("Dashboard");
            }

            if(actInDB.CreatorId == HttpContext.Session.GetInt32("Id")) {
                dbContext.Activities.Remove(actInDB);
                dbContext.SaveChanges();
                System.Console.WriteLine("Deleting Activitie ID: " + id);
                return RedirectToAction("Dashboard");
            }

            
            foreach(var act in AllActivities) {
                if(act.Date > Now) {
                    FurtureActivities.Add(act);
                }
            }
            ViewBag.Name = HttpContext.Session.GetString("Name");
            ViewBag.Id = HttpContext.Session.GetInt32("Id");
            ViewBag.AllActivities = FurtureActivities;
            ViewBag.error = "You cannot Delet another User's Event";
            return View("Dashboard");
        }

        [HttpGet("deletePart/{id}")]
         public IActionResult DeletePart(int id) {
            int UserId = (int)HttpContext.Session.GetInt32("Id");
            List<Activitee> AllActivities = dbContext.Activities
                // .Where(act => act.Date > Now)
                .Include(act => act.Participants)
                .ThenInclude(part => part.User)
                .OrderBy(act => act.Date).ToList();

            List<Activitee> FurtureActivities = new List<Activitee>();
            Activitee actInDB = dbContext.Activities.SingleOrDefault(act => act.id == id);
            DateTime Now = DateTime.Now;
            Participant PartInDB = dbContext.Participants.Where(g => g.ActiviteeId == id).SingleOrDefault(g => g.UserId == UserId);
            // if(actInDB.CreatorId != UserId) { 
            //     if(actInDB != null) {
            //         foreach(var act in AllActivities) {
            //             if(act.Date > Now) {
            //                 FurtureActivities.Add(act);
            //             }
            //         }
            //     }
            //     ViewBag.Name = HttpContext.Session.GetString("Name");
            //     ViewBag.Id = HttpContext.Session.GetInt32("Id");
            //     ViewBag.AllActivities = FurtureActivities;
            //     ViewBag.error = "You cannot Delete another User from an Event";
            //     return View("Dashboard");
            // } else 
            
            if (PartInDB == null){
                if(actInDB != null) {
                    foreach(var act in AllActivities) {
                        if(act.Date > Now) {
                            FurtureActivities.Add(act);
                        }
                    }
                ViewBag.Name = HttpContext.Session.GetString("Name");
                ViewBag.Id = HttpContext.Session.GetInt32("Id");
                ViewBag.AllActivities = FurtureActivities;
                ViewBag.error = "You cannot leave an Event that you have not even Joined!";
                return View("Dashboard");
            }
            } else if(PartInDB.UserId == HttpContext.Session.GetInt32("Id")) {
                dbContext.Participants.Remove(PartInDB);
                dbContext.SaveChanges();
            }
             return RedirectToAction("Dashboard");
        }

        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // public IActionResult Error()
        // {
        //     return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        // }
    }
}
