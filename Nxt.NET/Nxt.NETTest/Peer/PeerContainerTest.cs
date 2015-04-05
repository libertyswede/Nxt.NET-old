using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET;
using Nxt.NET.Peer;

namespace Nxt.NETTest.Peer
{
    [TestClass]
    public class PeerContainerTest : FakeHttpClientTestBase
    {
        private PeerContainer _peerContainer;
        private FakeHttpMessageHandler _connectedPeerMessageHandler;
        private Mock<IDnsWrapper> _dnsWrapper;
        private Mock<IPeerSaver> _peerSaverMock;

        [TestInitialize]
        public void TestInit()
        {
            _connectedPeerMessageHandler = FakeHttpMessageHandler.GetHttpMessageHandler(ValidPeerInfoResponse, HttpStatusCode.OK);
            _dnsWrapper = new Mock<IDnsWrapper>();
            _peerSaverMock = new Mock<IPeerSaver>();
            _peerContainer = new PeerContainer(new HttpClientFactory(_connectedPeerMessageHandler), _dnsWrapper.Object,
                _peerSaverMock.Object);
        }

        [TestMethod]
        public void InitShouldAddAllAddressesInRepository()
        {
            _dnsWrapper.Setup(dns => dns.GetIpAddress(It.IsAny<string>())).Throws(new SocketException());

            _peerContainer.Init(new List<string> { "test1", "test2", "test3" });
            Thread.Sleep(50); // Ugly hack to give Peers internal Tasks some time to complete

            Assert.AreEqual(3, _peerContainer.GetAllPeers().Count);
        }

        [TestMethod]
        public async Task AddEmptyAddressShouldReturnNull()
        {
            var peer = await _peerContainer.Add("");

            Assert.IsNull(peer);
            Assert.AreEqual(0, _peerContainer.GetAllPeers().Count);
        }

        [TestMethod]
        public async Task AddLocalhostAddressShouldReturnNull()
        {
            var peer1 = await _peerContainer.Add("127.0.0.1");
            var peer2 = await _peerContainer.Add("localhost");
            var peer3 = await _peerContainer.Add("0.0.0.0");

            Assert.IsNull(peer1);
            Assert.IsNull(peer2);
            Assert.IsNull(peer3);
            Assert.AreEqual(0, _peerContainer.GetAllPeers().Count);
        }

        [TestMethod]
        public async Task AddPeerShouldReturnPeerWithSameAddress()
        {
            const string announcedAddress = "test";
            _dnsWrapper.Setup(dns => dns.GetIpAddress(announcedAddress)).Throws(new SocketException());

            var peer = await _peerContainer.Add(announcedAddress);

            Assert.AreEqual(announcedAddress, peer.Address);
        }

        [TestMethod]
        public async Task AddPeerShouldReturnAlreadyExistingPeer()
        {
            const string announcedAddress = "test";
            _dnsWrapper.Setup(dns => dns.GetIpAddress(announcedAddress)).ReturnsAsync(announcedAddress);
            var expected = await _peerContainer.Add(announcedAddress);

            var actual = await _peerContainer.Add(announcedAddress);
            Assert.AreSame(expected, actual);
        }

        [TestMethod]
        public async Task AddPeerShouldReturnPeerWithAftonbladetHost()
        {
            const string announcedAddress = "aftonbladet.se";
            const string expected = "144.63.250.10";
            _dnsWrapper.Setup(dns => dns.GetIpAddress(announcedAddress)).ReturnsAsync(expected);

            var peer = await _peerContainer.Add(announcedAddress);

            Assert.AreEqual(peer.Address, expected);
        }

        [TestMethod]
        public async Task AddPeerWithPortAndInvalidIpShouldParsePort()
        {
            const string announcedAddress = "1.2.3.4:80";
            const int port = 80;

            _dnsWrapper.Setup(dns => dns.GetIpAddress("1.2.3.4")).Throws(new SocketException());
            var peer = await _peerContainer.Add(announcedAddress);

            StringAssert.EndsWith(peer.AnnouncedAddress, port.ToString(CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public async Task AddPeerWithPortShouldParsePort()
        {
            const string announcedAddress = "1.2.3.4:80";
            const int port = 80;

            _dnsWrapper.Setup(dns => dns.GetIpAddress("1.2.3.4")).ReturnsAsync("1.2.3.4");
            var peer = await _peerContainer.Add(announcedAddress);

            StringAssert.EndsWith(peer.AnnouncedAddress, port.ToString(CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public async Task AddedPeerShouldBePossibleToGet()
        {
            const string announcedAddress = "test";
            _dnsWrapper.Setup(dns => dns.GetIpAddress(announcedAddress)).Throws(new SocketException());

            var addedPeer = await _peerContainer.Add(announcedAddress);
            var gottenPeer = _peerContainer.GetPeer(addedPeer.Address);

            Assert.AreSame(addedPeer, gottenPeer);
        }

        [TestMethod]
        public void GetPeerShouldReturnNull()
        {
            var peer = _peerContainer.GetPeer("nonexisting");

            Assert.IsNull(peer);
        }

        [TestMethod]
        public async Task SavePeersShouldSavePeer()
        {
            _dnsWrapper.Setup(dns => dns.GetIpAddress("test")).ReturnsAsync("test");
            await _peerContainer.Add("test");

            await _peerContainer.SavePeers();

            // ReSharper disable PossibleMultipleEnumeration
            _peerSaverMock.Verify(s => s.SavePeers(It.Is<IEnumerable<IPeer>>(peers => peers.Count() == 1 && peers.Single().AnnouncedAddress == "test")));
            // ReSharper restore PossibleMultipleEnumeration
        }
    }
}
