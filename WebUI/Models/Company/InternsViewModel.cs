namespace WebUI.Models.Company
{
    public class InternsViewModel
    {
        public string CompanyName { get; set; } = "";
        public List<InternViewModel> ActiveInterns { get; set; } = new();
        public List<InternViewModel> CompletedInterns { get; set; } = new();
        public InternStatsViewModel Stats { get; set; } = new();
    }

    public class InternViewModel
    {
        public int Id { get; set; }
        public string StudentName { get; set; } = "";
        public string StudentEmail { get; set; } = "";
        public string University { get; set; } = "";
        public string Department { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Position { get; set; } = "";
        public string Status { get; set; } = "";
        public int CompletedDays { get; set; }
        public int TotalDays { get; set; }
        public decimal ProgressPercentage { get; set; }
    }

    public class InternStatsViewModel
    {
        public int TotalInterns { get; set; }
        public int ActiveInterns { get; set; }
        public int CompletedInterns { get; set; }
        public int UpcomingInterns { get; set; }
    }
}