using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET;
using Nxt.NET.Peer;
using Nxt.NET.Request;
using Nxt.NET.Response;

namespace Nxt.NETTest.Peer
{
    [TestClass]
    public class PeerAddressFetcherTest
    {
        private Mock<IConfiguration> _configurationMock;
        private Mock<IPeerContainer> _peersMock;
        private PeerAddressFetcher _peerAddressFetcher;
        private Mock<IPeer> _connectedPeerMock;
        private CancellationTokenSource _cancel;

        [TestInitialize]
        public void TestInit()
        {
            _configurationMock = new Mock<IConfiguration>();
            _peersMock = new Mock<IPeerContainer>();
            _connectedPeerMock = new Mock<IPeer>();
            _cancel = new CancellationTokenSource();

            _peersMock.Setup(p => p.SavePeers()).Callback(_cancel.Cancel);
            _configurationMock.SetupGet(c => c.TaskSleepInterval).Returns(0);
            _peersMock.Setup(p => p.GetAllPeers()).Returns(new List<IPeer> {_connectedPeerMock.Object});
            _connectedPeerMock.SetupGet(c => c.IsConnected).Returns(true);
            _peerAddressFetcher = new PeerAddressFetcher(_configurationMock.Object, _peersMock.Object);
        }

        [TestMethod]
        public void GetMorePeersTask()
        {
            var getPeersResponseMock = new Mock<IGetPeersResponse>();
            getPeersResponseMock.SetupGet(r => r.PeerAddresses)
                .Returns(new ReadOnlyCollection<string>(new[] {"1.2.3.4"}));
            _connectedPeerMock.Setup(p => p.SendRequest(It.IsAny<GetPeersRequest>()))
                .ReturnsAsync(getPeersResponseMock.Object);

            // ReSharper disable once CSharpWarnings::CS4014
            _peerAddressFetcher.GetMorePeersTask(_cancel.Token);
            _peersMock.Verify(p => p.Add(It.Is<string>(address => address.Equals("1.2.3.4"))), Times.Once);
        }
    }
}
