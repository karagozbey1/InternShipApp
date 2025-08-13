using Business.Interfaces.Chat;
using Core.Entity;
using DataAccess.Unit.Interface;
using System.Collections.Generic;
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
                    RoomIdentifier = Guid.NewGuid().ToString(),
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
                SentAt = DateTime.UtcNow
            };
            await _unitOfWork.ChatMessages.AddAsync(message);
            await _unitOfWork.SaveAsync();
            return message;
        }

        public async Task<List<ChatMessage>> GetChatHistoryAsync(int chatRoomId)
        {
            return await _unitOfWork.ChatMessages.GetMessagesByChatRoomIdAsync(chatRoomId);
        }
    }
}