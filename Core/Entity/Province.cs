using System.Collections.Generic;

namespace Core.Entity;

public class Province
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Status { get; set; }
    public ICollection<University> Universities { get; set; } = new List<University>();
}