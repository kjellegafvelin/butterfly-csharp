using OpenTracing;
using System;

namespace Butterfly.OpenTracing
{
    public static class SpanContextExtensions
    {
        public static SpanContextPackage Package(this ISpanContext spanContext)
        {
            if (spanContext == null)
            {
                throw new ArgumentNullException(nameof(spanContext));
            }
            var context = (SpanContext)spanContext;

            return new SpanContextPackage(context.TraceId, context.SpanId, context.Sampled, context.Baggage, null);
        }

        public static ISpanContext SetBaggage(this ISpanContext spanContext, string key, string value)
        {
            if (spanContext == null)
            {
                throw new ArgumentNullException(nameof(spanContext));
            }

            spanContext.SetBaggage(key, value);
            return spanContext;
        }
    }
}