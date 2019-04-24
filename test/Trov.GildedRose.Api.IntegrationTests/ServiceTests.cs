using System;
using FluentAssertions;
using NUnit.Framework;
using Trov.GildedRose.Api.Dto;

namespace Trov.GildedRose.Api.IntegrationTests
{
    [TestFixture]
    public class ServiceTests
    {
        [Test]
        public void CanHitMonitorAndGetCorrectEchoBackUnauthenticated()
        {
            var client = TestSetup.CreateTestClient();

            var echoId = Guid.NewGuid().ToString();

            var response = client.Get(new Monitor
                                      {
                                          Echo = echoId
                                      });

            response?.Response.Should().NotBeNull();
            response.Response.Should().Be(echoId);
        }
    }
}
