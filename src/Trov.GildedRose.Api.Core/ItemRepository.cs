using System;
using System.Collections.Generic;
using ServiceStack.Logging;
using Trov.GildedRose.Api.Dto;

namespace Trov.GildedRose.Api.Core
{
    public interface IItemRepository : ICrudRepository<Item>
    {
        List<Item> GetItemsForOrder(IEnumerable<string> itemIds);
        void SaveItems(IEnumerable<Item> items);
    }

    public class InMemoryItemRepository : InMemoryRepositoryBase<Item>, IItemRepository
    {
        private static readonly ILog _log = LogManager.GetLogger("InMemoryItemRepository");

        public List<Item> GetItemsForOrder(IEnumerable<string> itemIds)
        {
            var toReturn = new List<Item>();

            try
            {
                foreach (var itemId in itemIds)
                {
                    if (!_records.TryRemove(itemId, out var item))
                    {
                        throw new ApplicationException("All items requested for the order were not available");
                    }

                    toReturn.Add(item);
                }
            }
            catch when(ReturnItemsToInventory(toReturn))
            {   // Unreachable code...
                throw;
            }

            return toReturn;
        }

        public void SaveItems(IEnumerable<Item> items)
        {
            List<Exception> exceptions = null;

            foreach (var item in items)
            {
                try
                {
                    Save(item);
                }
                catch(Exception x)
                {
                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }

                    exceptions.Add(x);
                }
            }

            if (exceptions != null)
            {
                throw new AggregateException(exceptions);
            }
        }

        private bool ReturnItemsToInventory(IEnumerable<Item> items)
        {
            try
            {
                SaveItems(items);
            }
            catch(Exception x)
            {
                _log.Error("Unable to return all items to inventory...handle this...", x);
            }

            // Always return false, used for exception filter
            return false;
        }
    }
}
