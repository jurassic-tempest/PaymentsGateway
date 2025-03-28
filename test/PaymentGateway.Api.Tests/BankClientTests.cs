using System.Net;
using System.Net.Mime;
using System.Text;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Exceptions;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Tests.TestHelpers;

namespace PaymentGateway.Api.Tests;

public class BankClientTests
{
    private readonly HttpMessageHandlerStub _httpMessageHandlerStub;
    private readonly HttpClient _httpClient;
    private readonly Mock<ILogger<BankClient>> _loggerMock;
    private readonly Mock<IOptions<BankClientOptions>> _optionsMock;
    private readonly BankClient _bankClient;

    public BankClientTests()
    {
        _httpMessageHandlerStub = new HttpMessageHandlerStub();
        _httpClient = new HttpClient(_httpMessageHandlerStub)
        {
            BaseAddress = new Uri("https://example.com"),
        };
        _loggerMock = new Mock<ILogger<BankClient>>();
        _optionsMock = new Mock<IOptions<BankClientOptions>>();
        _optionsMock.Setup(x => x.Value).Returns(new BankClientOptions { BaseUrl = "https://example.com" });
        _bankClient = new BankClient(_httpClient, _loggerMock.Object, _optionsMock.Object);
    }

    [Fact]
    public async Task SendAsync_ValidRequest_ThenReturnsBankResponse()
    {
        // Arrange
        var request = new PostBankRequest
        {
            CardNumber = "1234567890123457",
            ExpiryDate = "07/2025",
            Currency = "GBP",
            Amount = 100,
            Cvv = 123
        };
        var expectedResponse = @"
        {
            ""authorized"" : true,
            ""authorization_code"" : ""320bf51d-65c7-433b-9853-b9cf74671a02""
        }";

        _httpMessageHandlerStub.Send = (message, cancellationToken) =>
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(expectedResponse, Encoding.UTF8, MediaTypeNames.Application.Json),
            };
        };

        // Act
        var response = await _bankClient.SendAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Authorized.Should().BeTrue();
        response.AuthorizationCode.Should().NotBeNull();
    }

    [Fact]
    public async Task SendAsync_BankThrowsError_ThrowsABankException()
    {
        // Arrange
        var request = new PostBankRequest
        {
            CardNumber = "1234567890123450",
            ExpiryDate = "07/2025",
            Currency = "GBP",
            Amount = 100,
            Cvv = 123
        };

        _httpMessageHandlerStub.Send = (message, cancellationToken) =>
        {
            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, "/"),
            };
        };

        // Act
        var act = async () => await _bankClient.SendAsync(request);

        // Assert
        act.Should().ThrowAsync<BankException>();
    }

}
