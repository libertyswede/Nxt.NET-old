using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nxt.NET;
using Nxt.NET.Request;

namespace Nxt.NETTest.Peer
{
    [TestClass]
    public class PeerTest : FakeHttpClientTestBase
    {
        private NET.Peer.Peer _connectedPeer;
        private FakeHttpMessageHandler _connectedPeerMessageHandler;

        [TestInitialize]
        public void TestInit()
        {
            _connectedPeerMessageHandler = FakeHttpMessageHandler.GetHttpMessageHandler(ValidPeerInfoResponse, HttpStatusCode.OK);
            _connectedPeer = new NET.Peer.Peer("test", "test", new HttpClientFactory(_connectedPeerMessageHandler));
            _connectedPeer.Connect().Wait();
        }

        [TestMethod]
        public async Task ConnectShouldSendPeerInfoRequest()
        {
            var httpMessageHandler = FakeHttpMessageHandler.GetHttpMessageHandler(ValidPeerInfoResponse, HttpStatusCode.OK);
            var peer = new NET.Peer.Peer("test", "test", new HttpClientFactory(httpMessageHandler));

            await peer.Connect();

            Assert.IsNotNull(httpMessageHandler.Request);
        }

        [TestMethod]
        public async Task ConnectShouldFailIfStatusCodeIs404()
        {
            var httpMessageHandler = FakeHttpMessageHandler.GetHttpMessageHandler(ValidPeerInfoResponse, HttpStatusCode.NotFound);
            var peer = new NET.Peer.Peer("test", "test", new HttpClientFactory(httpMessageHandler));

            var result = await peer.Connect();

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task ConnectShouldFailIfUnableToConnect()
        {
            var httpMessageHandler = new FakeExceptionMessageHandler();
            var peer = new NET.Peer.Peer("test", "test", new HttpClientFactory(httpMessageHandler));

            var result = await peer.Connect();

            Assert.IsFalse(result);
        }

        [TestMethod]
        [ExpectedException(typeof(NxtException))]
        public async Task SendRequestWhenNotConnectedShouldThrowException()
        {
            var peer = new NET.Peer.Peer("test", "test", null);
            await peer.SendRequest(new GetPeersRequest());
        }

        [TestMethod]
        public void NewPeerShouldNotBeBlacklisted()
        {
            var peer = new NET.Peer.Peer("test", "test", null);

            Assert.IsFalse(peer.IsBlacklisted);
        }

        [TestMethod]
        public void BlacklistShouldSetIsBlacklisted()
        {
            var peer = new NET.Peer.Peer("test", "test", null);
            peer.Blacklist();

            Assert.IsTrue(peer.IsBlacklisted);
        }
    }
}
