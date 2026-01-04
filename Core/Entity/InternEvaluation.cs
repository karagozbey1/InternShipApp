namespace Core.Entity;


public class InternEvaluation
{
    public int Id { get; set; }
    
    // Ýliþkili baþvuru
    public int ApplicationId { get; set; }
    public InternshipApplication? Application { get; set; }
    
    // Deðerlendiren þirket kullanýcýsý
    public int EvaluatorUserId { get; set; }
    public User? EvaluatorUser { get; set; }
    
    // Deðerlendirilen öðrenci
    public int StudentId { get; set; }
    public User? Student { get; set; }
    
    // Þirket bilgisi
    public int CompanyId { get; set; }
    public Company? Company { get; set; }
    
    // Puanlar (1-5 arasý)
    public int TechnicalSkillsRating { get; set; } // Teknik beceriler
    public int CommunicationRating { get; set; } // Ýletiþim becerileri
    public int TeamworkRating { get; set; } // Takým çalýþmasý
    public int PunctualityRating { get; set; } // Dakiklik ve devam
    public int InitiativeRating { get; set; } // Ýnisiyatif ve öðrenme isteði
    public int ProblemSolvingRating { get; set; } // Problem çözme
    public int OverallRating { get; set; } // Genel deðerlendirme
    
    // Yorum ve öneriler
    public string? StrengthsComment { get; set; } // Güçlü yönler
    public string? ImprovementAreasComment { get; set; } // Geliþtirilmesi gereken alanlar
    public string? GeneralComment { get; set; } // Genel yorum
    public string? RecommendationComment { get; set; } // Öðrenciye öneriler
    
    // Ýþe alým önerisi
    public bool WouldRecommendForHire { get; set; }
    public bool WouldAcceptAgain { get; set; } // Tekrar stajyer olarak kabul eder misiniz?
    
    // Tarih bilgileri
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Öðrenci deðerlendirmeyi gördü mü?
    public bool IsViewedByStudent { get; set; }
    public DateTime? ViewedAt { get; set; }
    
    // Hesaplanan ortalama puan
    public decimal AverageRating => 
        (TechnicalSkillsRating + CommunicationRating + TeamworkRating + 
         PunctualityRating + InitiativeRating + ProblemSolvingRating + OverallRating) / 7.0m;
}
