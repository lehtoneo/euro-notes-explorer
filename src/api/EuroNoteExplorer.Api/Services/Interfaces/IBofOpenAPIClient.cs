using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EuroNoteExplorer.Shared.DTOs;

namespace EuroNoteExplorer.Api.Services.Interfaces
{
	public interface IBoFOpenApiClient
	{
		Task<IReadOnlyList<BankNoteObservation>> GetBanknotePiecesAsync(
			string denominationCode,
			DateTime start,
			DateTime end);

		Task<Dictionary<string, string>> GetDailyExchangeRatesAsync(
			DateTime date,
			string[]? currencies = null);
	}


}
