namespace CathayBank.API.Models
{
    public class CurrentPriceResponse
    {
        public string? UpdatedTime { get; set; }
        public List<CurrencyInfo>? Currencies { get; set; }
    }

    public class CurrencyInfo
    {
        public string? Code { get; set; }
        public string? ChineseName { get; set; }
        public decimal Rate { get; set; }
    }
}
