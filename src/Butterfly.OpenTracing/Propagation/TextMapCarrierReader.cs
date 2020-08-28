using OpenTracing;
using OpenTracing.Propagation;
using System;

namespace Butterfly.OpenTracing.Propagation
{
    internal class TextMapCarrierReader : ICarrierReader
    {
        private readonly SpanContextFactory _spanContextFactory;

        public TextMapCarrierReader(SpanContextFactory spanContextFactory)
        {
            _spanContextFactory = spanContextFactory ?? throw new ArgumentNullException(nameof(spanContextFactory));
        }

        public ISpanContext Read(ITextMap carrier)
        {
            string traceId = null;
            string spanId = null;
            bool sampled = false;

            var baggage = new Baggage();

            foreach (var item in carrier)
            {
                if (item.Key.Equals(TextMapCarrierHelpers.prefix_traceId, StringComparison.OrdinalIgnoreCase))
                {
                    traceId = item.Value;
                }
                else if (item.Key.Equals(TextMapCarrierHelpers.prefix_spanId, StringComparison.OrdinalIgnoreCase))
                {
                    spanId = item.Value;
                }
                else if (item.Key.Equals(TextMapCarrierHelpers.prefix_sampled, StringComparison.OrdinalIgnoreCase))
                {
                    bool.TryParse(item.Value, out sampled);
                }
                else if (item.Key.StartsWith(TextMapCarrierHelpers.prefix_baggage, StringComparison.Ordinal))
                {
                    var key = item.Key.Substring(TextMapCarrierHelpers.prefix_baggage.Length);
                    baggage[key] = item.Value;
                }
            }

            if (traceId == null || spanId == null)
            {
                return null;
            }

            return _spanContextFactory.Create(new SpanContextPackage(traceId, spanId, sampled, baggage));
        }
    }
}