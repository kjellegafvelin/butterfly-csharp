using Butterfly.OpenTracing.Dispatcher;
using Butterfly.OpenTracing.Recorder;
using Butterfly.OpenTracing.Sampler;
using Butterfly.OpenTracing.Sender;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Util;
using System;
using System.Collections.Generic;

namespace Butterfly.OpenTracing
{
    public class Configuration
    {
        private readonly ILoggerFactory loggerFactory;

        internal ButterflyOptions Options { get; private set; } = new ButterflyOptions();

        internal ISampler Sampler { get; private set; } = new FullSampler();

        internal SpanContextFactory SpanContextFactory { get; } = new SpanContextFactory();

        internal IScopeManager ScopeManager { get; private set; } = new AsyncLocalScopeManager();

        internal ISpanRecorder SpanRecorder { get; private set; }


        public Configuration(string serviceName, ILoggerFactory loggerFactory)
        {
            this.Options.Service = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            this.Options.CollectorUrl = "http://localhost:9618";
        }

        public Tracer BuildTracer()
        {
            if (this.SpanRecorder == null)
            {
                var sender = new HttpButterflySender(this.Options.CollectorUrl);
                var callback = new SpanDispatchCallback(sender, this.loggerFactory);
                var dispatcher = new ButterflyDispatcher(new List<IDispatchCallback>() { callback },
                    loggerFactory,
                    this.Options.FlushInterval,
                    this.Options.BoundedCapacity,
                    this.Options.ConsumerCount);

                this.SpanRecorder = new AsyncSpanRecorder(dispatcher);
            }

            return new Tracer(this.Options.Service, this.SpanRecorder, this.Sampler, this.SpanContextFactory, this.ScopeManager);
        }

        public Configuration WithOptions(ButterflyOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrEmpty(options.Service))
            {
                throw new ArgumentException("Invalid service name.", "options.Service");
            }

            if (!Uri.TryCreate(options.CollectorUrl, UriKind.Absolute, out Uri _))
            {
                throw new ArgumentException("Invalid collector url.", "options.CollectorUrl");
            }

            this.Options = options;

            return this;
        }

        public Configuration WithRecorder(ISpanRecorder spanRecorder)
        {
            this.SpanRecorder = spanRecorder ?? throw new ArgumentNullException(nameof(spanRecorder));
            return this;
        }

        public Configuration WithSampler(ISampler sampler)
        {
            this.Sampler = sampler ?? throw new ArgumentNullException(nameof(sampler));
            return this;
        }

        public Configuration WithScopeManager(IScopeManager scopeManager)
        {
            this.ScopeManager = scopeManager ?? throw new ArgumentNullException(nameof(scopeManager));
            return this;
        }
    }
}
