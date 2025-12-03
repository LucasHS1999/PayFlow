using Microsoft.AspNetCore.Mvc;

namespace FastPay.Mock.Controllers;

[ApiController]
[Route("fastpay")]
public class FastPayController : ControllerBase
{
    [HttpPost("payments")]
    public IActionResult Pay([FromBody] object payload)
    {
        return Ok(new { id = "FP-884512", status = "approved", status_detail = "Pagamento aprovado" });
    }
}
