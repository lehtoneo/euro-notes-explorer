using EuroNoteExplorer.Shared.DTOs;

namespace EuroNoteExplorer.Api.Services.Interfaces
{
    public interface IEuroNoteService
    {
        public Task<IEnumerable<BankNoteSummary>> GetNoteSummariesAsync(BankNoteFilters filters);
    }
}
