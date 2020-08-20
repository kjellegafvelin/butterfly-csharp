using System;
using Microsoft.AspNetCore.Http;
using OpenTracing;

namespace Butterfly.Client.AspNetCore
{
    public interface IRequestTracer
    {

        ISpan OnBeginRequest(HttpContext httpContext);

        void OnEndRequest(HttpContext httpContext);

        void OnException(HttpContext httpContext, Exception exception, string @event);
    }
}