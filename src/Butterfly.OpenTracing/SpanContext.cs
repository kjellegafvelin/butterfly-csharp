using OpenTracing;
using System;
using System.Collections.Generic;

namespace Butterfly.OpenTracing
{
    public class SpanContext : ISpanContext
    {
        public string TraceId { get; }

        public string SpanId { get; }

        public bool Sampled { get; }

        public Baggage Baggage { get; }

        public SpanReferenceCollection References { get; }

        public SpanContext(string traceId, string spanId, bool sampled, Baggage baggage, SpanReferenceCollection references)
        {
            TraceId = traceId;
            SpanId = spanId;
            Sampled = sampled;
            Baggage = baggage ?? throw new ArgumentNullException(nameof(baggage));
            References = references ?? SpanReferenceCollection.Empty;
        }

        public IEnumerable<KeyValuePair<string, string>> GetBaggageItems()
        {
            return this.Baggage;
        }

        public string GetBaggageItem(string key)
        {
            return this.Baggage.TryGetValue(key, out string value) ? value : null;
        }
    }
}