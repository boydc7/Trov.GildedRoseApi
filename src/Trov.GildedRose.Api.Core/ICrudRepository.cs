using System.Collections.Generic;

namespace Trov.GildedRose.Api.Core
{
    public interface ICrudRepository<T>
    {
        void Save(T model);
        T Get(string id);
        IEnumerable<T> GetAll();
        bool Delete(string id);
    }
}
