using System;
using System.Collections.Generic;

namespace Core.Entity
{
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
        public int? UniversityId { get; set; }   
        public University? University { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Phone { get; set; }

        public string? Department { get; set; }
        public string? Title { get; set; }

        // New properties for student profile
        public string? StudentNumber { get; set; }
        public int? Grade { get; set; }

        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpireAt { get; set; }

        public ICollection<InternshipApplication>? StudentApplications { get; set; }
        public ICollection<InternshipApplication>? AdvisorApplications { get; set; }
        public ICollection<InternshipComment>? SentComments { get; set; }
        public ICollection<Notification>? Notifications { get; set; }
        public ICollection<UserChatRoom>? ChatRooms { get; set; }
        public ICollection<ChatMessage>? SentMessages { get; set; }
        public ICollection<CompanyUser>? CompanyUsers { get; set; }
        public ICollection<InternshipPeriod>? InternshipPeriods { get; set; }
        public ICollection<InternshipPeriodStudent>? InternshipPeriodStudents { get; set; }
   
        
    }
}
