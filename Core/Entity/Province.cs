using System.Collections.Generic;

namespace Core.Entity;

// İl
public class Province
{
    // Kimlik numarası
    public int Id { get; set; }
    
    // İl adı
    public string Name { get; set; } = string.Empty;
    
    // Durum
    public int Status { get; set; }
    
    // Üniversiteler
    public ICollection<University> Universities { get; set; } = new List<University>();
}