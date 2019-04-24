using ServiceStack;

namespace Trov.GildedRose.Api.Dto
{
    public abstract class ResponseBase
    {
        public ResponseStatus ResponseStatus { get; set; }
    }
}
