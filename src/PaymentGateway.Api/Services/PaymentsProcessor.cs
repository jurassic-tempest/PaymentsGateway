using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services
{
    public class PaymentsProcessor(
        IBankClient bankClient, 
        IPaymentsRepository paymentsRepository,
        IVerificationService verificationService) : IPaymentsProcessor
    {
        public async Task<PostPaymentResponse> Process(PostPaymentRequest request)
        {
            PostPaymentResponse postPaymentsResponse;
            var errors = verificationService.ValidateRequest(request);
            if (errors.Any())
            {
                postPaymentsResponse = CreatePostPaymentsResponse(request, PaymentStatus.Rejected);
                paymentsRepository.Add(postPaymentsResponse);
                return postPaymentsResponse;
            }

            var expiryMonthPrefix = request.ExpiryMonth.ToString().Length == 1 ? "0" : string.Empty;
            var bankRequest = new PostBankRequest
            {
                CardNumber = request.CardNumber,
                ExpiryDate = $"{expiryMonthPrefix}{request.ExpiryMonth}/{request.ExpiryYear}",
                Amount = request.Amount,
                Currency = request.Currency,
                Cvv = request.Cvv,
            };

            var response = await bankClient.SendAsync(bankRequest);
            var paymentStatus = response.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined;
            postPaymentsResponse = CreatePostPaymentsResponse(request, paymentStatus);
            paymentsRepository.Add(postPaymentsResponse);
            return postPaymentsResponse;
        }

        private static PostPaymentResponse CreatePostPaymentsResponse(PostPaymentRequest request, PaymentStatus paymentStatus)
        {
            return new PostPaymentResponse
            {
                Id = Guid.NewGuid(),
                Status = paymentStatus,
                CardNumberLastFour = int.Parse(request.CardNumber[^4..]),
                ExpiryMonth = request.ExpiryMonth,
                ExpiryYear = request.ExpiryYear,
                Currency = request.Currency,
                Amount = request.Amount,
            };
        }
    }
}
