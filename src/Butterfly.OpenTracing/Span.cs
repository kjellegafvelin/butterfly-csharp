using OpenTracing;
using OpenTracing.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Butterfly.OpenTracing
{
    public class Span : ISpan, IDisposable
    {
        private readonly Tracer _tracer;
        private DateTimeOffset _finishTimestamp;
        private int _state;

        public DateTimeOffset StartTimestamp { get; }

        public DateTimeOffset FinishTimestamp => _finishTimestamp;

        public TagCollection Tags { get; }
        
        public LogCollection Logs { get; }

        public string OperationName { get; private set; }

        public ISpanContext Context { get; }

        public Span(string operationName, DateTimeOffset startTimestamp, ISpanContext spanContext, Tracer tracer)
        {
            _state = 0;
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
            Context = spanContext ?? throw new ArgumentNullException(nameof(spanContext));
            Tags = new TagCollection();
            Logs = new LogCollection();
            OperationName = operationName;
            StartTimestamp = startTimestamp;
        }

        public void Dispose()
        {
            Finish();
        }

        public virtual void Finish(DateTimeOffset finishTimestamp)
        {
            if (Interlocked.CompareExchange(ref _state, 1, 0) != 1)
            {
                _finishTimestamp = DateTime.UtcNow;
                _tracer.Recorder.Record(this);
            }
        }

        public ISpan SetTag(string key, string value)
        {
            Tags[key] = value;
            return this;
        }

        public ISpan SetTag(string key, bool value)
        {
            Tags[key] = value.ToString();
            return this;
        }

        public ISpan SetTag(string key, int value)
        {
            Tags[key] = value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
            return this;
        }

        public ISpan SetTag(string key, double value)
        {
            Tags[key] = value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
            return this;
        }

        public ISpan SetTag(BooleanTag tag, bool value)
        {
            Tags[tag.Key] = value.ToString();
            return this;
        }

        public ISpan SetTag(IntOrStringTag tag, string value)
        {
            Tags[tag.Key] = value;
            return this;
        }

        public ISpan SetTag(IntTag tag, int value)
        {
            Tags[tag.Key] = value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
            return this;
        }

        public ISpan SetTag(StringTag tag, string value)
        {
            Tags[tag.Key] = value;
            return this;
        }

        public ISpan Log(IEnumerable<KeyValuePair<string, object>> fields)
        {
            Log(DateTimeOffset.UtcNow, fields);
            return this;
        }

        public ISpan Log(DateTimeOffset timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            Logs.Add(new LogData(timestamp.DateTime, (IDictionary<string, object>)fields));
            return this;
        }

        public ISpan Log(string @event)
        {
            Log(DateTimeOffset.UtcNow, @event);
            return this;
        }

        public ISpan Log(DateTimeOffset timestamp, string @event)
        {
            Logs.Add(new LogData(timestamp.DateTime, @event));
            return this;
        }

        public ISpan SetBaggageItem(string key, string value)
        {
            this.Context.SetBaggage(key, value);
            return this;
        }

        public string GetBaggageItem(string key)
        {
            return ((SpanContext)this.Context).GetBaggageItem(key);

        }

        public ISpan SetOperationName(string operationName)
        {
            this.OperationName = operationName;
            return this;
        }

        public void Finish()
        {
            Finish(DateTimeOffset.UtcNow);
        }
    }
}