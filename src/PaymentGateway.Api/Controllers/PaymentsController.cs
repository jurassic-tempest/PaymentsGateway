using System.Net.Mime;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(
    IPaymentsProcessor paymentsProcessor, 
    IPaymentsRepository paymentsRepository) : Controller
{
    [HttpGet("{id:guid}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(PostPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PostPaymentResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = paymentsRepository.Get(id);

        if (payment == null) return new NotFoundResult();

        return new OkObjectResult(payment);
    }

    [HttpPost(Name = "SendPayment")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(PostPaymentResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PostPaymentResponse>> SendPaymentAsync(
        [FromBody] PostPaymentRequest request)
    {
        var response = await paymentsProcessor.Process(request);
        
        return new OkObjectResult(response);
    }
}