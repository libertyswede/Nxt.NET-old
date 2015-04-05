using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Nxt.NET
{
    public class HttpLogHandler : DelegatingHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public HttpLogHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Logger.Debug(string.Format("Sending HTTP request to {0}", request.RequestUri.AbsoluteUri));
            //request.Headers.ToList().ForEach(h => Logger.Debug("Request Header: " + h));
            //if (request.Content != null)
            //{
            //    Logger.Debug("Request content: " + await request.Content.ReadAsStringAsync());
            //}

            var response = await base.SendAsync(request, cancellationToken);

            Logger.Debug(string.Format("Recieved HTTP response from {0}: {1} {2}", response.RequestMessage.RequestUri.AbsoluteUri, (int)response.StatusCode, response.ReasonPhrase));
            response.Headers.ToList().ForEach(h => Logger.Debug(string.Format("Response Header: {0}: {1}", h.Key, string.Join(", ", h.Value))));
            //if (response.Content != null)
            //{
            //    Logger.Debug("Response content: " + await response.Content.ReadAsStringAsync());
            //}

            return response;
        }
    }
}
