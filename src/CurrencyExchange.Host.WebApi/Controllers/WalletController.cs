using CurrencyExchange.Core.Abstractions.Enums;
using CurrencyExchange.Core.Abstractions.Managers;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchange.Host.WebApi.Controllers
{
    [ApiController]
    [Route("api/wallets")]
    public class WalletController : ControllerBase
    {
        private readonly ILogger<WalletController> _logger;
        private readonly IWalletManager _walletManager;

        public WalletController(ILogger<WalletController> logger, IWalletManager walletManager)
        {
            _logger = logger;
            _walletManager = walletManager;
        }

        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateWallet()
        {
            _logger.LogInformation("Creating new wallet.");

            var result = await _walletManager.CreateWallet();

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(result);
        }

        [HttpGet("{walletId}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetWalletBalance(int walletId, [FromQuery] string currency)
        {
            _logger.LogInformation($"Getting wallet balance for {walletId}.");

            var result = await _walletManager.GetBalance(walletId, currency);

            if (result.IsSuccess)
            {
                return Ok(new { balance = result.Payload, currency });
            }

            return BadRequest(new { message = result.ErrorMessage });
        }

        [HttpPost("{walletId}/adjustbalance")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AdjustBalance(int walletId,
            [FromQuery] decimal amount, [FromQuery] string currency, [FromQuery] SupportedFundStrategiesEnum strategy)
        {
            _logger.LogInformation($"Adjusting wallet balance for {walletId} by {amount} {currency} using {strategy} strategy.");

            var result = await _walletManager.AdjustBalance(walletId, amount, currency, strategy);

            if (result.IsSuccess)
            {
                return Ok(new { message = "Wallet balance adjusted successfully.", wallet = result.Payload });
            }

            return BadRequest(new { message = result.ErrorMessage });
        }
    }
}
