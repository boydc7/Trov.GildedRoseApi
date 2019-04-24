using Trov.GildedRose.Api.Dto;

namespace Trov.GildedRose.Api.Core
{
    public interface IOrderRepository : ICrudRepository<Order> { }

    public class InMemoryOrderRepository : InMemoryRepositoryBase<Order>, IOrderRepository { }
}
