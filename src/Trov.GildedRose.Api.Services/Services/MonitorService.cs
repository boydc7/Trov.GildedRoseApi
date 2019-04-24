using System;
using ServiceStack;
using Trov.GildedRose.Api.Dto;

namespace Trov.GildedRose.Api.Services.Services
{
    [Restrict(VisibleLocalhostOnly = true)]
    public class MonitorService : Service
    {
        public MonitorResponse Get(Monitor request)
        {
            if (TrovAppHostConfig.Instance.InShutdown)
            {
                throw new ApplicationException("API is Shutting down.");
            }

            return new MonitorResponse
                   {
                       Response = request.Echo ?? "OK"
                   };
        }
    }
}
