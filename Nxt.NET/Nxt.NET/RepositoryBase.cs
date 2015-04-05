using System.Data.SQLite;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NLog;

namespace Nxt.NET
{
    public abstract class RepositoryBase
    {
        protected readonly IDbController DbController;
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        protected SQLiteConnection Connection;

        protected RepositoryBase(IDbController dbController)
        {
            DbController = dbController;
            Connection = (SQLiteConnection) DbController.GetConnection();
        }

        protected static byte[] SerializeObject(object serializable)
        {
            if (serializable == null)
                return null;

            using (var memoryStream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, serializable);
                return memoryStream.ToArray();
            }
        }

        protected static object DeserializeObject(byte[] bytes)
        {
            if (bytes == null) return null;
            using (var memoryStream = new MemoryStream(bytes))
            {
                var formatter = new BinaryFormatter();
                return formatter.Deserialize(memoryStream);
            }
        }
    }
}