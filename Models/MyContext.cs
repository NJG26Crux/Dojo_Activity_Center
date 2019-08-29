using Microsoft.EntityFrameworkCore;
 
namespace beltexam.Models
{
    public class MyContext : DbContext
    {
        public MyContext() : base(){
        }
        public MyContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users {get;set;}
        public DbSet<Activitee> Activities {get;set;}
        public DbSet<Participant> Participants {get;set;}

    }
}