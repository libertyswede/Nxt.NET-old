using System;
using NLog;

namespace Nxt.NET
{
    public class DbVersion
    {
        public const int SupportedVersion = 12; // NRS version 52
        public int Version { get; private set; }
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IDbController _dbController;
        private readonly IConfiguration _configuration;

        public DbVersion(IDbController dbController, IConfiguration configuration)
        {
            _dbController = dbController;
            _configuration = configuration;
        }

        public void Init()
        {
            GetDbVersion();
            while (Version < SupportedVersion)
                Update();
        }

        private void GetDbVersion()
        {
            try
            {
                var connection = _dbController.GetConnection();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS version (db_version INT NOT NULL)";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "SELECT db_version FROM version";
                    var version = cmd.ExecuteScalar();
                    if (version == null)
                    {
                        const int initialVersion = 0;
                        cmd.CommandText = string.Format("INSERT INTO version VALUES ({0})", initialVersion);
                        cmd.ExecuteNonQuery();
                        Version = initialVersion;
                    }
                    else
                    {
                        Version = Int32.Parse(version.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Logger.FatalException("Unable to initiate SQLite DB", e);
                throw;
            }
        }
        private void Update()
        {
            switch (Version + 1)
            {
                case 1:
                    ApplyUpdate("CREATE TABLE IF NOT EXISTS block" +
                        "(" +
                            "id INTEGER NOT NULL," +
                            "version INTEGER NOT NULL," +
                            "timestamp INTEGER NOT NULL," +
                            "previous_block_id INTEGER," +
                            "total_amount INTEGER NOT NULL," +
                            "total_fee INTEGER NOT NULL," +
                            "payload_length INTEGER NOT NULL," +
                            "generator_public_key BLOB NOT NULL," +
                            "previous_block_hash BLOB," +
                            "cumulative_difficulty BLOB NOT NULL," +
                            "base_target INTEGER NOT NULL," +
                            "next_block_id INTEGER," +
                            "height INTEGER NOT NULL," +
                            "generation_signature BLOB NOT NULL," +
                            "block_signature BLOB NOT NULL," +
                            "payload_hash BLOB NOT NULL," +
                            "generator_id INTEGER NOT NULL," +
                            "FOREIGN KEY(previous_block_id) REFERENCES block(rowid) ON DELETE CASCADE," +
                            "FOREIGN KEY(next_block_id) REFERENCES block(rowid) ON DELETE SET NULL" +
                        ")");
                    break;
                case 2:
                    ApplyUpdate("CREATE UNIQUE INDEX IF NOT EXISTS block_id_idx ON block (id)");
                    break;
                case 3:
                    ApplyUpdate("CREATE UNIQUE INDEX IF NOT EXISTS block_height_idx ON block (height)");
                    break;
                case 4:
                    ApplyUpdate("CREATE INDEX IF NOT EXISTS block_generator_id_idx ON block (generator_id)");
                    break;
                case 5:
                    ApplyUpdate("CREATE TABLE IF NOT EXISTS peer (address)");
                    break;
                case 6:
                    ApplyUpdate("CREATE TABLE IF NOT EXISTS [transaction]" +
                        "(" +
	                        "id INTEGER NOT NULL," +
	                        "deadline INTEGER NOT NULL," +
	                        "sender_public_key BLOB NOT NULL," +
	                        "recipient_id INTEGER NOT NULL," +
	                        "amount INTEGER NOT NULL," +
	                        "fee INTEGER NOT NULL," +
	                        "height INTEGER NOT NULL," +
	                        "block_id INTEGER NOT NULL," +
	                        "signature BLOB NOT NULL, " +
	                        "timestamp INTEGER NOT NULL, " +
	                        "block_timestamp INTEGER NOT NULL," +
	                        "type INTEGER NOT NULL, " +
	                        "subtype INTEGER NOT NULL, " +
	                        "sender_id INTEGER NOT NULL, " +
	                        "full_hash BLOB NOT NULL," +
	                        "referenced_transaction_full_hash BLOB," +
	                        "attachment_bytes BLOB," +
	                        "FOREIGN KEY (block_id) REFERENCES block (rowid) ON DELETE CASCADE" +
                        ")");
                    break;
                case 7:
                    ApplyUpdate("CREATE UNIQUE INDEX IF NOT EXISTS transaction_id_idx ON [transaction] (id)");
                    break;
                case 8:
                    ApplyUpdate("CREATE UNIQUE INDEX IF NOT EXISTS transaction_full_hash_idx ON [transaction] (full_hash)");
                    break;
                case 9:
                    ApplyUpdate("CREATE INDEX IF NOT EXISTS transaction_timestamp_idx ON [transaction] (timestamp)");
                    break;
                case 10:
                    ApplyUpdate("CREATE INDEX IF NOT EXISTS transaction_sender_id_idx ON [transaction] (sender_id)");
                    break;
                case 11:
                    ApplyUpdate("CREATE INDEX IF NOT EXISTS transaction_recipient_id_idx ON [transaction] (recipient_id)");
                    break;
                case 12:
                    if (!_configuration.IsTestnet)
                    {
                        ApplyUpdate("INSERT INTO peer (address) VALUES " +
                                    "('samson.vps.nxtcrypto.org'), ('210.70.137.216:80'), ('178.15.99.67'), ('114.215.130.79'), " +
                                    "('85.25.198.120'), ('82.192.39.33'), ('84.242.91.139'), ('178.33.203.157'), ('bitsy08.vps.nxtcrypto.org'), " +
                                    "('oldnbold.vps.nxtcrypto.org'), ('46.28.111.249'), ('vps8.nxtcrypto.org'), ('95.24.81.240'), " +
                                    "('85.214.222.82'), ('188.226.179.119'), ('54.187.153.45'), ('89.176.190.43'), ('nxtpi.zapto.org'), " +
                                    "('89.70.254.145'), ('wallet.nxtty.com'), ('95.85.24.151'), ('95.188.237.51'), ('nrs02.nxtsolaris.info'), " +
                                    "('fsom.no-ip.org'), ('nrs01.nxtsolaris.info'), ('allbits.vps.nxtcrypto.org'), ('mycrypto.no-ip.biz'), " +
                                    "('72.14.177.42'), ('bitsy03.vps.nxtcrypto.org'), ('199.195.148.27'), ('bitsy06.vps.nxtcrypto.org'), " +
                                    "('188.226.139.71'), ('enricoip.no-ip.biz'), ('54.200.196.116'), ('24.161.110.115'), ('88.163.78.131'), " +
                                    "('vps12.nxtcrypto.org'), ('vps10.nxtcrypto.org'), ('bitsy09.vps.nxtcrypto.org'), ('nxtnode.noip.me'), " +
                                    "('49.245.183.103'), ('xyzzyx.vps.nxtcrypto.org'), ('nxt.ravensbloodrealms.com'), ('nxtio.org'), " +
                                    "('67.212.71.173'), ('xeqtorcreed2.vps.nxtcrypto.org'), ('195.154.127.172'), ('vps11.nxtcrypto.org'), " +
                                    "('184.57.30.220'), ('213.46.57.77'), ('162.243.159.190'), ('188.138.88.154'), ('178.150.207.53'), " +
                                    "('54.76.203.25'), ('146.185.168.129'), ('107.23.118.157'), ('bitsy04.vps.nxtcrypto.org'), " +
                                    "('nxt.alkeron.com'), ('23.88.229.194'), ('23.88.59.40'), ('77.179.121.91'), ('58.95.145.117'), " +
                                    "('188.35.156.10'), ('166.111.77.95'), ('pakisnxt.no-ip.org'), ('81.4.107.191'), ('192.241.190.156'), " +
                                    "('69.141.139.8'), ('nxs2.hanza.co.id'), ('bitsy01.vps.nxtcrypto.org'), ('209.126.73.158'), " +
                                    "('nxt.phukhew.com'), ('89.250.240.63'), ('cryptkeeper.vps.nxtcrypto.org'), ('54.213.122.21'), " +
                                    "('zobue.com'), ('91.69.121.229'), ('vps6.nxtcrypto.org'), ('54.187.49.62'), ('vps4.nxtcrypto.org'), " +
                                    "('80.86.92.139'), ('109.254.63.44'), ('nxtportal.org'), ('89.250.243.200'), ('nxt.olxp.in'), " +
                                    "('46.194.184.161'), ('178.63.69.203'), ('nxt.sx'), ('185.4.72.115'), ('178.198.145.191'), " +
                                    "('panzetti.vps.nxtcrypto.org'), ('miasik.no-ip.org'), ('screenname.vps.nxtcrypto.org'), ('87.230.14.1'), " +
                                    "('nacho.damnserver.com'), ('87.229.77.126'), ('bitsy05.vps.nxtcrypto.org'), ('lyynx.vps.nxtcrypto.org'), " +
                                    "('209.126.73.156'), ('62.57.115.23'), ('66.30.204.105'), ('vps1.nxtcrypto.org'), " +
                                    "('cubie-solar.mjke.de:7873'), ('192.99.212.121'), ('109.90.16.208')");
                    }
                    else
                    {
                        ApplyUpdate("INSERT INTO peer (address) VALUES " +
                                    "('178.150.207.53'), ('192.241.223.132'), ('node9.mynxtcoin.org'), ('node10.mynxtcoin.org'), " +
                                    "('node3.mynxtcoin.org'), ('109.87.169.253'), ('nxtnet.fr'), ('50.112.241.97'), " +
                                    "('2.84.142.149'), ('bug.airdns.org'), ('83.212.103.14'), ('62.210.131.30')");
                    }
                    break;
                default:
                    throw new NotSupportedException(string.Format("Unable to update database higher than version {0}", SupportedVersion));
            }
        }

        private void ApplyUpdate(string query)
        {
            try
            {
                var connection = _dbController.GetConnection();
                using (var transaction = connection.BeginTransaction())
                using (var cmd = connection.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    if (query != null)
                    {
                        Logger.Debug(string.Format("Will apply query:\n {0}", query));
                        cmd.CommandText = query;
                        cmd.ExecuteNonQuery();
                    }
                    cmd.CommandText = "UPDATE version SET db_version = (SELECT db_version + 1 FROM version)";
                    cmd.ExecuteNonQuery();
                    transaction.Commit();
                    Version++;
                }
            }
            catch (Exception e)
            {
                Logger.FatalException("Unable to apply query to SQLite db", e);
                throw;
            }
        }
    }
}
