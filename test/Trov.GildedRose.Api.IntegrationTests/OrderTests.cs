using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using ServiceStack;
using Trov.GildedRose.Api.Dto;

namespace Trov.GildedRose.Api.IntegrationTests
{
    [TestFixture]
    public class IndependentTests
    {
        [Test]
        public void OnlyOneOrderSucceedsWhenMultipleOrdersWithSameItemsArePlaced()
        {
            var testClient = TestSetup.CreateTestClient();

            testClient.BearerToken = TestSetup.TestUserBearerToken;

            var testClient2 = TestSetup.CreateTestClient();

            testClient2.BearerToken = TestSetup.TestUserBearerToken;

            var adminClient = TestSetup.CreateTestClient();

            adminClient.BearerToken = TestSetup.AdminUserBearerToken;

            // First put in a bunch of items
            70.Times(i => adminClient.Post(new PostItem
                                           {
                                               Item = new Item
                                                      {
                                                          Name = $"Parallel Test {i}",
                                                          Description = $"Parallel Test {i}",
                                                          Price = i + 1
                                                      }
                                           }));

            var itemsResponse = adminClient.Get(new GetItems());

            var clientIndex = 0;
            var maxDop = 4;
            var successfulResponses = new List<OrderIdResponse>();
            var successfulOrderIds = new HashSet<string>();
            var unsuccessfulCount = 0;

            // 20 orders each with 3 items, all distinct
            var orders = new ConcurrentQueue<PostOrder>(20.Times(i => new PostOrder
                                                                      {
                                                                          ItemIds = itemsResponse.Results
                                                                                                 .Skip(i * 3)
                                                                                                 .Take(3)
                                                                                                 .Select(item => item.Id)
                                                                                                 .ToList()
                                                                      }));

            var last2CreatedItems = itemsResponse.Results.TakeLast(2).ToList();

            var repeatItemId1 = last2CreatedItems.First().Id;
            var repeatItemId2 = last2CreatedItems.Last().Id;

            void doPostOrder()
            {
                while (orders.TryDequeue(out var postOrder))
                {
                    try
                    {
                        var myIndex = Interlocked.Increment(ref clientIndex);

                        var myClient = (myIndex % 2) > 0
                                           ? testClient2
                                           : testClient;

                        postOrder.ItemIds.Add((myIndex % 2) > 0
                                                  ? repeatItemId2
                                                  : repeatItemId1);

                        var result = myClient.Post(postOrder);

                        successfulResponses.Add(result);
                        postOrder.ItemIds.Each(id => successfulOrderIds.Add(id));
                    }
                    catch(WebServiceException)
                    {
                        unsuccessfulCount++;
                    }
                }
            }

            var consumers = Enumerable.Repeat((Action)doPostOrder, maxDop).ToArray();

            Parallel.Invoke(new ParallelOptions
                            {
                                MaxDegreeOfParallelism = maxDop,
                                TaskScheduler = TaskScheduler.Default
                            },
                            consumers);

            successfulResponses.Count.Should().Be(2);
            successfulOrderIds.Count.Should().Be(8);
            unsuccessfulCount.Should().Be(18);

            // Go get the remaining items and verify
            var postItemsResponse = adminClient.Get(new GetItems());

            postItemsResponse.Results
                             .Any(r => successfulOrderIds.Contains(r.Id))
                             .Should()
                             .BeFalse();

        }

        [Test]
        public void CanPostValidOrderAndGetItWhenAuthenticated()
        {
            var client = TestSetup.CreateTestClient();

            client.BearerToken = TestSetup.TestUserBearerToken;

            var items = client.Get(new GetItems());

            var response = client.Post(new PostOrder
                                       {
                                           ItemIds = items.Results
                                                          .Take(3)
                                                          .Select(i => i.Id)
                                                          .ToList()
                                       });

            response?.OrderId.Should().NotBeNullOrEmpty();

            var orderGetResponse = client.Get(new GetOrder
                                              {
                                                  OrderId = response.OrderId
                                              });

            orderGetResponse?.Order.Should().NotBeNull();
            orderGetResponse.Order.Items.Count.Should().Be(3);
        }

        [Test]
        public void CannotGetValidOrderThatWasNotCreatedByUser()
        {
            var client = TestSetup.CreateTestClient();

            client.BearerToken = TestSetup.AdminUserBearerToken;

            var items = client.Get(new GetItems());

            var response = client.Post(new PostOrder
                                       {
                                           ItemIds = items.Results
                                                          .Take(3)
                                                          .Select(i => i.Id)
                                                          .ToList()
                                       });

            response?.OrderId.Should().NotBeNullOrEmpty();

            var orderClient = TestSetup.CreateTestClient();

            orderClient.BearerToken = TestSetup.TestUserBearerToken;

            Func<GetOrderResponse> orderGetter = () => orderClient.Get(new GetOrder
                                                                       {
                                                                           OrderId = response.OrderId
                                                                       });

            orderGetter.Should().Throw<WebServiceException>();
        }

        [Test]
        public void CannotPostOrderWithItemThatDoesNotExist()
        {
            var client = TestSetup.CreateTestClient();

            client.BearerToken = TestSetup.TestUserBearerToken;

            var items = client.Get(new GetItems());

            Func<OrderIdResponse> orderer = () => client.Post(new PostOrder
                                                              {
                                                                  ItemIds = items.Results
                                                                                 .Take(3)
                                                                                 .Select(i => i.Id)
                                                                                 .Concat(new[]
                                                                                         {
                                                                                             "doesNotExistId"
                                                                                         })
                                                                                 .ToList()
                                                              });

            orderer.Should().Throw<WebServiceException>();
        }
    }
}
