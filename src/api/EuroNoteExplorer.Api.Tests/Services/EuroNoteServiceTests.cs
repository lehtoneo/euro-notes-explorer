using EuroNoteExplorer.Api.Services.Interfaces;
using EuroNoteExplorer.Shared.Caching;
using EuroNoteExplorer.Shared.DTOs;
using Microsoft.Extensions.Logging;
using Moq;

namespace EuroNoteExplorer.Api.Tests.Services
{
	[TestClass]
	public class EuroNoteServiceTests
	{
		private Mock<IBoFOpenApiClient> _apiClient = null!;
		private Mock<ICache> _cache = null!;
		private Mock<ILogger<EuroNoteService>> _logger = null!;
		private EuroNoteService _service = null!;

		[TestInitialize]
		public void Setup()
		{
			_apiClient = new Mock<IBoFOpenApiClient>();
			_cache = new Mock<ICache>();
			_logger = new Mock<ILogger<EuroNoteService>>();

			_service = new EuroNoteService(
				_apiClient.Object,
				_cache.Object,
				_logger.Object
			);
		}

		[TestMethod]
		public async Task GetNoteSummariesAsync_ReturnsSortedSummaries()
		{
			// Arrange
			var filters = new BankNoteFilters
			{
				StartPeriod = new DateTime(2024, 1, 1),
				EndPeriod = new DateTime(2024, 1, 31)
			};

			// Cache miss → returns null
			_cache.Setup(c => c.Get<Dictionary<string, string>>(It.IsAny<string>()))
				  .Returns((Dictionary<string, string>?)null);

			// FX returned from API
			_apiClient.Setup(a => a.GetDailyExchangeRatesAsync(It.IsAny<DateTime>(), null))
					  .ReturnsAsync(new Dictionary<string, string> { { "USD", "2,0" } });

			// All denominations return 100 notes
			_apiClient.Setup(a => a.GetBanknotePiecesAsync(
					It.IsAny<string>(),
					It.IsAny<DateTime>(),
					It.IsAny<DateTime>()))
				.ReturnsAsync(new List<BankNoteObservation>
				{
				new BankNoteObservation { Value = 100 }
				});

			// Act
			var result = (await _service.GetNoteSummariesAsync(filters)).ToList();

			// Assert
			var expectedOrder = new[] { 5, 10, 20, 50, 100, 200, 500 };
			CollectionAssert.AreEqual(expectedOrder, result.Select(r => r.Denomination).ToList());
		}

		[TestMethod]
		public async Task GetNoteSummariesAsync_MapsValuesCorrectly()
		{
			// Arrange
			var filters = new BankNoteFilters
			{
				StartPeriod = new DateTime(2024, 1, 1),
				EndPeriod = new DateTime(2024, 1, 31)
			};

			_cache.Setup(c => c.Get<Dictionary<string, string>>(It.IsAny<string>()))
				  .Returns((Dictionary<string, string>?)null);

			_apiClient.Setup(a => a.GetDailyExchangeRatesAsync(It.IsAny<DateTime>(), null))
					  .ReturnsAsync(new Dictionary<string, string> { { "USD", "1,5" } });

			_apiClient.Setup(a => a.GetBanknotePiecesAsync("B10",
					It.IsAny<DateTime>(),
					It.IsAny<DateTime>()))
				.ReturnsAsync(new List<BankNoteObservation>
				{
				new BankNoteObservation { Value = 50 }
				});

			_apiClient.Setup(a => a.GetBanknotePiecesAsync(
					It.IsNotIn("B10"),
					It.IsAny<DateTime>(),
					It.IsAny<DateTime>()))
				.ReturnsAsync(new List<BankNoteObservation>());

			// Act
			var result = (await _service.GetNoteSummariesAsync(filters))
				.First(r => r.Denomination == 10);

			// Assert
			Assert.AreEqual(50, result.Count);
			Assert.AreEqual(500, result.Value); // 50 × 10€

			var usd = result.CurrencyValues.First(c => c.CurrencyCode == "USD");
			Assert.AreEqual("750.0", usd.Value); // 500 × 1.5
		}

		[TestMethod]
		public async Task GetNoteSummariesAsync_HandlesMissingObservations()
		{
			// Arrange
			var filters = new BankNoteFilters
			{
				StartPeriod = DateTime.UtcNow,
				EndPeriod = DateTime.UtcNow
			};

			_cache.Setup(c => c.Get<Dictionary<string, string>>(It.IsAny<string>()))
				  .Returns((Dictionary<string, string>?)null);

			_apiClient.Setup(a => a.GetDailyExchangeRatesAsync(It.IsAny<DateTime>(), null))
					  .ReturnsAsync(new Dictionary<string, string> { { "USD", "1,0" } });

			_apiClient.Setup(a => a.GetBanknotePiecesAsync(
					It.IsAny<string>(),
					It.IsAny<DateTime>(),
					It.IsAny<DateTime>()))
				.ReturnsAsync(new List<BankNoteObservation>());

			// Act
			var results = await _service.GetNoteSummariesAsync(filters);

			// Assert
			foreach (var r in results)
			{
				Assert.AreEqual(0, r.Count);
				Assert.AreEqual(0, r.Value);
			}
		}

		[TestMethod]
		public async Task GetNoteSummariesAsync_LogsError_WhenApiThrows()
		{
			// Arrange
			var filters = new BankNoteFilters
			{
				StartPeriod = DateTime.UtcNow.AddDays(-1),
				EndPeriod = DateTime.UtcNow
			};

			_cache.Setup(c => c.Get<Dictionary<string, string>>(It.IsAny<string>()))
				  .Returns((Dictionary<string, string>?)null);

			_apiClient.Setup(a => a.GetDailyExchangeRatesAsync(It.IsAny<DateTime>(), null))
					  .ReturnsAsync(new Dictionary<string, string> { { "USD", "1,0" } });

			_apiClient.Setup(a => a.GetBanknotePiecesAsync("B10",
					It.IsAny<DateTime>(),
					It.IsAny<DateTime>()))
				.ThrowsAsync(new Exception("API failed"));

			// Act
			var results = await _service.GetNoteSummariesAsync(filters);

			// Assert
			_logger.Verify(
				x => x.Log(
					LogLevel.Error,
					It.IsAny<EventId>(),
					It.IsAny<It.IsAnyType>(),
					It.IsAny<Exception>(),
					(Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
				Times.AtLeastOnce
			);
		}
	}
}
