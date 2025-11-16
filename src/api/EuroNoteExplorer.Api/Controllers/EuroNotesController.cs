using EuroNoteExplorer.Api.Services.Interfaces;
using EuroNoteExplorer.Shared.Caching;
using EuroNoteExplorer.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EuroNoteExplorer.Api.Controllers
{

	[ApiController]
	[Route("api/v1/euronotes")]
	public class EuroNotesController : ControllerBase
	{
		private readonly IEuroNoteService _euroNoteService;
		private readonly ICache _cache;
		private readonly ILogger<EuroNotesController> _logger;

		public EuroNotesController(
			IEuroNoteService euroNoteService,
			ICache cache,
			ILogger<EuroNotesController> logger)
		{
			_euroNoteService = euroNoteService;
			_cache = cache;
			_logger = logger;
		}

		[HttpGet("", Name = "GetBankNotes")]
		public async Task<IEnumerable<BankNoteSummary>> GetAsync([FromQuery] BankNoteFilters filters)
		{
			string cacheKey = _buildCacheKey(filters);

			// Try get from cache
			var cached = _cache.Get<IEnumerable<BankNoteSummary>>(cacheKey);
			if (cached != null)
			{
				_logger.LogDebug("Banknote summary cache hit for key: {Key}", cacheKey);
				return cached;
			}

			// Not in cache → compute
			var result = await _euroNoteService.GetNoteSummariesAsync(filters);

			// Store in cache
			_cache.Set(cacheKey, result, TimeSpan.FromMinutes(1));

			return result;
		}

		private static string _buildCacheKey(BankNoteFilters filters)
		{
			// Stable JSON serialization for keys
			return "notes:" + JsonSerializer.Serialize(filters);
		}
	}

}
