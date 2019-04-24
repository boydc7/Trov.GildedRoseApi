using System.Collections.Generic;
using System.Runtime.Serialization;
using ServiceStack;
using ServiceStack.DataAnnotations;
using ServiceStack.Model;

namespace Trov.GildedRose.Api.Dto
{
    [Route("/orders/{orderid}", "GET")]
    public class GetOrder : IReturn<GetOrderResponse>, IGet
    {
        public string OrderId { get; set; }
    }

    [Route("/orders", "POST")]
    public class PostOrder : IReturn<OrderIdResponse>, IPost
    {
        public List<string> ItemIds { get; set; }
    }

    public class GetOrderResponse : ResponseBase
    {
        public Order Order { get; set; }
    }

    public class OrderIdResponse : ResponseBase
    {
        public string OrderId { get; set; }
    }

    public class Order : IHasStringId
    {
        public string Id { get; set; }
        public List<Item> Items { get; set; }

        [IgnoreDataMember]
        public string OrderedBy { get; set; }
    }
}
