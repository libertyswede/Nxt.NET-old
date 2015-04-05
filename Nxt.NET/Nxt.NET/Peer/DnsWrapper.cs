using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nxt.NET.Peer
{
    public interface IDnsWrapper
    {
        Task<string> GetIpAddress(string hostNameOrAddress);
    }

    public class DnsWrapper : IDnsWrapper
    {
        private readonly Regex _ipv4Regex = new Regex(@"[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+");
        public async Task<string> GetIpAddress(string hostNameOrAddress)
        {
            var hostEntry = await Dns.GetHostEntryAsync(hostNameOrAddress);
            if (hostEntry == null) 
                return hostNameOrAddress;

            var address = hostEntry.AddressList.Select(a => a.ToString()).FirstOrDefault(s => _ipv4Regex.Match(s).Success);
            return address ?? hostNameOrAddress;
        }
    }
}