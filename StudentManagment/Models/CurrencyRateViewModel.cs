using System.ComponentModel.DataAnnotations;

namespace StudentManagment.Models
{
    public class CurrencyRateViewModel
    {
        public int CurrencyPairId { get; set; }

        public string CurrencyPair { get; set; }

        [Required(ErrorMessage = "Please Enter Amount")]
        public decimal Rate { get; set; }

        public string BaseCurrency { get; set; }

        public string ToCurrency { get; set; }

        public decimal AskRate { get; set; }

        public DateTime ModifiedDate { get; set; }

        public List<string> Currencies { get; set; }

        public IList<CurrencyRateViewModel> CurrencyRates { get; set; }

        [Required(ErrorMessage = "Email is Required")]
        public string Email { get; set; }

        public int RateAlertId { get; set; }

        public int StudentId { get; set; }

        public decimal ExpectedRate {get; set;}
    }

}
