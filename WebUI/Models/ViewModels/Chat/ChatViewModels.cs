using System.ComponentModel.DataAnnotations;

namespace WebUI.Models.ViewModels.Chat
{
    public class ConversationViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public string RoomType { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime? LastActivityAt { get; set; }
        public List<MessageViewModel> Messages { get; set; } = new();
        public List<ParticipantViewModel> Participants { get; set; } = new();
        public bool AllowFileSharing { get; set; }
        public int MaxParticipants { get; set; }
        public bool IsActive { get; set; }
        
        // Additional properties needed by Chat/Index.cshtml
        public int UnreadCount { get; set; }
        public string LastMessageText { get; set; } = "";
        public string Avatar { get; set; } = "";
        public string StatusText { get; set; } = "";
        public bool IsOnline { get; set; }
        public bool IsPinned { get; set; }
        public bool IsMuted { get; set; }
        public string TimeText => LastActivityAt?.ToString("HH:mm") ?? "";
        
        // Properties needed by ChatController
        public string LastMessage { get; set; } = "";
        public DateTime LastMessageTime { get; set; }
    }

    public class MessageViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; } = "";
        public string SenderName { get; set; } = "";
        public int SenderId { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime? EditedAt { get; set; }
        public bool IsRead { get; set; }
        public bool IsDeleted { get; set; }
        public string MessageType { get; set; } = "";
        public int? ReplyToMessageId { get; set; }
        public List<AttachmentViewModel> Attachments { get; set; } = new();
        
        // Helper properties for UI
        public string TimeText => SentAt.ToString("HH:mm");
        public bool IsEdited { get; set; }
        public bool HasAttachments => Attachments.Any();
        public string StatusIcon => IsRead ? "fas fa-check-double" : "fas fa-check";
        public bool IsCurrentUser { get; set; }
        public bool IsReply => ReplyToMessageId.HasValue;
        public MessageViewModel? ReplyToMessage { get; set; }
        public string SenderAvatar { get; set; } = "";
        
        // Properties needed by ChatController
        public DateTime? ReadAt { get; set; }
    }

    public class ParticipantViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public bool IsAdmin { get; set; }
        public bool IsMuted { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime? LeftAt { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? LastSeen { get; set; }
        
        // Properties needed by ChatController
        public string FullName { get; set; } = "";
        public string Role { get; set; } = "";
        public string Avatar { get; set; } = "";
    }

    public class AttachmentViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; } = "";
        public string OriginalFileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public string ContentType { get; set; } = "";
        public string FileType { get; set; } = "";
        public long FileSize { get; set; }
        public string? ThumbnailPath { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? Duration { get; set; }
        public int? PageCount { get; set; }
        public DateTime UploadedAt { get; set; }
        public bool IsDeleted { get; set; }
        
        // Helper properties for UI
        public string IconClass
        {
            get
            {
                return FileType?.ToLower() switch
                {
                    "pdf" => "fas fa-file-pdf text-red-500",
                    "doc" or "docx" => "fas fa-file-word text-blue-500",
                    "xls" or "xlsx" => "fas fa-file-excel text-green-500",
                    "jpg" or "jpeg" or "png" or "gif" => "fas fa-file-image text-purple-500",
                    "mp4" or "avi" or "mov" => "fas fa-file-video text-indigo-500",
                    "mp3" or "wav" => "fas fa-file-audio text-orange-500",
                    "zip" or "rar" => "fas fa-file-archive text-yellow-500",
                    _ => "fas fa-file text-gray-500"
                };
            }
        }
        
        public string FileSizeText
        {
            get
            {
                if (FileSize < 1024)
                    return $"{FileSize} B";
                else if (FileSize < 1024 * 1024)
                    return $"{FileSize / 1024:F1} KB";
                else if (FileSize < 1024 * 1024 * 1024)
                    return $"{FileSize / (1024 * 1024):F1} MB";
                else
                    return $"{FileSize / (1024 * 1024 * 1024):F1} GB";
            }
        }
    }

    public class SendMessageViewModel
    {
        [Required(ErrorMessage = "Mesaj içeriði zorunludur")]
        [StringLength(1000, ErrorMessage = "Mesaj en fazla 1000 karakter olabilir")]
        public string Content { get; set; } = "";
        
        public int ConversationId { get; set; }
        public int? ReplyToMessageId { get; set; }
        public string MessageType { get; set; } = "Text";
        public List<IFormFile>? Attachments { get; set; }
    }

    public class CreateConversationViewModel
    {
        [Required(ErrorMessage = "Sohbet adý zorunludur")]
        [StringLength(100, ErrorMessage = "Sohbet adý en fazla 100 karakter olabilir")]
        public string Name { get; set; } = "";
        
        [StringLength(500, ErrorMessage = "Açýklama en fazla 500 karakter olabilir")]
        public string? Description { get; set; }
        
        [Required]
        public string RoomType { get; set; } = "Private";
        
        public List<int> ParticipantIds { get; set; } = new();
        public bool AllowFileSharing { get; set; } = true;
        public int MaxParticipants { get; set; } = 50;
    }

    public class EditMessageViewModel
    {
        [Required]
        public int MessageId { get; set; }
        
        [Required(ErrorMessage = "Mesaj içeriði boþ olamaz")]
        [MaxLength(1000, ErrorMessage = "Mesaj en fazla 1000 karakter olabilir")]
        public string Content { get; set; } = string.Empty;
    }

    public class MessageSearchViewModel
    {
        public string Query { get; set; } = string.Empty;
        public int? ConversationId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? MessageType { get; set; }
        public int? SenderId { get; set; }
        public bool HasAttachments { get; set; }
        
        public List<MessageViewModel> Results { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class TypingIndicatorViewModel
    {
        public int ConversationId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool IsTyping { get; set; }
    }

    public class OnlineStatusViewModel
    {
        public int UserId { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? LastSeen { get; set; }
    }

    public class ChatViewModel
    {
        public List<ConversationViewModel> Conversations { get; set; } = new();
        public ConversationViewModel? ActiveConversation { get; set; }
        public List<Core.Entity.User> AvailableUsers { get; set; } = new();
        public int CurrentUserId { get; set; }
        public string CurrentUserName { get; set; } = "";
        public bool CanCreateRooms { get; set; } = true;
        public int UnreadMessageCount { get; set; }
        
        // Additional properties needed by Chat/Index.cshtml
        public int? SelectedConversationId { get; set; }
        public List<MessageViewModel> Messages { get; set; } = new();
        public int TotalUnreadCount { get; set; }
        
        // Properties needed by ChatController
        public string CurrentUserAvatar { get; set; } = "";
    }
}