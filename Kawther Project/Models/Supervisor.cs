namespace Kawther_Project.Models
{
    public class Supervisor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Project> Projects { get; set; }

    }
}
