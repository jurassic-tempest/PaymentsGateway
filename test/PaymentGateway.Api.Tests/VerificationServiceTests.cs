using Microsoft.Extensions.Logging;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests
{
    public class VerificationServiceTests
    {
        private readonly Mock<ILogger<VerificationService>> _loggerMock;
        private readonly VerificationService _verificationService;

        public VerificationServiceTests()
        {
            _loggerMock = new Mock<ILogger<VerificationService>>();
            _verificationService = new VerificationService(_loggerMock.Object);
        }

        [Theory]
        [InlineData("1234567890123456", 12, "GBP", 100, 123)]
        public void ValidateRequest_ValidRequest_ReturnsEmptyListOfErrors(
            string cardNumber, int expiryMonth, string currency, int amount, int cvv)
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                CardNumber = cardNumber,
                ExpiryMonth = expiryMonth,
                ExpiryYear = DateTime.Now.AddYears(1).Year,
                Currency = currency,
                Amount = amount,
                Cvv = cvv
            };

            // Act
            var result = _verificationService.ValidateRequest(request);

            // Assert
            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData("123")]
        [InlineData("1233454325464562645623565364534645744364")]
        public void ValidateRequest_CardNumberTooShortOrLong_ReturnsError(
            string cardNumber)
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                CardNumber = cardNumber,
                ExpiryMonth = 12,
                ExpiryYear = DateTime.Now.AddYears(1).Year,
                Currency = "GBP",
                Amount = 100,
                Cvv = 123
            };

            // Act
            var result = _verificationService.ValidateRequest(request);

            // Assert
            result.Should().ContainSingle();
            result[0].Code.Should().Be("InvalidCardNumber");
            result[0].Message.Should().Be("Card number must be between 14 and 19 digits.");
        }

        [Theory]
        [InlineData("123abc123123456")]
        public void ValidateRequest_CardNumberNotAllDigits_ReturnsError(
            string cardNumber)
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                CardNumber = cardNumber,
                ExpiryMonth = 12,
                ExpiryYear = DateTime.Now.AddYears(1).Year,
                Currency = "GBP",
                Amount = 100,
                Cvv = 123
            };

            // Act
            var result = _verificationService.ValidateRequest(request);

            // Assert
            result.Should().ContainSingle();
            result[0].Code.Should().Be("InvalidCardNumber");
            result[0].Message.Should().Be("Card number is not a valid number.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(13)]
        public void ValidateRequest_ExpiryMonthOutOfRange_ReturnsError(
            int expiryMonth)
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                CardNumber = "1234567890123456",
                ExpiryMonth = expiryMonth,
                ExpiryYear = DateTime.Now.AddYears(1).Year,
                Currency = "GBP",
                Amount = 100,
                Cvv = 123
            };

            // Act
            var result = _verificationService.ValidateRequest(request);

            // Assert
            result.Should().ContainSingle();
            result[0].Code.Should().Be("InvalidExpiry");
            result[0].Message.Should().Be("Expiry month must be between 1 and 12.");
        }

        [Theory]
        [InlineData(1900, 12)]
        [InlineData(2025, 1)]
        public void ValidateRequest_ExpiryYearOrMonthInPast_ReturnsError(
            int expiryYear, int expiryMonth)
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                CardNumber = "1234567890123456",
                ExpiryMonth = expiryMonth,
                ExpiryYear = expiryYear,
                Currency = "GBP",
                Amount = 100,
                Cvv = 123
            };

            // Act
            var result = _verificationService.ValidateRequest(request);

            // Assert
            result.Should().ContainSingle();
            result[0].Code.Should().Be("InvalidExpiry");
            result[0].Message.Should().Be("Expiry month and year must be in the future.");
        }

        [Theory]
        [InlineData("USD")]
        public void ValidateRequest_InvalidCurrencyCode_ReturnsError(
                       string currency)
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                CardNumber = "1234567890123456",
                ExpiryMonth = 12,
                ExpiryYear = DateTime.Now.AddYears(1).Year,
                Currency = currency,
                Amount = 100,
                Cvv = 123
            };

            // Act
            var result = _verificationService.ValidateRequest(request);

            // Assert
            result.Should().ContainSingle();
            result[0].Code.Should().Be("InvalidCurrencyCode");
            result[0].Message.Should().Be("Currency code is not valid.");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void ValidateRequest_AmountLessThanOrEqualToZero_ReturnsError(
            int amount)
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                CardNumber = "1234567890123456",
                ExpiryMonth = 12,
                ExpiryYear = DateTime.Now.AddYears(1).Year,
                Currency = "GBP",
                Amount = amount,
                Cvv = 123
            };

            // Act
            var result = _verificationService.ValidateRequest(request);

            // Assert
            result.Should().ContainSingle();
            result[0].Code.Should().Be("InvalidAmount");
            result[0].Message.Should().Be("Amount must be greater than zero.");
        }

        [Theory]
        [InlineData(99)]
        [InlineData(10000)]
        public void ValidateRequest_CVVOutOfRange_ReturnsError(
            int cvv)
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                CardNumber = "1234567890123456",
                ExpiryMonth = 12,
                ExpiryYear = DateTime.Now.AddYears(1).Year,
                Currency = "GBP",
                Amount = 100,
                Cvv = cvv
            };

            // Act
            var result = _verificationService.ValidateRequest(request);

            // Assert
            result.Should().ContainSingle();
            result[0].Code.Should().Be("InvalidCVV");
            result[0].Message.Should().Be("CVV must be a 3 or 4 digit number.");
        }
    }
}
