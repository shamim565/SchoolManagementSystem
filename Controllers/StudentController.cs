using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserType") != UserType.Student.ToString())
                return RedirectToAction("Login", "User");

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "User");

           var student = _context.Students
                .Include(s => s.User)
                .Include(s => s.Grades)
                .ThenInclude(g => g.Teacher)
                .ThenInclude(t => t.User)
                .FirstOrDefault(s => s.UserId == userId.Value);
            return View(student);
        }
    }
}
