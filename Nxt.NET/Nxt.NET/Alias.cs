namespace Nxt.NET
{
    public interface IAlias
    {
        IAccount Account { get; }
        long Id { get; }
        string Name { get; }
        string Uri { get; }
        int Timestamp { get; }
    }

    public class Alias : IAlias
    {
        public IAccount Account { get; private set; }
        public long Id { get; private set; }
        public string Name { get; private set; }
        public string Uri { get; private set; }
        public int Timestamp { get; private set; }

        public Alias(IAccount account, long id, string name, string uri, int timestamp)
        {
            Account = account;
            Id = id;
            Name = name;
            Uri = uri;
            Timestamp = timestamp;
        }
    }
}