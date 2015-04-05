using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET;
using Nxt.NET.Peer;

namespace Nxt.NETTest.Peer
{
    [TestClass]
    public class PeerSaverTest
    {
        private PeerSaver _peerSaver;
        private Mock<IPeerRepository> _repositoryMock;
        private List<string> _repositoryAddresses;
        private Mock<IPeer> _peerMock;
        private const string Expected = "abc123";

        [TestInitialize]
        public void TestInit()
        {
            _repositoryMock = new Mock<IPeerRepository>();
            _repositoryAddresses = new List<string>();
            _repositoryMock.Setup(r => r.GetPeerAddresses()).ReturnsAsync(_repositoryAddresses);

            _peerMock = new Mock<IPeer>();
            _peerMock.SetupGet(p => p.AnnouncedAddress).Returns(Expected);
            _peerMock.SetupGet(p => p.IsBlacklisted).Returns(true);

            _peerSaver = new PeerSaver(_repositoryMock.Object);
        }

        [TestMethod]
        public async Task SavePeersShouldAddNewPeerAddress()
        {
            _peerMock.SetupGet(p => p.IsBlacklisted).Returns(false);
            
            await _peerSaver.SavePeers(new[] { _peerMock.Object });

            _repositoryMock.Verify(r => r.AddPeerAddresses(It.Is<List<string>>(addresses =>
                addresses.Count == 1 && addresses.First().Equals(Expected))));
            _repositoryMock.Verify(r => r.RemovePeerAddresses(It.IsAny<List<string>>()), Times.Never());
        }

        [TestMethod]
        public async Task SavePeersShouldIgnoreBlacklistedPeers()
        {
            await _peerSaver.SavePeers(new[] { _peerMock.Object });

            _repositoryMock.Verify(r => r.RemovePeerAddresses(It.IsAny<List<string>>()), Times.Never());
            _repositoryMock.Verify(r => r.AddPeerAddresses(It.IsAny<List<string>>()), Times.Never());
        }

        [TestMethod]
        public async Task SavePeersShouldIgnoreEmptyAnnouncedAddress()
        {
            _peerMock.SetupGet(p => p.AnnouncedAddress).Returns(string.Empty);

            await _peerSaver.SavePeers(new[] { _peerMock.Object });

            _repositoryMock.Verify(r => r.RemovePeerAddresses(It.IsAny<List<string>>()), Times.Never());
            _repositoryMock.Verify(r => r.AddPeerAddresses(It.IsAny<List<string>>()), Times.Never());
        }

        [TestMethod]
        public async Task SavePeersShouldRemoveOldPeerAddress()
        {
            _repositoryAddresses.Add(Expected);

            await _peerSaver.SavePeers(new List<IPeer>());

            _repositoryMock.Verify(r => r.RemovePeerAddresses(It.Is<List<string>>(addresses =>
                addresses.Count == 1 && addresses.First().Equals(Expected))));
            _repositoryMock.Verify(r => r.AddPeerAddresses(It.IsAny<List<string>>()), Times.Never());
        }
    }
}
