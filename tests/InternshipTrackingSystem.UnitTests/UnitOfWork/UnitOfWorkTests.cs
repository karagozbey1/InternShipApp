using DataAccess.Context;
using DataAccess.Interfaces.UserRepose;
using DataAccess.Unit.Interface;
using DataAccess.Unit.Repose;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace InternshipTrackingSystem.UnitTests.UnitOfWork;

/// <summary>
/// UnitOfWork pattern testleri
/// SaveAsync ve Repository erisimleri testleri
/// </summary>
public class UnitOfWorkTests
{
    [Fact]
    public void UnitOfWork_ShouldExposeAllRepositories()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var unitOfWork = new DataAccess.Unit.Repose.UnitOfWork(context);

        // Act & Assert - Tum repository'lerin erisilebildigini dogrula
        Assert.NotNull(unitOfWork.Users);
        Assert.NotNull(unitOfWork.Companies);
        Assert.NotNull(unitOfWork.InternshipOffers);
        Assert.NotNull(unitOfWork.InternshipApplications);
        Assert.NotNull(unitOfWork.InternshipApprovals);
        Assert.NotNull(unitOfWork.InternshipDocuments);
        Assert.NotNull(unitOfWork.Notifications);
        Assert.NotNull(unitOfWork.ChatRooms);
        Assert.NotNull(unitOfWork.ChatMessages);
    }

    [Fact]
    public async Task SaveAsync_WithChanges_ShouldReturnAffectedRows()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var unitOfWork = new DataAccess.Unit.Repose.UnitOfWork(context);

        // Act - Bir kayit ekle
        var user = new Core.Entity.User
        {
            FullName = "Test User",
            Email = "test@test.com",
            PasswordHash = "hashedpassword",
            Role = Core.Entity.UserRole.Student,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.Users.AddAsync(user);
        var result = await unitOfWork.SaveAsync();

        // Assert
        Assert.True(result > 0);
    }

    [Fact]
    public async Task SaveAsync_WithoutChanges_ShouldReturnZero()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var unitOfWork = new DataAccess.Unit.Repose.UnitOfWork(context);

        // Act - Hicbir degisiklik yapmadan kaydet
        var result = await unitOfWork.SaveAsync();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void UnitOfWork_ShouldImplementIDisposable()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);

        // Act & Assert
        var unitOfWork = new DataAccess.Unit.Repose.UnitOfWork(context);
        Assert.IsAssignableFrom<IDisposable>(unitOfWork);
        
        // Dispose cagrilabilmeli
        unitOfWork.Dispose();
    }

    [Fact]
    public async Task Repository_AddAndRetrieve_ShouldWork()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var unitOfWork = new DataAccess.Unit.Repose.UnitOfWork(context);

        var user = new Core.Entity.User
        {
            FullName = "Integration Test User",
            Email = "integration@test.com",
            PasswordHash = "hash123",
            Role = Core.Entity.UserRole.Student,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await unitOfWork.Users.AddAsync(user);
        await unitOfWork.SaveAsync();

        var retrievedUser = await unitOfWork.Users.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(retrievedUser);
        Assert.Equal("integration@test.com", retrievedUser!.Email);
    }

    [Fact]
    public async Task Repository_Update_ShouldPersistChanges()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var unitOfWork = new DataAccess.Unit.Repose.UnitOfWork(context);

        var user = new Core.Entity.User
        {
            FullName = "Original Name",
            Email = "update@test.com",
            PasswordHash = "hash123",
            Role = Core.Entity.UserRole.Student,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.Users.AddAsync(user);
        await unitOfWork.SaveAsync();

        // Act - Guncelle
        user.FullName = "Updated Name";
        unitOfWork.Users.Update(user);
        await unitOfWork.SaveAsync();

        var updatedUser = await unitOfWork.Users.GetByIdAsync(user.Id);

        // Assert
        Assert.Equal("Updated Name", updatedUser!.FullName);
    }

    [Fact]
    public async Task Repository_Remove_ShouldDeleteEntity()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var unitOfWork = new DataAccess.Unit.Repose.UnitOfWork(context);

        var user = new Core.Entity.User
        {
            FullName = "To Be Deleted",
            Email = "delete@test.com",
            PasswordHash = "hash123",
            Role = Core.Entity.UserRole.Student,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.Users.AddAsync(user);
        await unitOfWork.SaveAsync();
        var userId = user.Id;

        // Act - Sil
        unitOfWork.Users.Remove(user);
        await unitOfWork.SaveAsync();

        var deletedUser = await unitOfWork.Users.GetByIdAsync(userId);

        // Assert
        Assert.Null(deletedUser);
    }
}
