using OpenTracing.Propagation;
using System;

namespace Butterfly.OpenTracing.Propagation
{
    internal class TextMapCarrierWriter : ICarrierWriter
    {
        public void Write(SpanContextPackage spanContext, ITextMap carrier)
        {
            if (carrier == null)
            {
                throw new ArgumentNullException(nameof(carrier));
            }

            carrier.Set(TextMapCarrierHelpers.prefix_traceId, spanContext.TraceId);
            carrier.Set(TextMapCarrierHelpers.prefix_spanId, spanContext.SpanId);
            carrier.Set(TextMapCarrierHelpers.prefix_sampled, spanContext.Sampled.ToString());

            foreach (var item in spanContext.Baggage)
            {
                carrier.Set(TextMapCarrierHelpers.prefix_baggage + item.Key, item.Value);
            }
        }
    }
}