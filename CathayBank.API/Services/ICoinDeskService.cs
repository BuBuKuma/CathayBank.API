using CathayBank.API.Models;

namespace CathayBank.API.Services
{
    public interface ICoinDeskService
    {
        /// <summary>
        /// 呼叫 CoinDesk API 取得目前價格
        /// </summary>
        Task<CoinDeskResponse> GetCurrentPriceAsync();

        /// <summary>
        /// 呼叫 CoinDesk API 並轉換成新的 API 格式
        /// </summary>
        Task<CurrentPriceResponse> GetTransformedPriceDataAsync();
    }
}
