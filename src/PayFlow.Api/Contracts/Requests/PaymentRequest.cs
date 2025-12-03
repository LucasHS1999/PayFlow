namespace PayFlow.Api.Contracts.Requests;

public record PaymentRequest(decimal amount, string currency);
