using Core.Entity;

namespace Core.Entity;
public enum UserRole { Student, Advisor, Company, Admin }
public class User
{
    public int Id { get; set; } 
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;

    public bool IsActive { get; set; } = false; 
    public string? ActivationCode { get; set; }
    public DateTime? ActivationCodeExpireAt { get; set; }
    public UserRole Role { get; set; }

    public string? UniversityName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  
    
    public ICollection<InternshipApplication>? StudentApplications { get; set; }
    public ICollection<InternshipApplication>? AdvisorApplications { get; set; }
    public ICollection<InternshipComment>? SentComments { get; set; }
    public ICollection<Notification>? Notifications { get; set; }
    public ICollection<UserChatRoom>? ChatRooms { get; set; }
    public ICollection<ChatMessage>? SentMessages { get; set; }
    public ICollection<CompanyUser>? CompanyUsers { get; set; }
   
}
