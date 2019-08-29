using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
    
namespace beltexam.Models {
    public class User {
        [Key]
        public int id{get;set;}

        [Required]
        [MinLength(2)]
        [Display(Name = "First Name")] 
        public string FirstName {get;set;}

        [Required]
        [MinLength(2)]
        [Display(Name = "Last Name")]
        public string LastName {get;set;}

        [EmailAddress]
        [Required]
        public string Email {get;set;}

        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$", ErrorMessage = "Invalid password format. Must be between 8 and 15 characters long, must contain at least one number, one uppercase letter, one lowercase letter, and one special charater.")]          
        public string Password {get;set;}

        public List<Participant> Participants {get;set;}

        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;
        // Will not be mapped to your users table!
        [NotMapped]
        [Compare("Password")]
        [DataType(DataType.Password)]
        public string Confirm {get;set;}
    }
}
// [MinLength(8, ErrorMessage="Password must be 8 characters or longer!")]
