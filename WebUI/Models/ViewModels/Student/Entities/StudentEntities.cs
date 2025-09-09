namespace WebUI.Models.ViewModels.Student.Entities
{
    // Bu namespace student entities için ayrýlmýþtýr
    // Gerekli entity sýnýflarý burada tanýmlanacak
    
    public class StudentEntityBase
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}