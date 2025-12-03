namespace PayFlow.Api.Contracts.Responses;

public record CanonicalResponse(string externalId, string provider, string status, decimal grossAmount, decimal fee, decimal netAmount);
