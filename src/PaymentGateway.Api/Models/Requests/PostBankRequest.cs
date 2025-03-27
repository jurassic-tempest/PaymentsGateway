namespace PaymentGateway.Api.Models.Requests
{
    public class PostBankRequest
    {
        public string CardNumber { get; init; }

        public string ExpiryDate { get; init; }

        public string Currency { get; init; }

        public int Amount { get; init; }

        public int Cvv { get; init; }
    }
}
