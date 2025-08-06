using System.ComponentModel.DataAnnotations;

namespace Kawther_Project.Models
{
    public class CreateProjectViewModel
    {
        public string Name { get; set; }
        public IFormFile LogoImage { get; set; }
        public int SupervisorId { get; set; }
        public List<int> SelectedStudentIds { get; set; } = new List<int>();
        public List<Supervisor> Supervisors { get; set; }
        public List<Student> Students { get; set; }
    }

}
