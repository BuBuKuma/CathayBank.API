using CathayBank.API.Models;
using CathayBank.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CathayBank.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrenciesController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;

        public CurrenciesController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        /// <summary>
        /// 查詢全部
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Currency>), 200)]
        public async Task<ActionResult<IEnumerable<Currency>>> GetCurrencies()
        {
            var currencies = await _currencyService.GetCurrenciesAsync();
            return Ok(currencies);
        }

        /// <summary>
        /// 查詢單筆
        /// </summary>
        [HttpGet("{code}")]
        [ProducesResponseType(typeof(Currency), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Currency>> GetCurrency(string code)
        {
            var currency = await _currencyService.GetCurrencyByCodeAsync(code);

            if (currency == null)
            {
                return NotFound();
            }

            return Ok(currency);
        }

        /// <summary>
        /// 新增
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Currency), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Currency>> PostCurrency(Currency currency)
        {
            var createdCurrency = await _currencyService.CreateCurrencyAsync(currency);
            // 回傳 201 Created，並在 Location header 提供新資源的 URL
            return CreatedAtAction(nameof(GetCurrency), new { code = createdCurrency.Code }, createdCurrency);
        }

        /// <summary>
        /// 修改
        /// </summary>
        [HttpPut("{code}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> PutCurrency(string code, Currency currency)
        {
            if (code != currency.Code)
            {
                return BadRequest("URL 中的 Code 與 Request Body 中的 Code 不一致。");
            }

            var existingCurrency = await _currencyService.GetCurrencyByCodeAsync(code);
            if (existingCurrency == null)
            {
                return NotFound();
            }

            await _currencyService.UpdateCurrencyAsync(code, currency);
            return NoContent(); // 更新成功，回傳 204 No Content
        }

        /// <summary>
        /// 刪除
        /// </summary>
        [HttpDelete("{code}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteCurrency(string code)
        {
            var currency = await _currencyService.GetCurrencyByCodeAsync(code);
            if (currency == null)
            {
                return NotFound();
            }

            await _currencyService.DeleteCurrencyAsync(code);
            return NoContent(); // 刪除成功，回傳 204 No Content
        }
    }
}