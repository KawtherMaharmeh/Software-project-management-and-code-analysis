using Kawther_Project.Data;
using Kawther_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Kawther_Project.Controllers
{
    [Route("Supervisor")]

    public class SupervisorController(ApplicationDbContext context) : Controller
    {

        private readonly ApplicationDbContext _context = context;

        /*=============Supervisor Home Page==========*/
        /*===========================================*/
        [HttpGet("Supervisor/Index")]
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

            var supervisor = _context.Supervisors.FirstOrDefault(s => s.Id == user.SupervisorId);
            if (supervisor == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var supervisorProjects = _context.Projects
                .Include(p => p.Students)
                .Where(p => p.SupervisorId == supervisor.Id)
                .Take(3)
                .ToList();

            var viewModel = new TeamLeaderViewModel
            {
                AllProjects = supervisorProjects,
                GraduationProjects = _context.GraduationProjects.ToList()
            };

            return View(viewModel);
        }
        /*=============View All Projects==========*/
        /*========================================*/
        [HttpGet("Supervisor/AllProjects")]
        public IActionResult AllProjects()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Accounts.FirstOrDefault(u => u.Email == userEmail);
            if (user == null || user.SupervisorId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var supervisor = _context.Supervisors.FirstOrDefault(s => s.Id == user.SupervisorId);
            if (supervisor == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var supervisorProjects = _context.Projects
                .Include(p => p.Students)
                .Where(p => p.SupervisorId == supervisor.Id)
                .ToList();

            var viewModel = new TeamLeaderViewModel
            {
                AllProjects = supervisorProjects,
                GraduationProjects = _context.GraduationProjects.ToList()
            };

            return View(viewModel);
        }
        /*=============View Edit Project==========*/
        /*========================================*/
        [HttpGet("Supervisor/EditProject/{id}")]
        public IActionResult EditProject(int id)
        {
            var project = _context.Projects
                .Include(p => p.Students)
                .FirstOrDefault(p => p.Id == id);

            if (project == null)
                return NotFound();

            var allStudents = _context.Students
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToList();

            var viewModel = new EditProjectViewModel
            {
                Id = project.Id,
                Name = project.Name,
                TeamLeaders = project.TeamLeaders,
                LogoUrl = project.LogoUrl,
                SupervisorId = project.SupervisorId,
                SelectedStudentIds = project.Students.Select(s => s.Id).ToList(),
                AllStudents = allStudents
            };

            return View(viewModel);
        }
        /*=============Send Update to Data Base==========*/
        /*===============================================*/
        [HttpPost]
        [Route("Supervisor/EditProjectPost")]
        public async Task<IActionResult> EditProjectPost(EditProjectViewModel model)
        {
            var project = _context.Projects
                .Include(p => p.Students)
                .FirstOrDefault(p => p.Id == model.Id);

            if (project == null)
                return NotFound();

            project.Name = model.Name;
            project.TeamLeaders = model.TeamLeaders;

            if (model.LogoFile != null && model.LogoFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.LogoFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.LogoFile.CopyToAsync(stream);
                }

                project.LogoUrl = "/uploads/" + fileName; 
            }

            project.Students.Clear();
            var selectedStudents = _context.Students
                .Where(s => model.SelectedStudentIds.Contains(s.Id))
                .ToList();

            foreach (var student in selectedStudents)
            {
                project.Students.Add(student);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        /*=============View Details Project==========*/
        /*===========================================*/
        [HttpGet("Supervisor/DetailsProject/{projectId}")]
        public IActionResult DetailsProject(int projectId)
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

            var student = _context.Students.Include(s => s.Project).FirstOrDefault(s => s.ProjectId == projectId);
            if (student == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var tasks = _context.Tasks
                                .Where(t => t.ProjectId == projectId)
                                .ToList();

            var studentNames = _context.Students.ToDictionary(s => s.Id, s => s.Name);
            var subTaskNames = _context.SubTasks.ToDictionary(st => st.Id, st => st.Name);

            var model = new ProjectTasksViewModel
            {
                Tasks = tasks.Select(t => new TaskModel
                {
                    Note = t.Note,
                    TaskStatus = t.TaskStatus,
                    StartTime = t.StartTime,
                    EndTime = t.EndTime,
                    SubTaskId = t.SubTaskId,
                    StudentId = t.StudentId,
                    ProjectId = t.ProjectId,
                    Student = new Student { Name = studentNames.ContainsKey(t.StudentId) ? studentNames[t.StudentId] : "Unknown" },
                    SubTask = new SubTask { Name = subTaskNames.ContainsKey(t.SubTaskId) ? subTaskNames[t.SubTaskId] : "Unknown" }
                }).ToList(),
                Project = student.Project
            };

            if (model.Project != null)
            {
                ViewBag.ProjectName = model.Project.Name;
                ViewBag.LogoUrl = model.Project.LogoUrl;
                ViewBag.TeamLeaders = model.Project.TeamLeaders;
            }
            else
            {
                ViewBag.ProjectName = "لم يتم العثور على مشروع";
                ViewBag.TeamLeaders = "لا يوجد قادة فريق";
                ViewBag.LogoUrl = "~/images/default.png";
            }

            return View(model);
        }
        /*=============View Consciousness Page==========*/
        /*==============================================*/
        [HttpGet("Supervisor/Consciousness")]
        public IActionResult Consciousness()
        {
            var viewModel = new TeamLeaderViewModel
            {
                DoctorMessages = _context.DoctorMessages.ToList(),

            };

            return View(viewModel);
        }
        /*=============Add Consciousness ===============*/
        /*==============================================*/
        [HttpPost("Supervisor/AddConsciousness")]
        public IActionResult AddConsciousness(string messageText)
        {
            var doctorEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(doctorEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var doctor = _context.Accounts.FirstOrDefault(d => d.Email == doctorEmail);
            if (doctor == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var doctorMessage = new DoctorMessage
            {
                DoctorName = doctor.UserName,
                MessageText = messageText
            };

            _context.DoctorMessages.Add(doctorMessage);
            _context.SaveChanges();

            return RedirectToAction("Consciousness"); // أو أي صفحة تريد الرجوع لها
        }
    }
}
