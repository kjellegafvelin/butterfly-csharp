using Butterfly.OpenTracing.Propagation;
using Butterfly.OpenTracing.Recorder;
using Butterfly.OpenTracing.Sampler;
using OpenTracing;
using OpenTracing.Propagation;
using System;

namespace Butterfly.OpenTracing
{
    public class Tracer : ITracer
    {
        private readonly SpanContextFactory _spanContextFactory;
        private readonly ISampler _sampler;

        public string ServiceName { get; }

        internal Tracer(string serviceName, ISpanRecorder spanRecorder, ISampler sampler, SpanContextFactory spanContextFactory, IScopeManager scopeManager)
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                throw new ArgumentException(nameof(serviceName));
            }
            this.ServiceName = serviceName;
            this.Recorder = spanRecorder ?? throw new ArgumentNullException(nameof(spanRecorder));
            this.ScopeManager = scopeManager ?? throw new ArgumentNullException(nameof(scopeManager));
            _sampler = sampler ?? new FullSampler();
            _spanContextFactory = spanContextFactory ?? throw new ArgumentNullException(nameof(spanContextFactory));
        }

        public ISpanRecorder Recorder { get; }

        public IScopeManager ScopeManager { get; }

        public ISpan ActiveSpan => ScopeManager.Active?.Span;

        public ISpanBuilder BuildSpan(string operationName)
        {
            if (operationName == null)
            {
                throw new ArgumentNullException(nameof(operationName));
            }

            return new SpanBuilder(this, operationName, _sampler, _spanContextFactory);
        }

        public ISpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            return new TextMapCarrierReader(this._spanContextFactory).Read(carrier as ITextMap);
        }

        public void Inject<TCarrier>(ISpanContext spanContext, IFormat<TCarrier> format, TCarrier carrier)
        {
            new TextMapCarrierWriter().Write(spanContext.Package(), carrier as ITextMap);
        }
    }
}