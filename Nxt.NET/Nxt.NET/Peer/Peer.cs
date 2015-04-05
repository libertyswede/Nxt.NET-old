using System;
using System.Net.Http;
using System.Threading.Tasks;
using NLog;
using Nxt.NET.Request;
using Nxt.NET.Response;

namespace Nxt.NET.Peer
{
    public interface IPeer
    {
        bool IsConnected { get; }
        string Address { get; }
        string Application { get; }
        string Version { get; }
        string Platform { get; }
        bool ShareAddress { get; }
        string Hallmark { get; }
        string AnnouncedAddress { get; }
        bool IsBlacklisted { get; }
        Task<bool> Connect();
        Task<IGetCumulativeDifficultyResponse> SendRequest(GetCumulativeDifficultyRequest request);
        Task<IGetNextBlockIdsResponse> SendRequest(GetNextBlockIdsRequest request);
        Task<IGetInfoResponse> SendRequest(GetInfoRequest request);
        Task<IGetMilestoneBlockIdsResponse> SendRequest(GetMilestoneBlockIdsRequestBase request);
        Task<IGetNextBlocksResponse> SendRequest(GetNextBlocksRequest request);
        Task<IGetPeersResponse> SendRequest(GetPeersRequest request);
        Task<IGetUnconfirmedTransactionsResponse> SendRequest(GetUnconfirmedTransactionsRequest request);
        void Blacklist();
    }

    public class Peer : IPeer
    {
        public bool IsConnected { get; private set; }
        public string Address { get; private set; }
        public string AnnouncedAddress { get; private set; }
        public bool IsBlacklisted { get { return _blacklistingTime.CompareTo(DateTime.MinValue) > 0; } }
        public Uri HttpAddress { get; private set; }
        public string Application { get; private set; }
        public string Version { get; private set; }
        public string Platform { get; private set; }
        public bool ShareAddress { get; private set; }
        public string Hallmark { get; private set; }
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IHttpClientFactory _httpClientFactory;
        private DateTime _blacklistingTime = DateTime.MinValue;

        public Peer(string address, string announcedAddress, IHttpClientFactory httpClientFactory, int port = 7874)
        {
            _httpClientFactory = httpClientFactory;

            Address = address;
            AnnouncedAddress = announcedAddress;
            HttpAddress = new Uri("http://" + Address + ":" + port + "/nxt");
        }

        public async Task<bool> Connect()
        {
            var response = await SendRequest(new GetInfoRequest());
            if (response == null)
            {
                IsConnected = false;
                return IsConnected;
            }

            var peerInfoResponse = (GetInfoResponse)response;
            Application = peerInfoResponse.Application;
            Version = peerInfoResponse.Version;
            Platform = peerInfoResponse.Platform;
            ShareAddress = peerInfoResponse.ShareAddress;
            Hallmark = peerInfoResponse.Hallmark;
            Logger.Info("Connected to {0} ({1} {2})", Address, Application, Version);
            IsConnected = true;
            return IsConnected;
        }

        public async Task<IGetCumulativeDifficultyResponse> SendRequest(GetCumulativeDifficultyRequest request)
        {
            return await SendRequest((IRequest)request) as IGetCumulativeDifficultyResponse;
        }

        public async Task<IGetNextBlockIdsResponse> SendRequest(GetNextBlockIdsRequest request)
        {
            return await SendRequest((IRequest) request) as IGetNextBlockIdsResponse;
        }

        public async Task<IGetInfoResponse> SendRequest(GetInfoRequest request)
        {
            return await SendRequest((IRequest)request) as IGetInfoResponse;
        }

        public async Task<IGetMilestoneBlockIdsResponse> SendRequest(GetMilestoneBlockIdsRequestBase request)
        {
            return await SendRequest((IRequest)request) as IGetMilestoneBlockIdsResponse;
        }

        public async Task<IGetNextBlocksResponse> SendRequest(GetNextBlocksRequest request)
        {
            return await SendRequest((IRequest)request) as IGetNextBlocksResponse;
        }

        public async Task<IGetPeersResponse> SendRequest(GetPeersRequest request)
        {
            return await SendRequest((IRequest)request) as IGetPeersResponse;
        }

        public async Task<IGetUnconfirmedTransactionsResponse> SendRequest(GetUnconfirmedTransactionsRequest request)
        {
            return await SendRequest((IRequest)request) as IGetUnconfirmedTransactionsResponse;
        }

        private async Task<object> SendRequest(IRequest request)
        {
            if (!IsConnected && request.GetType() != typeof(GetInfoRequest))
                throw new NxtException("Not connected.");

            var httpResponse = await Send(request);

            if (httpResponse == null || !httpResponse.IsSuccessStatusCode)
                return null;

            var jsonResponse = await httpResponse.Content.ReadAsStringAsync();
            Logger.Debug("Recieved from {0}: {1}", Address, jsonResponse);
            return request.ParseResponse(jsonResponse);
        }

        public void Blacklist()
        {
            _blacklistingTime = DateTime.UtcNow;
        }

        private async Task<HttpResponseMessage> Send(IRequest request)
        {
            try
            {
                using (var webClient = _httpClientFactory.CreateClient())
                {
                    var response = await webClient.PostAsJsonAsync(HttpAddress, request);
                    return response;
                }
            }
            catch (HttpRequestException e)
            {
                Logger.DebugException(string.Format("{0} is not online.", Address), e);
            }
            catch (TaskCanceledException e)
            {
                Logger.DebugException(string.Format("{0} is not online.", Address), e);
            }
            IsConnected = false;
            return null;
        }
    }
}
