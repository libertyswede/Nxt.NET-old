using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NLog;

namespace Nxt.NET.Peer
{
    public interface IPeerContainer
    {
        void Init(IList<string> peerAddresses);
        Task<IPeer> Add(string announcedAddress);
        IPeer GetPeer(string address);
        IList<IPeer> GetAllPeers();
        Task SavePeers();
    }

    public class PeerContainer : IPeerContainer
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDnsWrapper _dnsWrapper;
        private readonly IPeerSaver _peerSaver;
        private readonly ConcurrentDictionary<string, IPeer> _peers = new ConcurrentDictionary<string, IPeer>();
        private static readonly Regex PortRegex = new Regex("(.*):([0-9]+)");
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public PeerContainer(IHttpClientFactory httpClientFactory, IDnsWrapper dnsWrapper, IPeerSaver peerSaver)
        {
            _httpClientFactory = httpClientFactory;
            _dnsWrapper = dnsWrapper;
            _peerSaver = peerSaver;
        }

        public void Init(IList<string> peerAddresses)
        {
            Task.Factory.StartNew(() => Task.WaitAll(peerAddresses.Select(Add).ToArray<Task>()));
        }

        public async Task<IPeer> Add(string announcedAddress)
        {
            if (IsInvalidAddress(announcedAddress))
                return null;

            var portAndHost = ParseAnnouncedAddress(announcedAddress);
            var port = portAndHost.Item2;
            var ipAddress = await TryGetIpAddress(portAndHost.Item1);
            IPeer peer;
            if (_peers.TryGetValue(ipAddress, out peer)) 
                return peer;

            try
            {
                peer = port.HasValue
                    ? new Peer(ipAddress, announcedAddress, _httpClientFactory, port.Value)
                    : new Peer(ipAddress, announcedAddress, _httpClientFactory);
            }
            catch (UriFormatException e)
            {
                Logger.WarnException("Invalid peer address or port: " + announcedAddress, e);
                return null;
            }
            _peers[ipAddress] = peer;
            return peer;
        }

        private static bool IsInvalidAddress(string announcedAddress)
        {
            return string.IsNullOrEmpty(announcedAddress) || announcedAddress.Equals("127.0.0.1") ||
                   announcedAddress.Equals("localhost") || announcedAddress.Equals("0.0.0.0");
        }

        private static Tuple<string, int?> ParseAnnouncedAddress(string announcedAddress)
        {
            int? port = null;
            var hostname = announcedAddress;
            var match = PortRegex.Match(announcedAddress);
            if (match.Success)
            {
                port = Int32.Parse(match.Groups[2].ToString());
                hostname = match.Groups[1].ToString();
            }

            return new Tuple<string, int?>(hostname, port);
        }

        private async Task<string> TryGetIpAddress(string announcedAddress)
        {
            var hostAddress = announcedAddress.Trim();
            try
            {
                hostAddress = await _dnsWrapper.GetIpAddress(hostAddress);
            }
            catch (SocketException)
            {
                // Ignore
            }
            return hostAddress;
        }

        public IPeer GetPeer(string address)
        {
            IPeer peer;
            _peers.TryGetValue(address, out peer);
            return peer;
        }

        public IList<IPeer> GetAllPeers()
        {
            return _peers.Select(kvp => kvp.Value).ToList();
        }

        public async Task SavePeers()
        {
            await _peerSaver.SavePeers(GetAllPeers());
        }
    }
}