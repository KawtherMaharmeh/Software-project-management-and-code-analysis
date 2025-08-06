using Kawther_Project.Models;
using System.ComponentModel.DataAnnotations;

public class Project
{
    public int Id { get; set; }

    [Required(ErrorMessage = "The project name is required.")]
    public string Name { get; set; }
    public string TeamLeaders { get; set; }
    public string LogoUrl { get; set; } 
    public int SupervisorId { get; set; }
    public Supervisor Supervisor { get; set; }
    public ICollection<Student> Students { get; set; }
    public ICollection<Message> Messages { get; set; }
}
