using CathayBank.API.Models;

namespace CathayBank.API.Services
{
    public interface ICurrencyService
    {
        /// <summary>
        /// 取得所有幣別資料
        /// </summary>
        /// <returns>幣別列表</returns>
        Task<IEnumerable<Currency>> GetCurrenciesAsync();

        /// <summary>
        /// 透過代碼取得單一幣別資料
        /// </summary>
        /// <param name="code">幣別代碼</param>
        /// <returns>單一幣別資料</returns>
        Task<Currency?> GetCurrencyByCodeAsync(string code);

        /// <summary>
        /// 新增一筆幣別資料
        /// </summary>
        /// <param name="currency">要新增的幣別物件</param>
        /// <returns>新增後的幣別物件</returns>
        Task<Currency> CreateCurrencyAsync(Currency currency);

        /// <summary>
        /// 更新一筆幣別資料
        /// </summary>
        /// <param name="code">要更新的幣別代碼</param>
        /// <param name="currency">要更新的幣別物件</param>
        /// <returns>Task</returns>
        Task UpdateCurrencyAsync(string code, Currency currency);

        /// <summary>
        /// 刪除一筆幣別資料
        /// </summary>
        /// <param name="code">要刪除的幣別代碼</param>
        /// <returns>Task</returns>
        Task DeleteCurrencyAsync(string code);
    }
}
