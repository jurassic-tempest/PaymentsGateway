using AutoFixture.Xunit2;

using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests
{
    public class PaymentsProcessorTests
    {
        private readonly Mock<IBankClient> _bankClientMock;
        private readonly Mock<IPaymentsRepository> _paymentsRepositoryMock;
        private readonly Mock<IVerificationService> _verificationServiceMock;
        private readonly PaymentsProcessor _paymentsProcessor;

        public PaymentsProcessorTests()
        {
            _bankClientMock = new Mock<IBankClient>();
            _paymentsRepositoryMock = new Mock<IPaymentsRepository>();
            _verificationServiceMock = new Mock<IVerificationService>();
            _paymentsProcessor = new PaymentsProcessor(
                _bankClientMock.Object, _paymentsRepositoryMock.Object, _verificationServiceMock.Object);
        }

        [Theory]
        [InlineAutoData(true, PaymentStatus.Authorized)]
        [InlineAutoData(false, PaymentStatus.Declined)]
        public async Task Process_ValidRequest_CallsServiceAndReturnsPostPaymentResponseWithExpectedStatus(
            bool isBankResponseAuthorized, PaymentStatus expectedPaymentStatus)
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                CardNumber = "1234567890123456",
                ExpiryMonth = 12,
                ExpiryYear = DateTime.Now.AddYears(1).Year,
                Currency = "GBP",
                Amount = 100,
                Cvv = 123
            };
            var postBankResponse = new PostBankResponse
            {
                Authorized = isBankResponseAuthorized,
                AuthorizationCode = "",
            };
            _verificationServiceMock.Setup(v => v.ValidateRequest(request)).Returns(new List<Error>());
            _bankClientMock.Setup(b => b.SendAsync(It.IsAny<PostBankRequest>())).ReturnsAsync(postBankResponse);
            _paymentsRepositoryMock.Setup(p => p.Add(It.IsAny<PostPaymentResponse>()));

            // Act
            var result = await _paymentsProcessor.Process(request);

            // Assert
            result.Should().BeOfType<PostPaymentResponse>();
            result.Status.Should().Be(expectedPaymentStatus);
            _verificationServiceMock.Verify(v => v.ValidateRequest(request), Times.Once);
            _bankClientMock.Verify(b => b.SendAsync(It.IsAny<PostBankRequest>()), Times.Once);
            _paymentsRepositoryMock.Verify(p => p.Add(It.IsAny<PostPaymentResponse>()), Times.Once);
        }

        [Theory]
        [AutoData]
        public async Task Process_InvalidRequest_ReturnsPostPaymentResponseWithStatusDeclined(
            List<Error> errors)
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                CardNumber = "1234567890123456",
                ExpiryMonth = 12,
                ExpiryYear = DateTime.Now.AddYears(1).Year,
                Currency = "GBP",
                Amount = 100,
                Cvv = 123
            };
            _verificationServiceMock.Setup(v => v.ValidateRequest(request)).Returns(errors);

            // Act
            var result = await _paymentsProcessor.Process(request);

            // Assert
            result.Should().BeOfType<PostPaymentResponse>();
            result.Status.Should().Be(PaymentStatus.Rejected);
            _verificationServiceMock.Verify(v => v.ValidateRequest(request), Times.Once);
            _bankClientMock.Verify(b => b.SendAsync(It.IsAny<PostBankRequest>()), Times.Never);
            _paymentsRepositoryMock.Verify(p => p.Add(It.IsAny<PostPaymentResponse>()), Times.Once);
        }
    }
}
