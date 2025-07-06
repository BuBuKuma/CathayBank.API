using Microsoft.EntityFrameworkCore;
using Moq;
using CathayBank.API.Data;
using CathayBank.API.Models;
using CathayBank.API.Services;

namespace CathayBank.Tests
{
    public class CurrencyServiceTests
    {
        private readonly Mock<ICryptoService> _mockCryptoService;

        public CurrencyServiceTests()
        {
            // 在建構函式中，初始化並設定 Mock 物件的預設行為
            _mockCryptoService = new Mock<ICryptoService>();

            // 設定 Encrypt 方法的行為：當被呼叫時，在傳入的字串後方加上 "_encrypted"
            _mockCryptoService.Setup(s => s.Encrypt(It.IsAny<string>()))
                              .Returns((string s) => $"{s}_encrypted");

            // 設定 Decrypt 方法的行為：當被呼叫時，移除字串後方的 "_encrypted"
            _mockCryptoService.Setup(s => s.Decrypt(It.IsAny<string>()))
                              .Returns((string s) => s.Replace("_encrypted", ""));
        }

        private ApiDbContext GetDbContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(databaseName: databaseName)
                .Options;
            var dbContext = new ApiDbContext(options);
            return dbContext;
        }

        [Fact]
        public async Task GetCurrenciesAsync_ShouldReturnDecryptedAndSortedCurrencies()
        {
            // Arrange
            var dbContext = GetDbContext("GetCurrencies");
            // 在資料庫中存入 "加密後" 的名稱
            dbContext.Currencies.AddRange(
                new Currency { Code = "TWD", ChineseName = "新台幣_encrypted" },
                new Currency { Code = "USD", ChineseName = "美元_encrypted" },
                new Currency { Code = "JPY", ChineseName = "日圓_encrypted" }
            );
            await dbContext.SaveChangesAsync();

            var service = new CurrencyService(dbContext, _mockCryptoService.Object);

            // Act
            var result = (await service.GetCurrenciesAsync()).ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            // 驗證排序是否正確
            Assert.Equal("JPY", result[0].Code);
            // 驗證回傳的名稱是否已被 "解密"
            Assert.Equal("日圓", result[0].ChineseName);
            Assert.Equal("美元", result[2].ChineseName);
        }

        [Fact]
        public async Task CreateCurrencyAsync_ShouldEncryptAndReturnDecrypted()
        {
            // Arrange
            var dbContext = GetDbContext("CreateCurrency");
            var service = new CurrencyService(dbContext, _mockCryptoService.Object);
            var newCurrency = new Currency { Code = "HKD", ChineseName = "港幣" };

            // Act
            var result = await service.CreateCurrencyAsync(newCurrency);

            // Assert
            // 1. 驗證回傳給呼叫端的物件，其名稱是 "解密" 後的明文
            Assert.NotNull(result);
            Assert.Equal("港幣", result.ChineseName);

            // 2. 驗證實際存入資料庫的資料，其名稱是 "加密" 後的
            var savedCurrency = await dbContext.Currencies.FindAsync("HKD");
            Assert.NotNull(savedCurrency);
            Assert.Equal("港幣_encrypted", savedCurrency.ChineseName);
        }

        [Fact]
        public async Task UpdateCurrencyAsync_ShouldEncryptNameInDb()
        {
            // Arrange
            var dbContext = GetDbContext("UpdateCurrency");
            var originalCurrency = new Currency { Code = "USD", ChineseName = "舊名稱_encrypted" };
            dbContext.Currencies.Add(originalCurrency);
            await dbContext.SaveChangesAsync();

            var service = new CurrencyService(dbContext, _mockCryptoService.Object);
            var updatedCurrencyData = new Currency { Code = "USD", ChineseName = "美元" };

            // Act
            await service.UpdateCurrencyAsync("USD", updatedCurrencyData);

            // Assert
            var currencyAfterUpdate = await dbContext.Currencies.FindAsync("USD");
            Assert.NotNull(currencyAfterUpdate);
            // 驗證資料庫中的名稱已被更新為 "加密" 後的新名稱
            Assert.Equal("美元_encrypted", currencyAfterUpdate.ChineseName);
        }
    }
}
