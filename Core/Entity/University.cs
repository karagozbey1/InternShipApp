namespace Core.Entity
{
    // Üniversite
    public class University
    {
        // Kimlik numarası
        public int Id { get; set; }
        
        // Üniversite adı
        public string Name { get; set; } = string.Empty;
        
        // Durum
        public byte Status { get; set; } = 1;

        // İl kimliği
        public int ProvinceId { get; set; }
        
        // İl bilgileri
        public Province? Province { get; set; }

        // Kullanıcılar
        public ICollection<User>? Users { get; set; }
    }
}
