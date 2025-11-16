using EuroNoteExplorer.Api.Controllers;
using EuroNoteExplorer.Api.Services.Interfaces;
using EuroNoteExplorer.Shared.Caching;
using EuroNoteExplorer.Shared.DTOs;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace EuroNoteExplorer.Api.Tests.Controllers
{
    [TestClass]
    public class EuroNotesControllerTests
    {
        private Mock<IEuroNoteService> _mockEuroNoteService;
        private Mock<ILogger<EuroNotesController>> _mockLogger;
        private Mock<ICache> _cache;
        private EuroNotesController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockEuroNoteService = new Mock<IEuroNoteService>();
            _mockLogger = new Mock<ILogger<EuroNotesController>>();
            _cache = new();
            _cache.Setup(c => c.Get<IEnumerable<BankNoteSummary>>(It.IsAny<string>())).Returns((IEnumerable<BankNoteSummary>)null);
            _controller = new EuroNotesController(_mockEuroNoteService.Object, _cache.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task GetAsync_WithValidFilters_ReturnsBankNoteSummaries()
        {
            // Arrange
            var filters = new BankNoteFilters
            {
                StartPeriod = new DateTime(2024, 1, 1),
                EndPeriod = new DateTime(2024, 12, 31)
            };

            var expectedSummaries = new List<BankNoteSummary>
            {
                new BankNoteSummary
                {
                    Denomination = 5,
                    DenominationCode = "B5",
                    CurrencyCode = "EUR",
                    Count = 1000000,
                    Value = 5000000m,
                    CurrencyValues = new List<CurrencyValue>
                    {
                        new CurrencyValue
                        {
                            CurrencyCode = "USD",
                            ExchangeRate = "1.1",
                            Value = "5500000"
                        }
                    }
                },
                new BankNoteSummary
                {
                    Denomination = 10,
                    DenominationCode = "B10",
                    CurrencyCode = "EUR",
                    Count = 500000,
                    Value = 5000000m,
                    CurrencyValues = new List<CurrencyValue>
                    {
                        new CurrencyValue
                        {
                            CurrencyCode = "USD",
                            ExchangeRate = "1.1",
                            Value = "5500000"
                        }
                    }
                }
            };

            _mockEuroNoteService
                .Setup(s => s.GetNoteSummariesAsync(It.Is<BankNoteFilters>(f =>
                    f.StartPeriod == filters.StartPeriod &&
                    f.EndPeriod == filters.EndPeriod)))
                .ReturnsAsync(expectedSummaries);

            // Act
            var result = await _controller.GetAsync(filters);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(expectedSummaries);

            _mockEuroNoteService.Verify(
                s => s.GetNoteSummariesAsync(It.IsAny<BankNoteFilters>()),
                Times.Once);
        }

        [TestMethod]
        public async Task GetAsync_WithEmptyResult_ReturnsEmptyCollection()
        {
            // Arrange
            var filters = new BankNoteFilters
            {
                StartPeriod = DateTime.Today.AddDays(-7),
                EndPeriod = DateTime.Today
            };

            _mockEuroNoteService
                .Setup(s => s.GetNoteSummariesAsync(It.IsAny<BankNoteFilters>()))
                .ReturnsAsync(new List<BankNoteSummary>());

            // Act
            var result = await _controller.GetAsync(filters);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetAsync_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            var filters = new BankNoteFilters
            {
                StartPeriod = DateTime.Today.AddDays(-7),
                EndPeriod = DateTime.Today
            };

            _mockEuroNoteService
                .Setup(s => s.GetNoteSummariesAsync(It.IsAny<BankNoteFilters>()))
                .ThrowsAsync(new HttpRequestException("API is unavailable"));

            // Act
            Func<Task> act = async () => await _controller.GetAsync(filters);

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>()
                .WithMessage("API is unavailable");
        }

        [TestMethod]
        public async Task GetAsync_WithDefaultFilters_CallsService()
        {
            // Arrange
            var filters = new BankNoteFilters();

            _mockEuroNoteService
                .Setup(s => s.GetNoteSummariesAsync(It.IsAny<BankNoteFilters>()))
                .ReturnsAsync(new List<BankNoteSummary>());

            // Act
            await _controller.GetAsync(filters);

            // Assert
            _mockEuroNoteService.Verify(
                s => s.GetNoteSummariesAsync(It.IsAny<BankNoteFilters>()),
                Times.Once);
        }
    }
}
