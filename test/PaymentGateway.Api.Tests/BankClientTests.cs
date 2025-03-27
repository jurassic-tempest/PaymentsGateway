//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;

//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;

//using Moq;

//using PaymentGateway.Api.Clients;
//using PaymentGateway.Api.Models;

//namespace PaymentGateway.Api.Tests;

//public class BankClientTests
//{
//    private readonly HttpClient _httpClient;
//    private readonly ILogger<BankClient> _logger;
//    private readonly IOptions<BankClientOptions> _options;
//    private readonly BankClient _bankClient;

//    public BankClientTests()
//    {
//        _httpClient = new HttpClient(_httpMessageHandlerStub)
//        {
//            BaseAddress = new Uri("https://example.com"),
//        };
//        _logger = Mock<ILogger<BankClient>>();
//        _options = Mock<IOptions<BankClientOptions>>();
//        _bankClient = new BankClient(_httpClient, _logger.Object, _options.Object);
//    }

//    [Fact]
//    public async Task GivenValidRequest_WhenProcessPayment_ThenReturnBankResponse()
//    {
//        // Arrange
//        var bankClient = new BankClient();
//        var request = new PostPaymentRequest
//        {
//            CardNumber = "1234567890123456",
//            ExpiryMonth = 12,
//            ExpiryYear = 2023,
//            Currency = "GBP",
//            Amount = 100,
//            Cvv = 123
//        };

//        // Act
//        var response = await bankClient.ProcessPayment(request);

//        // Assert
//        Assert.NotNull(response);
//        Assert.True(response.Authorized);
//        Assert.NotNull(response.AuthorizationCode);
//    }

//}
