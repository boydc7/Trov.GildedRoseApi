using ServiceStack;
using ServiceStack.FluentValidation;
using Trov.GildedRose.Api.Core;

namespace Trov.GildedRose.Api.Services.Validators
{
    public abstract class BaseTrovValidator<T> : AbstractValidator<T>
    {
        protected BaseTrovValidator()
        {
            RuleFor(e => e)
                .Must(e => Request != null)
                .WithName("Request")
                .WithMessage("Request is invalid");

            RuleFor(e => e)
                .Must(e =>
                      {
                          var userSession = Request.SessionAs<TrovUserSession>();

                          if (userSession == null)
                          {
                              return false;
                          }

                          if (Request.IsInProcessRequest() || Request.RequestAttributes.HasFlag(RequestAttributes.MessageQueue))
                          {
                              return true;
                          }

                          return !userSession.UserAuthId.IsNullOrEmpty();
                      })
                .When(e => Request != null)
                .WithName("SessionState")
                .WithMessage("Session state is invalid");
        }
    }
}
