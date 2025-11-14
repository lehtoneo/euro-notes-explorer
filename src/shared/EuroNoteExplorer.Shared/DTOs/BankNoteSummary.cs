using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EuroNoteExplorer.Shared.DTOs
{
    public class BankNoteSummary
    {
        public int Denomination { get; set; }
        public string DenominationCode { get; set; } = string.Empty;
        public string CurrencyCode { get; set; }
        public decimal Value { get; set; }
        public long Count { get; set; }

        public IEnumerable<CurrencyValue> CurrencyValues { get; set; }
    }

    public class CurrencyValue
    {
        public string CurrencyCode { get; set; }

        public decimal Value { get; set; }

        public decimal ExchangeRate { get; set; }
    }
    
}
