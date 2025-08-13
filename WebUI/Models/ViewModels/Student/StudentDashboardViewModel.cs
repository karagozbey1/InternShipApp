using System.ComponentModel.DataAnnotations;

namespace WebUI.Models.ViewModels.Student
{
    public class StudentDashboardViewModel
    {
        public string FullName { get; set; }= null!;

        [EmailAddress]
        public string Email { get; set; }= null!;
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int RejectedApplications { get; set; }
    }
}
