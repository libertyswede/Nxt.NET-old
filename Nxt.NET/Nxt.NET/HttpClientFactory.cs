using System.Net.Http;

namespace Nxt.NET
{
    public interface IHttpClientFactory
    {
        HttpClient CreateClient();
    }

    public class HttpClientFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _httpMessageHandler;

        public HttpClientFactory(HttpMessageHandler httpMessageHandler = null)
        {
            _httpMessageHandler = httpMessageHandler;
        }

        public HttpClient CreateClient()
        {
            return _httpMessageHandler == null ? new HttpClient() : new HttpClient(_httpMessageHandler);
        }
    }
}