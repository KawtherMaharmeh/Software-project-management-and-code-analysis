using System.ComponentModel.DataAnnotations;

namespace Kawther_Project.Models
{
    public class TaskModel
    {
        [Key]
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int ProjectId { get; set; }
        public int MainTaskId { get; set; }
        public int SubTaskId { get; set; }
        public string? Note { get; set; }
        public string TaskStatus { get; set; } = "In Progress";
        public DateTime EndTime { get; set; }
        public DateTime StartTime { get; set; }
        public byte[]? FileData { get; set; }      
        public string? FileName { get; set; }      
        public string? FileContentType { get; set; }
        public Student Student { get; set; }
        public SubTask SubTask { get; set; }
        public Project Project { get; set; }  
    }
}
