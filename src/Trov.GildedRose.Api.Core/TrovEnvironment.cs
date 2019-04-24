using System;
using Microsoft.Extensions.Hosting;
using ServiceStack.Configuration;

namespace Trov.GildedRose.Api.Core
{
    public static class TrovEnvironment
    {
#if DEBUG
        public const bool IsDebug = true;
#else
        public const bool IsDebug = false;
#endif

        public static T GetAppSetting<T>(string key, T defaultIfMissing = default) => AppSettings.Get(key, defaultIfMissing);

        public static IAppSettings AppSettings { get; private set; }

        public static void Init(IAppSettings appSettings) => AppSettings = appSettings;

        public static readonly string Configuration = IsDebug
                                                          ? "Debug"
                                                          : Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? EnvironmentName.Production;
    }
}
