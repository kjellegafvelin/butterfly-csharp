using System;
using System.Collections.Generic;
using System.Text;

namespace Butterfly.OpenTracing
{
    public class Configuration
    {
        public string ServiceName { get; }

        public ISampler Sampler { get; private set; } = new FullSampler();

        public ISpanContextFactory SpanContextFactory { get; private set; } = new SpanContextFactory();
       

        public Configuration(string serviceName)
        {
            ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        }

        public Tracer BuildTracer()
        {

            ISpanRecorder spanRecorder = null;

            return new Tracer(spanRecorder, this.Sampler, this.SpanContextFactory);
        }

        public Configuration WithSampler(ISampler sampler)
        {
            this.Sampler = sampler ?? throw new ArgumentNullException(nameof(sampler));
            return this;
        }

        public Configuration WithSpanContextFactory(ISpanContextFactory spanContextFactory)
        {
            this.SpanContextFactory = spanContextFactory ?? throw new ArgumentNullException(nameof(spanContextFactory));
            return this;
        }
    }
}
