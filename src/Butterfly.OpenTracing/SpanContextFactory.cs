﻿using OpenTracing;

namespace Butterfly.OpenTracing
{
    internal class SpanContextFactory
    {
        public ISpanContext Create(SpanContextPackage spanContextPackage)
        {
            return new SpanContext(
                spanContextPackage.TraceId ?? RandomUtils.NextLong().ToString(),
                spanContextPackage.SpanId ?? RandomUtils.NextLong().ToString(),
                spanContextPackage.Sampled,
                spanContextPackage.Baggage ?? new Baggage(),
                spanContextPackage.References);
        }
    }
}