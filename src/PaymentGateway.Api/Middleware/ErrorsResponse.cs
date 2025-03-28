using System.Text.Json;

using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Middleware
{
    public class ErrorsResponse
    {
        private static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        public List<Error> Errors { get; set; } = new List<Error>();

        public string Serialize() => JsonSerializer.Serialize(this, _serializerOptions);
    }
}
