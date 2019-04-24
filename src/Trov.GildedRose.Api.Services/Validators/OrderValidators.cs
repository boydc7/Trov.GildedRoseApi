using System;
using System.Linq;
using ServiceStack;
using ServiceStack.FluentValidation;
using Trov.GildedRose.Api.Core;
using Trov.GildedRose.Api.Dto;

namespace Trov.GildedRose.Api.Services.Validators
{
    public class GetOrderValidator : BaseTrovValidator<GetOrder>
    {
        public GetOrderValidator(IOrderRepository orderRepository)
        {
            RuleFor(e => e.OrderId)
                .NotEmpty();

            RuleFor(e => e.OrderId)
                .Must(i =>
                      {
                          var order = orderRepository.Get(i);

                          var userSession = Request.SessionAs<TrovUserSession>();

                          // Must exist and user either an admin or the owner of the order
                          return order != null &&
                                 (userSession.IsAdmin || order.OrderedBy.Equals(userSession.UserAuthId, StringComparison.Ordinal));
                      })
                .When(r => !r.OrderId.IsNullOrEmpty())
                .WithMessage("Order requested does not exist or you do not have access to it");
        }
    }

    public class PostOrderValidator : BaseTrovValidator<PostOrder>
    {
        public PostOrderValidator(IItemRepository itemRepository)
        {
            RuleFor(e => e.ItemIds)
                .NotNull()
                .NotEmpty()
                .WithMessage("At least 1 item must be specified for order");

            RuleFor(e => e.ItemIds)
                .Must(ids => ids.All(id => itemRepository.Get(id) != null))
                .When(e => !e.ItemIds.IsNullOrEmpty())
                .WithMessage("1 or more of the items requested does not exist");
        }
    }
}
