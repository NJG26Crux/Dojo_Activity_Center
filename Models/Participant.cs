using System.ComponentModel.DataAnnotations;
using System;
namespace beltexam.Models {
    public class Participant {

        [Key]
        public int id { get; set; }
        
        public int UserId {get;set;}
        public int ActiviteeId {get;set;}
        public Activitee Activitee {get;set;}
        public User User {get;set;}
    }
}