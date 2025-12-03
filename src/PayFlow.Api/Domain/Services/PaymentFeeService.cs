namespace PayFlow.Api.Domain.Services;

public class PaymentFeeService : IPaymentFeeService
{
    public decimal CalculateFee(decimal amount, string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
            throw new ArgumentException("Provider is required.", nameof(provider));

        var rate = 0.0m;
        var fixedFee = 0.0m;

        switch (provider.Trim().ToUpperInvariant())
        {
            case "FASTPAY":
                rate = 0.0349m; // 3,49%
                break;

            case "SECUREPAY":
                rate = 0.0299m; // 2,99%
                fixedFee = 0.40m; // + R$ 0,40
                break;

            default:
                throw new ArgumentException("Provider not found.", nameof(provider));
        }

        var fee = (amount * rate) + fixedFee;
        return decimal.Round(fee, 2);
    }
}
