namespace Core.Entity;

public enum InternshipApplicationStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public class InternshipApplication
{
    public int Id { get; set; }
    public InternshipApplicationStatus Status { get; set; }
    public string? StudentNote { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int InternshipOfferId { get; set; }
    public InternshipOffer? InternshipOffer { get; set; }
    public int StudentId { get; set; }
    public User? Student { get; set; }
    public int? AdvisorId { get; set; }
    public User? Advisor { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ICollection<InternshipDocument>? InternshipDocuments { get; set; }
    public ICollection<InternshipComment>? InternshipComments { get; set; }
    public DateTime UpdatedAt { get; set; }
    // New: marks whether this internship is compulsory (zorunlu) for the student
    public bool IsCompulsory { get; set; } = false;
}
