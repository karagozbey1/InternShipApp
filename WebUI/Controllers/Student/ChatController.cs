using Business.Interfaces.Chat;
using Business.Interfaces.UserRepose;
using Business.Interfaces.Internship;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebUI.Models.ViewModels.Chat;
using WebUI.Controllers;
using Core.Entity;

namespace WebUI.Controllers.Student
{
    [Authorize]
    public class ChatController : BaseAppController
    {
        private readonly IIChatService _chatService;
        private readonly IIUserService _userService;
        private readonly IInternshipDocumentService _documentService;
        private readonly IWebHostEnvironment _environment;

        public ChatController(
            IIChatService chatService, 
            IIUserService userService,
            IInternshipDocumentService documentService,
            IWebHostEnvironment environment)
        {
            _chatService = chatService;
            _userService = userService;
            _documentService = documentService;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var user = await _userService.GetUserByIdAsync(userId);
            
            // Get user's chat rooms
            var userChatRooms = await _chatService.GetUserChatRoomsAsync(userId);
            var conversations = new List<ConversationViewModel>();
            
            foreach (var chatRoom in userChatRooms)
            {
                var lastMessage = chatRoom.GetLastMessage();
                var unreadCount = chatRoom.GetUnreadCount(userId);
                
                conversations.Add(new ConversationViewModel
                {
                    Id = chatRoom.Id,
                    Name = chatRoom.Name,
                    Description = chatRoom.Description,
                    RoomType = chatRoom.RoomType,
                    LastMessage = lastMessage?.Content ?? "",
                    LastMessageTime = lastMessage?.SentAt ?? chatRoom.CreatedAt,
                    UnreadCount = unreadCount,
                    IsActive = chatRoom.IsActive,
                    Avatar = GetRoomAvatar(chatRoom.Name),
                    Participants = chatRoom.Participants?.Select(p => new ParticipantViewModel
                    {
                        UserId = p.UserId,
                        FullName = p.User?.FullName ?? "",
                        Email = p.User?.Email ?? "",
                        Role = p.User?.Role.ToString() ?? "",
                        IsOnline = IsUserOnline(p.UserId),
                        Avatar = GetUserAvatar(p.User?.FullName ?? "")
                    }).ToList() ?? new List<ParticipantViewModel>()
                });
            }

            // If no conversations, create default support conversation
            if (!conversations.Any())
            {
                conversations = await CreateDefaultConversationsAsync(userId);
            }

            var selectedConversationId = conversations.FirstOrDefault()?.Id ?? 0;
            var messages = selectedConversationId > 0 
                ? await GetConversationMessagesAsync(selectedConversationId, userId)
                : new List<MessageViewModel>();

            var model = new ChatViewModel
            {
                CurrentUserId = userId,
                CurrentUserName = user?.FullName ?? "Kullanýcý",
                CurrentUserAvatar = GetUserAvatar(user?.FullName ?? ""),
                Conversations = conversations.OrderByDescending(c => c.LastMessageTime).ToList(),
                Messages = messages,
                SelectedConversationId = selectedConversationId,
                TotalUnreadCount = conversations.Sum(c => c.UnreadCount)
            };

            return View("~/Views/Student/Chat/Index.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageViewModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _userService.GetUserByIdAsync(userId);
                
                // Debug logging
                Console.WriteLine($"SendMessage - UserId: {userId}, ConversationId: {model.ConversationId}, Content: {model.Content}");
                
                // Validate conversation access
                var hasAccess = await _chatService.HasUserAccessToChatRoomAsync(userId, model.ConversationId);
                if (!hasAccess)
                {
                    Console.WriteLine($"User {userId} does not have access to conversation {model.ConversationId}");
                    return Json(new { success = false, error = "Bu konuþmaya eriþim yetkiniz yok." });
                }

                // Create message
                var chatMessage = new ChatMessage
                {
                    Content = model.Content,
                    SenderId = userId,
                    ChatRoomId = model.ConversationId,
                    SentAt = DateTime.UtcNow,
                    MessageType = "Text",
                    ReplyToMessageId = model.ReplyToMessageId
                };

                var savedMessage = await _chatService.SendMessageAsync(chatMessage);
                Console.WriteLine($"Message saved with ID: {savedMessage.Id}");

                // Create response
                var messageViewModel = new MessageViewModel
                {
                    Id = savedMessage.Id,
                    SenderId = userId,
                    SenderName = user?.FullName ?? "Ben",
                    SenderAvatar = GetUserAvatar(user?.FullName ?? ""),
                    Content = savedMessage.Content,
                    SentAt = savedMessage.SentAt,
                    IsCurrentUser = true,
                    MessageType = savedMessage.MessageType ?? "Text",
                    IsRead = false,
                    ReplyToMessageId = savedMessage.ReplyToMessageId
                };

                // If it's a reply, get the original message
                if (savedMessage.ReplyToMessageId.HasValue)
                {
                    var originalMessage = await _chatService.GetMessageByIdAsync(savedMessage.ReplyToMessageId.Value);
                    if (originalMessage != null)
                    {
                        var originalSender = await _userService.GetUserByIdAsync(originalMessage.SenderId);
                        messageViewModel.ReplyToMessage = new MessageViewModel
                        {
                            Id = originalMessage.Id,
                            SenderName = originalSender?.FullName ?? "Kullanýcý",
                            Content = originalMessage.Content,
                            SentAt = originalMessage.SentAt
                        };
                    }
                }

                return Json(new { success = true, message = messageViewModel });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendMessage Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Json(new { success = false, error = "Mesaj gönderilirken bir hata oluþtu: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessageWithAttachment([FromForm] SendMessageViewModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _userService.GetUserByIdAsync(userId);
                
                // Validate conversation access
                var hasAccess = await _chatService.HasUserAccessToChatRoomAsync(userId, model.ConversationId);
                if (!hasAccess)
                {
                    return BadRequest("Bu konuþmaya eriþim yetkiniz yok.");
                }

                // Create message
                var chatMessage = new ChatMessage
                {
                    Content = model.Content,
                    SenderId = userId,
                    ChatRoomId = model.ConversationId,
                    SentAt = DateTime.UtcNow,
                    MessageType = model.Attachments?.Any() == true ? "File" : "Text",
                    ReplyToMessageId = model.ReplyToMessageId
                };

                var savedMessage = await _chatService.SendMessageAsync(chatMessage);

                // Handle attachments
                var attachmentViewModels = new List<AttachmentViewModel>();
                if (model.Attachments?.Any() == true)
                {
                    foreach (var file in model.Attachments)
                    {
                        var attachment = await SaveAttachmentAsync(file, savedMessage.Id);
                        if (attachment != null)
                        {
                            attachmentViewModels.Add(new AttachmentViewModel
                            {
                                Id = attachment.Id,
                                FileName = attachment.FileName,
                                OriginalFileName = attachment.OriginalFileName,
                                FilePath = attachment.FilePath,
                                ContentType = attachment.ContentType,
                                FileSize = attachment.FileSize,
                                FileType = attachment.FileType
                            });
                        }
                    }
                }

                var messageViewModel = new MessageViewModel
                {
                    Id = savedMessage.Id,
                    SenderId = userId,
                    SenderName = user?.FullName ?? "Ben",
                    SenderAvatar = GetUserAvatar(user?.FullName ?? ""),
                    Content = savedMessage.Content,
                    SentAt = savedMessage.SentAt,
                    IsCurrentUser = true,
                    MessageType = savedMessage.MessageType ?? "Text",
                    Attachments = attachmentViewModels
                };

                return Json(new { success = true, message = messageViewModel });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Mesaj gönderilirken bir hata oluþtu." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages(int conversationId, int page = 1, int pageSize = 50)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                // Validate access
                var hasAccess = await _chatService.HasUserAccessToChatRoomAsync(userId, conversationId);
                if (!hasAccess)
                {
                    return BadRequest("Bu konuþmaya eriþim yetkiniz yok.");
                }

                var messages = await GetConversationMessagesAsync(conversationId, userId, page, pageSize);
                return Json(messages);
            }
            catch (Exception ex)
            {
                return Json(new List<MessageViewModel>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _chatService.MarkMessagesAsReadAsync(request.ConversationId, userId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Mesajlar okundu olarak iþaretlenirken hata oluþtu." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessage([FromBody] DeleteMessageRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var message = await _chatService.GetMessageByIdAsync(request.MessageId);
                
                if (message == null || message.SenderId != userId)
                {
                    return BadRequest("Bu mesajý silme yetkiniz yok.");
                }

                await _chatService.DeleteMessageAsync(request.MessageId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Mesaj silinirken hata oluþtu." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMessage([FromBody] EditMessageViewModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                var message = await _chatService.GetMessageByIdAsync(model.MessageId);
                
                if (message == null || message.SenderId != userId)
                {
                    return BadRequest("Bu mesajý düzenleme yetkiniz yok.");
                }

                message.Content = model.Content;
                message.EditedAt = DateTime.UtcNow;
                
                await _chatService.UpdateMessageAsync(message);
                
                return Json(new { success = true, message = new { id = message.Id, content = message.Content, editedAt = message.EditedAt } });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Mesaj düzenlenirken hata oluþtu." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateConversation([FromBody] CreateConversationViewModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                var chatRoom = new ChatRoom
                {
                    Name = model.Name,
                    Description = model.Description,
                    RoomType = model.RoomType,
                    CreatedByUserId = userId,
                    AllowFileSharing = model.AllowFileSharing,
                    MaxParticipants = model.MaxParticipants,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var createdRoom = await _chatService.CreateChatRoomAsync(chatRoom);

                // Add participants
                foreach (var participantId in model.ParticipantIds)
                {
                    await _chatService.AddUserToChatRoomAsync(createdRoom.Id, participantId);
                }

                // Add creator
                await _chatService.AddUserToChatRoomAsync(createdRoom.Id, userId);

                return Json(new { success = true, conversationId = createdRoom.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Konuþma oluþturulurken hata oluþtu." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchMessages(string query, int? conversationId = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var searchResults = await _chatService.SearchMessagesAsync(query, conversationId, userId);
                
                var results = searchResults.Select(m => new MessageViewModel
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    SenderName = m.Sender?.FullName ?? "Kullanýcý",
                    Content = m.Content,
                    SentAt = m.SentAt,
                    IsCurrentUser = m.SenderId == userId
                }).ToList();
                
                return Json(results);
            }
            catch (Exception ex)
            {
                return Json(new List<MessageViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                var unreadCount = await _chatService.GetUnreadCountAsync(userId);
                return Json(new { unreadCount });
            }
            catch (Exception ex)
            {
                return Json(new { unreadCount = 0 });
            }
        }

        // Helper Methods
        private async Task<List<ConversationViewModel>> CreateDefaultConversationsAsync(int userId)
        {
            try
            {
                // Try to create a real default chat room in database
                var defaultRoom = new ChatRoom
                {
                    Name = "Staj Koordinatörü",
                    Description = "Staj süreciniz hakkýnda sorularýnýz için",
                    RoomType = "Support",
                    CreatedByUserId = userId, // Current user creates it
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    AllowFileSharing = true,
                    MaxParticipants = 2,
                    RoomIdentifier = Guid.NewGuid().ToString()
                };

                var createdRoom = await _chatService.CreateChatRoomAsync(defaultRoom);
                
                // Add current user to the chat room
                await _chatService.AddUserToChatRoomAsync(createdRoom.Id, userId);

                // Create a welcome message
                var welcomeMessage = new ChatMessage
                {
                    ChatRoomId = createdRoom.Id,
                    SenderId = userId, // System message from current user
                    Content = "Merhaba! Staj süreciniz hakkýnda sorularýnýzý bu konuþmadan sorabilirsiniz.",
                    SentAt = DateTime.UtcNow,
                    MessageType = "Text",
                    IsRead = true // Mark as read since it's a system message
                };

                await _chatService.SendMessageAsync(welcomeMessage);

                return new List<ConversationViewModel>
                {
                    new ConversationViewModel
                    {
                        Id = createdRoom.Id,
                        Name = "Staj Koordinatörü",
                        Description = "Staj süreciniz hakkýnda sorularýnýz için",
                        RoomType = "Support",
                        LastMessage = welcomeMessage.Content,
                        LastMessageTime = DateTime.UtcNow,
                        IsOnline = true,
                        UnreadCount = 0,
                        Avatar = "SK",
                        IsActive = true
                    }
                };
            }
            catch (Exception ex)
            {
                // Fallback to mock data if database operations fail
                Console.WriteLine($"Error creating default conversation: {ex.Message}");
                
                return new List<ConversationViewModel>
                {
                    new ConversationViewModel
                    {
                        Id = 999, // Use a high number to avoid conflicts
                        Name = "Staj Koordinatörü (Demo)",
                        Description = "Staj süreciniz hakkýnda sorularýnýz için",
                        RoomType = "Support",
                        LastMessage = "Bu bir demo konuþmasýdýr. Gerçek mesajlaþma için sistem yöneticinize baþvurun.",
                        LastMessageTime = DateTime.Now.AddHours(-1),
                        IsOnline = false,
                        UnreadCount = 0,
                        Avatar = "SK",
                        IsActive = true
                    }
                };
            }
        }

        private async Task<List<MessageViewModel>> GetConversationMessagesAsync(int conversationId, int userId, int page = 1, int pageSize = 50)
        {
            try
            {
                var messages = await _chatService.GetChatHistoryAsync(conversationId);
                
                return messages
                    .Where(m => !m.IsDeleted)
                    .OrderBy(m => m.SentAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(m => new MessageViewModel
                    {
                        Id = m.Id,
                        SenderId = m.SenderId,
                        SenderName = m.Sender?.FullName ?? "Kullanýcý",
                        SenderAvatar = GetUserAvatar(m.Sender?.FullName ?? ""),
                        Content = m.Content,
                        SentAt = m.SentAt,
                        ReadAt = m.ReadAt,
                        IsRead = m.IsRead,
                        IsCurrentUser = m.SenderId == userId,
                        MessageType = m.MessageType ?? "Text",
                        IsDeleted = m.IsDeleted,
                        IsEdited = m.EditedAt.HasValue,
                        EditedAt = m.EditedAt,
                        ReplyToMessageId = m.ReplyToMessageId,
                        Attachments = m.Attachments?.Select(a => new AttachmentViewModel
                        {
                            Id = a.Id,
                            FileName = a.FileName,
                            OriginalFileName = a.OriginalFileName,
                            FilePath = a.FilePath,
                            ContentType = a.ContentType,
                            FileSize = a.FileSize,
                            FileType = a.FileType,
                            ThumbnailPath = a.ThumbnailPath
                        }).ToList() ?? new List<AttachmentViewModel>()
                    }).ToList();
            }
            catch (Exception ex)
            {
                return new List<MessageViewModel>();
            }
        }

        private async Task<MessageAttachment?> SaveAttachmentAsync(IFormFile file, int messageId)
        {
            try
            {
                if (file == null || file.Length == 0) return null;

                // Validate file
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" };
                if (!allowedTypes.Contains(file.ContentType))
                {
                    throw new InvalidOperationException("Desteklenmeyen dosya türü.");
                }

                if (file.Length > 10 * 1024 * 1024) // 10MB
                {
                    throw new InvalidOperationException("Dosya boyutu çok büyük (maksimum 10MB).");
                }

                // Create upload directory
                var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "chat", messageId.ToString());
                Directory.CreateDirectory(uploadsDir);

                // Generate unique filename
                var fileExtension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsDir, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Create attachment entity
                var attachment = new MessageAttachment
                {
                    ChatMessageId = messageId,
                    FileName = fileName,
                    OriginalFileName = file.FileName,
                    FilePath = $"/uploads/chat/{messageId}/{fileName}",
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    FileType = GetFileType(file.ContentType),
                    UploadedAt = DateTime.UtcNow
                };

                return await _chatService.SaveAttachmentAsync(attachment);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private string GetFileType(string contentType)
        {
            return contentType switch
            {
                var ct when ct.StartsWith("image/") => "Image",
                "application/pdf" => "Document",
                var ct when ct.Contains("word") => "Document",
                var ct when ct.StartsWith("video/") => "Video",
                var ct when ct.StartsWith("audio/") => "Audio",
                _ => "Other"
            };
        }

        private string GetUserAvatar(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return "U";
            var parts = fullName.Split(' ');
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            return fullName[0].ToString().ToUpper();
        }

        private string GetRoomAvatar(string roomName)
        {
            if (string.IsNullOrEmpty(roomName)) return "G";
            var parts = roomName.Split(' ');
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            return roomName.Substring(0, Math.Min(2, roomName.Length)).ToUpper();
        }

        private bool IsUserOnline(int userId)
        {
            return new Random().NextDouble() > 0.5; // Mock implementation
        }
    }

    // Request models
    public class SendMessageRequest
    {
        public string Content { get; set; } = string.Empty;
        public int ConversationId { get; set; }
        public int? ReplyToMessageId { get; set; }
    }

    public class MarkAsReadRequest
    {
        public int ConversationId { get; set; }
    }

    public class DeleteMessageRequest
    {
        public int MessageId { get; set; }
    }
}