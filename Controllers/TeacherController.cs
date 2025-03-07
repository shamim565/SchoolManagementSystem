﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Controllers
{
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeacherController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchTerm, string searchType)
        {
            if (HttpContext.Session.GetString("UserType") != UserType.Teacher.ToString())
                return RedirectToAction("Login", "User");

            var userId = HttpContext.Session.GetInt32("UserId").Value;
            var teacher = await _context.Teachers.Include(t => t.User)
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null)
            {
                return NotFound("Teacher not found.");
            }

            var students = _context.Students
            .Include(s => s.User)
            .Include(s => s.Grades)
            .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                if (searchType == "email")
                {
                    students = students.Where(s => s.User.Email.ToLower().Contains(searchTerm));
                }
                else if (searchType == "lrn")
                {
                    students = students.Where(s => s.LRN.ToLower().Contains(searchTerm));
                }
            }

            ViewBag.Students = await students.ToListAsync();
            return View(teacher);
        }

        public IActionResult Grades(int studentId)
        {
            if (HttpContext.Session.GetString("UserType") != UserType.Teacher.ToString())
                return RedirectToAction("Login", "User");

            var grades = _context.Grades
                .Include(g => g.Student).ThenInclude(s => s.User)
                .Include(g => g.Teacher).ThenInclude(t => t.User)
                .Where(g => g.StudentId == studentId)
                .Where(g => g.Teacher.UserId == HttpContext.Session.GetInt32("UserId").Value)
                .ToList();
            ViewBag.StudentId = studentId;
            return View(grades);
        }

        [HttpGet]
        public IActionResult EditGrade(int id)
        {
            var grade = _context.Grades
                .Include(g => g.Teacher).ThenInclude(t => t.User)
                .Include(g => g.Student)
                .FirstOrDefault(g => g.Id == id);
            if (grade == null)
            {
                return NotFound();
            }
            ViewBag.Teachers = _context.Teachers.ToList();
            return View(grade);
        }

        [HttpPost]
        public IActionResult EditGrade(Grade grade)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"{error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }
            if (ModelState.IsValid)
            {
                var existingGrade = _context.Grades.Find(grade.Id);
                if (existingGrade == null)
                {
                    return NotFound();
                }

                existingGrade.Quarter1 = grade.Quarter1;
                existingGrade.Quarter2 = grade.Quarter2;
                existingGrade.Quarter3 = grade.Quarter3;
                existingGrade.Quarter4 = grade.Quarter4;

                _context.SaveChanges();
                return RedirectToAction("Grades", new { studentId = grade.StudentId });
            }
            ViewBag.Teachers = _context.Teachers.ToList();
            return View(grade);
        }
    }
}