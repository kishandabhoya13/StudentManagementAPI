using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement_API.Models.Models.DTO
{
    public class ExchangeRatesDto
    {
        public int ExchangeRateId { get; set; }

        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Base { get; set; }

        public string To { get; set; }

        public Dictionary<string, string> Symbols { get; set; }
        public Dictionary<string, Dictionary<string, decimal>> Rates { get; set; }

        public Dictionary<string, decimal> ratesWithDate { get; set; }
    }


}
