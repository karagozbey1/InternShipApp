using System.ComponentModel.DataAnnotations;

namespace WebUI.Models.ViewModels.Student.ViewModels
{
    public class CalendarViewModel
    {
        public List<CalendarEvent> Events { get; set; } = new();
        public DateTime CurrentDate { get; set; } = DateTime.Today;
        public string ViewType { get; set; } = "month"; // month, week, day
        
        // Additional properties needed by Calendar.cshtml
        public string MonthTitle { get; set; } = "";
        public int SelectedDay { get; set; }
        public List<int> PeriodDays { get; set; } = new();
        public List<int> ApplicationDays { get; set; } = new();
        public List<int> DiaryDays { get; set; } = new();
        
        // Data for events
        public List<CalendarPeriod> Periods { get; set; } = new();
        public List<CalendarApplication> Applications { get; set; } = new();
        public List<CalendarDiaryEntry> DiaryEntries { get; set; } = new();
    }

    public class CalendarEvent
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Type { get; set; } = "";
        public string Color { get; set; } = "";
        public bool AllDay { get; set; }
    }
    
    public class CalendarPeriod
    {
        public string Title { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; } = "";
    }
    
    public class CalendarApplication
    {
        public string Title { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CompanyName { get; set; } = "";
    }
    
    public class CalendarDiaryEntry
    {
        public string Title { get; set; } = "";
        public DateTime Date { get; set; }
        public string Content { get; set; } = "";
    }
}