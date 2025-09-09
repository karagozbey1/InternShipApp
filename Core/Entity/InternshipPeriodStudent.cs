namespace Core.Entity;

public class InternshipPeriodStudent
{
    public int InternshipPeriodId { get; set; }
    public InternshipPeriod? InternshipPeriod { get; set; }

    public int StudentId { get; set; }
    public User? Student { get; set; }
}