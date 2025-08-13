using Business.Interfaces.Email;
using Business.Interfaces.UserRepose;
using Business.OrderMethod;
using Core.Entity;
using DataAccess.Unit.Interface;
using NETCore.MailKit.Core;


namespace Business.Services.UserRepose
{
    public class UserService : IIUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIActivationEmailService _activationEmailService;

        public UserService(IUnitOfWork unitOfWork, IIActivationEmailService activationEmailService)
        {
            _unitOfWork = unitOfWork;
            _activationEmailService = activationEmailService;
        }


        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _unitOfWork.Users.GetByIdAsync(userId);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _unitOfWork.Users.GetUserByEmailAsync(email);
        }

        public async Task<List<User>> GetUsersByRoleAsync(UserRole role)
        {
            return await _unitOfWork.Users.GetUsersByRoleAsync(role);
        }

        public async Task<List<User>> SearchUsersByFullNameAsync(string keyword)
        {
            return await _unitOfWork.Users.SearchUsersByFullNameAsync(keyword);
        }

        public async Task<User> AddUserAsync(User user)
        {
            if (!await _unitOfWork.Users.IsEmailUniqueAsync(user.Email))
                throw new InvalidOperationException("Bu e-posta adresi zaten kullanımda.");

            user.PasswordHash = PasswordSecurity.HashPassword(user.PasswordHash);
            user.ActivationCode = Guid.NewGuid().ToString();
            user.ActivationCodeExpireAt = DateTime.UtcNow.AddHours(24); // 24 saat geçerli
            user.IsActive = false;
            user.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveAsync();

          
            var activationLink = $"https://localhost:7211/account/activate?code={user.ActivationCode}";

            // E-posta gönderme
            var subject = "Hesabınızı Aktifleştirin";
            var message = $@"
                Merhaba {user.FullName},<br/><br/>
                Hesabınızı aktifleştirmek için aşağıdaki bağlantıya tıklayın:<br/>
                <a href='{activationLink}'>Hesabı Aktifleştir</a><br/><br/>
                Bu bağlantı 24 saat boyunca geçerlidir.
            ";

            await _activationEmailService.SendEmailAsync(user.Email, subject, message);

            return user;
        }

        public async Task UpdateUserAsync(User user)
        {
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user != null)
            {
                _unitOfWork.Users.Remove(user);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task<User?> GetUserByActivationCodeAsync(string code)
        {
            return await _unitOfWork.Users.GetUserByActivationCodeAsync(code);
        }

        public async Task ActivateUserAsync(string code)
        {
            var user = await _unitOfWork.Users.GetUserByActivationCodeAsync(code);
            if (user == null)
                throw new InvalidOperationException("Geçersiz aktivasyon kodu.");

            if (user.ActivationCodeExpireAt < DateTime.UtcNow)
                throw new InvalidOperationException("Aktivasyon kodunun süresi dolmuş.");

            user.IsActive = true;
            user.ActivationCode = null;
            user.ActivationCodeExpireAt = null;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveAsync();
        }
    }
}
