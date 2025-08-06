namespace Kawther_Project.Models
{
    public class TeamLeaderViewModel
    {
        public Project Project { get; set; }
        public List<Project> AllProjects { get; set; }
        public List<GraduationProject> GraduationProjects { get; set; }
        public List<DoctorMessage> DoctorMessages { get; set; }
        public List<Student> Students { get; set; }
        public List<Supervisor>  Supervisor { get; set; }

    }
}
