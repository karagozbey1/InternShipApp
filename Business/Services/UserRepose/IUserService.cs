using Business.Interfaces.Email;
using Business.Interfaces.UserRepose;
using Business.OrderMethod;
using Core.Entity;
using DataAccess.Unit.Interface;
using System.Linq;

namespace Business.Services.UserRepose;

public class UserService : IIUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIActivationEmailService _activationEmailService;

    public UserService(IUnitOfWork unitOfWork, IIActivationEmailService activationEmailService)
    {
        _unitOfWork = unitOfWork;
        _activationEmailService = activationEmailService;
    }

    // Kimlik ile kullanıcı getirir
    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _unitOfWork.Users.GetByIdAsync(userId);
    }

    // Üniversite ile kullanıcı getirir
    public async Task<User?> GetUserWithUniversityAsync(int userId)
    {
        return await _unitOfWork.Users.GetUserWithUniversityAsync(userId);
    }

    // E-posta ile kullanıcı getirir
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _unitOfWork.Users.GetUserByEmailAsync(email);
    }
    
    // Aktivasyon kodu ile kullanıcı getirir
    public async Task<User?> GetUserByActivationCodeAsync(string code)
    {
        return await _unitOfWork.Users.GetUserByActivationCodeAsync(code);
    }
    
    // Kullanıcı günceller
    public async Task UpdateUserAsync(User user)
    {
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveAsync();
    }

    // Kullanıcı siler
    public async Task DeleteUserAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user != null)
        {
            _unitOfWork.Users.Remove(user);
            await _unitOfWork.SaveAsync();
        }
    }
    
    // Role göre kullanıcıları getirir
    public async Task<List<User>> GetUsersByRoleAsync(UserRole role)
    {
        return await _unitOfWork.Users.GetUsersByRoleAsync(role);
    }

    // Ada göre kullanıcı arar
    public async Task<List<User>> SearchUsersByFullNameAsync(string keyword)
    {
        return await _unitOfWork.Users.SearchUsersByFullNameAsync(keyword);
    }

    // Yeni kullanıcı ekler
    public async Task<User> AddUserAsync(User user)
    {
        if (!await _unitOfWork.Users.IsEmailUniqueAsync(user.Email))
            throw new InvalidOperationException("Bu e-posta adresi zaten kullanımda.");

        user.PasswordHash = PasswordSecurity.HashPassword(user.PasswordHash);
        user.ActivationCode = Guid.NewGuid().ToString();
        user.ActivationCodeExpireAt = DateTime.UtcNow.AddHours(24);
        user.IsActive = false;
        user.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveAsync();


        await _activationEmailService.SendEmailAsync(user.Email, user.FullName, user.ActivationCode);


        return user;
    }


    #region Şifre Sıfırlama
    // Şifre sıfırlama e-postası gönderir
    public async Task SendForgetPasswordAsync(string email, string resetUrl)
    {
        var user = await GetUserByEmailAsync(email);
        if (user == null)
            throw new InvalidOperationException("Bu e-posta adresi ile kayıtlı bir kullanıcı bulunamadı.");

        // Token üret
        var token = Guid.NewGuid().ToString();
        user.PasswordResetToken = token;
        user.PasswordResetTokenExpireAt = DateTime.UtcNow.AddHours(3);

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveAsync();

        var message = $"Şifrenizi sıfırlamak için lütfen aşağıdaki bağlantıya tıklayın.";
        await _activationEmailService.SendEmailForgetPassAsync(user.Email, token, resetUrl, user.FullName);
    }

    // Şifre sıfırlar
    public async Task ResetPasswordAsync(string token, string newPassword)
    {
        var user = await _unitOfWork.Users.GetUserByResetPasswordCodeAsync(token);
        if (user == null)
            throw new InvalidOperationException("Geçersiz şifre sıfırlama talebi.");

        if (user.PasswordResetTokenExpireAt == null || user.PasswordResetTokenExpireAt < DateTime.UtcNow)
            throw new InvalidOperationException("Şifre sıfırlama talebinin süresi dolmuş.");

        user.PasswordHash = PasswordSecurity.HashPassword(newPassword);
        user.IsActive = true;
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpireAt = null;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveAsync();
    }

    #endregion
    // Kullanıcı aktive eder
    public async Task ActivateUserAsync(string code)
    {
        await _unitOfWork.Users.ActivateUserAsync(code);
    }



    // Tüm öğrencileri getirir
    public async Task<List<User>> GetAllStudentsAsync()
    {
        return await _unitOfWork.Users.GetUsersByRoleAsync(UserRole.Student);
    }

    // Kimlik ile öğrenci getirir
    public async Task<User?> GetStudentByIdAsync(int id)
        => await _unitOfWork.Users.GetByIdAsync(id);

    // Şifre değiştirir
    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _unitOfWork.Users.GetUserByIdWithPasswordAsync(userId);
        if (user == null)
            throw new InvalidOperationException("Kullanıcı bulunamadı.");

      
        if (!PasswordSecurity.VerifyPassword(currentPassword, user.PasswordHash))
            throw new InvalidOperationException("Mevcut şifre hatalı.");

   
        user.PasswordHash = PasswordSecurity.HashPassword(newPassword);
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveAsync();

        return true;
    }

}
