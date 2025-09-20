namespace Core.Entity
{
    // Üniversite entity'si - akademik kurumları temsil eder
    public class University
    {
        // Primary key - üniversite benzersiz tanımlayıcısı
        public int Id { get; set; }
        
        // Üniversitenin adı - zorunlu alan
        public string Name { get; set; } = string.Empty;
        
        // Üniversitenin durumu - aktif/pasif kontrolü
        public byte Status { get; set; } = 1;

        // Foreign key - üniversitenin bulunduğu ilin ID'si
        public int ProvinceId { get; set; }
        
        // Navigation property - üniversitenin bulunduğu il
        public Province? Province { get; set; }

        // Navigation property - bu üniversiteye kayıtlı kullanıcılar
        public ICollection<User>? Users { get; set; }
    }
}
