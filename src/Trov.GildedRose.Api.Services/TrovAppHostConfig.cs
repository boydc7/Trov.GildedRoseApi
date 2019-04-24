using System;
using System.Text.RegularExpressions;
using System.Threading;
using Funq;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.Caching;
using ServiceStack.Configuration;
using ServiceStack.Logging;
using ServiceStack.Logging.NLogger;
using ServiceStack.Text;
using ServiceStack.Validation;
using Trov.GildedRose.Api.Core;
using Trov.GildedRose.Api.Dto;

namespace Trov.GildedRose.Api.Services
{
    public class TrovAppHostConfig
    {
        private int _inShutdown;

        public static TrovAppHostConfig Instance { get; } = new TrovAppHostConfig();

        public ILog Log { get; private set; }

        public bool InShutdown => _inShutdown > 0;

        public void Configure(ServiceStackHost host, Container container)
        {
            LogManager.LogFactory = new NLogFactory();

            Log = LogManager.GetLogger(GetType());

            Log.Info($"AppHost Starting in Environment.Configuration of [{TrovEnvironment.Configuration}]");

            // Configure SS host
            host.SetConfig(new HostConfig
                           {
                               DebugMode = TrovEnvironment.IsDebug,
                               DefaultContentType = MimeTypes.Json,
                               UseSecureCookies = !TrovEnvironment.IsDebug,
                               AllowNonHttpOnlyCookies = TrovEnvironment.IsDebug,
                               AllowSessionIdsInHttpParams = TrovEnvironment.IsDebug,
                               AllowSessionCookies = true,
                               EnableAccessRestrictions = true,
                               AdminAuthSecret = "7x8-AA79WcVJ_4r6549e438UArY-vGnvdY9z6_G3",
                               EnableOptimizations = true,
                               UseCamelCase = true,
                               AddRedirectParamsToQueryString = true
                           });

            host.Plugins.Add(new ValidationFeature());

            JsConfig.TextCase = TextCase.CamelCase;
            JsConfig.DateHandler = DateHandler.ISO8601;
            JsConfig.AssumeUtc = true;

            // Register services, validators, request pipeline things....
            host.RegisterServicesInAssembly(GetType().Assembly);
            container.RegisterValidators(GetType().Assembly);

            if (TrovEnvironment.IsDebug)
            {
                host.Plugins.Add(new RequestLogsFeature
                                 {
                                     EnableErrorTracking = true,
                                     EnableSessionTracking = false,
                                     EnableResponseTracking = true,
                                     EnableRequestBodyTracking = true,
                                     Capacity = 5000,
                                     RequiredRoles = new[]
                                                     {
                                                         RoleNames.Admin
                                                     }
                                 });
            }

            // Authentication related setup
            ConfigureAuthentication(host);

            // Register services...only handling in-memory stuff for now
            container.Register<ICacheClient>(new MemoryCacheClient());

            // Repos
            container.Register<IItemRepository>(c => new InMemoryItemRepository())
                     .ReusedWithin(ReuseScope.Hierarchy);

            container.Register<IOrderRepository>(c => new InMemoryOrderRepository())
                     .ReusedWithin(ReuseScope.Hierarchy);

            // Seed data for debug runs
            AddSeedData(host);
        }

        public void Shutdown(Container container)
        {
            if (Interlocked.Exchange(ref _inShutdown, 1) > 0)
            {
                return;
            }

            try
            {
                Log.Info("Shutdown of AppHost starting");

                // Shutdown background services/etc. here...

                Log.Info("Shutdown of AppHost complete");
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Exception trying to Shutdown the AppHost.");
            }
        }

        private void ConfigureAuthentication(ServiceStackHost host)
        {
            host.Plugins.Add(new CorsFeature("*",
                                             CorsFeature.DefaultMethods,
                                             allowCredentials: true,
                                             allowedHeaders: "Content-Type, Authorization, X-Requested-With, X-ss-pid, X-ss-id, X-ss-tok")
                             {
                                 AutoHandleOptionsRequests = true
                             });

            // For now just use a key generated on each startup
            var jwtKey64 = Convert.ToBase64String(AesUtils.CreateKey());

            host.Plugins.Add(new AuthFeature(() => new TrovUserSession(),
                                             new IAuthProvider[]
                                             {
                                                 new JwtAuthProvider // ../auth/jwt
                                                 {
                                                     AuthKeyBase64 = jwtKey64,
                                                     RequireSecureConnection = !TrovEnvironment.IsDebug,
                                                     HashAlgorithm = "HS384",
                                                     ExpireTokensIn = TimeSpan.FromDays(1),
                                                     ExpireRefreshTokensIn = TimeSpan.FromDays(10),
                                                     CreatePayloadFilter = (p, s) =>
                                                                           {
                                                                               if (!(s is TrovUserSession tus)) { }

                                                                               // Normally would fill the session with app-specific stuff here
                                                                           },
                                                     PopulateSessionFilter = (s, p, r) =>
                                                                             {
                                                                                 if (!(s is TrovUserSession tus)) { }

                                                                                 // Normally would fill the model with app-specific stuff here
                                                                             }
                                                 },
                                                 new ApiKeyAuthProvider // ../auth/apikey
                                                 {
                                                     SessionCacheDuration = TimeSpan.FromMinutes(5),
                                                     RequireSecureConnection = !TrovEnvironment.IsDebug,
                                                     Environments = new[]
                                                                    {
                                                                        "live"
                                                                    },
                                                     KeySizeBytes = 50,
                                                     AllowInHttpParams = true,
                                                     ExpireKeysAfter = TimeSpan.FromDays(3650)
                                                 },
                                                 new CredentialsAuthProvider // ../auth/credentials
                                                 {
                                                     SkipPasswordVerificationForInProcessRequests = true
                                                 }
                                             })
                             {
                                 SaveUserNamesInLowerCase = true,
                                 ValidateUniqueUserNames = true,
                                 MaxLoginAttempts = 6,
                                 DeleteSessionCookiesOnLogout = true,
                                 GenerateNewSessionCookiesOnAuthentication = false,
                                 IncludeRegistrationService = true,
                                 IncludeAssignRoleServices = false,
                                 HtmlRedirect = null,
                                 HtmlLogoutRedirect = null,
                                 ValidUserNameRegEx = new Regex("^[A-Za-z0-9-.+_@]{6,50}$", RegexOptions.Compiled)
                             });

            // In-memory only for now
            host.Container.Register<IAuthRepository>(new InMemoryAuthRepository());
        }

        private void AddSeedData(ServiceStackHost host)
        {
            if (!TrovEnvironment.IsDebug)
            {
                return;
            }

            var itemRepo = host.Container.Resolve<IItemRepository>();

            itemRepo.SaveItems(11.Times(i => new Item
                                             {
                                                 Id = Guid.NewGuid().ToString("N"),
                                                 Name = $"Test Item {i}",
                                                 Description = $"Description for test itme {i}",
                                                 Price = i * 3
                                             }));

            var authRepo = host.GetAuthRepository();

            authRepo.CreateUserAuth(new UserAuth
                                    {
                                        UserName = "testUser1@trovinterviewapi.com",
                                        Email = "testUser1@trovinterviewapi.com",
                                        FirstName = "Test",
                                        LastName = "User1",
                                        DisplayName = "Test User1"
                                    },
                                    "testUser1Trov");

            var adminUserAuth = new UserAuth
                                {
                                    UserName = "adminUser1@trovinterviewapi.com",
                                    Email = "adminUser1@trovinterviewapi.com",
                                    FirstName = "Admin",
                                    LastName = "User1",
                                    DisplayName = "Admin User1"
                                };

            var createdAdminAuth = authRepo.CreateUserAuth(adminUserAuth, "adminUser1Trov");

            authRepo.AssignRoles(createdAdminAuth, new[]
                                                   {
                                                       RoleNames.Admin
                                                   });
        }
    }
}
