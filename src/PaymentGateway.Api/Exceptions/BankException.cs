using System.Net;

using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Exceptions
{
    public class BankException : Exception
    {
        public string Code { get; set; }
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.InternalServerError;

        public List<Error> Errors { get; set; } = new List<Error>();

        public BankException(string code, string message) : base(message)
        {
            Code = code;
            StatusCode = HttpStatusCode.InternalServerError;
        }
    }
}
