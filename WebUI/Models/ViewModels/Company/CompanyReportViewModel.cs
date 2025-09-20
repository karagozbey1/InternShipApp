using Core.Entity;

namespace WebUI.Models.ViewModels.Company
{
    // Þirket rapor dashboard'u için ana ViewModel
    // Þirket yöneticilerinin baþvuru istatistiklerini, trendleri ve performans metriklerini
    // tek bir sayfada görüntülemesi için tasarlanmýþ kapsamlý veri modeli
    public class CompanyReportViewModel
    {
        // Genel baþvuru istatistikleri - özet bilgiler için
        public int TotalApplications { get; set; }        // Toplam alýnan baþvuru sayýsý
        public int PendingApplications { get; set; }      // Deðerlendirme bekleyen baþvurular
        public int ApprovedApplications { get; set; }     // Kabul edilen baþvuru sayýsý
        public int RejectedApplications { get; set; }     // Reddedilen baþvuru sayýsý
        
        // View'lar tarafýndan kullanýlan ek istatistikler
        public int ThisMonthApplications { get; set; }    // Bu aydaki baþvuru sayýsý
        public int ActiveInterns { get; set; }            // Þu anda aktif olan stajyer sayýsý
        
        // Performans metrikleri - KPI'lar için
        public decimal ApprovalRate { get; set; }         // Onay oraný (approved/total)

        // Tamamlanan staj istatistiði - baþarýyla bitirilen stajlarýn sayýsý
        // Business value: þirketin staj programý baþarý oranýný gösterir
        public int CompletedInterns { get; set; }

        // Grafik ve chart verileri için koleksiyonlar - dashboard visualization
        public List<MonthlyApplicationData> MonthlyData { get; set; } = new();        // Aylýk baþvuru trendi
        public List<DepartmentApplicationData> DepartmentData { get; set; } = new();  // Bölüm bazlý daðýlým
        public List<StatusApplicationData> StatusData { get; set; } = new();          // Durum bazlý pie chart
        public List<RecentActivityData> RecentActivities { get; set; } = new();      // Son aktiviteler timeline
    }

    // Aylýk baþvuru trend verisi - time series chart için
    // Þirketin hangi aylarda daha fazla baþvuru aldýðýný analiz etmek için
    // Mevsimsel trendleri ve dönemsel deðiþimleri gösterir
    public class MonthlyApplicationData
    {
        public string Month { get; set; } = "";          // Ay adý display (örn: "Ocak", "Þubat")
        public int ApplicationCount { get; set; }        // O aydaki toplam baþvuru sayýsý
        public int ApprovalCount { get; set; }           // O aydaki onaylanan baþvuru sayýsý
        public int MonthNumber { get; set; }             // Numerik ay deðeri (1-12) sýralama için
    }

    // Departman/bölüm bazlý baþvuru analizi - demographic insight için
    // Hangi üniversite bölümlerinden daha fazla baþvuru geldiðini gösterir
    // ÝK stratejileri ve hedef kitle analizi için kullanýlýr
    public class DepartmentApplicationData
    {
        public string Department { get; set; } = "";     // Bölüm adý (örn: "Bilgisayar Mühendisliði")
        public int Count { get; set; }                   // Bu bölümden gelen baþvuru sayýsý
        public decimal Percentage { get; set; }          // Toplam baþvuru içindeki yüzde oraný
    }

    // Baþvuru durumu daðýlým verisi - status distribution pie chart için
    // Pending/Approved/Rejected baþvurularýn oransal daðýlýmýný gösterir
    // Decision making ve recruitment efficiency analizi için
    public class StatusApplicationData
    {
        public string Status { get; set; } = "";         // Durum adý ("Bekliyor", "Onaylandý" vb.)
        public int Count { get; set; }                   // Bu durumdaki baþvuru sayýsý
        public decimal Percentage { get; set; }          // Toplam içindeki yüzde oraný
        public string Color { get; set; } = "";          // Chart'ta kullanýlacak renk kodu
    }

    // Son aktiviteler timeline verisi - activity feed için
    // Þirketin son iþlemlerini chronological order'da gösterir
    // Real-time business activity tracking için
    public class RecentActivityData
    {
        public string Activity { get; set; } = "";       // Aktivite açýklamasý (örn: "Yeni baþvuru alýndý")
        public DateTime ActivityDate { get; set; }       // Aktivite gerçekleþme zamaný
        public string ActivityType { get; set; } = "";   // Aktivite kategorisi (Application, Approval vb.)
        public string? StudentName { get; set; }         // Ýlgili öðrenci adý (varsa)
        public string? PositionTitle { get; set; }       // Ýlgili pozisyon baþlýðý (varsa)
        public int? RelatedId { get; set; }              // Ýlgili entity ID'si (link için)
    }

    // Detaylý baþvuru raporu ViewModel'i - drill-down analiz için
    // Specific baþvuru üzerinde detaylý analiz yapmak isteyenler için
    public class ApplicationReportDetailViewModel
    {
        // Baþvuru temel bilgileri
        public int ApplicationId { get; set; }
        public string CompanyName { get; set; } = "";
        public string PositionTitle { get; set; } = "";
        public string StudentName { get; set; } = "";
        public string University { get; set; } = "";
        public string Department { get; set; } = "";
        
        // Timeline verileri
        public DateTime ApplicationDate { get; set; }     // Baþvuru tarihi
        public DateTime? ReviewDate { get; set; }         // Ýnceleme tarihi
        public DateTime? DecisionDate { get; set; }       // Karar tarihi
        
        // Süreç metrikleri
        public int ProcessingDays { get; set; }           // Kaç günde karar verildi
        public InternshipApplicationStatus CurrentStatus { get; set; }
        
        // Performance insights
        public List<string> ProcessingSteps { get; set; } = new();    // Süreç adýmlarý
        public List<string> RequiredDocuments { get; set; } = new(); // Gerekli belgeler
        public List<string> SubmittedDocuments { get; set; } = new();// Sunulan belgeler
    }

    // Baþvuru kalitesi analiz ViewModel'i - recruitment quality assessment
    // HR department'ýn baþvuru kalitesini deðerlendirmesi için
    public class ApplicationQualityAnalysisViewModel
    {
        // Kalite metrikleri
        public decimal AverageGPA { get; set; }                    // Ortalama GPA
        public int TotalCVsReviewed { get; set; }                 // Ýncelenen CV sayýsý
        public int QualifiedCandidates { get; set; }              // Nitelikli aday sayýsý
        public decimal QualificationRate { get; set; }            // Niteliklendirme oraný
        
        // Skill analysis
        public List<SkillDemandData> SkillDemands { get; set; } = new();
        
        // University ranking analysis
        public List<UniversityPerformanceData> UniversityPerformance { get; set; } = new();
    }

    // Beceri talep analizi - skill gap analysis için
    // Þirketin hangi becerilerde daha fazla talep ettiðini gösterir
    public class SkillDemandData
    {
        public string SkillName { get; set; } = "";      // Beceri adý
        public int DemandCount { get; set; }             // Kaç pozisyonda arandý
        public int SupplyCount { get; set; }             // Kaç baþvuruda mevcut
        public decimal GapPercentage { get; set; }       // Arz-talep açýðý yüzdesi
    }

    // Üniversite performans analizi - academic institution insights
    // Hangi üniversitelerden daha kaliteli adaylar geldiðini gösterir
    public class UniversityPerformanceData
    {
        public string UniversityName { get; set; } = "";
        public int ApplicationCount { get; set; }        // Bu üniversiteden gelen baþvuru sayýsý
        public int HiredCount { get; set; }              // Ýþe alýnan öðrenci sayýsý
        public decimal HireRate { get; set; }            // Ýþe alým oraný
        public decimal AveragePerformance { get; set; }  // Ortalama performans skoru
    }

    // Zaman bazlý performans analizi - temporal analytics
    // Belirli zaman dilimlerinde þirket performansýný karþýlaþtýrma
    public class TimeBasedPerformanceViewModel
    {
        public string Period { get; set; } = "";         // Zaman dilimi ("Q1 2024", "Ocak 2024" vb.)
        public int Applications { get; set; }            // O dönemdeki baþvuru sayýsý
        public int Interviews { get; set; }              // Mülakat sayýsý
        public int Hires { get; set; }                   // Ýþe alým sayýsý
        public decimal ConversionRate { get; set; }      // Baþvuru-iþe alým dönüþüm oraný
        public decimal AverageTimeToHire { get; set; }   // Ortalama iþe alým süresi (gün)
    }

    // Export/Download için rapor ViewModel'i
    // Excel, PDF export iþlemleri için structured data
    public class ExportableReportViewModel
    {
        public string ReportTitle { get; set; } = "";
        public DateTime GeneratedAt { get; set; }        // Rapor oluþturma zamaný
        public string CompanyName { get; set; } = "";
        public string ReportPeriod { get; set; } = "";   // Rapor dönemi
        
        // Export edilecek data sections
        public CompanyReportViewModel SummaryData { get; set; } = new();
        public List<ApplicationReportDetailViewModel> DetailedApplications { get; set; } = new();
        public ApplicationQualityAnalysisViewModel QualityAnalysis { get; set; } = new();
        public List<TimeBasedPerformanceViewModel> TimeBasedAnalysis { get; set; } = new();
        
        // Meta information
        public string GeneratedBy { get; set; } = "";    // Raporu oluþturan kullanýcý
        public List<string> IncludedSections { get; set; } = new(); // Dahil edilen bölümler
        public string ExportFormat { get; set; } = "";   // Export formatý (PDF, Excel vb.)
    }
}