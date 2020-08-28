using OpenTracing;
using System;

namespace Butterfly.OpenTracing
{
    internal static class SpanContextExtensions
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
    }
}