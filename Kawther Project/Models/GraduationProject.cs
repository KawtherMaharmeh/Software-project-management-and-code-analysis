using System.ComponentModel.DataAnnotations.Schema;

namespace Kawther_Project.Models
{
    public class GraduationProject
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public string Students { get; set; }
        public string Supervisor { get; set; }
        public string? ImagePath { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }
    }
}
