using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;
using Butterfly.OpenTracing;
using OpenTracing;
using OpenTracing.Propagation;

namespace Butterfly.Client.Tracing
{
    [NonAspect]
    public class HttpTracingHandler : DelegatingHandler
    {
        private readonly IServiceTracer _tracer;

        public HttpTracingHandler(IServiceTracer tracer, HttpMessageHandler httpMessageHandler = null)
        {
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
            InnerHandler = httpMessageHandler ?? new HttpClientHandler();
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _tracer.ChildTraceAsync($"httpclient {request.Method}", DateTimeOffset.UtcNow, span => TracingSendAsync(span, request, cancellationToken));
        }

        protected virtual async Task<HttpResponseMessage> TracingSendAsync(ISpan span, HttpRequestMessage request, CancellationToken cancellationToken)
        {

            ((Span)span).Tags.Client().Component("HttpClient")
                .HttpMethod(request.Method.Method)
                .HttpUrl(request.RequestUri.OriginalString)
                .HttpHost(request.RequestUri.Host)
                .HttpPath(request.RequestUri.PathAndQuery)
                .PeerAddress(request.RequestUri.OriginalString)
                .PeerHostName(request.RequestUri.Host)
                .PeerPort(request.RequestUri.Port);

            var headers = new Dictionary<string, string>();

            _tracer.Tracer.Inject(span.Context, BuiltinFormats.HttpHeaders,
                new TextMapInjectAdapter(headers));

            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            span.Log(LogField.CreateNew().ClientSend());

            var responseMessage = await base.SendAsync(request, cancellationToken);

            span.Log(LogField.CreateNew().ClientReceive());

            return responseMessage;
        }
    }
}