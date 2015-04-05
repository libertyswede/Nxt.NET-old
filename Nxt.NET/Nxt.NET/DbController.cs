using System.Data;
using System.Data.SQLite;
using System.IO;

namespace Nxt.NET
{
    public interface IDbController
    {
        IDbConnection GetConnection();
    }

    public class DbController : IDbController
    {
        public static readonly string DbFileName = "NxtDb.sqlite";
        public static readonly string ConnectionString = string.Format("data source=\"{0}\"", DbFileName);
        private IDbConnection _connection;

        public DbController()
        {
            if (!File.Exists(DbFileName))
                SQLiteConnection.CreateFile(DbFileName);
        }

        public IDbConnection GetConnection()
        {
            if (_connection == null)
            {
                _connection = new SQLiteConnection(ConnectionString);
                _connection.Open();
            }

            return _connection;
        }
    }
}
