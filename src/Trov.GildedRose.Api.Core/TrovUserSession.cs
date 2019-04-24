using ServiceStack;
using ServiceStack.Configuration;

namespace Trov.GildedRose.Api.Core
{
    public class TrovUserSession : AuthUserSession
    {
        // Nothing unique to app here at the moment

        public bool IsAdmin => !Roles.IsNullOrEmpty() && Roles.Contains(RoleNames.Admin);
    }
}
