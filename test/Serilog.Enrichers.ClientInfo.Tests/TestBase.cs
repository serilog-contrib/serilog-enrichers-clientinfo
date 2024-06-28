using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using NSubstitute;
using System.IO;
using System.Reflection;

#if NETFULL

using System.Web;
using Serilog.Enrichers.ClientInfo.Accessors;

#else
using Microsoft.AspNetCore.Http;
#endif

namespace Serilog.Enrichers.ClientInfo.Tests
{
    public abstract class TestBase
    {
        public IHttpContextAccessor MockHttpContextAccessor(Dictionary<string, string> headerDict = null)
        {
#if NETFULL
            HttpContext.Current = new HttpContext(new HttpRequest("", "http://tempuri.org", ""), new HttpResponse(new StringWriter()));

            NameValueCollection headers = HttpContext.Current.Request.Headers;

            Type t = headers.GetType();
            const BindingFlags nonPublicInstanceMethod = BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance;

            t.InvokeMember("MakeReadWrite", nonPublicInstanceMethod, null, headers, null);
            t.InvokeMember("InvalidateCachedArrays", nonPublicInstanceMethod, null, headers, null);

            if (headerDict != null)
            {
                foreach (var keyValue in headerDict)
                {
                    t.InvokeMember("BaseRemove", nonPublicInstanceMethod, null, headers, new object[] { keyValue.Key });
                    t.InvokeMember("BaseAdd", nonPublicInstanceMethod, null, headers, new object[] { keyValue.Key, new ArrayList { keyValue.Value } });
                    t.InvokeMember("MakeReadOnly", nonPublicInstanceMethod, null, headers, null);
                }
            }

            var contextAccessor = new HttpContextAccessor { HttpContext = HttpContext.Current };

            return contextAccessor;
#else
            var httpContext = new DefaultHttpContext();
            var contextAccessor = Substitute.For<IHttpContextAccessor>();
            contextAccessor.HttpContext.Returns(httpContext);

            if (headerDict != null)
            {
                foreach (var keyValue in headerDict)
                {
                    contextAccessor.HttpContext.Request.Headers.Add(keyValue.Key, keyValue.Value);
                }
            }

            return contextAccessor;
#endif
        }
    }
}