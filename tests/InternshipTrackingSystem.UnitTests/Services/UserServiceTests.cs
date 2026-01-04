using Business.Services.UserRepose;
using Business.Interfaces.Email;
using Business.Interfaces.UserRepose;
using Core.Entity;
using DataAccess.Interfaces.UserRepose;
using DataAccess.Unit.Interface;
using Moq;
using Xunit;

namespace InternshipTrackingSystem.UnitTests.Services;

/// <summary>
/// UserService sinifi icin unit testler
/// UnitOfWork mock kullanilarak servis logic testleri
/// </summary>
public class UserServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IIActivationEmailService> _mockEmailService;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockEmailService = new Mock<IIActivationEmailService>();

        _mockUnitOfWork.Setup(u => u.Users).Returns(_mockUserRepository.Object);

        _userService = new UserService(_mockUnitOfWork.Object, _mockEmailService.Object);
    }

    [Fact]
    public async Task GetUserByIdAsync_ExistingUser_ShouldReturnUser()
    {
        // Arrange
        var userId = 1;
        var expectedUser = new User 
        { 
            Id = userId, 
            FullName = "Test User", 
            Email = "test@test.com",
            Role = UserRole.Student 
        };

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result!.Id);
        Assert.Equal("Test User", result.FullName);
        _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetUserByIdAsync_NonExistingUser_ShouldReturnNull()
    {
        // Arrange
        var userId = 999;
        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByEmailAsync_ExistingEmail_ShouldReturnUser()
    {
        // Arrange
        var email = "student@university.edu.tr";
        var expectedUser = new User { Id = 1, Email = email, FullName = "Student" };

        _mockUserRepository
            .Setup(r => r.GetUserByEmailAsync(email))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userService.GetUserByEmailAsync(email);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email, result!.Email);
    }

    [Fact]
    public async Task AddUserAsync_DuplicateEmail_ShouldThrowException()
    {
        // Arrange
        var user = new User 
        { 
            Email = "existing@test.com", 
            FullName = "New User",
            PasswordHash = "Password123!"
        };

        _mockUserRepository
            .Setup(r => r.IsEmailUniqueAsync(user.Email))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _userService.AddUserAsync(user));
        // Sadece exception type kontrolü yeterli - mesaj içeriði encoding nedeniyle deðiþebilir
        Assert.NotNull(exception);
        Assert.NotNull(exception.Message);
    }

    [Fact]
    public async Task AddUserAsync_NewUser_ShouldHashPasswordAndSave()
    {
        // Arrange
        var user = new User 
        { 
            Email = "new@test.com", 
            FullName = "New User",
            PasswordHash = "PlainPassword123!"
        };

        _mockUserRepository
            .Setup(r => r.IsEmailUniqueAsync(user.Email))
            .ReturnsAsync(true);

        _mockUserRepository
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveAsync())
            .ReturnsAsync(1);

        _mockEmailService
            .Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userService.AddUserAsync(user);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual("PlainPassword123!", result.PasswordHash); // Hashlenmi? olmali
        Assert.False(string.IsNullOrEmpty(result.ActivationCode));
        Assert.False(result.IsActive);

        _mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldCallRepositoryAndSave()
    {
        // Arrange
        var user = new User { Id = 1, FullName = "Updated Name", Email = "test@test.com" };

        _mockUnitOfWork
            .Setup(u => u.SaveAsync())
            .ReturnsAsync(1);

        // Act
        await _userService.UpdateUserAsync(user);

        // Assert
        _mockUserRepository.Verify(r => r.Update(user), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_ExistingUser_ShouldRemoveAndSave()
    {
        // Arrange
        var userId = 1;
        var user = new User { Id = userId, Email = "test@test.com" };

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _mockUnitOfWork
            .Setup(u => u.SaveAsync())
            .ReturnsAsync(1);

        // Act
        await _userService.DeleteUserAsync(userId);

        // Assert
        _mockUserRepository.Verify(r => r.Remove(user), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_NonExistingUser_ShouldNotCallRemove()
    {
        // Arrange
        var userId = 999;
        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        await _userService.DeleteUserAsync(userId);

        // Assert
        _mockUserRepository.Verify(r => r.Remove(It.IsAny<User>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task GetUsersByRoleAsync_ShouldReturnUsersWithRole()
    {
        // Arrange
        var role = UserRole.Student;
        var users = new List<User>
        {
            new User { Id = 1, Role = UserRole.Student },
            new User { Id = 2, Role = UserRole.Student }
        };

        _mockUserRepository
            .Setup(r => r.GetUsersByRoleAsync(role))
            .ReturnsAsync(users);

        // Act
        var result = await _userService.GetUsersByRoleAsync(role);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, u => Assert.Equal(UserRole.Student, u.Role));
    }

    [Fact]
    public async Task ChangePasswordAsync_CorrectCurrentPassword_ShouldUpdateAndSave()
    {
        // Arrange
        var userId = 1;
        var currentPassword = "OldPassword123!";
        var newPassword = "NewPassword456!";
        
        // Hash the current password as it would be stored in DB
        var hashedCurrentPassword = Business.OrderMethod.PasswordSecurity.HashPassword(currentPassword);
        
        var user = new User 
        { 
            Id = userId, 
            PasswordHash = hashedCurrentPassword 
        };

        _mockUserRepository
            .Setup(r => r.GetUserByIdWithPasswordAsync(userId))
            .ReturnsAsync(user);

        _mockUnitOfWork
            .Setup(u => u.SaveAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _userService.ChangePasswordAsync(userId, currentPassword, newPassword);

        // Assert
        Assert.True(result);
        _mockUserRepository.Verify(r => r.Update(It.IsAny<User>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_WrongCurrentPassword_ShouldThrowException()
    {
        // Arrange
        var userId = 1;
        var hashedPassword = Business.OrderMethod.PasswordSecurity.HashPassword("CorrectPassword!");
        var user = new User { Id = userId, PasswordHash = hashedPassword };

        _mockUserRepository
            .Setup(r => r.GetUserByIdWithPasswordAsync(userId))
            .ReturnsAsync(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _userService.ChangePasswordAsync(userId, "WrongPassword!", "NewPassword!"));
        // Sadece exception type kontrolü yeterli - mesaj içeriði encoding nedeniyle deðiþebilir
        Assert.NotNull(exception);
        Assert.NotNull(exception.Message);
    }
}
