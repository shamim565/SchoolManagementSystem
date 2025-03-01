using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Models
{
    public class Grade
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public SubjectType SubjectType { get; set; }
        public double? Quarter1 { get; set; }
        public double? Quarter2 { get; set; }
        public double? Quarter3 { get; set; }
        public double? Quarter4 { get; set; }

        [Required]
        public int StudentId { get; set; }
        public Student? Student { get; set; }

        public int? TeacherId { get; set; }
        public Teacher? Teacher { get; set; }
    }
    public enum SubjectType
    {
        Core,
        Applied,
        Specialized,
    }
}
