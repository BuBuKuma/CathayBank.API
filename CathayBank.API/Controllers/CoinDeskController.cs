using CathayBank.API.Models;
using CathayBank.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CathayBank.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoinDeskController : ControllerBase
    {
        private readonly ICoinDeskService _coinDeskService;

        public CoinDeskController(ICoinDeskService coinDeskService)
        {
            _coinDeskService = coinDeskService;
        }

        /// <summary>
        /// 查詢 CurrentPrice 失敗回傳 Mock
        /// </summary>
        [HttpGet("currentprice")]
        [ProducesResponseType(typeof(CoinDeskResponse), 200)]
        public async Task<ActionResult<CoinDeskResponse>> GetCurrentPrice()
        {
            var data = await _coinDeskService.GetCurrentPriceAsync();
            return Ok(data);
        }

        /// <summary>
        /// 資料轉換
        /// </summary>
        [HttpGet("transformed-price")]
        [ProducesResponseType(typeof(CurrentPriceResponse), 200)]
        public async Task<ActionResult<CurrentPriceResponse>> GetTransformedPrice()
        {
            var data = await _coinDeskService.GetTransformedPriceDataAsync();
            return Ok(data);
        }
    }
}
