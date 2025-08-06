using Microsoft.AspNetCore.Mvc;
using Kawther_Project.Models;
using Kawther_Project.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Kawther_Project
{
    [Route("Chat")]
    public class ChatController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        /*=============Chat Page==========*/
        /*================================*/
        [HttpGet]
        public IActionResult Index(int id, string userType, string name, int projectId)
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

            if (userType == "Student")
            {
                var student = _context.Students.FirstOrDefault(s => s.Id == id);
                if (student == null)
                {
                    return NotFound("الطالب غير موجود");
                }

                var userName = user.UserName;
                var messages = _context.Messages
                    .Where(m => (m.Sender == student.Name && m.Receiver == userName) ||
                                (m.Sender == userName && m.Receiver == student.Name))
                    .OrderBy(m => m.SentAt)
                    .ToList();

                ViewBag.StudentName = student.Name;
                ViewBag.StudentId = student.Id;
                ViewBag.UserName = userName;
                ViewBag.ProjectId = projectId;

                return View(messages);
            }
            else if (userType == "Supervisor")
            {
                var supervisor = _context.Supervisors.FirstOrDefault(s => s.Id == id);
                if (supervisor == null)
                {
                    return NotFound("المشرف غير موجود");
                }

                var userName = user.UserName;
                var messages = _context.Messages
                    .Where(m => (m.Sender == supervisor.Name && m.Receiver == userName) ||
                                (m.Sender == userName && m.Receiver == supervisor.Name))
                    .OrderBy(m => m.SentAt)
                    .ToList();

                ViewBag.SupervisorName = supervisor.Name;
                ViewBag.SupervisorId = supervisor.Id;
                ViewBag.UserName = userName;
                ViewBag.ProjectId = projectId;

                return View(messages);
            }
            else
            {
                return BadRequest("نوع المستخدم غير صالح.");
            }
        }
        /*===========Send Action==========*/
        /*================================*/
        [HttpPost]
        public IActionResult Send(string text, int receiverStudentId, int receiverSuperVisorId, int projectId, string userType)
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

            var project = _context.Projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                return BadRequest("معرف المشروع غير موجود في قاعدة البيانات.");
            }

            var senderName = user.UserName;

            string receiverName = null;
            if (userType == "Student")
            {
                receiverName = _context.Students
                    .Where(s => s.Id == receiverStudentId)
                    .Select(s => s.Name)
                    .FirstOrDefault();
            }
            else if (userType == "Supervisor")
            {
                receiverName = _context.Supervisors
                    .Where(s => s.Id == receiverSuperVisorId)
                    .Select(s => s.Name)
                    .FirstOrDefault();
            }

            if (string.IsNullOrEmpty(receiverName))
            {
                return BadRequest("المستلم غير موجود أو تم إرسال معرف غير صحيح.");
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return BadRequest("نص الرسالة فارغ.");
            }

            var message = new Message
            {
                Sender = senderName,
                Receiver = receiverName,
                Text = text,
                SentAt = DateTime.Now,
                ProjectId = projectId
            };

            _context.Messages.Add(message);
            _context.SaveChanges();

            if (userType == "Student")
            {
                
                return RedirectToAction("Index", new { id = receiverStudentId, userType = userType, name = receiverName, projectId = projectId });

            }
            else if (userType == "Supervisor")
            {
               
                return RedirectToAction("Index", new { id = receiverSuperVisorId, userType = userType, name = receiverName, projectId = projectId });

            }
            else {
                return RedirectToAction("Index");
            }

        }
        /*==========Student List==========*/
        /*================================*/
        [HttpGet("StudentsList")]
        public IActionResult StudentsList(int projectId)
        {
            var project = _context.Projects
                .Include(p => p.Students)
                .FirstOrDefault(p => p.Id == projectId);

            if (project == null)
                return NotFound();

            ViewBag.ProjectId = projectId;
            ViewBag.Students = project.Students;

            return View("StudentsList");
        }
        /*=======View Users Message=======*/
        /*================================*/
        [HttpGet("SupervisorMessages")]
        public IActionResult SupervisorMessages(int projectId)
        {
            var project = _context.Projects
                .Include(p => p.Students)
                .Include(p => p.Supervisor)
                .FirstOrDefault(p => p.Id == projectId);

            if (project == null)
                return NotFound();

            var viewModel = new TeamLeaderViewModel
            {
                Project = project,
                Students = project.Students.ToList()
            };

            return View(viewModel);
        }
    }
}
