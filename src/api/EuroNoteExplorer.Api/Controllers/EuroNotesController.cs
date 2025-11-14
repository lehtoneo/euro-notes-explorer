using EuroNoteExplorer.Api.Services.Interfaces;
using EuroNoteExplorer.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace EuroNoteExplorer.Api.Controllers
{
    [ApiController]
    [Route("api/v1/euronotes")]
    public class EuroNotesController : ControllerBase
    {
        private readonly IEuroNoteService _euroNoteService;
        private readonly ILogger<EuroNotesController> _logger;

        public EuroNotesController(IEuroNoteService euroNoteService, ILogger<EuroNotesController> logger)
        {
            _euroNoteService = euroNoteService;
            _logger = logger;
        }

        [HttpGet("", Name = "GetBankNotes")]
        public async Task<IEnumerable<BankNoteSummary>> GetAsync([FromQuery] BankNoteFilters filters)
        {
            return await _euroNoteService.GetNoteSummariesAsync(filters);
        }
    }
}
