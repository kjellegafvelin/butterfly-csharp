using OpenTracing;
using OpenTracing.Propagation;
using System;
using System.Threading.Tasks;

namespace Butterfly.OpenTracing
{
    public class Tracer : ITracer
    {
        private readonly ISpanContextFactory _spanContextFactory;
        private readonly ISampler _sampler;

        public Tracer(ISpanRecorder spanRecorder, ISampler sampler = null, ISpanContextFactory spanContextFactory = null)
        {
            Recorder = spanRecorder ?? throw new ArgumentNullException(nameof(spanRecorder));
            _sampler = sampler ?? new FullSampler();
            _spanContextFactory = spanContextFactory ?? new SpanContextFactory();
        }

        public ISpanRecorder Recorder { get; }

        public IScopeManager ScopeManager => throw new NotImplementedException();

        public ISpan ActiveSpan => throw new NotImplementedException();

        public ISpanBuilder BuildSpan(string operationName)
        {
            if (operationName == null)
            {
                throw new ArgumentNullException(nameof(operationName));
            }

            return new SpanBuilder(this, operationName, _sampler, _spanContextFactory);

        }

        public ISpanContext Extract(ICarrierReader carrierReader, ICarrier carrier)
        {
            if (carrierReader == null)
            {
                throw new ArgumentNullException(nameof(carrierReader));
            }

            return carrierReader.Read(carrier);
        }

        public ISpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            throw new NotImplementedException();
        }

        public Task<ISpanContext> ExtractAsync(ICarrierReader carrierReader, ICarrier carrier)
        {
            return carrierReader.ReadAsync(carrier);
        }

        public void Inject(ISpanContext spanContext, ICarrierWriter carrierWriter, ICarrier carrier)
        {
            if (carrierWriter == null)
            {
                throw new ArgumentNullException(nameof(carrierWriter));
            }

            if (spanContext == null)
            {
                throw new ArgumentNullException(nameof(spanContext));
            }

            carrierWriter.Write(spanContext.Package(), carrier);
        }

        public void Inject<TCarrier>(ISpanContext spanContext, IFormat<TCarrier> format, TCarrier carrier)
        {
            throw new NotImplementedException();
        }

        public Task InjectAsync(ISpanContext spanContext, ICarrierWriter carrierWriter, ICarrier carrier)
        {
            if (carrierWriter == null)
            {
                throw new ArgumentNullException(nameof(carrierWriter));
            }

            if (spanContext == null)
            {
                throw new ArgumentNullException(nameof(spanContext));
            }

            return carrierWriter.WriteAsync(spanContext.Package(), carrier);
        }
    }
}