using Core.Entity;

namespace Core.Entity;

public class Company
{
    public int CompanyId { get; set; }

    public string CompanyName { get; set; } = null!;
    public string? Sector { get; set; }
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool Approved { get; set; }

    public ICollection<InternshipOffer>? InternshipOffers { get; set; }
    public ICollection<CompanyUser>? CompanyUsers { get; set; }
}
