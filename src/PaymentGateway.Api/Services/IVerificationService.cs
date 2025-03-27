using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Services
{
    public interface IVerificationService
    {
        List<Error> ValidateRequest(PostPaymentRequest request);
    }
}
