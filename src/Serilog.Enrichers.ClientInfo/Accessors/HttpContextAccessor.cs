#if NETFULL
using System.Web;

namespace Serilog.Enrichers.ClientInfo.Accessors
{


    public interface IHttpContextAccessor
    {
        HttpContext HttpContext { get; }
    }

    public class HttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext HttpContext { get; set; } = HttpContext.Current;
    }
}
#endif