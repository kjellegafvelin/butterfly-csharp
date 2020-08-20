using System;
using Butterfly.OpenTracing;
using OpenTracing;

namespace Butterfly.Client.Tracing
{
    public class ServiceSpan : Span
    {
        private readonly ISpan _parent;
        private readonly ITracer _tracer;


        public ServiceSpan(Span span, ITracer tracer) : base(span.OperationName, span.StartTimestamp,
            span.Context, (Tracer)tracer)
        {
            _tracer = tracer;
            _parent = _tracer.GetCurrentSpan();
            _tracer.SetCurrentSpan(span);
        }

        public override void Finish(DateTimeOffset finishTimestamp)
        {
            base.Finish(finishTimestamp);
            _tracer.SetCurrentSpan(_parent);
        }

    }
}