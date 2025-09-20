using System.ComponentModel.DataAnnotations;

namespace WebUI.Models.ViewModels.Company
{
    /// <summary>
    /// Þirket raporlarý sayfasý için ViewModel
    /// Dashboard ve analitik verilerini içerir
    /// </summary>
    public class CompanyReportViewModel
    {
        /// <summary>
        /// Toplam baþvuru sayýsý
        /// Þirketin aldýðý tüm baþvurularýn toplamý
        /// </summary>
        public int TotalApplications { get; set; }

        /// <summary>
        /// Bu ay alýnan baþvuru sayýsý
        /// Aylýk performans göstergesi
        /// </summary>
        public int ThisMonthApplications { get; set; }

        /// <summary>
        /// Aktif stajyer sayýsý
        /// Þu anda çalýþmakta olan stajyerler
        /// </summary>
        public int ActiveInterns { get; set; }

        /// <summary>
        /// Onaylanan baþvuru sayýsý
        /// Kabul edilmiþ baþvurularýn toplamý
        /// </summary>
        public int ApprovedApplications { get; set; }

        /// <summary>
        /// Beklemede olan baþvuru sayýsý
        /// Henüz deðerlendirilmemiþ baþvurular
        /// </summary>
        public int PendingApplications { get; set; }

        /// <summary>
        /// Reddedilen baþvuru sayýsý
        /// Kabul edilmeyen baþvurularýn toplamý
        /// </summary>
        public int RejectedApplications { get; set; }

        /// <summary>
        /// Onaylanma oraný (yüzde)
        /// Onaylanan baþvuru / Toplam baþvuru * 100
        /// </summary>
        public decimal ApprovalRate { get; set; }

        /// <summary>
        /// Tamamlanan staj sayýsý
        /// Baþarýyla bitirilen stajlarýn toplamý
        /// </summary>
        public int CompletedInterns { get; set; }

        /// <summary>
        /// Aylýk baþvuru verileri
        /// Grafik gösterimi için aylara göre daðýlým
        /// </summary>
        public List<MonthlyApplicationData> MonthlyData { get; set; } = new();

        /// <summary>
        /// Bölüm bazýnda baþvuru daðýlýmý
        /// Hangi bölümlerden ne kadar baþvuru geldiði
        /// </summary>
        public List<DepartmentApplicationData> DepartmentData { get; set; } = new();

        /// <summary>
        /// Durum bazýnda baþvuru daðýlýmý
        /// Baþvuru durumlarýnýn oransal daðýlýmý
        /// </summary>
        public List<StatusApplicationData> StatusData { get; set; } = new();

        /// <summary>
        /// Son aktiviteler
        /// Þirketin son iþlemleri ve güncellemeleri
        /// </summary>
        public List<RecentActivityData> RecentActivities { get; set; } = new();
    }

    /// <summary>
    /// Aylýk baþvuru verisi
    /// Belirli bir ay için baþvuru sayýsý
    /// </summary>
    public class MonthlyApplicationData
    {
        /// <summary>
        /// Ay adý (örn: "Ocak", "Þubat")
        /// </summary>
        public string Month { get; set; } = "";

        /// <summary>
        /// O aydaki toplam baþvuru sayýsý
        /// </summary>
        public int ApplicationCount { get; set; }

        /// <summary>
        /// O aydaki onaylanan baþvuru sayýsý
        /// </summary>
        public int ApprovalCount { get; set; }

        /// <summary>
        /// Ay numarasý (1-12)
        /// Sýralama için kullanýlýr
        /// </summary>
        public int MonthNumber { get; set; }
    }

    /// <summary>
    /// Bölüm bazýnda baþvuru verisi
    /// Üniversite bölümlerine göre baþvuru daðýlýmý
    /// </summary>
    public class DepartmentApplicationData
    {
        /// <summary>
        /// Bölüm adý (örn: "Bilgisayar Mühendisliði")
        /// </summary>
        public string Department { get; set; } = "";

        /// <summary>
        /// Bu bölümden gelen baþvuru sayýsý
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Toplam baþvuru içindeki yüzde oraný
        /// </summary>
        public decimal Percentage { get; set; }
    }

    /// <summary>
    /// Durum bazýnda baþvuru verisi
    /// Baþvuru durumlarýna göre daðýlým
    /// </summary>
    public class StatusApplicationData
    {
        /// <summary>
        /// Baþvuru durumu (örn: "Beklemede", "Onaylandý")
        /// </summary>
        public string Status { get; set; } = "";

        /// <summary>
        /// Bu durumdaki baþvuru sayýsý
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Toplam baþvuru içindeki yüzde oraný
        /// </summary>
        public decimal Percentage { get; set; }

        /// <summary>
        /// Grafik gösterimi için renk kodu
        /// </summary>
        public string ColorCode { get; set; } = "";
    }

    /// <summary>
    /// Son aktivite verisi
    /// Þirketin son iþlemleri için
    /// </summary>
    public class RecentActivityData
    {
        /// <summary>
        /// Aktivite baþlýðý
        /// </summary>
        public string Title { get; set; } = "";

        /// <summary>
        /// Aktivite açýklamasý
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Aktivite tarihi
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Aktivite türü (örn: "application", "approval")
        /// </summary>
        public string ActivityType { get; set; } = "";

        /// <summary>
        /// Ýlgili kayýt ID'si (opsiyonel)
        /// </summary>
        public int? RelatedId { get; set; }

        /// <summary>
        /// Aktivite ikonu CSS sýnýfý
        /// </summary>
        public string IconClass { get; set; } = "fas fa-info-circle";

        /// <summary>
        /// Zaman farký metni (örn: "2 saat önce")
        /// </summary>
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.Now - CreatedAt;
                if (timeSpan.TotalMinutes < 1)
                    return "Az önce";
                if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes} dakika önce";
                if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours} saat önce";
                if (timeSpan.TotalDays < 7)
                    return $"{(int)timeSpan.TotalDays} gün önce";
                return CreatedAt.ToString("dd MMM yyyy");
            }
        }
    }
}