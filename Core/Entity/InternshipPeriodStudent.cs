namespace Core.Entity;

// Staj dönemi öðrenci
public class InternshipPeriodStudent
{
    // Staj dönemi kimliði
    public int InternshipPeriodId { get; set; }
    
    // Staj dönemi
    public InternshipPeriod? InternshipPeriod { get; set; }

    // Öðrenci kimliði
    public int StudentId { get; set; }
    
    // Öðrenci
    public User? Student { get; set; }
}