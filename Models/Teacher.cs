using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Models
{
    public class Teacher
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Position { get; set; }

        [Required]
        public string AdvisoryClass { get; set; }

        [Required]
        public string SubjectsSpecialization { get; set; }

        [Required]
        public int UserId { get; set; } // Foreign key to User.Id

        public User? User { get; set; }
    }
}
