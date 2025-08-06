namespace Kawther_Project.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Status { get; set; }
        public int TasksCompleted { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Specialization { get; set; }
        public string? Details { get; set; }
        public int? ProjectId { get; set; }
        public Project Project { get; set; }
        public ICollection<User>? Users { get; set; }


    }
}
