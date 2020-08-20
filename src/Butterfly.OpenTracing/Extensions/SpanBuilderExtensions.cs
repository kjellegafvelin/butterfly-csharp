using OpenTracing;
using System;

namespace Butterfly.OpenTracing
{
    public static class SpanBuilderExtensions
    {
        public static ISpanBuilder AsChildOf(this ISpanBuilder spanBuilder, ISpanContext spanContext)
        {
            if (spanBuilder == null)
            {
                throw new ArgumentNullException(nameof(spanBuilder));
            }
            spanBuilder.AddReference(References.ChildOf, spanContext);
            return spanBuilder;
        }

        public static ISpanBuilder FollowsFrom(this ISpanBuilder spanBuilder, ISpanContext spanContext)
        {
            if (spanBuilder == null)
            {
                throw new ArgumentNullException(nameof(spanBuilder));
            }
            spanBuilder.AddReference(References.FollowsFrom, spanContext);
            return spanBuilder;
        }
    }
}
