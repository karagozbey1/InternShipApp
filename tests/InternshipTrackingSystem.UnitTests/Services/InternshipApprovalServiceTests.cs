using Business.Services.Internship;
using Business.Interfaces.Order;
using Core.Entity;
using DataAccess.Interfaces.Internship;
using DataAccess.Unit.Interface;
using Moq;
using Xunit;

namespace InternshipTrackingSystem.UnitTests.Services;

/// <summary>
/// InternshipApprovalService sinifi icin unit testler
/// Onay formu islemleri ve UnitOfWork commit testleri
/// </summary>
public class InternshipApprovalServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IInternshipApprovalRepository> _mockApprovalRepository;
    private readonly Mock<IINotificationService> _mockNotificationService;
    private readonly InternshipApprovalService _approvalService;

    public InternshipApprovalServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockApprovalRepository = new Mock<IInternshipApprovalRepository>();
        _mockNotificationService = new Mock<IINotificationService>();

        _mockUnitOfWork.Setup(u => u.InternshipApprovals).Returns(_mockApprovalRepository.Object);

        _approvalService = new InternshipApprovalService(_mockUnitOfWork.Object, _mockNotificationService.Object);
    }

    [Fact]
    public async Task GetApprovalFormsByStudentIdAsync_ValidId_ShouldReturnForms()
    {
        // Arrange
        var studentId = 1;
        var forms = new List<InternshipApprovalForm>
        {
            new InternshipApprovalForm { Id = 1, StudentId = studentId, Status = InternshipApprovalStatus.Pending },
            new InternshipApprovalForm { Id = 2, StudentId = studentId, Status = InternshipApprovalStatus.Approved }
        };

        _mockApprovalRepository
            .Setup(r => r.GetApprovalFormsByStudentIdAsync(studentId))
            .ReturnsAsync(forms);

        // Act
        var result = await _approvalService.GetApprovalFormsByStudentIdAsync(studentId);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, f => Assert.Equal(studentId, f.StudentId));
    }

    [Fact]
    public async Task GetApprovalFormsByStudentIdAsync_InvalidId_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidStudentId = 0;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _approvalService.GetApprovalFormsByStudentIdAsync(invalidStudentId));
        // Sadece exception type kontrolü yeterli - mesaj içeriði encoding nedeniyle deðiþebilir
        Assert.NotNull(exception);
        Assert.NotNull(exception.Message);
    }

    [Fact]
    public async Task GetApprovalFormsByAdvisorIdAsync_ValidId_ShouldReturnForms()
    {
        // Arrange
        var advisorId = 5;
        var forms = new List<InternshipApprovalForm>
        {
            new InternshipApprovalForm { Id = 1, AdvisorId = advisorId, Status = InternshipApprovalStatus.Pending }
        };

        _mockApprovalRepository
            .Setup(r => r.GetApprovalFormsByAdvisorIdAsync(advisorId))
            .ReturnsAsync(forms);

        // Act
        var result = await _approvalService.GetApprovalFormsByAdvisorIdAsync(advisorId);

        // Assert
        Assert.Single(result);
        _mockApprovalRepository.Verify(r => r.GetApprovalFormsByAdvisorIdAsync(advisorId), Times.Once);
    }

    [Fact]
    public async Task GetApprovalFormsByAdvisorIdAsync_InvalidId_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidAdvisorId = -1;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _approvalService.GetApprovalFormsByAdvisorIdAsync(invalidAdvisorId));
    }

    [Fact]
    public async Task GetPendingApprovalFormsAsync_ShouldReturnOnlyPendingForms()
    {
        // Arrange
        var pendingForms = new List<InternshipApprovalForm>
        {
            new InternshipApprovalForm { Id = 1, Status = InternshipApprovalStatus.Pending },
            new InternshipApprovalForm { Id = 2, Status = InternshipApprovalStatus.Pending }
        };

        _mockApprovalRepository
            .Setup(r => r.GetPendingApprovalFormsAsync())
            .ReturnsAsync(pendingForms);

        // Act
        var result = await _approvalService.GetPendingApprovalFormsAsync();

        // Assert
        Assert.All(result, f => Assert.Equal(InternshipApprovalStatus.Pending, f.Status));
    }

    [Fact]
    public async Task GetApprovalFormWithDetailsAsync_ExistingForm_ShouldReturnForm()
    {
        // Arrange
        var formId = 1;
        var form = new InternshipApprovalForm 
        { 
            Id = formId,
            CompanyName = "Test Company",
            Status = InternshipApprovalStatus.Pending
        };

        _mockApprovalRepository
            .Setup(r => r.GetApprovalFormWithDetailsAsync(formId))
            .ReturnsAsync(form);

        // Act
        var result = await _approvalService.GetApprovalFormWithDetailsAsync(formId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(formId, result!.Id);
        Assert.Equal("Test Company", result.CompanyName);
    }

    [Fact]
    public async Task GetApprovalFormWithDetailsAsync_InvalidId_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidFormId = 0;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _approvalService.GetApprovalFormWithDetailsAsync(invalidFormId));
    }

    [Fact]
    public async Task GetApprovalFormByApplicationIdAsync_ExistingApplication_ShouldReturnForm()
    {
        // Arrange
        var applicationId = 10;
        var form = new InternshipApprovalForm { Id = 1, ApplicationId = applicationId };

        _mockApprovalRepository
            .Setup(r => r.GetApprovalFormByApplicationIdAsync(applicationId))
            .ReturnsAsync(form);

        // Act
        var result = await _approvalService.GetApprovalFormByApplicationIdAsync(applicationId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(applicationId, result!.ApplicationId);
    }

    [Fact]
    public async Task GetApprovalFormByApplicationIdAsync_NonExistingApplication_ShouldReturnNull()
    {
        // Arrange
        var applicationId = 999;
        _mockApprovalRepository
            .Setup(r => r.GetApprovalFormByApplicationIdAsync(applicationId))
            .ReturnsAsync((InternshipApprovalForm?)null);

        // Act
        var result = await _approvalService.GetApprovalFormByApplicationIdAsync(applicationId);

        // Assert
        Assert.Null(result);
    }
}
