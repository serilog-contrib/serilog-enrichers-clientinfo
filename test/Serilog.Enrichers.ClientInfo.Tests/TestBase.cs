using System.Collections.Specialized;
using NSubstitute;
using System.IO;

#if NETFULL

using System.Collections;
using System.Web;
using Serilog.Enrichers.ClientInfo.Accessors;

#else
using Microsoft.AspNetCore.Http;
#endif

namespace Serilog.Enrichers.ClientInfo.Tests
{
    public abstract class TestBase
    {
        public IHttpContextAccessor GetMockHttpContextAccessor
        {
            get
            {
#if NETFULL
                var httpContextBaseSub = Substitute.For<HttpContextBase>();
                var requestSub = Substitute.For<HttpRequestBase>();
                var responseSub = Substitute.For<HttpResponseBase>();
                var serverUtilitySub = Substitute.For<HttpServerUtilityBase>();
                var itemsSub = Substitute.For<IDictionary>();
                httpContextBaseSub.Request.Returns(requestSub);
                httpContextBaseSub.Response.Returns(responseSub);
                httpContextBaseSub.Server.Returns(serverUtilitySub);
                httpContextBaseSub.Items.Returns(itemsSub);

                //var requestSub = Substitute.For<HttpRequest>();
                //var request = new HttpRequest("", "http://tempuri.org", "")
                //{
                //    Headers =
                //    {
                //        ["test"] = "test"
                //    }
                //};

                var contextAccessor = new HttpContextAccessor
                {
                    HttpContextBase = httpContextBaseSub
                };

                return contextAccessor;
#else
            var httpContext = new DefaultHttpContext();
            IHttpContextAccessor contextAccessor = Substitute.For<IHttpContextAccessor>();
            contextAccessor.HttpContext.Returns(httpContext);
            return contextAccessor;
#endif
            }
        }
    }
}