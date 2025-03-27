using AutoFixture.Xunit2;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTests
{
    private readonly Random _random = new();
    private readonly Mock<IPaymentsProcessor> _paymentsProcessorMock;
    private readonly Mock<IPaymentsRepository> _paymentsRepositoryMock;
    private readonly PaymentsController _paymentsController;

    public PaymentsControllerTests()
    {
        _paymentsProcessorMock = new Mock<IPaymentsProcessor>();
        _paymentsRepositoryMock = new Mock<IPaymentsRepository>();
        _paymentsController = new PaymentsController(
            _paymentsProcessorMock.Object, _paymentsRepositoryMock.Object);
    }

    [Theory]
    [AutoData]
    public async Task GetPaymentAsync_RetrievesAPaymentSuccessfully(
        Guid guid, PostPaymentResponse postPaymentResponse)
    {
        // Arrange
        _paymentsRepositoryMock.Setup(x => x.Get(guid)).Returns(postPaymentResponse);

        // Act
        var result = await _paymentsController.GetPaymentAsync(guid);
        
        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        result.Result.As<OkObjectResult>().Value.Should().Be(postPaymentResponse);
    }

    [Theory]
    [AutoData]
    public async Task GetPaymentAsync_Returns404IfPaymentNotFound(Guid guid)
    {
        // Arrange
        _paymentsRepositoryMock.Setup(x => x.Get(guid)).Returns((PostPaymentResponse)null);

        // Act
        var result = await _paymentsController.GetPaymentAsync(guid);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Theory]
    [AutoData]
    public async Task SendPaymentAsync_ReturnsOkIfPaymentAuthorized(
        PostPaymentRequest postPaymentRequest, PostPaymentResponse postPaymentResponse)
    {
        // Arrange
        _paymentsProcessorMock.Setup(x => x.Process(postPaymentRequest)).ReturnsAsync(postPaymentResponse);

        // Act
        var result = await _paymentsController.SendPaymentAsync(postPaymentRequest);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        result.Result.As<OkObjectResult>().Value.Should().Be(postPaymentResponse);
    }
}