using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
    
namespace beltexam.Models {
    public class Activitee{

        [Key]
        public int id {get;set;}

        [Required]
        public string Title {get;set;}

        // [Required]
        [DataType(DataType.Time)]
        public DateTime Time {get;set;}

        // [Required]
        [DataType(DataType.Date)]
        // [CustomDate]
        public DateTime Date {get;set;} = DateTime.Now;

        public DateTime StartDate {get;set;}

        public DateTime EndDate {get;set;}

        // [Required] 
        public int Duration {get;set;}

        // [Required] 
        [Display(Name = "")]
        public string DurationType {get;set;}

        [Required]
        public string Description {get;set;}

        public int CreatorId {get;set;}

        public string CreatorName {get;set;}

        public List<Participant> Participants {get;set;}

        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;
    }

    public class CustomDateAttribute : RangeAttribute {
    public CustomDateAttribute()
        : base(typeof(DateTime), 
                DateTime.Now.ToShortDateString(),
                DateTime.Now.AddYears(5).ToShortDateString())
    { } 
    }
}