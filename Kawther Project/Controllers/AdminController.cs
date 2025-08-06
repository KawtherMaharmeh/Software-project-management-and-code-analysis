using Kawther_Project.Data;
using Kawther_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Kawther_Project.Controllers
{
    public class AdminController(ApplicationDbContext context, IWebHostEnvironment env) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IWebHostEnvironment _env = env;
 public IActionResult Index()
        {
            return View();
        }

        /*================================*/
        /*================================*/
        /*==========User Manage===========*/
        /*================================*/
        /*================================*/
        [HttpGet]
        public IActionResult IndexUser(string searchName, string userType)
        {
            var query = _context.Accounts.AsQueryable();

            if (!string.IsNullOrEmpty(searchName))
            {
                query = query.Where(u => u.UserName.Contains(searchName));
            }

            if (!string.IsNullOrEmpty(userType) && userType != "All")
            {
                query = query.Where(u => u.UserType == userType);
            }

            var users = query.ToList();

            ViewBag.SelectedType = userType;
            return View(users);
        }


        /*=======View Create User=======*/
        /*================================*/
        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }
        /*=======Send Users To Data Base=======*/
        /*=====================================*/
        //[HttpPost]
        //public IActionResult CreateUser(string email, string username, string phoneNumber, string password, string userType)
        //{
        //    if (_context.Accounts.Any(u => u.Email == email))
        //    {
        //        ViewBag.Error = "هذا البريد الإلكتروني مسجل بالفعل.";
        //        return View();
        //    }

        //    var user = new User
        //    {
        //        Email = email,
        //        UserName = username,
        //        PhoneNumber = phoneNumber,
        //        Password = password, // في بيئة حقيقية، يجب تشفير كلمة المرور
        //        UserType = userType
        //    };

        //    // أول شي نضيف اليوزر مؤقتاً (بدون SaveChanges)
        //    _context.Accounts.Add(user);

        //    if (userType == "Student")
        //    {
        //        var student = new Student
        //        {
        //            Name = username,
        //            Status = true,
        //            TasksCompleted = 0,
        //            ProjectId = null,
        //        };
        //        _context.Students.Add(student);
        //        _context.SaveChanges(); // هيك رح ينولد ID للطالب

        //        // نربط الطالب باليوزر
        //        user.StudentId = student.Id;
        //    }
        //    else if (userType == "Supervisor")
        //    {
        //        var supervisor = new Supervisor
        //        {
        //            Name = username,
        //        };
        //        _context.Supervisors.Add(supervisor);
        //        _context.SaveChanges(); // ينولد ID للمشرف

        //        // نربط المشرف باليوزر
        //        user.SupervisorId = supervisor.Id;
        //    }
        //    else if (userType == "Admin")
        //    {
        //        var admin = new Admin
        //        {
        //            Name = username,
        //        };
        //        _context.Admins.Add(admin);
        //        _context.SaveChanges(); // ينولد ID للمشرف

        //        // نربط المشرف باليوزر
        //        user.AdminId = admin.Id;
        //    }

        //    // الآن نحفظ اليوزر بعد ما أضفنا الربط
        //    _context.SaveChanges();

        //    return RedirectToAction("Index");
        //}
     
        /*================================*/
        /*================================*/
        /*=========Project Manage=========*/
        /*================================*/
        /*================================*/
        public IActionResult AllProjects()
        {

            var allProjects = _context.Projects
                .Include(p => p.Students)
                .ToList();

            var viewModel = new TeamLeaderViewModel
            {
                AllProjects = allProjects,
            };

            return View(viewModel);
        }
        /*=======View Create Project======*/
        /*================================*/
        [HttpGet]
        public IActionResult CreateProject()
        {
            var model = new CreateProjectViewModel
            {
                Supervisors = _context.Supervisors.ToList(),
                Students = _context.Students
                    .Where(s => s.ProjectId == null)
                    .ToList()
            };
            return View(model);
        }
        /*======Send Project To Data Base======*/
        /*=====================================*/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProject(CreateProjectViewModel model)
        {
         
            string logoUrl = null;
            if (model.LogoImage != null && model.LogoImage.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.LogoImage.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.LogoImage.CopyToAsync(stream);
                }

                logoUrl = "/uploads/" + fileName;
            }

            var project = new Project
            {
                Name = model.Name,
                SupervisorId = model.SupervisorId,
                TeamLeaders = "",
                LogoUrl = logoUrl,
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var selectedStudents = await _context.Students
                .Where(s => model.SelectedStudentIds.Contains(s.Id))
                .ToListAsync();

            foreach (var student in selectedStudents)
            {
                student.ProjectId = project.Id;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index"); 
        }
        /*======Get TeamLeaders======*/
        /*===========================*/
        [HttpGet]
        public IActionResult GetTeamLeaders(int[] studentIds)
        {
            var students = _context.Students
                .Where(s => studentIds.Contains(s.Id)) 
                .Select(s => new { id = s.Id, name = s.Name })
                .ToList();

            return Json(students);
        }
        /*========View Edit Project========*/
        /*=================================*/
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
                AllStudents = allStudents,
                ProjectStudents = new SelectList(project.Students, "Id", "Name"),
                TeamLeaderId = null 
            };

            return View(viewModel);
        }
        /*========Edit Project========*/
        /*============================*/
        [HttpPost]
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
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(model.LogoFile.FileName);
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

            if (model.TeamLeaderId.HasValue)
            {
                var teamLeaderStudentId = model.TeamLeaderId.Value;

                var teamLeaderStudent = _context.Students.FirstOrDefault(s => s.Id == teamLeaderStudentId);
                if (teamLeaderStudent != null)
                {
                    project.TeamLeaders = teamLeaderStudent.Name;

                    var user = _context.Accounts.FirstOrDefault(u => u.StudentId == teamLeaderStudentId);
                    if (user != null)
                    {
                        user.UserType = "TeamLeader";
                    }
                }
            }
            else
            {
                project.TeamLeaders = "بدون قائد"; 
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("EditProject", new { id = model.Id });
        }
        /*==========================================*/
        /*==========================================*/
        /*=============Main Task Manage=============*/
        /*==========================================*/
        /*==========================================*/
        public async Task<IActionResult> IndexMainTask()
        {
            var tasks = await _context.MainTasks.ToListAsync();
            return View(tasks);
        }
        /*========View Create Main Task========*/
        /*=====================================*/
        public IActionResult CreateMainTask()
        {
            return View();
        }
        /*========Create Main Task========*/
        /*================================*/
        [HttpPost]
        public IActionResult CreateMainTask(MainTask mainTask)
        {

            _context.MainTasks.Add(mainTask);
            _context.SaveChanges();
            return RedirectToAction("Index");

        }
        /*========View Edit Main Task========*/
        /*===================================*/
        public async Task<IActionResult> EditMainTask(int id)
        {
            var task = await _context.MainTasks.FindAsync(id);
            return View(task);
        }
        /*========Edit Main Task========*/
        /*==============================*/
        [HttpPost]
        public async Task<IActionResult> EditMainTask(MainTask mainTask)
        {

            _context.Update(mainTask);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");

        }
        /*==========================================*/
        /*==========================================*/
        /*=============Sub Task Manage==============*/
        /*==========================================*/
        /*==========================================*/
        public async Task<IActionResult> IndexSubTask(int? mainTaskId)
        {
            ViewBag.MainTasks = new SelectList(_context.MainTasks, "Id", "Name");

            var subTasks = _context.SubTasks.Include(s => s.MainTask).AsQueryable();

            if (mainTaskId.HasValue)
            {
                subTasks = subTasks.Where(s => s.MainTaskId == mainTaskId);
            }

            return View(await subTasks.ToListAsync());
        }

        /*========View Create Sub Task========*/
        /*====================================*/
        public IActionResult CreateSubTask()
        {
            ViewBag.MainTasks = _context.MainTasks.ToList();
            return View();
        }
        /*========Create Sub Task========*/
        /*===============================*/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSubTask(SubTask subTask)
        {

            _context.SubTasks.Add(subTask);
            await _context.SaveChangesAsync();
            return RedirectToAction("IndexSubTask");
        }
        /*========View Edit Sub Task========*/
        /*==================================*/
        public IActionResult EditSubTask(int id)
        {
            var subTask = _context.SubTasks.Find(id);
            if (subTask == null)
                return NotFound();

            ViewBag.MainTaskList = _context.MainTasks
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(), // أو أي مفتاح رئيسي
                    Text = m.Name            // أو اسم المهمة
                })
                .ToList(); return View(subTask);
        }
        /*========Edit Sub Task========*/
        /*=============================*/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSubTask(SubTask subTask)
        {

            _context.SubTasks.Update(subTask);
            await _context.SaveChangesAsync();
            return RedirectToAction("IndexSubTask");
        }
        /*====================================*/
        /*====================================*/
        /*======Graduation Project Manage=====*/
        /*====================================*/
        /*====================================*/
        public IActionResult IndexGraduationProject()
        {
            return View(_context.GraduationProjects.ToList());
        }
        /*========View Create Graduation Project========*/
        /*==============================================*/
        public IActionResult CreateGraduationProject()
        {
            return View();
        }
        /*========Create Graduation Project========*/
        /*=========================================*/
        [HttpPost]
        public IActionResult CreateGraduationProject(GraduationProject project)
        {
            if (ModelState.IsValid)
            {
                if (project.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + project.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        project.ImageFile.CopyTo(fileStream);
                    }

                    project.ImagePath = "/uploads/" + uniqueFileName;
                }

                _context.GraduationProjects.Add(project);
                _context.SaveChanges();
                return RedirectToAction("IndexGraduationProject");
            }
            return View(project);
        }
        /*========View Edit Graduation Project========*/
        /*============================================*/
        public IActionResult EditGraduationProject(int id)
        {
            var project = _context.GraduationProjects.Find(id);
            if (project == null) return NotFound();
            return View(project);
        }
        /*========Edit Graduation Project========*/
        /*=======================================*/
        [HttpPost]
        public IActionResult EditGraduationProject(GraduationProject project)
        {
            if (ModelState.IsValid)
            {
                var existingProject = _context.GraduationProjects.FirstOrDefault(p => p.Id == project.Id);
                if (existingProject == null)
                    return NotFound();

                existingProject.Title = project.Title;
                existingProject.Year = project.Year;
                existingProject.Students = project.Students;
                existingProject.Supervisor = project.Supervisor;

                if (project.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + project.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        project.ImageFile.CopyTo(fileStream);
                    }

                    if (!string.IsNullOrEmpty(existingProject.ImagePath))
                    {
                        string oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingProject.ImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    existingProject.ImagePath = "/uploads/" + uniqueFileName;
                }

                _context.GraduationProjects.Update(existingProject);
                _context.SaveChanges();
                return RedirectToAction("IndexGraduationProject");
            }
            return View(project);
        }
        [HttpPost]
        public IActionResult SaveUserToDatabase([FromBody] CreateUserModel model)
        {
            if (_context.Accounts.Any(u => u.Email == model.Email))
            {
                return BadRequest("البريد الإلكتروني موجود مسبقاً.");
            }

            var user = new User
            {
                Email = model.Email,
                UserName = model.Username,
                PhoneNumber = model.PhoneNumber,
                Password = model.Password, // يجب تشفيرها في البيئة الفعلية
                UserType = model.UserType
            };

            _context.Accounts.Add(user);

            if (model.UserType == "Student")
            {
                var student = new Student
                {
                    Name = model.Username,
                    Status = true,
                    TasksCompleted = 0,
                    ProjectId = null,
                };
                _context.Students.Add(student);
                _context.SaveChanges();
                user.StudentId = student.Id;
            }
            else if (model.UserType == "Supervisor")
            {
                var supervisor = new Supervisor
                {
                    Name = model.Username
                };
                _context.Supervisors.Add(supervisor);
                _context.SaveChanges();
                user.SupervisorId = supervisor.Id;
            }
            else if (model.UserType == "Admin")
            {
                var admin = new Admin
                {
                    Name = model.Username
                };
                _context.Admins.Add(admin);
                _context.SaveChanges();
                user.AdminId = admin.Id;
            }

            _context.SaveChanges();
            return Ok();
        }

    }
}

