using Funq;
using ServiceStack;
using Trov.GildedRose.Api.Services;

namespace Trov.GildedRose.Api.IntegrationTests
{
    public class TestAppHost : AppSelfHostBase
    {
        public TestAppHost() : base("Trov GildedRose IntegrationTests", typeof(TestAppHost).Assembly) { }

        public override void Configure(Container container)
        {
            // Normally this would be composed of some shared and some non-shared (i.e. real repositories vs. in-memory for example), but for now it's all the same
            TrovAppHostConfig.Instance.Configure(this, container);
        }
    }
}
