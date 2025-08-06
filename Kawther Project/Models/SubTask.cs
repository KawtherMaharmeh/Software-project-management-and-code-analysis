namespace Kawther_Project.Models
{
    public class SubTask
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int MainTaskId { get; set; }
        public MainTask MainTask { get; set; }
    }

}
