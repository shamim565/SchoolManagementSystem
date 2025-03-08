using SelectPdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;
using SchoolManagementSystem.Services;


namespace SchoolManagementSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserType") != UserType.Administrator.ToString())
                return RedirectToAction("Login", "User");
            return View();
        }


        public async Task<IActionResult> Students(string searchTerm, string searchType, string gradeFilter, string sortField, string sortOrder)
        {
            if (HttpContext.Session.GetString("UserType") != UserType.Administrator.ToString())
                return RedirectToAction("Login", "User");

            var students = _context.Students
            .Include(s => s.User)
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

            // Grade Filtering
            if (!string.IsNullOrEmpty(gradeFilter))
            {
                students = students.Where(s => s.Grade == gradeFilter);
            }

            // Sorting Logic
            if (!string.IsNullOrEmpty(sortField))
            {
                bool isDescending = sortOrder == "desc";
                students = sortField switch
                {
                    "lrn" => isDescending ? students.OrderByDescending(s => s.LRN) : students.OrderBy(s => s.LRN),
                    "name" => isDescending ? students.OrderByDescending(s => s.User.FirstName) : students.OrderBy(s => s.User.FirstName),
                    "grade" => isDescending ? students.OrderByDescending(s => s.Grade) : students.OrderBy(s => s.Grade),
                    "section" => isDescending ? students.OrderByDescending(s => s.Section) : students.OrderBy(s => s.Section),
                    "email" => isDescending ? students.OrderByDescending(s => s.User.Email) : students.OrderBy(s => s.User.Email),
                    _ => students
                };
            }

            ViewBag.SortField = sortField;
            ViewBag.SortOrder = sortOrder == "asc" ? "desc" : "asc";
            //ViewBag.Students = await students.ToListAsync();

            return View(await students.ToListAsync());
        }

       
        // Students - List (Read)
        [HttpGet]
        public IActionResult CreateStudent()
        {
            if (HttpContext.Session.GetString("UserType") != UserType.Administrator.ToString())
                return RedirectToAction("Login", "User");
            return View(new Student { User = new User() });
        }

        // POST: Student/CreateStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateStudent(Student student)
        {

            if (HttpContext.Session.GetString("UserType") != UserType.Administrator.ToString())
                return RedirectToAction("Login", "User");

            try
            {
                student.User.UserType = UserType.Student;

                _context.Users.Add(student.User);
                _context.SaveChanges();

                student.UserId = student.User.Id;

                _context.Students.Add(student);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Student added successfully!";

                // Redirect to the list of students
                return RedirectToAction("Students", "Admin");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while saving the data. Please try again.");
            }

            return View();
        }



        // Students - Edit
        [HttpGet]
        public IActionResult EditStudent(int id)
        {
            if (HttpContext.Session.GetString("UserType") != UserType.Administrator.ToString())
                return RedirectToAction("Login", "User");

            var student = _context.Students.Include(s => s.User).FirstOrDefault(s => s.Id == id);

            if (student == null) return NotFound();
            return View(student);
        }

        // POST: Admin/EditStudent/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditStudent(int id, Student student)
        {
            if (HttpContext.Session.GetString("UserType") != UserType.Administrator.ToString())
                return RedirectToAction("Login", "User");

            if (id != student.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingStudent = _context.Students.Include(s => s.User).FirstOrDefault(s => s.Id == id);
                    if (existingStudent == null)
                    {
                        return NotFound();
                    }

                    // Update the User details (excluding LRN)
                    existingStudent.User.FirstName = student.User.FirstName;
                    existingStudent.User.LastName = student.User.LastName;
                    existingStudent.User.MiddleName = student.User.MiddleName;
                    existingStudent.User.Sex = student.User.Sex;
                    existingStudent.User.DOB = student.User.DOB;
                    existingStudent.User.Email = student.User.Email;

                    // Update Student details
                    existingStudent.Grade = student.Grade;
                    existingStudent.Year = student.Year;
                    existingStudent.Track = student.Track;
                    existingStudent.Section = student.Section;

                    _context.Update(existingStudent);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "Student updated successfully!";

                    return RedirectToAction("Students");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while updating the student.");
                }
            }
            else
            {
                foreach (var error in ModelState)
                {
                    foreach (var err in error.Value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Property: {error.Key}, Error: {err.ErrorMessage}");
                    }
                }
                ModelState.AddModelError("", "Model state is not valid.");
            }

                return View(student);
        }

        // Students - Delete
        [HttpPost]
        public IActionResult DeleteStudent(int id)
        {
            if (HttpContext.Session.GetString("UserType") != UserType.Administrator.ToString())
                return RedirectToAction("Login", "User");

            var student = _context.Students.Include(s => s.User).FirstOrDefault(s => s.Id == id);
            if (student != null)
            {
                _context.Users.Remove(student.User); // Delete associated User
                _context.Students.Remove(student);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Student deleted successfully!";
            }
            return RedirectToAction("Students");
        }

        // Teachers - List (Read)
        public async Task<IActionResult> Teachers(string searchTerm, string sortField, string sortOrder)
        {
            if (HttpContext.Session.GetString("UserType") != UserType.Administrator.ToString())
                return RedirectToAction("Login", "User");

            var teachers = _context.Teachers.Include(t => t.User).AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                teachers = teachers.Where(t =>
                    t.User.Email.ToLower().Contains(searchTerm) || 
                    t.User.FirstName.ToLower().Contains(searchTerm) ||
                    t.User.LastName.ToLower().Contains(searchTerm) ||
                    t.SubjectsSpecialization.ToLower().Contains(searchTerm) ||
                    t.Position.ToLower().Contains(searchTerm)
                );
            }

            // Sorting Logic
            if (!string.IsNullOrEmpty(sortField))
            {
                bool isDescending = sortOrder == "desc";
                teachers = sortField switch
                {
                    "name" => isDescending ? teachers.OrderByDescending(t => t.User.FirstName) : teachers.OrderBy(t => t.User.FirstName),
                    "email" => isDescending ? teachers.OrderByDescending(t => t.User.Email) : teachers.OrderBy(t => t.User.Email),
                    "advisory" => isDescending ? teachers.OrderByDescending(t => t.AdvisoryClass) : teachers.OrderBy(t => t.AdvisoryClass),
                    "position" => isDescending ? teachers.OrderByDescending(t => t.Position) : teachers.OrderBy(t => t.Position),
                    _ => teachers
                };
            }

            ViewBag.SortField = sortField;
            ViewBag.SortOrder = sortOrder == "asc" ? "desc" : "asc";

            return View(await teachers.ToListAsync());
        }

        // Teachers - Create
        [HttpGet]
        public IActionResult CreateTeacher()
        {
            if (HttpContext.Session.GetString("UserType") != UserType.Administrator.ToString())
                return RedirectToAction("Login", "User");
            return View();
        }

        [HttpPost]
        public IActionResult CreateTeacher(Teacher teacher)
        {
            if (HttpContext.Session.GetString("UserType") != UserType.Administrator.ToString())
                return RedirectToAction("Login", "User");

            try
            {
                teacher.User.UserType = UserType.Teacher;

                _context.Users.Add(teacher.User);
                _context.SaveChanges(); 

                teacher.UserId = teacher.User.Id;

                _context.Teachers.Add(teacher);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Teacher added successfully!";
                // Redirect to the list of teacgers
                return RedirectToAction("Teachers", "Admin");
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., using a logging framework)
                ModelState.AddModelError("", "An error occurred while saving the data. Please try again.");
            }
            return View(teacher);
        }

        // Teachers - Edit
        [HttpGet]
        public IActionResult EditTeacher(int id)
        {
            if (HttpContext.Session.GetString("UserType") != UserType.Administrator.ToString())
                return RedirectToAction("Login", "User");

            var teacher = _context.Teachers.Include(t => t.User).FirstOrDefault(t => t.Id == id);
            if (teacher == null) return NotFound();
            return View(teacher);
        }

        [HttpPost]
        public IActionResult EditTeacher(int id, Teacher teacher)
        {
            if (HttpContext.Session.GetString("UserType") != UserType.Administrator.ToString())
                return RedirectToAction("Login", "User");

            if (id != teacher.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var existingTeacher = _context.Teachers.Include(t => t.User).FirstOrDefault(t => t.Id == teacher.Id);
                if (existingTeacher == null) return NotFound();

                // Update User fields
                existingTeacher.User.FirstName = teacher.User.FirstName;
                existingTeacher.User.LastName = teacher.User.LastName;
                existingTeacher.User.MiddleName = teacher.User.MiddleName;
                existingTeacher.User.Sex = teacher.User.Sex;
                existingTeacher.User.DOB = teacher.User.DOB;
                existingTeacher.User.Email = teacher.User.Email;

                // Update Teacher fields
                existingTeacher.Position = teacher.Position;
                existingTeacher.AdvisoryClass = teacher.AdvisoryClass;
                existingTeacher.SubjectsSpecialization = teacher.SubjectsSpecialization;

                _context.SaveChanges();
                TempData["SuccessMessage"] = "Teacher updated successfully!";
                return RedirectToAction("Teachers");
            }
            return View(teacher);
        }

        // Teachers - Delete
        [HttpPost]
        public IActionResult DeleteTeacher(int id)
        {
            if (HttpContext.Session.GetString("UserType") != UserType.Administrator.ToString())
                return RedirectToAction("Login", "User");

            var teacher = _context.Teachers.Include(t => t.User).FirstOrDefault(t => t.Id == id);

            if (teacher == null)
            {
                TempData["ErrorMessage"] = "Teacher not found.";
                return RedirectToAction("Teachers");
            }
            
            try{
                _context.Users.Remove(teacher.User);
                _context.Teachers.Remove(teacher);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Teacher deleted successfully!";
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 547)
            {
                TempData["ErrorMessage"] = $"{teacher.User.FirstName} {teacher.User.LastName} cannot be deleted because he/she is  associated with one or more grades.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the teacher.";
            }
            return RedirectToAction("Teachers");
        }

        public IActionResult Grades(int studentId)
        {
            if (HttpContext.Session.GetString("UserType") != UserType.Administrator.ToString())
                return RedirectToAction("Index", "Home");

            var grades = _context.Grades
                .Include(g => g.Student).ThenInclude(s => s.User)
                .Include(g => g.Teacher).ThenInclude(t => t.User)
                .Where(g => g.StudentId == studentId)
                .ToList();
            ViewBag.StudentId = studentId;
            return View(grades);
        }

        [HttpGet]
        public IActionResult CreateGrade(int studentId)
        {
            var studentExists = _context.Students.Any(s => s.Id == studentId);
            if (!studentExists)
            {
                return NotFound("Student not found.");
            }

            ViewBag.StudentId = studentId;
            ViewBag.Teachers = _context.Teachers.Include(t => t.User).ToList(); // Ensure User is included

            return View();
        }

        [HttpPost]
        public IActionResult CreateGrade(Grade grade)
        {
            // Validate Student
            var studentExists = _context.Students.Any(s => s.Id == grade.StudentId);
            if (!studentExists)
            {
                ModelState.AddModelError("StudentId", "Invalid Student ID.");
            }

            // Validate Teacher if assigned
            if (grade.TeacherId.HasValue && !_context.Teachers.Any(t => t.Id == grade.TeacherId.Value))
            {
                ModelState.AddModelError("TeacherId", "Invalid Teacher ID.");
            }

            if (ModelState.IsValid)
            {
                _context.Grades.Add(grade);
                _context.SaveChanges();
                return RedirectToAction("Grades", new { studentId = grade.StudentId });
            }

            ViewBag.StudentId = grade.StudentId;
            ViewBag.Teachers = _context.Teachers.ToList();
            return View(grade);
        }

        [HttpGet]
        public IActionResult EditGrade(int id)
        {
            var grade = _context.Grades.FirstOrDefault(g => g.Id == id);
            if (grade == null) return NotFound();

            ViewBag.Teachers = _context.Teachers.Include(t => t.User).ToList(); // Ensure User is included
            return View(grade);
        }

        [HttpPost]
        public IActionResult EditGrade(Grade grade)
        {
            if (ModelState.IsValid)
            {
                _context.Grades.Update(grade);
                _context.SaveChanges();
                return RedirectToAction("Grades", new { studentId = grade.StudentId });
            }
            ViewBag.Teachers = _context.Teachers.ToList();
            return View(grade);
        }

        [HttpPost]
        public IActionResult DeleteGrade(int id)
        {
            var grade = _context.Grades.FirstOrDefault(g => g.Id == id);
            if (grade == null)
            {
                TempData["ErrorMessage"] = "Grade not found.";
                return RedirectToAction("Grades", new { studentId = grade.StudentId });
            }

            _context.Grades.Remove(grade);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Grade deleted successfully.";
            return RedirectToAction("Grades", new { studentId = grade.StudentId });
        }

        [HttpGet]
        public IActionResult DownloadGrade(int studentId)
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != UserType.Administrator.ToString() && userType != UserType.Teacher.ToString())
                return RedirectToAction("Login", "User");

            try
            {
                GradeReportService gradeReportService = new GradeReportService();
                var pdfBytes = gradeReportService.GenerateGradeReportPdf(studentId, _context);
                return File(pdfBytes, "application/pdf", $"GradeReport_{_context.Students.First(s => s.Id == studentId).User.FirstName}_{studentId}.pdf");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error generating PDF: {ex.Message}";
                return RedirectToAction("Grades", new { studentId });
            }
        }

    }
}
