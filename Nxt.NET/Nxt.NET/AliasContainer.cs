using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Nxt.NET.Transaction;

namespace Nxt.NET
{
    public interface IAliasContainer
    {
        IReadOnlyCollection<IAlias> Aliases { get; }
        IAlias GetAlias(string aliasName);
        IAlias GetAlias(long id);
        void AddOrUpdate(IAccount account, ITransaction transaction, IAliasAssignmentAttachment attachment);
        void Remove(IAlias alias);
        void Clear();
    }

    public class AliasContainer : IAliasContainer
    {
        private readonly ConcurrentDictionary<string, IAlias> _aliases = new ConcurrentDictionary<string, IAlias>();
        private readonly ConcurrentDictionary<long, IAlias> _idToAliasMapping = new ConcurrentDictionary<long, IAlias>();

        public IReadOnlyCollection<IAlias> Aliases
        {
            get { return new ReadOnlyCollection<IAlias>(_aliases.Values.ToList()); }
        }

        public IAlias GetAlias(string aliasName)
        {
            IAlias alias;
            _aliases.TryGetValue(aliasName.ToLower(), out alias);
            return alias;
        }

        public IAlias GetAlias(long id)
        {
            IAlias alias;
            _idToAliasMapping.TryGetValue(id, out alias);
            return alias;
        }

        public void AddOrUpdate(IAccount account, ITransaction transaction, IAliasAssignmentAttachment attachment)
        {
            var normalizedAlias = attachment.AliasName.ToLower();
            var newAlias = new Alias(account, transaction.Id, attachment.AliasName, attachment.AliasUri, transaction.BlockTimestamp);
            _aliases.AddOrUpdate(normalizedAlias, newAlias, (key, @alias) => new Alias(alias.Account, alias.Id, alias.Name, attachment.AliasUri, transaction.BlockTimestamp));
            _idToAliasMapping.AddOrUpdate(transaction.Id, newAlias, (key, @alias) => _aliases[normalizedAlias]);
        }

        public void Remove(IAlias alias)
        {
            _aliases.Remove(alias.Name.ToLower());
            _idToAliasMapping.Remove(alias.Id);
        }

        public void Clear()
        {
            _aliases.Clear();
            _idToAliasMapping.Clear();
        }
    }
}
