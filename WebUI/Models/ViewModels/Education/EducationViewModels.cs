using System.ComponentModel.DataAnnotations;

namespace WebUI.Models.ViewModels.Education
{
    public class AcademicYearInfo
    {
        public string AcademicYear { get; set; } = "";
        public string CurrentSemester { get; set; } = "";
        public DateTime SemesterStartDate { get; set; }
        public DateTime SemesterEndDate { get; set; }
        public int WeeksRemaining { get; set; }
        public string InternshipPeriod { get; set; } = "";
        public string Year { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public string Semester { get; set; } = "";
    }

    public class UniversityDropdownViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public string City { get; set; } = "";
        public string Type { get; set; } = ""; // State, Private, Foundation
    }

    public class UniversityViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public string Address { get; set; } = "";
        public string City { get; set; } = "";
        public string Country { get; set; } = "Türkiye";
        public string Type { get; set; } = "";
        public string Website { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime FoundedYear { get; set; }
        public string Logo { get; set; } = "";
        public bool IsActive { get; set; } = true;
        
        // Statistics
        public int StudentCount { get; set; }
        public int AdvisorCount { get; set; }
        public int DepartmentCount { get; set; }
        public int ActiveInternshipCount { get; set; }
    }

    public class DepartmentViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public string Description { get; set; } = "";
        public int UniversityId { get; set; }
        public string UniversityName { get; set; } = "";
        public string Faculty { get; set; } = "";
        public bool IsActive { get; set; } = true;
        
        // Statistics
        public int StudentCount { get; set; }
        public int AdvisorCount { get; set; }
        public int InternshipCount { get; set; }
    }

    public class CreateUniversityViewModel
    {
        [Required(ErrorMessage = "Üniversite adý zorunludur")]
        [StringLength(200, ErrorMessage = "Üniversite adý 200 karakterden uzun olamaz")]
        public string Name { get; set; } = "";
        
        [Required(ErrorMessage = "Üniversite kodu zorunludur")]
        [StringLength(10, ErrorMessage = "Üniversite kodu 10 karakterden uzun olamaz")]
        public string Code { get; set; } = "";
        
        [Required(ErrorMessage = "Adres zorunludur")]
        [StringLength(500, ErrorMessage = "Adres 500 karakterden uzun olamaz")]
        public string Address { get; set; } = "";
        
        [Required(ErrorMessage = "Þehir zorunludur")]
        [StringLength(50, ErrorMessage = "Þehir adý 50 karakterden uzun olamaz")]
        public string City { get; set; } = "";
        
        [StringLength(50, ErrorMessage = "Ülke adý 50 karakterden uzun olamaz")]
        public string Country { get; set; } = "Türkiye";
        
        [Required(ErrorMessage = "Üniversite türü zorunludur")]
        public string Type { get; set; } = "";
        
        [Url(ErrorMessage = "Geçerli bir web sitesi adresi giriniz")]
        public string? Website { get; set; }
        
        [Phone(ErrorMessage = "Geçerli bir telefon numarasý giriniz")]
        public string? Phone { get; set; }
        
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string? Email { get; set; }
        
        [Required(ErrorMessage = "Kuruluþ yýlý zorunludur")]
        public DateTime FoundedYear { get; set; }
        
        public string? Logo { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CreateDepartmentViewModel
    {
        [Required(ErrorMessage = "Bölüm adý zorunludur")]
        [StringLength(100, ErrorMessage = "Bölüm adý 100 karakterden uzun olamaz")]
        public string Name { get; set; } = "";
        
        [Required(ErrorMessage = "Bölüm kodu zorunludur")]
        [StringLength(10, ErrorMessage = "Bölüm kodu 10 karakterden uzun olamaz")]
        public string Code { get; set; } = "";
        
        [StringLength(500, ErrorMessage = "Açýklama 500 karakterden uzun olamaz")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Üniversite seçimi zorunludur")]
        public int UniversityId { get; set; }
        
        [Required(ErrorMessage = "Fakülte adý zorunludur")]
        [StringLength(100, ErrorMessage = "Fakülte adý 100 karakterden uzun olamaz")]
        public string Faculty { get; set; } = "";
        
        public bool IsActive { get; set; } = true;
        
        // For dropdown
        public List<UniversityDropdownViewModel> Universities { get; set; } = new();
    }

    public class UniversityListViewModel
    {
        public List<UniversityViewModel> Universities { get; set; } = new();
        public string SearchTerm { get; set; } = "";
        public string CityFilter { get; set; } = "";
        public string TypeFilter { get; set; } = "";
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalCount { get; set; }
        
        // Filter options
        public List<string> AvailableCities { get; set; } = new();
        public List<string> AvailableTypes { get; set; } = new();
    }

    public class DepartmentListViewModel
    {
        public List<DepartmentViewModel> Departments { get; set; } = new();
        public string SearchTerm { get; set; } = "";
        public int? UniversityFilter { get; set; }
        public string FacultyFilter { get; set; } = "";
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalCount { get; set; }
        
        // Filter options
        public List<UniversityDropdownViewModel> Universities { get; set; } = new();
        public List<string> AvailableFaculties { get; set; } = new();
    }
}