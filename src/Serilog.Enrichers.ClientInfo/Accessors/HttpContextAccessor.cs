#if NETFULL
using System.Web;

namespace Serilog.Enrichers.ClientInfo.Accessors
{
    //    public interface IHttpContextAccessor
    //    {
    //        HttpContext HttpContext { get; }
    //    }

    //    internal class HttpContextAccessor : IHttpContextAccessor
    //    {
    //        public HttpContext HttpContext { get; set; } = HttpContext.Current;
    //    }
    //}


    public interface IHttpContextAccessor
    {
        HttpContext HttpContext { get; }
    }

    public class HttpContextAccessor : IHttpContextAccessor
    {
        public HttpContextBase HttpContextBase { get; set; } = new HttpContextWrapper(HttpContext.Current);

        public HttpContext HttpContext => HttpContextBase.ApplicationInstance.Context;
    }
}
#endif