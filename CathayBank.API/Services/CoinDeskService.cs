using CathayBank.API.Models;
using System.Globalization;

namespace CathayBank.API.Services
{
    public class CoinDeskService : ICoinDeskService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICurrencyService _currencyService;
        private const string CoinDeskApiUrl = "https://api.coindesk.com/v1/bpi/currentprice.json";

        public CoinDeskService(IHttpClientFactory httpClientFactory, ICurrencyService currencyService)
        {
            _httpClientFactory = httpClientFactory;
            _currencyService = currencyService;
        }

        public async Task<CoinDeskResponse> GetCurrentPriceAsync()
        {
            var client = _httpClientFactory.CreateClient("CoinDeskClient");
            try
            {
                var response = await client.GetAsync(CoinDeskApiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var coinDeskResponse = await response.Content.ReadFromJsonAsync<CoinDeskResponse>();
                    // 確保反序列化成功
                    if (coinDeskResponse != null)
                    {
                        return coinDeskResponse;
                    }
                }
                // 失敗，回傳 Mock 資料
                return GetMockCoinDeskData();
            }
            catch (HttpRequestException)
            {
                // 錯誤，回傳 Mock 資料
                return GetMockCoinDeskData();
            }
        }

        public async Task<CurrentPriceResponse> GetTransformedPriceDataAsync()
        {
            var coinDeskData = await GetCurrentPriceAsync();

            var localCurrencies = await _currencyService.GetCurrenciesAsync();
            var currencyNameMap = localCurrencies.ToDictionary(c => c.Code, c => c.ChineseName);

            var result = new CurrentPriceResponse
            {
                UpdatedTime = ParseAndFormatTime(coinDeskData.Time?.Updated),
                Currencies = new List<CurrencyInfo>()
            };

            if (coinDeskData.Bpi != null)
            {
                foreach (var bpiDetail in coinDeskData.Bpi.Values)
                {
                    if (bpiDetail.Code != null)
                    {
                        result.Currencies.Add(new CurrencyInfo
                        {
                            Code = bpiDetail.Code,
                            ChineseName = currencyNameMap.GetValueOrDefault(bpiDetail.Code, ""),
                            Rate = bpiDetail.RateFloat
                        });
                    }
                }
            }
            return result;
        }

        private string ParseAndFormatTime(string? updatedTime)
        {
            if (string.IsNullOrEmpty(updatedTime))
            {
                return DateTime.UtcNow.ToString("yyyy/MM/dd HH:mm:ss");
            }
            if (DateTime.TryParse(updatedTime.Replace(" UTC", ""), CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                return parsedDate.ToString("yyyy/MM/dd HH:mm:ss");
            }
            return DateTime.UtcNow.ToString("yyyy/MM/dd HH:mm:ss");
        }

        private CoinDeskResponse GetMockCoinDeskData()
        {
            return new CoinDeskResponse
            {
                Time = new TimeInfo
                {
                    Updated = "Aug 3, 2022 20:25:00 UTC",
                    UpdatedISO = "2022-08-03T20:25:00+00:00",
                    Updateduk = "Aug 3, 2022 at 21:25 BST"
                },
                Disclaimer = "This data was produced from the CoinDesk Bitcoin Price Index (USD). Non-USD currency data converted using hourly conversion rate from openexchangerates.org",
                ChartName = "Bitcoin",
                Bpi = new Dictionary<string, BpiDetail>
                {
                    { "USD", new BpiDetail { Code = "USD", Symbol = "$", Rate = "23,342.0112", Description = "US Dollar", RateFloat = 23342.0112m } },
                    { "GBP", new BpiDetail { Code = "GBP", Symbol = "£", Rate = "19,504.3978", Description = "British Pound Sterling", RateFloat = 19504.3978m } },
                    { "EUR", new BpiDetail { Code = "EUR", Symbol = "€", Rate = "22,738.5269", Description = "Euro", RateFloat = 22738.5269m } }
                }
            };
        }
    }
}