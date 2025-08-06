namespace Kawther_Project.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string UserType { get; set; }
        public int? StudentId { get; set; }
        public Student? Student { get; set; }
        public int? SupervisorId { get; set; }
        public Supervisor? Supervisor { get; set; }
        public int? AdminId { get; set; }
        public Admin? Admin { get; set; }
    }
}
