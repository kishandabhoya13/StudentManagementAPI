using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement_API.Models.Models.DTO
{
    public class CurrencyPairDto
    {
        public int CurrencyPairId { get; set; }

        public string CurrencyPair { get; set; }

        public decimal Rate { get; set; }

        public string BaseCurrency { get; set; }

        public string ToCurrency { get; set; }

        public decimal AskRate { get; set; }

        public DateTime ModifiedDate { get; set; }

        public List<string> Currencies { get; set; }

        public IList<CurrencyPairDto> CurrencyRates { get; set; }

        public int RateAlertId { get; set; }

        public int StudentId { get; set; }

        public string Email { get; set; }

        public decimal ExpectedRate { get; set; }
    }
}
