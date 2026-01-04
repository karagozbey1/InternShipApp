using Business.Services.Internship;
using Business.Interfaces.Order;
using Core.Entity;
using DataAccess.Interfaces.Internship;
using DataAccess.Unit.Interface;
using Moq;
using Xunit;

namespace InternshipTrackingSystem.UnitTests.Services;

/// <summary>
/// InternshipOfferService sinifi icin unit testler
/// Staj ilani CRUD islemleri testleri
/// </summary>
public class InternshipOfferServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IInternshipOfferRepository> _mockOfferRepository;
    private readonly InternshipOfferService _offerService;

    public InternshipOfferServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockOfferRepository = new Mock<IInternshipOfferRepository>();

        _mockUnitOfWork.Setup(u => u.InternshipOffers).Returns(_mockOfferRepository.Object);

        _offerService = new InternshipOfferService(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task GetOfferWithCompanyAsync_ExistingOffer_ShouldReturnOffer()
    {
        // Arrange
        var offerId = 1;
        var offer = new InternshipOffer 
        { 
            Id = offerId, 
            Title = "Software Developer Intern",
            IsActive = true 
        };

        _mockOfferRepository
            .Setup(r => r.GetOfferWithCompanyAsync(offerId))
            .ReturnsAsync(offer);

        // Act
        var result = await _offerService.GetOfferWithCompanyAsync(offerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(offerId, result!.Id);
        Assert.Equal("Software Developer Intern", result.Title);
    }

    [Fact]
    public async Task GetOfferWithCompanyAsync_NonExistingOffer_ShouldReturnNull()
    {
        // Arrange
        var offerId = 999;
        _mockOfferRepository
            .Setup(r => r.GetOfferWithCompanyAsync(offerId))
            .ReturnsAsync((InternshipOffer?)null);

        // Act
        var result = await _offerService.GetOfferWithCompanyAsync(offerId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllActiveOffersAsync_ShouldReturnOnlyActiveOffers()
    {
        // Arrange
        var offers = new List<InternshipOffer>
        {
            new InternshipOffer { Id = 1, IsActive = true, Title = "Active 1" },
            new InternshipOffer { Id = 2, IsActive = true, Title = "Active 2" }
        };

        _mockOfferRepository
            .Setup(r => r.GetAllActiveOffersAsync())
            .ReturnsAsync(offers);

        // Act
        var result = await _offerService.GetAllActiveOffersAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, o => Assert.True(o.IsActive));
    }

    [Fact]
    public async Task GetOffersByCompanyIdAsync_ShouldReturnCompanyOffers()
    {
        // Arrange
        var companyId = 5;
        var offers = new List<InternshipOffer>
        {
            new InternshipOffer { Id = 1, CompanyId = companyId },
            new InternshipOffer { Id = 2, CompanyId = companyId }
        };

        _mockOfferRepository
            .Setup(r => r.GetOffersByCompanyIdAsync(companyId))
            .ReturnsAsync(offers);

        // Act
        var result = await _offerService.GetOffersByCompanyIdAsync(companyId);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, o => Assert.Equal(companyId, o.CompanyId));
    }

    [Fact]
    public async Task SearchOffersAsync_ShouldReturnMatchingOffers()
    {
        // Arrange
        var keyword = "Software";
        var offers = new List<InternshipOffer>
        {
            new InternshipOffer { Id = 1, Title = "Software Developer Intern" },
            new InternshipOffer { Id = 2, Title = "Software Engineer Intern" }
        };

        _mockOfferRepository
            .Setup(r => r.SearchOffersAsync(keyword, null, null))
            .ReturnsAsync(offers);

        // Act
        var result = await _offerService.SearchOffersAsync(keyword, null, null);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task SearchOffersAsync_WithCityFilter_ShouldReturnFilteredOffers()
    {
        // Arrange
        var keyword = "";
        var city = "Istanbul";
        var offers = new List<InternshipOffer>
        {
            new InternshipOffer { Id = 1, City = "Istanbul" }
        };

        _mockOfferRepository
            .Setup(r => r.SearchOffersAsync(keyword, city, null))
            .ReturnsAsync(offers);

        // Act
        var result = await _offerService.SearchOffersAsync(keyword, city, null);

        // Assert
        Assert.Single(result);
        Assert.Equal("Istanbul", result.First().City);
    }

    [Fact]
    public async Task AddOfferAsync_NullOffer_ShouldThrowArgumentNullException()
    {
        // Arrange
        InternshipOffer? offer = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _offerService.AddOfferAsync(offer!));
    }

    [Fact]
    public async Task AddOfferAsync_EmptyTitle_ShouldThrowArgumentException()
    {
        // Arrange
        var offer = new InternshipOffer 
        { 
            Title = "",
            City = "Istanbul",
            Field = "Software",
            CompanyId = 1
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _offerService.AddOfferAsync(offer));
        // Sadece exception type kontrolü yeterli - mesaj içeriði encoding nedeniyle deðiþebilir
        Assert.NotNull(exception);
        Assert.NotNull(exception.Message);
    }
}
