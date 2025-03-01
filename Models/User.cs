using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; } // Primary Key

        [Required]
        public string Password { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string? MiddleName { get; set; }

        [Required]
        public string Sex { get; set; }

        [Required]
        public DateTime DOB { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public UserType UserType { get; set; }

        //Navigation Properties
        public Teacher? Teacher { get; set; }
        public Student? Student { get; set; }
    }

    public enum UserType
    {
        Administrator,
        Teacher,
        Student
    }
}
