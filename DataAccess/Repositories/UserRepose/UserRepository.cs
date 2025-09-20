using Core.Entity;
using DataAccess.Context;
using DataAccess.GenericRepository.Repository;
using DataAccess.Interfaces.UserRepose;
using DataAccess.Unit.Interface;
using Microsoft.EntityFrameworkCore;

// Kullanıcı veri erişim sınıfı
public class UserRepository : Repository<User>, IUserRepository
{
    // Kurucu metod
    public UserRepository(ApplicationDbContext context) : base(context)
    {
       
    }

    // E-posta ile kullanıcı getirir
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    // Role göre kullanıcıları getirir
    public async Task<List<User>> GetUsersByRoleAsync(UserRole role)
    {
        return await _context.Users.Where(u => u.Role == role).ToListAsync();
    }

    // Detaylı kullanıcı bilgisi getirir
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

    // Üniversite ile kullanıcı getirir
    public async Task<User?> GetUserWithUniversityAsync(int userId)
    {
        return await _context.Users
            .Include(u => u.University)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    // Ada göre kullanıcı arar
    public async Task<List<User>> SearchUsersByFullNameAsync(string keyword)
    {
        return await _context.Users.Where(u => u.FullName.Contains(keyword)).ToListAsync();
    }

    // E-posta benzersiz mi kontrol eder
    public async Task<bool> IsEmailUniqueAsync(string email)
    {
        return !await _context.Users.AnyAsync(u => u.Email == email);
    }

    // Aktivasyon kodu ile kullanıcı getirir
    public async Task<User?> GetUserByActivationCodeAsync(string ActivationCodeAsync)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.ActivationCode == ActivationCodeAsync);
    }

    // Kullanıcı aktive eder
    public async Task ActivateUserAsync(string code)
    {
        var user = await GetUserByActivationCodeAsync(code);
        if (user == null)
            throw new InvalidOperationException("Geçersiz aktivasyon kodu.");

        if (user.ActivationCodeExpireAt < DateTime.UtcNow)
            throw new InvalidOperationException("Aktivasyon kodunun süresi dolmuş.");

        user.IsActive = true;
        user.ActivationCode = null;
        user.ActivationCodeExpireAt = null;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    // Şifre sıfırlama kodu ile kullanıcı getirir
    public async Task<User?> GetUserByResetPasswordCodeAsync(string ResetPasswordCodeAsync)
    {
      return await _context.Users.FirstOrDefaultAsync(x=>x.PasswordResetToken== ResetPasswordCodeAsync);
    }

    // Tüm öğrencileri getirir
    public async Task<List<User>> GetAllStudentsAsync()
       => await _context.Users
           .Where(u => u.Role == UserRole.Student)
           .ToListAsync();

    // Kimlik ile öğrenci getirir
    public async Task<User?> GetStudentByIdAsync(int id)
        => await _context.Users
            .Where(u => u.Id == id && u.Role == UserRole.Student)
            .FirstOrDefaultAsync();
            
    // Şifre ile kullanıcı getirir
    public async Task<User?> GetUserByIdWithPasswordAsync(int userId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }
}
