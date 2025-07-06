using CathayBank.API.Models;
using CathayBank.API.Services;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace CathayBank.Tests
{
    public class CoinDeskServiceTests
    {
        private readonly Mock<ICurrencyService> _mockCurrencyService;

        public CoinDeskServiceTests()
        {
            _mockCurrencyService = new Mock<ICurrencyService>();
            var currencies = new List<Currency>
            {
                new Currency { Code = "USD", ChineseName = "美元" },
                new Currency { Code = "GBP", ChineseName = "英鎊" },
                new Currency { Code = "EUR", ChineseName = "歐元" }
            };
            _mockCurrencyService.Setup(s => s.GetCurrenciesAsync()).ReturnsAsync(currencies);
        }

        private Mock<IHttpClientFactory> CreateMockHttpClientFactory(HttpResponseMessage httpResponseMessage)
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);

            var client = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://api.coindesk.com/")
            };

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            return mockHttpClientFactory;
        }

        [Fact]
        public async Task GetTransformedPriceDataAsync_APISuccess()
        {
            // Arrange
            var mockApiResponse = new CoinDeskResponse
            {
                Time = new TimeInfo { Updated = "Jul 4, 2025 15:00:00 UTC" },
                Bpi = new Dictionary<string, BpiDetail>
                {
                    { "USD", new BpiDetail { Code = "USD", RateFloat = 50000.00m } }
                }
            };
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(mockApiResponse))
            };

            var mockHttpClientFactory = CreateMockHttpClientFactory(httpResponse);

            var service = new CoinDeskService(mockHttpClientFactory.Object, _mockCurrencyService.Object);

            // Act
            var result = await service.GetTransformedPriceDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("2025/07/04 15:00:00", result.UpdatedTime);
            var usdCurrency = result.Currencies?.FirstOrDefault(c => c.Code == "USD");
            Assert.NotNull(usdCurrency);
            Assert.Equal("美元", usdCurrency.ChineseName);
            Assert.Equal(50000.00m, usdCurrency.Rate);
        }

        [Fact]
        public async Task GetTransformedPriceDataAsync_APIFail()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            };

            var mockHttpClientFactory = CreateMockHttpClientFactory(httpResponse);

            var service = new CoinDeskService(mockHttpClientFactory.Object, _mockCurrencyService.Object);

            // Act
            var result = await service.GetTransformedPriceDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("2022/08/03 20:25:00", result.UpdatedTime); // 驗證時間是否為 Mock 時間
            var usdCurrency = result.Currencies?.FirstOrDefault(c => c.Code == "USD");
            Assert.NotNull(usdCurrency);
            Assert.Equal(23342.0112m, usdCurrency.Rate); // 驗證匯率是否為 Mock 匯率
        }
    }
}
