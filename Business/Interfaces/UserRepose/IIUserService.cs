using Core.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Interfaces.UserRepose
{
    public interface IIUserService
    {
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<List<User>> GetUsersByRoleAsync(UserRole role);
        Task<List<User>> SearchUsersByFullNameAsync(string keyword);
        Task<User> AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int userId);
        Task<User?> GetUserByActivationCodeAsync(string code);
        Task ActivateUserAsync(string code);
    }
}