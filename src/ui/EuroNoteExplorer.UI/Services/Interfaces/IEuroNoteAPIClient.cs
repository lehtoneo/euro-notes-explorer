using EuroNoteExplorer.Shared.DTOs;

namespace EuroNoteExplorer.UI.Services.Interfaces
{
    public interface IEuroNoteAPIClient
    {
        public Task<IEnumerable<BankNoteSummary>> GetBankNoteSummaryAsync(BankNoteFilters filters);
    }
}
