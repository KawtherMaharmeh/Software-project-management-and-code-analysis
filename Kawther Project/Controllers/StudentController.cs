using Kawther_Project.Data;
using Kawther_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kawther_Project.Controllers
{
    [Route("Student")]

    public class StudentController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        /*=============Home Page Student==========*/
        /*========================================*/
        [HttpGet("Student/Index")]
        public IActionResult Index()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Accounts.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var student = _context.Students.FirstOrDefault(s => s.Id == user.StudentId);
            if (student == null || student.ProjectId == 0)
            {
                return View(new TeamLeaderViewModel
                {
                    GraduationProjects = new List<GraduationProject>(),
                    DoctorMessages = new List<DoctorMessage>(),
                    Students = new List<Student>()
                });
            }

            var projectId = student.ProjectId;

            var viewModel = new TeamLeaderViewModel
            {
                GraduationProjects = _context.GraduationProjects.ToList(),
                DoctorMessages = _context.DoctorMessages.ToList(),
                Students = _context.Students
                   .Where(s => s.ProjectId == projectId)
                   .ToList()
            };

            return View(viewModel);
        }
        /*=============Task Student==========*/
        /*===================================*/
        [HttpGet("Student/MyTask")]
        public IActionResult MyTask()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Accounts.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var student = _context.Students.FirstOrDefault(s => s.Id == user.StudentId);
            if (student == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var tasks = _context.Tasks
                .Where(t => t.StudentId == student.Id)
                .ToList();

            var mainTaskNames = _context.MainTasks.ToDictionary(mt => mt.Id, mt => mt.Name);
            var subTaskNames = _context.SubTasks.ToDictionary(st => st.Id, st => st.Name);

            var taskViewModels = tasks.Select(t => new TaskDisplayViewModel
            {
                TaskId = t.Id,
                MainTaskName = mainTaskNames.ContainsKey(t.MainTaskId) ? mainTaskNames[t.MainTaskId] : "غير معروف",
                SubTaskName = subTaskNames.ContainsKey(t.SubTaskId) ? subTaskNames[t.SubTaskId] : "غير معروف",
                TaskStatus = t.TaskStatus
            }).ToList();

            return View(taskViewModels);
        }
        /*=============View My Profile==========*/
        /*======================================*/
        [HttpGet("Student/My_Profile")]
        public IActionResult My_Profile()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Accounts
                .Include(u => u.Student)
                .FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
        /*=============Update My Profile==========*/
        /*========================================*/
        [HttpPost("Student/UpdateProfile")]
        public IActionResult UpdateProfile(string Email, string UserName, string PhoneNumber, string BirthDate, string Specialization, string Details)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
                return RedirectToAction("Login", "Account");

            var user = _context.Accounts
                .Include(u => u.Student)
                .FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
                return NotFound();

            user.Email = Email;
            user.UserName = UserName;
            user.PhoneNumber = PhoneNumber;

            if (user.Student != null)
            {
                if (DateTime.TryParseExact(BirthDate, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
                {
                    user.Student.BirthDate = parsedDate;
                }

                user.Student.Specialization = Specialization;
                user.Student.Details = Details;
            }

            _context.SaveChanges();

            return RedirectToAction("My_Profile");
        }

    }
}
