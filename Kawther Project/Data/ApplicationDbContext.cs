using Kawther_Project.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Kawther_Project.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<GraduationProject> GraduationProjects { get; set; }
        public DbSet<DoctorMessage> DoctorMessages { get; set; }



        public DbSet<User> Accounts { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<MainTask> MainTasks { get; set; }
        public DbSet<SubTask> SubTasks { get; set; }
        public DbSet<TaskModel> Tasks { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Supervisor> Supervisors { get; set; }
        public DbSet<Admin> Admins { get; set; }


        public DbSet<Message> Messages { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Student>()
              .HasOne(s => s.Project)
              .WithMany(p => p.Students)
              .HasForeignKey(s => s.ProjectId)
              .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Student)
                .WithMany(s => s.Users)
                .HasForeignKey(u => u.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Supervisor)
                .WithMany()
                .HasForeignKey(u => u.SupervisorId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Admin)
                .WithMany()
                .HasForeignKey(u => u.AdminId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SubTask>()
                .HasOne(s => s.MainTask)
                .WithMany(m => m.SubTasks)
                .HasForeignKey(s => s.MainTaskId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskModel>()
                .HasOne(t => t.Student)
                .WithMany()
                .HasForeignKey(t => t.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskModel>()
                .HasOne(t => t.Project)
                .WithMany()
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskModel>()
                .HasOne(t => t.SubTask)
                .WithMany()
                .HasForeignKey(t => t.SubTaskId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}