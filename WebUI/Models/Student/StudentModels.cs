using System.ComponentModel.DataAnnotations;

namespace WebUI.Models.Student
{
    public class StudentModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string StudentNumber { get; set; } = "";
        public string Department { get; set; } = "";
        public int Grade { get; set; }
        public string University { get; set; } = "";
    }

    public class StudentSummary
    {
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int CompletedInternships { get; set; }
        public int TotalDiaryEntries { get; set; }
        public int UnreadNotifications { get; set; }
    }
}