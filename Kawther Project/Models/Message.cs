namespace Kawther_Project.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string Text { get; set; }
        public DateTime SentAt { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; }
    }
}
