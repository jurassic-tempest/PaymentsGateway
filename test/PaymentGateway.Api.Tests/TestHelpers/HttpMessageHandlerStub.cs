namespace PaymentGateway.Api.Tests.TestHelpers
{
    public class HttpMessageHandlerStub : HttpMessageHandler
    {
        public new Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> Send { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(Send(request, cancellationToken));
    }
}
