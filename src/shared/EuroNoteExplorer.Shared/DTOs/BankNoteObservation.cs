using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EuroNoteExplorer.Shared.DTOs
{
    public class BankNoteObservation
    {
        public DateTime Period { get; set; }
        public string PeriodCode { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }
}
