using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement_API.Models.Models
{
    public class ExchangeRate
    {
        public int ExchangeRateId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string BaseCurrency { get; set; }

        public string ToCurrency { get; set; }

        public string Rate { get; set; }

        public Dictionary<string, Dictionary<string, decimal>> Rates { get; set; }

        public Dictionary<string, decimal> ratesWithDate { get; set; }
    }
}
