using Microsoft.EntityFrameworkCore;
using CathayBank.API.Data;
using CathayBank.API.Models;

namespace CathayBank.API.Services
{
    /// <summary>
    ///處理與資料庫中幣別資料相關的所有操作。
    /// </summary>
    public class CurrencyService : ICurrencyService
    {
        private readonly ApiDbContext _context;
        private readonly ICryptoService _cryptoService;

        public CurrencyService(ApiDbContext context, ICryptoService cryptoService)
        {
            _context = context;
            _cryptoService = cryptoService;
        }

        public async Task<IEnumerable<Currency>> GetCurrenciesAsync()
        {
            var currenciesFromDb = await _context.Currencies.OrderBy(c => c.Code).ToListAsync();

            foreach (var currency in currenciesFromDb)
            {
                try
                {
                    currency.ChineseName = _cryptoService.Decrypt(currency.ChineseName);
                }
                catch (System.Security.Cryptography.CryptographicException)
                {
                    // 如果解密失敗 (例如，資料庫中存的是舊的未加密資料)，則保持原樣或標記為錯誤
                    // 這裡我們選擇保持原樣，讓前端看到加密後的字串，方便除錯
                }
            }
            return currenciesFromDb;
        }

        public async Task<Currency?> GetCurrencyByCodeAsync(string code)
        {
            var currency = await _context.Currencies.FindAsync(code);
            if (currency != null)
            {
                try
                {
                    currency.ChineseName = _cryptoService.Decrypt(currency.ChineseName);
                }
                catch
                {
                }
            }
            return currency;
        }

        public async Task<Currency> CreateCurrencyAsync(Currency currency)
        {
            var entityToSave = new Currency
            {
                Code = currency.Code,
                ChineseName = _cryptoService.Encrypt(currency.ChineseName)
            };

            _context.Currencies.Add(entityToSave);
            await _context.SaveChangesAsync();

            return currency;
        }

        public async Task UpdateCurrencyAsync(string code, Currency currency)
        {
            var existingCurrency = await _context.Currencies.FindAsync(code);
            if (existingCurrency != null)
            {
                existingCurrency.ChineseName = _cryptoService.Encrypt(currency.ChineseName);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteCurrencyAsync(string code)
        {
            var currency = await _context.Currencies.FindAsync(code);
            if (currency != null)
            {
                _context.Currencies.Remove(currency);
                await _context.SaveChangesAsync();
            }
        }
    }
}
