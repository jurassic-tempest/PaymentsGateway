namespace PaymentGateway.Api.Models.Responses
{
    public class PostBankResponse
    {
        public bool Authorized { get; init; }
        public string AuthorizationCode { get; init; }
    }
}
