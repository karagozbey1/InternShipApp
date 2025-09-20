using System.Collections.Generic;

namespace Core.Entity;

// İl entity'si - Türkiye illerini temsil eder
public class Province
{
    // Primary key - il benzersiz tanımlayıcısı
    public int Id { get; set; }
    
    // İl adı - zorunlu alan
    public string Name { get; set; } = string.Empty;
    
    // İl durumu - aktif/pasif kontrolü
    public int Status { get; set; }
    
    // Navigation property - bu ildeki üniversiteler
    public ICollection<University> Universities { get; set; } = new List<University>();
}