using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kawther_Project.Data;
using Kawther_Project.Models;


namespace Kawther_Project.Controllers
{
    [Route("TeamLeader")]
    public class TeamLeaderController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        /*=============TeamLeader Home Page==========*/
        /*===========================================*/
        [HttpGet("TeamLeader/Index")]
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
        /*=============View My Task==========*/
        /*==============================================*/
        [HttpGet("TeamLeader/MyTask")]
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
        /*=============Submit Task==========*/
        /*==================================*/
        [HttpPost]
        [Route("TeamLeader/MarkTaskAsCompleted")]
        public async Task<IActionResult> MarkTaskAsCompleted(int taskId, IFormFile file)
        {
            var task = _context.Tasks.FirstOrDefault(t => t.Id == taskId);

            if (task != null && task.TaskStatus == "In Progress")
            {
                if (file != null && file.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);

                        task.FileData = memoryStream.ToArray();
                        task.FileName = file.FileName;
                        task.FileContentType = file.ContentType;
                    }

                    task.TaskStatus = "Completed";

                    var student = _context.Students.FirstOrDefault(s => s.Id == task.StudentId);
                    if (student != null)
                    {
                        student.TasksCompleted += 1;
                        student.Status = true;
                    }

                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("MyTask");
        }
        /*=============Cancel Task==========*/
        /*==================================*/
        [HttpPost]
        [Route("TeamLeader/MarkTaskAsCanceled")]
        public IActionResult MarkTaskAsCanceled(int taskId)
        {
            var task = _context.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null && task.TaskStatus == "In Progress")
            {
                task.TaskStatus = "Cancel";
                _context.SaveChanges();
            }
            return RedirectToAction("MyTask");
        }
        /*=============Veiw Add Task Page==========*/
        /*=========================================*/
        [HttpGet("TeamLeader/AddTask")]
        public async Task<IActionResult> AddTask()
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
                return RedirectToAction("Index");
            }

            var projectId = student.ProjectId;

            ViewBag.Students = await _context.Students
             .Where(s => s.ProjectId == projectId && s.Status == true)
             .ToListAsync();

            ViewBag.MainTaskTypes = await _context.MainTasks.ToListAsync();
            ViewBag.SubTasks = await _context.SubTasks.ToListAsync();

            return View();
        }
        /*=============View Sub Task==========*/
        /*====================================*/
        [HttpGet("GetSubTasks")]
        public IActionResult GetSubTasks(int mainTaskId)
        {
            if (mainTaskId == 0)
            {
                return BadRequest("لم يتم تحديد المهمة الرئيسية.");
            }

            var subTasks = _context.SubTasks
                .Where(st => st.MainTaskId == mainTaskId)
                .Select(st => new { st.Id, st.Name })
                .ToList();

            if (subTasks == null || subTasks.Count == 0)
            {
                return NotFound("لا توجد مهام فرعية متاحة.");
            }

            return Json(subTasks);
        }
        /*=============Add New Task==========*/
        /*===================================*/
        [HttpPost("AddTask")]
        public IActionResult AddTask([FromBody] TaskModel model)
        {
            if (model == null)
            {
                return BadRequest("بيانات غير صالحة.");
            }

            var student = _context.Students.FirstOrDefault(s => s.Id == model.StudentId);
            if (student == null)
            {
                return BadRequest("الطالب المحدد غير موجود.");
            }

            int projectId = (int)student.ProjectId;

            var newTask = new TaskModel
            {
                StudentId = model.StudentId,
                MainTaskId = model.MainTaskId,
                SubTaskId = model.SubTaskId,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                ProjectId = projectId,
                Note = model.Note,
                TaskStatus = "In Progress"
            };

            _context.Tasks.Add(newTask);

            student.Status = false;

            _context.SaveChanges();

            return Json(new { success = true, message = "تمت إضافة المهمة بنجاح!" });
        }
        /*=============Follow Project Progress==========*/
        /*==============================================*/
        [HttpGet("TeamLeader/FollowProjectProgress")]
        public IActionResult FollowProjectProgress()
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

            var student = _context.Students.Include(s => s.Project).FirstOrDefault(s => s.Id == user.StudentId);

            if (student == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var tasks = _context.Tasks
                                .Where(t => t.ProjectId == student.ProjectId)
                                .ToList();

            var studentNames = _context.Students.ToDictionary(s => s.Id, s => s.Name);
            var subTaskNames = _context.SubTasks.ToDictionary(st => st.Id, st => st.Name);

            var model = new ProjectTasksViewModel
            {
                Tasks = tasks.Select(t => new TaskModel
                {
                    Id = t.Id,
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
        /*=============View File Task==========*/
        /*=====================================*/
        [HttpGet("TeamLeader/ViewFile/{taskId}")]
        public IActionResult ViewFile(int taskId)
        {
            var task = _context.Tasks.FirstOrDefault(t => t.Id == taskId);

            if (task == null || task.FileData == null || task.FileContentType == null)
            {
                return NotFound("File not found");
            }

            return File(task.FileData, task.FileContentType, task.FileName);
        }
        /*=============Download All File For Project==========*/
        /*====================================================*/
        [HttpGet("TeamLeader/DownloadAllFiles/{projectId}")]
        public IActionResult DownloadAllFiles(int projectId)
        {
            var tasksWithFiles = _context.Tasks
                .Where(t => t.ProjectId == projectId && t.FileData != null)
                .ToList();

            if (!tasksWithFiles.Any())
            {
                return NotFound("No files found for this project.");
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
                {
                    foreach (var task in tasksWithFiles)
                    {
                        var entry = archive.CreateEntry(task.FileName ?? $"file_{task.Id}", System.IO.Compression.CompressionLevel.Fastest);
                        using var entryStream = entry.Open();
                        entryStream.Write(task.FileData, 0, task.FileData.Length);
                    }
                }

                memoryStream.Position = 0;
                return File(memoryStream.ToArray(), "application/zip", $"ProjectFiles_{projectId}.zip");
            }
        }
        /*=============View Task Student==========*/
        /*========================================*/
        [HttpGet("TeamLeader/StudentTasks/{studentId}")]
        public IActionResult StudentTasks(int studentId)
        {
            var student = _context.Students.FirstOrDefault(s => s.Id == studentId);
            if (student == null)
            {
                return NotFound("الطالب غير موجود");
            }

            var tasks = _context.Tasks
                .Where(t => t.StudentId == studentId)
                .ToList();

            var mainTaskNames = _context.MainTasks.ToDictionary(mt => mt.Id, mt => mt.Name);
            var subTaskNames = _context.SubTasks.ToDictionary(st => st.Id, st => st.Name);

            var taskViewModels = tasks.Select(t => new TaskDisplayViewModel
            {
                MainTaskName = mainTaskNames.ContainsKey(t.MainTaskId) ? mainTaskNames[t.MainTaskId] : "غير معروف",
                SubTaskName = subTaskNames.ContainsKey(t.SubTaskId) ? subTaskNames[t.SubTaskId] : "غير معروف",
                TaskStatus = t.TaskStatus
            }).ToList();

            ViewBag.StudentName = student.Name;

            return View("StudentTasks", taskViewModels);
        }
        /*=============View Review Code Page==========*/
        /*============================================*/
        [HttpGet("ReviewCode")]
        public IActionResult ReviewCode()
        {
            return View();
        }
        /*=============Call API AI & Review Code==========*/
        /*================================================*/
        [HttpPost("ReviewCode")]
        public async Task<IActionResult> ReviewCodeApi([FromBody] CodeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
                return BadRequest("Code is empty");

            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "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");

            var data = new
            {
                model = "gpt-4",
                messages = new[]
                {
                   new { role = "user", content = $"Analyze this code and suggest improvements:\n\n{request.Code}" }
                },
                temperature = 0.5
            };

            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(data), System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var result = await response.Content.ReadAsStringAsync();

            using var doc = System.Text.Json.JsonDocument.Parse(result);
            var reply = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            return Ok(reply);
        }
    }
}
