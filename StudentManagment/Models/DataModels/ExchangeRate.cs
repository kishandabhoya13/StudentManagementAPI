namespace StudentManagment.Models.DataModels
{
    public class ExchangeRate
    {
        public int ExchangeRateId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string BaseCurrency { get; set; }

        public string ToCurrency { get; set; }

        public string Rate { get; set; }

        public List<string> Currencies { get; set; }

        public Dictionary<string, decimal> ratesWithDate { get; set; }

    }

    public class ExchangeRateViewMOdel
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
