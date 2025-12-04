using Microsoft.AspNetCore.Mvc;

namespace FastPay.Mock.Controllers;

[ApiController]
[Route("fastpay")]
public class FastPayController : ControllerBase
{
    public record FastPayPaymentPayload(decimal transaction_amount, string currency, Payer payer, int installments, string description);
    public record Payer(string email);

    [HttpPost("payments")]
    public IActionResult Pay([FromBody] FastPayPaymentPayload payload)
    {
        if (payload.transaction_amount <= 0 || string.IsNullOrWhiteSpace(payload.currency) || payload.payer is null || string.IsNullOrWhiteSpace(payload.payer.email))
        {
            return BadRequest(new { error = "invalid fastpay payload" });
        }

        return Ok(new { id = "FP-884512", status = "approved", status_detail = "Pagamento aprovado" });
    }
}
