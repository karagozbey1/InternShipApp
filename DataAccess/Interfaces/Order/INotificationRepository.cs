using Core.Entity;
using DataAccess.GenericRepository.İnternface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces.Order
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<List<Notification>> GetUnreadNotificationsByUserIdAsync(int userId);

        Task<List<Notification>> GetAllNotificationsByUserIdAsync(int userId);
    }
}