namespace Models.ViewModels.Chat
{
    public class ChatViewModel
    {
        public int CurrentUserId { get; set; }
        public string CurrentUserName { get; set; } = string.Empty;
        public List<ConversationViewModel> Conversations { get; set; } = new List<ConversationViewModel>();
        public List<MessageViewModel> Messages { get; set; } = new List<MessageViewModel>();
        public int SelectedConversationId { get; set; }
    }

    public class ConversationViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public DateTime LastMessageTime { get; set; }
        public bool IsOnline { get; set; }
        public int UnreadCount { get; set; }
        public string Avatar { get; set; } = string.Empty;
    }

    public class MessageViewModel
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsCurrentUser { get; set; }
        public bool IsRead { get; set; } = true;
    }

    public class SendMessageViewModel
    {
        public string Content { get; set; } = string.Empty;
        public int ConversationId { get; set; }
    }
}