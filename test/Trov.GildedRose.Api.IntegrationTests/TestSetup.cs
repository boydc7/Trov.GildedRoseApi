using System.Net;
using NUnit.Framework;
using ServiceStack;
using ServiceStack.Auth;

namespace Trov.GildedRose.Api.IntegrationTests
{
    [SetUpFixture]
    public class TestSetup
    {
        private const string _baseUrl = "http://localhost:8889/";

        private ServiceStackHost _appHost;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            // Initialize the host
            _appHost = new TestAppHost().Init().Start(_baseUrl);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _appHost?.Dispose();
        }

        public static IServiceClient CreateTestClient() => new JsonServiceClient(_baseUrl);


        public static string AdminUserBearerToken { get; } = new JsonServiceClient(_baseUrl).Post(new Authenticate
                                                                                                 {
                                                                                                     UserName = "adminUser1@trovinterviewapi.com",
                                                                                                     Password = "adminUser1Trov",
                                                                                                     provider = CredentialsAuthProvider.Name
                                                                                                 })
                                                                                           .BearerToken;

        public static string TestUserBearerToken { get; } = new JsonServiceClient(_baseUrl).Post(new Authenticate
                                                                                                 {
                                                                                                     UserName = "testUser1@trovinterviewapi.com",
                                                                                                     Password = "testUser1Trov",
                                                                                                     provider = CredentialsAuthProvider.Name
                                                                                                 })
                                                                                           .BearerToken;


    }
}
