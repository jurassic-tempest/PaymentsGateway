using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Options;

using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Clients
{
    public class BankClient(
        HttpClient httpClient, 
        ILogger<BankClient> logger, 
        IOptions<BankClientOptions> options) : IBankClient
    {
        public async Task<PostBankResponse> SendAsync(PostBankRequest request)
        {
            try
            {
                logger.LogInformation("Sending payment request to bank...");
                var serializerOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                };
                var response = await httpClient.PostAsync($"{options.Value.BaseUrl}/payments", new StringContent(
                    JsonSerializer.Serialize(request, serializerOptions), Encoding.UTF8, "application/json"));


                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<PostBankResponse>(serializerOptions);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "An error occured when sending payment request to bank");
                throw;
            }
        }
    }
}
