using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET;
using Nxt.NET.Peer;

namespace Nxt.NETTest.Peer
{
    [TestClass]
    public class PeerConnectorTest
    {
        private Mock<IPeerContainer> _peersMock;
        private Mock<IConfiguration> _configurationMock;
        private PeerConnector _peerConnector;
        private CancellationTokenSource _cancel;
        private int _callbackCount;
        private Mock<IPeer> _connectedPeer;
        private Mock<IPeer> _notConnectedPeer;
        private List<IPeer> _allPeersList;
        private const int LoopCountUntilCancel = 1;

        [TestInitialize]
        public void TestInit()
        {
            _peersMock = new Mock<IPeerContainer>();
            _configurationMock = new Mock<IConfiguration>();
            _connectedPeer = new Mock<IPeer>();
            _notConnectedPeer = new Mock<IPeer>();
            _cancel = new CancellationTokenSource();
            _callbackCount = 0;
            _allPeersList = new List<IPeer>();

            _connectedPeer.SetupGet(c => c.IsConnected).Returns(true);
            _notConnectedPeer.SetupGet(c => c.IsConnected).Returns(false);
            _configurationMock.SetupGet(c => c.TaskSleepInterval).Returns(0);
            _peersMock.Setup(p => p.GetAllPeers()).Returns(_allPeersList).Callback(LoopCallback);

            _allPeersList.Add(_connectedPeer.Object);
            _allPeersList.Add(_connectedPeer.Object);
            _allPeersList.Add(_notConnectedPeer.Object);

            _peerConnector = new PeerConnector(_peersMock.Object, _configurationMock.Object);
        }

        [TestMethod]
        public async Task PeerConnectingTaskShouldNotConnectToMorePeersWhenLimitIsReached()
        {
            _configurationMock.SetupGet(c => c.MaxNumberOfConnectedPublicPeers).Returns(2);
            
            await _peerConnector.PeerConnectingTask(_cancel.Token);

            _notConnectedPeer.Verify(c => c.Connect(), Times.Never);
        }

        [TestMethod]
        public async Task PeerConnectingTaskShouldConnectPeer()
        {
            _configurationMock.SetupGet(c => c.MaxNumberOfConnectedPublicPeers).Returns(3);

            await _peerConnector.PeerConnectingTask(_cancel.Token);

            _notConnectedPeer.Verify(c => c.Connect(), Times.AtLeastOnce);
        }

        private void LoopCallback()
        {
            if (++_callbackCount >= LoopCountUntilCancel)
                _cancel.Cancel();
        }
    }
}
