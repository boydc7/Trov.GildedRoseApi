using ServiceStack;
using ServiceStack.FluentValidation;
using Trov.GildedRose.Api.Core;
using Trov.GildedRose.Api.Dto;

namespace Trov.GildedRose.Api.Services.Validators
{
    public class GetItemsValidator : AbstractValidator<GetItems>
    {
        public GetItemsValidator(IItemRepository itemRepository)
        {
            RuleFor(e => e.ItemId)
                .Must(i => itemRepository.Get(i) != null)
                .When(r => !r.ItemId.IsNullOrEmpty())
                .WithMessage("Item requested does not exist");
        }
    }

    public class PostItemValidator : BaseTrovValidator<PostItem>
    {
        public PostItemValidator()
        {
            RuleFor(e => e.Item)
                .NotNull()
                .WithMessage("Item must be specified");

            RuleFor(e => e.Item)
                .SetValidator(new ItemValidator())
                .When(e => e.Item != null);
        }
    }

    public class PutItemValidator : BaseTrovValidator<PutItem>
    {
        public PutItemValidator(IItemRepository itemRepository)
        {
            RuleFor(e => e.ItemId)
                .NotEmpty();

            RuleFor(e => e.ItemId)
                .Must(i => itemRepository.Get(i) != null)
                .When(r => !r.ItemId.IsNullOrEmpty())
                .WithMessage("Item requested does not exist");

            RuleFor(e => e.Item)
                .NotNull()
                .WithMessage("Item must be specified");

            RuleFor(e => e.Item)
                .SetValidator(new ItemValidator())
                .When(e => e.Item != null);
        }
    }

    public class ItemValidator : AbstractValidator<Item>
    {
        public ItemValidator()
        {
            RuleSet(ApplyTo.Post | ApplyTo.Put,
                    () =>
                    {
                        RuleFor(i => i.Id)
                            .Empty();

                        RuleFor(i => i.Name)
                            .NotEmpty();

                        RuleFor(i => i.Description)
                            .NotEmpty();

                        RuleFor(i => i.Price)
                            .GreaterThan(0);
                    });

            RuleFor(e => e.Name)
                .MaximumLength(100);

            RuleFor(e => e.Description)
                .MaximumLength(2000);

            RuleFor(e => e.Price)
                .GreaterThanOrEqualTo(0);
        }
    }
}
