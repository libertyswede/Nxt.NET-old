using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nxt.NETTest.Peer
{
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        private HttpResponseMessage _response;
        public HttpRequestMessage Request { get; private set; }

        public FakeHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        public static FakeHttpMessageHandler GetHttpMessageHandler(string content, HttpStatusCode httpStatusCode)
        {
            var response = CreateResponse(content, httpStatusCode);
            return new FakeHttpMessageHandler(response);
        }

        public void SetResponse(string content, HttpStatusCode httpStatusCode)
        {
            _response = CreateResponse(content, httpStatusCode);
        }

        private static HttpResponseMessage CreateResponse(string content, HttpStatusCode httpStatusCode)
        {
            var memStream = new MemoryStream();

            var sw = new StreamWriter(memStream);
            sw.Write(content);
            sw.Flush();
            memStream.Position = 0;

            var httpContent = new StreamContent(memStream);

            var response = new HttpResponseMessage
            {
                StatusCode = httpStatusCode,
                Content = httpContent
            };
            return response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            var tcs = new TaskCompletionSource<HttpResponseMessage>();
            tcs.SetResult(_response);
            return tcs.Task;
        }
    }
}