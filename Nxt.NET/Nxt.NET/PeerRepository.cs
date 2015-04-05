using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Nxt.NET
{
    public interface IPeerRepository
    {
        Task<IList<string>> GetPeerAddresses();
        Task AddPeerAddresses(IList<string> peerAddresses);
        Task RemovePeerAddresses(IList<string> peerAddresses);
    }

    public class PeerRepository : RepositoryBase, IPeerRepository
    {
        public PeerRepository(IDbController dbController) : base(dbController)
        {
        }

        public async Task<IList<string>> GetPeerAddresses()
        {
            var list = new List<string>();
            try
            {
                using (var cmd = Connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT address FROM peer";
                    var peerReader = await cmd.ExecuteReaderAsync();

                    while (await peerReader.ReadAsync())
                    {
                        list.Add(peerReader.GetString(0));
                    }
                }
            }
            catch (Exception e)
            {
                Logger.FatalException("Unable to read peers from local database", e);
            }
            return list;
        }

        public async Task AddPeerAddresses(IList<string> peerAddresses)
        {
            try
            {
                using (var cmd = Connection.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO peer (address) VALUES (@address)";
                    foreach (var peerAddress in peerAddresses)
                    {
                        cmd.Parameters.Add("@address", DbType.String).Value = peerAddress;
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.FatalException("Unable to save peer addresses to local database", e);
            }
        }

        public async Task RemovePeerAddresses(IList<string> peerAddresses)
        {
            try
            {
                using (var cmd = Connection.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM peer WHERE address = @address";
                    foreach (var peerAddress in peerAddresses)
                    {
                        cmd.Parameters.Add("@address", DbType.String).Value = peerAddress;
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.FatalException("Unable to remove peer addresses from local database", e);
            }
        }
    }
}