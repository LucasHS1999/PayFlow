namespace PayFlow.Api.Domain.Services;

public interface IPaymentFeeService
{
    decimal CalculateFee(decimal amount, string provider);
    
}
