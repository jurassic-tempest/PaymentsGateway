using System.Net;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using FluentAssertions;
using System.Text.Json;
using System.Text.Json.Serialization;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Clients;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PaymentGateway.Api.Models;
using Microsoft.Extensions.Options;

namespace PaymentGateway.Api.IntegrationTests
{
    public class PaymentsTests
    {
        private readonly Random _random = new();

        [Fact]
        public async Task GetPaymentPaymentNotFound_Returns404()
        {
            // Arrange
            var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
            var client = webApplicationFactory.CreateClient();

            // Act
            var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetPayment_PaymentExists_RetrievesPaymentSuccessfully()
        {
            // Arrange
            var payment = new PostPaymentResponse
            {
                Id = Guid.NewGuid(),
                ExpiryYear = _random.Next(2026, 2030),
                ExpiryMonth = _random.Next(1, 12),
                Amount = _random.Next(1, 10000),
                CardNumberLastFour = _random.Next(1111, 9999),
                Currency = "GBP"
            };
            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() }
            };

            var paymentsRepository = new PaymentsRepository();
            paymentsRepository.Add(payment);

            var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
            var client = webApplicationFactory.WithWebHostBuilder(builder =>
                builder.ConfigureServices(services => ((ServiceCollection)services)
                    .AddSingleton(typeof(IPaymentsRepository), paymentsRepository)))
                .CreateClient();

            // Act
            var response = await client.GetAsync($"/api/Payments/{payment.Id}");
            var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>(options);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            paymentResponse.Should().NotBeNull();
        }

        [Theory]
        [InlineData("1234567890123457")]
        [InlineData("1234567890123455")]
        [InlineData("1234567890123453")]
        [InlineData("1234567890123451")]
        public async Task SendPayment_PaymentAuthorized_ReturnsOkWithPaymentStatusAuthorized( string cardNumber)
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
            
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                Converters = { new JsonStringEnumConverter() }
            };

            var client = SetupClient();

            // Act
            var response = await client.PostAsJsonAsync("/api/Payments", request, options);
            var paymentResponseResult = await response.Content.ReadFromJsonAsync<PostPaymentResponse>(options);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            paymentResponseResult.Should().NotBeNull();
            paymentResponseResult.Status.Should().Be(PaymentStatus.Authorized);
        }

        [Theory]
        [InlineData("1234567890123452")]
        [InlineData("1234567890123454")]
        [InlineData("1234567890123456")]
        [InlineData("1234567890123458")]
        public async Task SendPayment_PaymentDeclined_ReturnsOkWithPaymentStatusDeclined(string cardNumber)
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
            
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                Converters = { new JsonStringEnumConverter() }
            };

            var client = SetupClient();

            // Act
            var response = await client.PostAsJsonAsync("/api/Payments", request, options);
            var paymentResponseResult = await response.Content.ReadFromJsonAsync<PostPaymentResponse>(options);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            paymentResponseResult.Should().NotBeNull();
            paymentResponseResult.Status.Should().Be(PaymentStatus.Declined);
        }

        [Theory]
        [InlineData("1234567890123453")]
        public async Task SendPayment_InvalidPayment_ReturnsOkWithPaymentStatusRejected(string cardNumber)
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                CardNumber = cardNumber,
                ExpiryMonth = 12,
                ExpiryYear = DateTime.Now.AddYears(-1).Year,
                Currency = "GBP",
                Amount = 100,
                Cvv = 123
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                Converters = { new JsonStringEnumConverter() }
            };

            var client = SetupClient();

            // Act
            var response = await client.PostAsJsonAsync("/api/Payments", request, options);
            var paymentResponseResult = await response.Content.ReadFromJsonAsync<PostPaymentResponse>(options);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            paymentResponseResult.Should().NotBeNull();
            paymentResponseResult.Status.Should().Be(PaymentStatus.Rejected);
        }

        private static HttpClient SetupClient()
        {
            var httpClient = new HttpClient();
            var paymentsRepository = new PaymentsRepository();
            var bankLogger = new Logger<BankClient>(new NullLoggerFactory());
            var verificationServiceLogger = new Logger<VerificationService>(new NullLoggerFactory());
            var bankClientOptions = new BankClientOptions
            {
                BaseUrl = "http://localhost:8080"
            };
            var verificationService = new VerificationService(verificationServiceLogger);
            var bankClient = new BankClient(httpClient, bankLogger, new OptionsWrapper<BankClientOptions>(bankClientOptions));
            var paymentsProcessor = new PaymentsProcessor(bankClient, new PaymentsRepository(), verificationService);
            var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
            var client = webApplicationFactory.WithWebHostBuilder(builder =>
                builder.ConfigureServices(services =>
                    ((ServiceCollection)services)
                        .AddSingleton<IBankClient, BankClient>()
                        .AddScoped<IVerificationService, VerificationService>()
                        .AddSingleton(typeof(IPaymentsRepository), paymentsRepository)
                        .AddSingleton(typeof(IPaymentsProcessor), paymentsProcessor)))
                .CreateClient();
            return client;
        }
    }
}