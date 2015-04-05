using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Nxt.NET.Request;

namespace Nxt.NET.Peer
{
    public interface IPeerAddressFetcher
    {
        Task GetMorePeersTask(CancellationToken token);
    }

    public class PeerAddressFetcher : IPeerAddressFetcher
    {
        private readonly IConfiguration _configuration;
        private readonly IPeerContainer _peerContainer;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public PeerAddressFetcher(IConfiguration configuration, IPeerContainer peerContainer)
        {
            _configuration = configuration;
            _peerContainer = peerContainer;
        }

        public async Task GetMorePeersTask(CancellationToken token)
        {
            Logger.Info("Starting Get more peers task");
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(_configuration.TaskSleepInterval, token);

                var peer = GetRandomConnectedPeer();
                if (peer == null)
                    continue;

                var response = await peer.SendRequest(new GetPeersRequest());
                if (response == null || !response.PeerAddresses.Any())
                    continue;

                response.PeerAddresses.ToList().ForEach(async a => await _peerContainer.Add(a));
                await _peerContainer.SavePeers();
            }
        }

        private IPeer GetRandomConnectedPeer()
        {
            return _peerContainer.GetAllPeers().RandomizedSingleOrDefault(p => p.IsConnected);
        }
    }
}
