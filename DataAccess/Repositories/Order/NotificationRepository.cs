using Core.Entity;
using DataAccess.Context;
using DataAccess.GenericRepository.Repository;
using DataAccess.Interfaces.Order;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Order
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
       

        public NotificationRepository(ApplicationDbContext context) : base(context)
        {
          
        }

        public async Task<List<Notification>> GetUnreadNotificationsByUserIdAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetAllNotificationsByUserIdAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }
    }
}