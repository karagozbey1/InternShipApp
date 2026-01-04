using Business.OrderMethod;
using Xunit;

namespace InternshipTrackingSystem.UnitTests.Security;

/// <summary>
/// PasswordSecurity sinifi icin unit testler
/// PBKDF2 + SHA256 tabanli hash ve dogrulama testleri
/// </summary>
public class PasswordSecurityTests
{
    [Fact]
    public void HashPassword_ShouldReturnNonEmptyString()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash = PasswordSecurity.HashPassword(password);

        // Assert
        Assert.False(string.IsNullOrEmpty(hash));
        Assert.True(hash.Length > 0);
    }

    [Fact]
    public void HashPassword_SamePasword_ShouldReturnDifferentHashes()
    {
        // Arrange - Ayni sifre
        var password = "TestPassword123!";

        // Act - Iki kez hashle (farkli salt kullanilacak)
        var hash1 = PasswordSecurity.HashPassword(password);
        var hash2 = PasswordSecurity.HashPassword(password);

        // Assert - Salt farkli oldugu icin hashler farkli olmali
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "MySecurePassword!2024";
        var hash = PasswordSecurity.HashPassword(password);

        // Act
        var result = PasswordSecurity.VerifyPassword(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ShouldReturnFalse()
    {
        // Arrange
        var correctPassword = "MySecurePassword!2024";
        var wrongPassword = "WrongPassword!2024";
        var hash = PasswordSecurity.HashPassword(correctPassword);

        // Act
        var result = PasswordSecurity.VerifyPassword(wrongPassword, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_EmptyHash_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var result = PasswordSecurity.VerifyPassword(password, "");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_NullHash_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var result = PasswordSecurity.VerifyPassword(password, null!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_InvalidBase64Hash_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var invalidHash = "NotAValidBase64String!!!";

        // Act
        var result = PasswordSecurity.VerifyPassword(password, invalidHash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_TooShortHash_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var shortHash = Convert.ToBase64String(new byte[10]); // 48 byte'dan kisa

        // Act
        var result = PasswordSecurity.VerifyPassword(password, shortHash);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("123")]
    [InlineData("VeryLongPasswordWithSpecialChars!@#$%^&*()_+")]
    public void HashPassword_VariousPasswords_ShouldWork(string password)
    {
        // Act
        var hash = PasswordSecurity.HashPassword(password);

        // Assert
        Assert.False(string.IsNullOrEmpty(hash));
        
        // Bos sifre disindakiler dogrulanabilmeli
        if (!string.IsNullOrEmpty(password))
        {
            Assert.True(PasswordSecurity.VerifyPassword(password, hash));
        }
    }

    [Fact]
    public void HashPassword_UnicodePassword_ShouldWork()
    {
        // Arrange - Turkce ve ozel karakterler
        var password = "?ifre123!??????????";

        // Act
        var hash = PasswordSecurity.HashPassword(password);

        // Assert
        Assert.False(string.IsNullOrEmpty(hash));
        Assert.True(PasswordSecurity.VerifyPassword(password, hash));
    }
}
