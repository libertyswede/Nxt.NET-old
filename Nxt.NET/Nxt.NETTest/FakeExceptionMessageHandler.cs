using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nxt.NETTest
{
    class FakeExceptionMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new HttpRequestException("Unit test exception.");
        }
    }
}
