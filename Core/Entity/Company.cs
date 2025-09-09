using Core.Entity;

namespace Core.Entity;

public class Company
{
    public int CompanyId { get; set; }

    public string CompanyName { get; set; } = null!;
    public string? Sector { get; set; }
   
    public string? Phone { get; set; }
    public bool Approved { get; set; }

    public ICollection<InternshipOffer>? InternshipOffers { get; set; }
    public ICollection<CompanyUser>? CompanyUsers { get; set; }
    public string ?TaxNumber { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? EmployeeCount { get; set; }
    public DateTime? EstablishedDate { get; set; }
    public string? LogoPath { get; set; }
}
