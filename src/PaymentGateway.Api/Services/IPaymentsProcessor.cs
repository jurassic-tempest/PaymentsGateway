using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services
{
    public interface IPaymentsProcessor
    {
        Task<PostPaymentResponse> Process(PostPaymentRequest request);
    }
}
