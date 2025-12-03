using PayFlow.Api.Domain.Interfaces;

namespace PayFlow.Api.Domain.Services;

public class PaymentFeeService : IPaymentFeeService
{
    public decimal CalculateFee(decimal amount, string currency)
    {
        var rate = amount < 100m ? 0.015m : 0.025m;
        return decimal.Round(amount * rate, 2);
    }
}
