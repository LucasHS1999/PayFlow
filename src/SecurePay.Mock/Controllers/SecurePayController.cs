using Microsoft.AspNetCore.Mvc;

namespace SecurePay.Mock.Controllers;

[ApiController]
[Route("securepay")]
public class SecurePayController : ControllerBase
{
    [HttpPost("transactions")]
    public IActionResult Transaction([FromBody] object payload)
    {
        return Ok(new { transaction_id = "SP-19283", result = "success" });
    }
}
