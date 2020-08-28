using Butterfly.Client.Tracing;
using Butterfly.OpenTracing.Sampler;
using OpenTracing;
using OpenTracing.Tag;
using System;
using System.Globalization;
using System.Linq;
using OT = OpenTracing;

namespace Butterfly.OpenTracing
{
    public class SpanBuilder : ISpanBuilder
    {
        private bool ignoreActiveSpan;

        internal string OperationName { get; }

        internal DateTimeOffset? StartTimestamp { get; private set; }

        internal Baggage Baggage { get; }

        internal SpanReferenceCollection References { get; }

        private readonly TagCollection tags;
        private readonly Tracer tracer;
        private readonly SpanContextFactory spanContextFactory;
        private readonly ISampler sampler;

        public bool? Sampled
        {
            get
            {
                var context = (SpanContext)References.FirstOrDefault()?.SpanContext;
                return context?.Sampled;
            }
        }

        internal SpanBuilder(Tracer tracer, string operationName, ISampler sampler, SpanContextFactory spanContextFactory)
            : this(tracer, operationName, null, sampler, spanContextFactory)
        {
        }

        internal SpanBuilder(Tracer tracer, string operationName, DateTimeOffset? startTimestamp, ISampler sampler, SpanContextFactory spanContextFactory)
        {
            this.tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
            this.OperationName = operationName ?? throw new ArgumentNullException(nameof(operationName));
            this.spanContextFactory = spanContextFactory ?? throw new ArgumentNullException(nameof(spanContextFactory));
            this.sampler = sampler ?? throw new ArgumentNullException(nameof(sampler));

            StartTimestamp = startTimestamp;
            Baggage = new Baggage();
            References = new SpanReferenceCollection();
            tags = new TagCollection();
        }

        public ISpanBuilder AsChildOf(ISpanContext parent)
        {
            return AddReference(OT.References.ChildOf, parent);
        }

        public ISpanBuilder AsChildOf(ISpan parent)
        {
            return AsChildOf(parent?.Context);
        }

        public ISpanBuilder AddReference(string referenceType, ISpanContext referencedContext)
        {
            if (referencedContext == null)
            {
                return this;
            }

            SpanReferenceOptions spanReferenceOption;

            switch (referenceType)
            {
                case OT.References.ChildOf:
                    spanReferenceOption = SpanReferenceOptions.ChildOf;
                    break;
                case OT.References.FollowsFrom:
                    spanReferenceOption = SpanReferenceOptions.FollowsFrom;
                    break;
                default:
                    return this;
            }

            this.References.Add(new SpanReference(spanReferenceOption, referencedContext));
            return this;
        }

        public ISpanBuilder IgnoreActiveSpan()
        {
            this.ignoreActiveSpan = true;
            return this;
        }

        public ISpanBuilder WithTag(string key, string value)
        {
            this.tags.Add(key, value);
            return this;
        }

        public ISpanBuilder WithTag(string key, bool value)
        {
            return this.WithTag(key, value.ToString());
        }

        public ISpanBuilder WithTag(string key, int value)
        {
            return this.WithTag(key, value.ToString(NumberFormatInfo.InvariantInfo));
        }

        public ISpanBuilder WithTag(string key, double value)
        {
            return this.WithTag(key, value.ToString(NumberFormatInfo.InvariantInfo));
        }

        public ISpanBuilder WithTag(BooleanTag tag, bool value)
        {
            return this.WithTag(tag.Key, value.ToString());
        }

        public ISpanBuilder WithTag(IntOrStringTag tag, string value)
        {
            return this.WithTag(tag.Key, value);
        }

        public ISpanBuilder WithTag(IntTag tag, int value)
        {
            return this.WithTag(tag.Key, value.ToString(NumberFormatInfo.InvariantInfo));
        }

        public ISpanBuilder WithTag(StringTag tag, string value)
        {
            return this.WithTag(tag.Key, value);
        }

        public ISpanBuilder WithStartTimestamp(DateTimeOffset timestamp)
        {
            if (this.StartTimestamp != null)
            {
                this.StartTimestamp = timestamp;
            }

            return this;
        }

        public IScope StartActive()
        {
            return this.StartActive(finishSpanOnDispose: true);
        }

        public IScope StartActive(bool finishSpanOnDispose)
        {
            return tracer.ScopeManager.Activate(this.Start(), finishSpanOnDispose);
        }

        public ISpan Start()
        {
            var activeSpanContext = this.tracer.ActiveSpan?.Context;

            if (!this.References.Any() && !this.ignoreActiveSpan && activeSpanContext != null)
            {
                this.AsChildOf(activeSpanContext);
            }

            var traceId = this.References?.FirstOrDefault()?.SpanContext?.TraceId;

            var baggage = new Baggage();

            if (this.References != null)
            {
                foreach (var reference in this.References)
                {
                    var context = (SpanContext)reference.SpanContext;
                    baggage.Merge(context.Baggage);
                }
            }

            var sampled = this.Sampled ?? this.sampler.ShouldSample();
            var spanContext = this.spanContextFactory.Create(new SpanContextPackage(traceId, null, sampled, baggage, this.References));
            return new Span(this.OperationName, this.StartTimestamp ?? DateTimeOffset.UtcNow, spanContext, this.tracer, this.tags)
                .SetTag(Tags.Service, this.tracer.ServiceName)
                .SetTag(ServiceTags.ServiceHost, Environment.MachineName);
        }
    }
}