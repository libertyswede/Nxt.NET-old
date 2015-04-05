using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nxt.NET.Peer
{
    public interface IPeerSaver
    {
        Task SavePeers(IEnumerable<IPeer> allPeers);
    }

    public class PeerSaver : IPeerSaver
    {
        private readonly IPeerRepository _peerRepository;

        public PeerSaver(IPeerRepository peerRepository)
        {
            _peerRepository = peerRepository;
        }

        public async Task SavePeers(IEnumerable<IPeer> allPeers)
        {
            var dbPeers = await GetDbPeers();
            var currentPeers = GetCurrentValidPeers(allPeers);
            var toDelete = CreateDeleteList(dbPeers, currentPeers);
            var toAdd = CreateAddList(currentPeers, dbPeers);
            await SaveNewPeerAddresses(toAdd);
            await RemoveOldPeerAddresses(toDelete);
        }

        private async Task RemoveOldPeerAddresses(IList<string> toDelete)
        {
            if (toDelete.Any())
                await _peerRepository.RemovePeerAddresses(toDelete);
        }

        private async Task SaveNewPeerAddresses(IList<string> toAdd)
        {
            if (toAdd.Any())
                await _peerRepository.AddPeerAddresses(toAdd);
        }

        private async Task<List<string>> GetDbPeers()
        {
            return (await _peerRepository.GetPeerAddresses()).ToList();
        }

        private static List<string> CreateAddList(IEnumerable<string> currentPeers, List<string> dbPeers)
        {
            var toAdd = new List<string>(currentPeers);
            toAdd.RemoveAll(dbPeers.Contains);
            return toAdd;
        }

        private static List<string> CreateDeleteList(IEnumerable<string> dbPeers, List<string> currentPeers)
        {
            var toDelete = new List<string>(dbPeers);
            toDelete.RemoveAll(currentPeers.Contains);
            return toDelete;
        }

        private static List<string> GetCurrentValidPeers(IEnumerable<IPeer> allPeers)
        {
            return allPeers
                .Where(p => !string.IsNullOrEmpty(p.AnnouncedAddress) && !p.IsBlacklisted)
                .Select(p => p.AnnouncedAddress)
                .ToList();
        }
    }
}
