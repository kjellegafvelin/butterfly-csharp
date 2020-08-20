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
        private bool ingoreActiveSpan;

        public string OperationName { get; }

        public DateTimeOffset? StartTimestamp { get; private set; }

        public Baggage Baggage { get; }

        public SpanReferenceCollection References { get; }

        private readonly TagCollection tags;
        private readonly Tracer tracer;
        private readonly ISpanContextFactory spanContextFactory;
        private readonly ISampler sampler;

        public bool? Sampled
        {
            get
            {
                var context = (SpanContext)References.FirstOrDefault()?.SpanContext;
                return context?.Sampled;
            }
        }

        internal SpanBuilder(Tracer tracer, string operationName, ISampler sampler, ISpanContextFactory spanContextFactory)
            : this(tracer, operationName, null, sampler, spanContextFactory)
        {
        }

        internal SpanBuilder(Tracer tracer, string operationName, DateTimeOffset? startTimestamp, ISampler sampler, ISpanContextFactory spanContextFactory)
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
            ingoreActiveSpan = true;
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
            throw new NotImplementedException();
        }

        public ISpan Start()
        {
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
            return new Span(this.OperationName, this.StartTimestamp ?? DateTimeOffset.UtcNow, spanContext, this.tracer);
        }
    }
}