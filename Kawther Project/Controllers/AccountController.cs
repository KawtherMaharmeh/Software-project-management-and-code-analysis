using Kawther_Project.Data;
using Kawther_Project.Models;
using Microsoft.AspNetCore.Mvc;

namespace Kawther_Project.Controllers
{
    public class AccountController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        /*================================*/
        /*=============Log in=============*/
        /*================================*/
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                string userType = HttpContext.Session.GetString("UserType");
                return userType switch
                {
                    "Student" => RedirectToAction("Index", "Student"),
                    "Supervisor" => RedirectToAction("Index", "Supervisor"),
                    "TeamLeader" => RedirectToAction("Index", "TeamLeader"),
                    "Admin" => RedirectToAction("Index", "Admin"),
                    _ => RedirectToAction("Index", "Visitor") 

                };
            }
            return View();
        }
        /*=======Veryfication Data On Data Base=======*/
        /*============================================*/
        //[HttpPost]
        //public IActionResult Login(string email, string password)
        //{
        //    var user = _context.Accounts
        //        .FirstOrDefault(u => u.Email == email && u.Password == password);

        //    if (user != null)
        //    {
        //        HttpContext.Session.SetString("UserEmail", user.Email);
        //        HttpContext.Session.SetString("UserType", user.UserType);

        //        if (user.UserType == "Student")
        //        {
        //            var student = _context.Students
        //                .FirstOrDefault(s => s.Id == user.StudentId);

        //            if (student != null && student.Project != null)
        //            {
        //                HttpContext.Session.SetString("ProjectName", student.Project.Name);
        //                HttpContext.Session.SetInt32("ProjectId", student.Project.Id);
        //            }
        //        }

        //        return user.UserType switch
        //        {
        //            "Student" => RedirectToAction("Index", "Student"),
        //            "Supervisor" => RedirectToAction("Index", "Supervisor"),
        //            "TeamLeader" => RedirectToAction("Index", "TeamLeader"),
        //            "Admin" => RedirectToAction("Index", "Admin"), 
        //            _ => RedirectToAction("Login"),
        //        };
        //    }

        //    ViewBag.Error = "البريد الإلكتروني أو كلمة المرور غير صحيحة.";
        //    return View();
        //}
        [HttpPost]
        public IActionResult LoginFromFirebase([FromBody] FirebaseLoginModel model)
        {
            var user = _context.Accounts.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                return Unauthorized();
            }

            // تخزين البيانات في الجلسة
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserType", user.UserType);

            return user.UserType switch
            {
                "Student" => RedirectToAction("Index", "Student"),
                "Supervisor" => RedirectToAction("Index", "Supervisor"),
                "TeamLeader" => RedirectToAction("Index", "TeamLeader"),
                "Admin" => RedirectToAction("Index", "Admin"),
                _ => RedirectToAction("Index", "Visitor")
            };
        }
        /*================================*/
        /*=============Log out============*/
        /*================================*/
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
        [HttpGet]
        public IActionResult RedirectBasedOnRole()
        {
            var userType = HttpContext.Session.GetString("UserType");

            return userType switch
            {
                "Student" => RedirectToAction("Index", "Student"),
                "Supervisor" => RedirectToAction("Index", "Supervisor"),
                "TeamLeader" => RedirectToAction("Index", "TeamLeader"),
                "Admin" => RedirectToAction("Index", "Admin"),
                _ => RedirectToAction("Index", "Visitor") 
            };
        }
    }
}
