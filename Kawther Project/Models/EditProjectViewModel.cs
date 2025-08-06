using Microsoft.AspNetCore.Mvc.Rendering;

namespace Kawther_Project.Models
{
    public class EditProjectViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string TeamLeaders { get; set; }
        public string LogoUrl { get; set; }
        public int SupervisorId { get; set; }
        public List<int> SelectedStudentIds { get; set; } = new List<int>();
        public List<SelectListItem> AllStudents { get; set; } = new List<SelectListItem>();
        public int? TeamLeaderId { get; set; }
        public SelectList ProjectStudents { get; set; }
        public IFormFile LogoFile { get; set; }
    }

}
