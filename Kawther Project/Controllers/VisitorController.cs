using Kawther_Project.Data;
using Kawther_Project.Models;
using Microsoft.AspNetCore.Mvc;

namespace Kawther_Project.Controllers
{
    public class VisitorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VisitorController(ApplicationDbContext context)
        {
            _context = context;

        }
        /*=============Visitor Home Page==========*/
        /*========================================*/
        public IActionResult Index()
        {
            var viewModel = new VisitorViewModel
            {
                GraduationProjects = _context.GraduationProjects.ToList(),
                DoctorMessages = _context.DoctorMessages.ToList()
            };

            return View(viewModel);
        }
    }
}
