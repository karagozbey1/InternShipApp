using Core.Entity;
using DataAccess.Context;
using DataAccess.GenericRepository.Repository;
using DataAccess.Interfaces.UserRepose;
using Microsoft.EntityFrameworkCore;

public class UserRepository : Repository<User>, IUserRepository
{

 
    public UserRepository(ApplicationDbContext context) : base(context)
    {
       
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<List<User>> GetUsersByRoleAsync(UserRole role)
    {
        return await _context.Users.Where(u => u.Role == role).ToListAsync();
    }

    public async Task<User?> GetUserWithDetailsAsync(int userId)
    {
        return await _context.Users
            .Include(u => u.StudentApplications)
            .Include(u => u.AdvisorApplications)
            .Include(u => u.SentComments)
            .Include(u => u.Notifications)
            .Include(u => u.ChatRooms)
            .Include(u => u.SentMessages)
            .Include(u => u.CompanyUsers)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<List<User>> SearchUsersByFullNameAsync(string keyword)
    {
        return await _context.Users.Where(u => u.FullName.Contains(keyword)).ToListAsync();
    }

    public async Task<bool> IsEmailUniqueAsync(string email)
    {
        return !await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<User?> GetUserByActivationCodeAsync(string code)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.ActivationCode == code);
    }

    public async Task ActivateUserAsync(string code)
    {
        var user = await GetUserByActivationCodeAsync(code);
        if (user == null)
            throw new InvalidOperationException("Geçersiz aktivasyon kodu.");

        user.IsActive = true;
        user.ActivationCode = null;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
}
