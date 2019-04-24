using System;
using ServiceStack;
using ServiceStack.Logging;
using Trov.GildedRose.Api.Core;
using Trov.GildedRose.Api.Dto;

namespace Trov.GildedRose.Api.Services.Services
{
    [Authenticate]
    public class OrderService : Service
    {
        private static readonly ILog _log = LogManager.GetLogger("OrderService");

        private readonly IOrderRepository _orderRepository;
        private readonly IItemRepository _itemRepository;

        public OrderService(IOrderRepository orderRepository, IItemRepository itemRepository)
        {
            _orderRepository = orderRepository;
            _itemRepository = itemRepository;
        }

        public GetOrderResponse Get(GetOrder request)
        {
            var order = _orderRepository.Get(request.OrderId) ?? throw HttpError.NotFound("Order not found or not available");

            return new GetOrderResponse
                   {
                       Order = order
                   };
        }

        public OrderIdResponse Post(PostOrder request)
        {
            var order = new Order
                        {
                            Id = Guid.NewGuid().ToString("N"),
                            OrderedBy = GetSession().UserAuthId
                        };

            try
            {   // Get all the items from inventory
                order.Items = _itemRepository.GetItemsForOrder(request.ItemIds);

                // Place the order
                _orderRepository.Save(order);
            }
            catch
            {
                if (!order.Items.IsNullOrEmpty())
                {
                    try
                    {
                        _itemRepository.SaveItems(order.Items);
                    }
                    catch(Exception x)
                    {
                        _log.Error($"Unable to return all items to inventory...handle this...", x);
                    }
                }

                throw;
            }

            return new OrderIdResponse
                   {
                       OrderId = order.Id
                   };
        }
    }
}
