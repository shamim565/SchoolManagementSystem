﻿using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "LRN must be 13 characters long.")]
        public string LRN { get; set; }

        [Required]
        public string Grade { get; set; }

        [Required]
        public string Year { get; set; }

        [Required]
        public string Track { get; set; }

        [Required]
        public string Section { get; set; }

        [Required]
        public int UserId { get; set; } 

        public List<Grade>? Grades { get; set; }
        public User? User { get; set; }
    }
}
