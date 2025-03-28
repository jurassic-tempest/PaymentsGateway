using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Services
{
    public class VerificationService(ILogger<VerificationService> logger) : IVerificationService
    {
        private static readonly HashSet<string> ValidCurrencyCodes = new HashSet<string> { "GBP", "EUR", "BOB" };

        public List<Error> ValidateRequest(PostPaymentRequest request)
        {
            var errors = new List<Error>();
            ValidateCardNumber(request, errors);
            ValidateExpiry(request, errors);
            ValidateCurrency(request, errors);
            ValidateAmount(request, errors);
            ValidateCVV(request, errors);

            if (errors.Any())
            {
                foreach (var error in errors)
                {
                    logger.LogError("Validation error - Code: {Code}, Message: {Message}", error.Code, error.Message);
                }
            }
            
            return errors;
        }
       
        private void ValidateCardNumber(PostPaymentRequest request, List<Error> errors)
        {
            if (request.CardNumber.Length < 14 || request.CardNumber.Length > 19)
            {
                errors.Add(new Error
                {
                    Code = "InvalidCardNumber",
                    Message = "Card number must be between 14 and 19 digits."
                });
            }
            if(!request.CardNumber.All(char.IsDigit))
            {
                errors.Add(new Error
                {
                    Code = "InvalidCardNumber",
                    Message = "Card number is not a valid number."
                });
            }

        }

        private void ValidateExpiry(PostPaymentRequest request, List<Error> errors)
        {
            var now = DateTime.UtcNow;
            var (month, year) = (now.Month, now.Year);
            if (request.ExpiryMonth < 1 || request.ExpiryMonth > 12)
            {
                errors.Add(new Error
                {
                    Code = "InvalidExpiry",
                    Message = "Expiry month must be between 1 and 12."
                });
            }
            if (request.ExpiryYear < year || (request.ExpiryMonth < month && request.ExpiryYear == year))
            {
                errors.Add(new Error
                {
                    Code = "InvalidExpiry",
                    Message = "Expiry month and year must be now or in the future."
                });
            }
        }

        private void ValidateCurrency(PostPaymentRequest request, List<Error> errors)
        {
            if (!ValidCurrencyCodes.Contains(request.Currency.ToUpper())) 
            {
                errors.Add(new Error
                {
                    Code = "InvalidCurrencyCode",
                    Message = "Currency code is not valid."
                });
            }
        }

        private void ValidateAmount(PostPaymentRequest request, List<Error> errors)
        {
            if (request.Amount <= 0)
            {
                errors.Add(new Error
                {
                    Code = "InvalidAmount",
                    Message = "Amount must be greater than zero."
                });
            }
        }

        private void ValidateCVV(PostPaymentRequest request, List<Error> errors)
        {
            if (request.Cvv < 100 || request.Cvv > 9999)
            {
                errors.Add(new Error
                {
                    Code = "InvalidCVV",
                    Message = "CVV must be a 3 or 4 digit number."
                });
            }
        }
       
    }
}
