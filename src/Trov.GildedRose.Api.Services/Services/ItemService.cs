using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack;
using Trov.GildedRose.Api.Core;
using Trov.GildedRose.Api.Dto;

namespace Trov.GildedRose.Api.Services.Services
{
    public class ItemService : Service
    {
        private readonly IItemRepository _itemRepository;

        public ItemService(IItemRepository itemRepository)
        {
            _itemRepository = itemRepository;
        }

        public GetItemsResponse Get(GetItems request)
        {
            var response = new GetItemsResponse();

            if (request.ItemId.IsNullOrEmpty())
            {
                response.Results = _itemRepository.GetAll().Take(1000).ToList();
            }
            else
            {
                var item = _itemRepository.Get(request.ItemId) ?? throw HttpError.NotFound("Order not found or not available");

                response.Results = new List<Item>
                                   {
                                       item
                                   };
            }

            return response;
        }

        [Authenticate]
        [RequiredRole("Admin")]
        public ItemIdResponse Post(PostItem request)
        {
            request.Item.Id = Guid.NewGuid().ToString("N");

            _itemRepository.Save(request.Item);

            return new ItemIdResponse
                   {
                       ItemId = request.Item.Id
                   };
        }

        [Authenticate]
        [RequiredRole("Admin")]
        public void Put(PutItem request)
        {
            request.Item.Id = request.ItemId;

            _itemRepository.Save(request.Item);
        }
    }
}
