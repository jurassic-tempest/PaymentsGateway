using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Clients
{
    public interface IBankClient
    {
        Task<PostBankResponse> SendAsync(PostBankRequest request);
    }
}