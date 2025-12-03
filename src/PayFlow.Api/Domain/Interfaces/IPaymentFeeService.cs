namespace PayFlow.Api.Domain.Interfaces;

public interface IPaymentFeeService
{
    decimal CalculateFee(decimal amount, string currency);
}
