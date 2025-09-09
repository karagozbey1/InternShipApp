using Business.Interfaces.Chat;
using Core.Entity;
using DataAccess.Unit.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Business.Services.Chat
{
    public class IChatService : IIChatService
    {
        private readonly IUnitOfWork _unitOfWork;

        public IChatService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ChatRoom?> GetChatRoomByIdAsync(int chatRoomId)
        {
            return await _unitOfWork.ChatRooms.GetChatRoomWithMessagesAndParticipantsAsync(chatRoomId);
        }

        public async Task<List<ChatRoom>> GetUserChatRoomsAsync(int userId)
        {
            return await _unitOfWork.ChatRooms.GetChatRoomsByUserIdAsync(userId);
        }

        public async Task<ChatRoom> CreateOrGetChatRoomAsync(int user1Id, int user2Id)
        {
            var chatRoom = await _unitOfWork.ChatRooms.FindChatRoomBetweenUsersAsync(user1Id, user2Id);
            if (chatRoom == null)
            {
                chatRoom = new ChatRoom
                {
                    Name = "Private Chat",
                    RoomType = "Private",
                    RoomIdentifier = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Participants = new List<UserChatRoom>
                    {
                        new UserChatRoom { UserId = user1Id },
                        new UserChatRoom { UserId = user2Id }
                    }
                };
                await _unitOfWork.ChatRooms.AddAsync(chatRoom);
                await _unitOfWork.SaveAsync();
            }
            return chatRoom;
        }

        public async Task<ChatMessage> SendMessageAsync(int chatRoomId, int senderId, string content)
        {
            var message = new ChatMessage
            {
                ChatRoomId = chatRoomId,
                SenderId = senderId,
                Content = content,
                SentAt = DateTime.UtcNow,
                MessageType = "Text"
            };
            await _unitOfWork.ChatMessages.AddAsync(message);
            await _unitOfWork.SaveAsync();
            return message;
        }

        public async Task<ChatMessage> SendMessageAsync(ChatMessage message)
        {
            await _unitOfWork.ChatMessages.AddAsync(message);
            await _unitOfWork.SaveAsync();
            
            // Update room last activity
            var chatRoom = await _unitOfWork.ChatRooms.GetByIdAsync(message.ChatRoomId);
            if (chatRoom != null)
            {
                chatRoom.LastActivityAt = DateTime.UtcNow;
                await _unitOfWork.ChatRooms.UpdateAsync(chatRoom);
                await _unitOfWork.SaveAsync();
            }
            
            return message;
        }

        public async Task<List<ChatMessage>> GetChatHistoryAsync(int chatRoomId)
        {
            return await _unitOfWork.ChatMessages.GetMessagesByChatRoomIdAsync(chatRoomId);
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _unitOfWork.ChatMessages.GetUnreadCountByUserIdAsync(userId);
        }

        public async Task<List<ChatMessage>> GetRecentMessagesAsync(int userId)
        {
            var userChatRooms = await _unitOfWork.ChatRooms.GetChatRoomsByUserIdAsync(userId);
            var messages = new List<ChatMessage>();

            foreach (var chatRoom in userChatRooms)
            {
                var roomMessages = await _unitOfWork.ChatMessages.GetMessagesByChatRoomIdAsync(chatRoom.Id);
                messages.AddRange(roomMessages.OrderByDescending(m => m.SentAt).Take(5));
            }

            return messages.OrderByDescending(m => m.SentAt).Take(10).ToList();
        }

        public async Task<bool> HasUserAccessToChatRoomAsync(int userId, int chatRoomId)
        {
            var userChatRooms = await _unitOfWork.ChatRooms.GetChatRoomsByUserIdAsync(userId);
            return userChatRooms.Any(cr => cr.Id == chatRoomId);
        }

        public async Task<ChatMessage?> GetMessageByIdAsync(int messageId)
        {
            return await _unitOfWork.ChatMessages.GetByIdAsync(messageId);
        }

        public async Task<ChatMessage> UpdateMessageAsync(ChatMessage message)
        {
            await _unitOfWork.ChatMessages.UpdateAsync(message);
            await _unitOfWork.SaveAsync();
            return message;
        }

        public async Task DeleteMessageAsync(int messageId)
        {
            var message = await _unitOfWork.ChatMessages.GetByIdAsync(messageId);
            if (message != null)
            {
                message.IsDeleted = true;
                message.DeletedAt = DateTime.UtcNow;
                await _unitOfWork.ChatMessages.UpdateAsync(message);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task MarkMessagesAsReadAsync(int chatRoomId, int userId)
        {
            var messages = await _unitOfWork.ChatMessages.GetUnreadMessagesByChatRoomIdAsync(chatRoomId, userId);
            foreach (var message in messages)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.UtcNow;
            }
            await _unitOfWork.SaveAsync();
        }

        public async Task<ChatRoom> CreateChatRoomAsync(ChatRoom chatRoom)
        {
            await _unitOfWork.ChatRooms.AddAsync(chatRoom);
            await _unitOfWork.SaveAsync();
            return chatRoom;
        }

        public async Task AddUserToChatRoomAsync(int chatRoomId, int userId)
        {
            var userChatRoom = new UserChatRoom
            {
                ChatRoomId = chatRoomId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            };
            await _unitOfWork.ChatRooms.AddUserToChatRoomAsync(userChatRoom);
            await _unitOfWork.SaveAsync();
        }

        public async Task RemoveUserFromChatRoomAsync(int chatRoomId, int userId)
        {
            await _unitOfWork.ChatRooms.RemoveUserFromChatRoomAsync(chatRoomId, userId);
            await _unitOfWork.SaveAsync();
        }

        public async Task<List<ChatMessage>> SearchMessagesAsync(string query, int? chatRoomId = null, int? userId = null)
        {
            return await _unitOfWork.ChatMessages.SearchMessagesAsync(query, chatRoomId, userId);
        }

        public async Task<MessageAttachment> SaveAttachmentAsync(MessageAttachment attachment)
        {
            // This would be implemented when MessageAttachment repository is available
            // For now, return the attachment as-is
            return attachment;
        }

        public async Task<List<MessageAttachment>> GetMessageAttachmentsAsync(int messageId)
        {
            // This would be implemented when MessageAttachment repository is available
            return new List<MessageAttachment>();
        }

        public async Task DeleteAttachmentAsync(int attachmentId)
        {
            // This would be implemented when MessageAttachment repository is available
        }

        public async Task<bool> IsUserOnlineAsync(int userId)
        {
            // This would typically use a cache or real-time tracking
            // For now, return a mock value
            return new Random().NextDouble() > 0.5;
        }

        public async Task SetUserOnlineStatusAsync(int userId, bool isOnline)
        {
            // This would typically update a cache or real-time tracking
            // Implementation depends on chosen real-time technology (SignalR, etc.)
            await Task.CompletedTask;
        }

        public async Task<DateTime?> GetUserLastSeenAsync(int userId)
        {
            // This would typically get from cache or database
            // For now, return a mock value
            return DateTime.UtcNow.AddMinutes(-new Random().Next(1, 60));
        }

        public async Task UpdateUserLastSeenAsync(int userId)
        {
            // This would typically update cache or database
            await Task.CompletedTask;
        }

        public async Task<List<int>> GetTypingUsersAsync(int chatRoomId)
        {
            // This would typically get from cache or real-time tracking
            return new List<int>();
        }

        public async Task SetUserTypingAsync(int chatRoomId, int userId, bool isTyping)
        {
            // This would typically update cache or real-time tracking
            await Task.CompletedTask;
        }
    }
}
