using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("UserId").HasValue)
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == HttpContext.Session.GetInt32("UserId"));
                if (user != null)
                {
                    switch (user.UserType)
                    {
                        case UserType.Administrator:
                            return RedirectToAction("Index", "Admin");
                        case UserType.Student:
                            return RedirectToAction("Index", "Student");
                        case UserType.Teacher:
                            return RedirectToAction("Index", "Teacher");
                    }
                }
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserType", user.UserType.ToString());
                switch (user.UserType)
                {
                    case UserType.Administrator:
                        return RedirectToAction("Index", "Admin");
                    case UserType.Student:
                        return RedirectToAction("Index", "Student");
                    case UserType.Teacher:
                        return RedirectToAction("Index", "Teacher");
                }
            }
            TempData["ErrorMessage"] = "Invalid email or password.";
            return View();
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "User");
        }
    }
}