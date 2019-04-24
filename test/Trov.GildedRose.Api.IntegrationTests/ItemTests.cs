using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ServiceStack;
using Trov.GildedRose.Api.Dto;

namespace Trov.GildedRose.Api.IntegrationTests
{
    [TestFixture]
    public class ItemTests
    {
        [Test]
        public void CanGetAllItemsAndRepeatableReadIndividualUnauthenticated()
        {
            var client = TestSetup.CreateTestClient();

            var response = client.Get(new GetItems());

            response?.Results.Should().NotBeNullOrEmpty();
            response.Results.Count.Should().BeGreaterThan(1);

            var specificItem = response.Results.Skip(6).First();

            var singleResponse = client.Get(new GetItems
                                            {
                                                ItemId = specificItem.Id
                                            });

            singleResponse?.Results.Should().NotBeNullOrEmpty();
            singleResponse.Results.Count.Should().Be(1);

            singleResponse.Results.Single().Should().BeEquivalentTo(specificItem);
        }

        [Test]
        public void CannotPostValidNewItemUnauthenticated()
        {
            var client = TestSetup.CreateTestClient();

            Func<ItemIdResponse> responseFunc = () => client.Post(new PostItem
                                                                  {
                                                                      Item = new Item
                                                                             {
                                                                                 Name = "Test",
                                                                                 Description = "Test",
                                                                                 Price = 1
                                                                             }
                                                                  });

            responseFunc.Should().Throw<WebServiceException>();
        }

        [Test]
        public void CannotGetItemWithIdThatDoesNotExist()
        {
            var client = TestSetup.CreateTestClient();

            Func<GetItemsResponse> responseFunc = () => client.Get(new GetItems
                                                                   {
                                                                       ItemId = "doesNotExistId"
                                                                   });

            responseFunc.Should().Throw<WebServiceException>();
        }

        [Test]
        public void CannotPostItemWithIdSpecified()
        {
            var client = TestSetup.CreateTestClient();

            client.BearerToken = TestSetup.AdminUserBearerToken;

            Func<ItemIdResponse> responseFunc = () => client.Post(new PostItem
                                                                  {
                                                                      Item = new Item
                                                                             {
                                                                                 Id = "123",
                                                                                 Name = "Test",
                                                                                 Description = "Test",
                                                                                 Price = 1
                                                                             }
                                                                  });

            responseFunc.Should().Throw<WebServiceException>();
        }

        [Test]
        public void CannotPostItemWithPriceOfZero()
        {
            var client = TestSetup.CreateTestClient();

            client.BearerToken = TestSetup.AdminUserBearerToken;

            Func<ItemIdResponse> responseFunc = () => client.Post(new PostItem
                                                                  {
                                                                      Item = new Item
                                                                             {
                                                                                 Name = "Test",
                                                                                 Description = "Test",
                                                                                 Price = 0
                                                                             }
                                                                  });

            responseFunc.Should().Throw<WebServiceException>();
        }

        [Test]
        public void CanPostNewItemAsAdmin()
        {
            var client = TestSetup.CreateTestClient();

            client.BearerToken = TestSetup.AdminUserBearerToken;

            var response = client.Post(new PostItem
                                       {
                                           Item = new Item
                                                  {
                                                      Name = "Test",
                                                      Description = "Test",
                                                      Price = 1
                                                  }
                                       });

            response?.ItemId.Should().NotBeNullOrEmpty();
        }
    }
}
