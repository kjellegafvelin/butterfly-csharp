using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpanContract = Butterfly.DataContract.Tracing.Span;
using Butterfly.OpenTracing.Dispatcher;
using Butterfly.OpenTracing.Sender;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace Butterfly.OpenTracing
{
    internal class SpanDispatchCallback : IDispatchCallback
    {
        private const int DefaultChunked = 500;
        private readonly IButterflySender _butterflySender;
        private readonly ILogger _logger;

        public SpanDispatchCallback(IButterflySender sender, ILoggerFactory loggerFactory)
        {
            _butterflySender = sender ?? throw new ArgumentNullException(nameof(sender));

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger(typeof(SpanDispatchCallback));
            Filter = token => token == DispatchableToken.SpanToken;
        }

        public Func<DispatchableToken, bool> Filter { get; }

        public async Task Accept(IEnumerable<IDispatchable> dispatchables)
        {
            foreach(var block in dispatchables.Chunked(DefaultChunked))
            {
                try
                {
                    SpanContract[] spans = block.Select(x => x.RawInstance).OfType<SpanContract>().ToArray();
                    await _butterflySender.SendSpanAsync(spans).ConfigureAwait(false);
                }
                catch(HttpRequestException)
                {
                    foreach (var item in block)
                    {
                        item.State = SendState.Untreated;
                        item.Error();
                    }
                    _logger.LogWarning("Failed to connect to collector.");
                }
                catch(Exception exception)
                {
                    foreach(var item in block)
                    {
                        item.State = SendState.Untreated;
                        item.Error();
                    }
                    _logger.LogError(new EventId(2), exception, "Flush span to collector error.");
                }
            }
        }
    }
}
