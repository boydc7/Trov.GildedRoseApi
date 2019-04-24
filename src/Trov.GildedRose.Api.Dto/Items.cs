using System.Collections.Generic;
using System.Runtime.Serialization;
using ServiceStack;
using ServiceStack.Model;

namespace Trov.GildedRose.Api.Dto
{
    [Route("/items", "GET")]
    [Route("/items/{itemid}", "GET")]
    public class GetItems : IReturn<GetItemsResponse>, IGet
    {
        public string ItemId { get; set; }
    }

    [Route("/items", "POST")]
    public class PostItem : IReturn<ItemIdResponse>, IGet
    {
        public Item Item { get; set; }
    }

    [Route("/items/{itemid}", "PUT")]
    public class PutItem : IReturnVoid, IGet
    {
        public string ItemId { get; set; }
        public Item Item { get; set; }
    }

    public class GetItemsResponse : ResponseBase
    {
        public List<Item> Results { get; set; }
    }

    public class ItemIdResponse : ResponseBase
    {
        public string ItemId { get; set; }
    }

    public class Item : IHasStringId
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
    }
}
