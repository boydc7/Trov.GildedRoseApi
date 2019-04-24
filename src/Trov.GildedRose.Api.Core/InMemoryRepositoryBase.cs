using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ServiceStack;
using ServiceStack.Model;

namespace Trov.GildedRose.Api.Core
{
    public abstract class InMemoryRepositoryBase<T> : ICrudRepository<T>
        where T : IHasStringId
    {
        protected readonly ConcurrentDictionary<string, T> _records = new ConcurrentDictionary<string, T>(StringComparer.OrdinalIgnoreCase);

        public void Save(T model) => _records[model.Id] = model;

        public T Get(string id) => _records.TryGetValue(id, out var record)
                                       ? record
                                       : default;

        public IEnumerable<T> GetAll() => _records.Values;

        public bool Delete(string id)
            => _records.TryRemove(id, out _);
    }
}
