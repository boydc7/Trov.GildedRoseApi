using ServiceStack;

namespace Trov.GildedRose.Api.Dto
{
    [Route("/monitor", "GET")]
    [Route("/monitor/{echo}", "GET")]
    public class Monitor : IReturn<MonitorResponse>, IGet
    {
        public string Echo { get; set; }
    }

    public class MonitorResponse : ResponseBase
    {
        public string Response { get; set; }
    }
}
