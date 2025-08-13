using Core.Entity;
using DataAccess.GenericRepository.İnternface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces.UserRepose
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<List<User>> GetUsersByRoleAsync(UserRole role); 
        Task<User?> GetUserWithDetailsAsync(int userId);
        Task<List<User>> SearchUsersByFullNameAsync(string keyword);
        Task<bool> IsEmailUniqueAsync(string email);
        Task<User?> GetUserByActivationCodeAsync(string code);
        Task ActivateUserAsync(string code);
    }
}