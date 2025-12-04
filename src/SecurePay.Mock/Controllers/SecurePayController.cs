using Microsoft.AspNetCore.Mvc;

namespace SecurePay.Mock.Controllers;

[ApiController]
[Route("securepay")]
public class SecurePayController : ControllerBase
{
    public record SecurePayTransactionPayload(int amount_cents, string currency_code, string client_reference);

    [HttpPost("transactions")]
    public IActionResult Transaction([FromBody] SecurePayTransactionPayload payload)
    {
        if (payload.amount_cents <= 0 || string.IsNullOrWhiteSpace(payload.currency_code) || string.IsNullOrWhiteSpace(payload.client_reference))
        {
            return BadRequest(new { error = "invalid securepay payload" });
        }

        return Ok(new { transaction_id = "SP-19283", result = "success" });
    }
}
