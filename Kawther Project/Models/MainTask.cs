namespace Kawther_Project.Models
{
    public class MainTask
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<SubTask> SubTasks { get; set; }

    }

}
