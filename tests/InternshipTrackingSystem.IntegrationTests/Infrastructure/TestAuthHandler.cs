using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace InternshipTrackingSystem.IntegrationTests.Infrastructure;

/// <summary>
/// Test authentication handler options
/// </summary>
public class TestAuthHandlerOptions : AuthenticationSchemeOptions
{
    public string DefaultUserId { get; set; } = "1";
    public string DefaultUserRole { get; set; } = "Student";
    public string DefaultUserName { get; set; } = "Test User";
    public string DefaultEmail { get; set; } = "test@test.com";
}

/// <summary>
/// Test authentication handler
/// Integration testlerde kullanici kimligini simule eder
/// </summary>
public class TestAuthHandler : AuthenticationHandler<TestAuthHandlerOptions>
{
    public const string AuthenticationScheme = "TestScheme";
    public const string TestUserIdHeader = "X-Test-UserId";
    public const string TestUserRoleHeader = "X-Test-UserRole";
    public const string TestUserNameHeader = "X-Test-UserName";

    public TestAuthHandler(
        IOptionsMonitor<TestAuthHandlerOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Header'lardan test kullanici bilgilerini al
        var userId = Request.Headers[TestUserIdHeader].FirstOrDefault() ?? Options.DefaultUserId;
        var userRole = Request.Headers[TestUserRoleHeader].FirstOrDefault() ?? Options.DefaultUserRole;
        var userName = Request.Headers[TestUserNameHeader].FirstOrDefault() ?? Options.DefaultUserName;
        var email = Options.DefaultEmail;

        // Anonim istek kontrolu
        if (Request.Headers.ContainsKey("X-Anonymous"))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, userName),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, userRole)
        };

        var identity = new ClaimsIdentity(claims, AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

/// <summary>
/// HttpClient extension metodlari - test authentication icin
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Student rolu ile kimlik dogrulama ekler
    /// </summary>
    public static HttpClient AuthenticateAsStudent(this HttpClient client, int userId = 1, string userName = "Test Student")
    {
        client.DefaultRequestHeaders.Remove(TestAuthHandler.TestUserIdHeader);
        client.DefaultRequestHeaders.Remove(TestAuthHandler.TestUserRoleHeader);
        client.DefaultRequestHeaders.Remove(TestAuthHandler.TestUserNameHeader);
        client.DefaultRequestHeaders.Remove("X-Anonymous");

        client.DefaultRequestHeaders.Add(TestAuthHandler.TestUserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.TestUserRoleHeader, "Student");
        client.DefaultRequestHeaders.Add(TestAuthHandler.TestUserNameHeader, userName);
        
        return client;
    }

    /// <summary>
    /// Advisor rolu ile kimlik dogrulama ekler
    /// </summary>
    public static HttpClient AuthenticateAsAdvisor(this HttpClient client, int userId = 2, string userName = "Test Advisor")
    {
        client.DefaultRequestHeaders.Remove(TestAuthHandler.TestUserIdHeader);
        client.DefaultRequestHeaders.Remove(TestAuthHandler.TestUserRoleHeader);
        client.DefaultRequestHeaders.Remove(TestAuthHandler.TestUserNameHeader);
        client.DefaultRequestHeaders.Remove("X-Anonymous");

        client.DefaultRequestHeaders.Add(TestAuthHandler.TestUserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.TestUserRoleHeader, "Advisor");
        client.DefaultRequestHeaders.Add(TestAuthHandler.TestUserNameHeader, userName);
        
        return client;
    }

    /// <summary>
    /// Company rolu ile kimlik dogrulama ekler
    /// </summary>
    public static HttpClient AuthenticateAsCompany(this HttpClient client, int userId = 3, string userName = "Test Company")
    {
        client.DefaultRequestHeaders.Remove(TestAuthHandler.TestUserIdHeader);
        client.DefaultRequestHeaders.Remove(TestAuthHandler.TestUserRoleHeader);
        client.DefaultRequestHeaders.Remove(TestAuthHandler.TestUserNameHeader);
        client.DefaultRequestHeaders.Remove("X-Anonymous");

        client.DefaultRequestHeaders.Add(TestAuthHandler.TestUserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.TestUserRoleHeader, "Company");
        client.DefaultRequestHeaders.Add(TestAuthHandler.TestUserNameHeader, userName);
        
        return client;
    }

    /// <summary>
    /// Anonim kullanici olarak istek yapar
    /// </summary>
    public static HttpClient AsAnonymous(this HttpClient client)
    {
        client.DefaultRequestHeaders.Remove(TestAuthHandler.TestUserIdHeader);
        client.DefaultRequestHeaders.Remove(TestAuthHandler.TestUserRoleHeader);
        client.DefaultRequestHeaders.Remove(TestAuthHandler.TestUserNameHeader);
        client.DefaultRequestHeaders.Add("X-Anonymous", "true");
        
        return client;
    }
}
