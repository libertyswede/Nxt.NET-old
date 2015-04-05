using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Nxt.NET.Peer
{
    public interface IPeerConnector
    {
        Task PeerConnectingTask(CancellationToken token);
    }

    public class PeerConnector : IPeerConnector
    {
        private readonly IPeerContainer _peerContainer;
        private readonly IConfiguration _configuration;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public PeerConnector(IPeerContainer peerContainer, IConfiguration configuration)
        {
            _peerContainer = peerContainer;
            _configuration = configuration;
        }

        public async Task PeerConnectingTask(CancellationToken token)
        {
            Logger.Info("Starting Peer connecting task");
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(_configuration.TaskSleepInterval, token);
                var allPeers = _peerContainer.GetAllPeers();
                if (ConnectedPeersLimitReached(allPeers))
                    continue;

                var peer = GetRandomUnConnectedPeer(allPeers);
                if (peer == null)
                    continue;

                await peer.Connect();
            }
        }

        private static IPeer GetRandomUnConnectedPeer(IEnumerable<IPeer> allPeers)
        {
            return allPeers.RandomizedSingleOrDefault(p => !p.IsConnected);
        }

        private bool ConnectedPeersLimitReached(IEnumerable<IPeer> allPeers)
        {
            return allPeers.Count(p => p.IsConnected) >= _configuration.MaxNumberOfConnectedPublicPeers;
        }
    }
}
