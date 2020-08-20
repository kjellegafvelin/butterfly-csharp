using System.Linq;
using BaggageContract = Butterfly.DataContract.Tracing.Baggage;
using LogFieldContract = Butterfly.DataContract.Tracing.LogField;
using SpanReferenceContract = Butterfly.DataContract.Tracing.SpanReference;
using SpanContract = Butterfly.DataContract.Tracing.Span;
using LogContract = Butterfly.DataContract.Tracing.Log;
using TagContract = Butterfly.DataContract.Tracing.Tag;
using Butterfly.OpenTracing;

namespace Butterfly.Client.Tracing
{
    public static class SpanContractUtils
    {
        public static SpanContract CreateFromSpan(Span span)
        {
            var context = (SpanContext)span.Context;

            var spanContract = new SpanContract
            {
                FinishTimestamp = span.FinishTimestamp,
                StartTimestamp = span.StartTimestamp,
                Sampled = context.Sampled,
                SpanId = context.SpanId,
                TraceId = context.TraceId,
                OperationName = span.OperationName,
                Duration = (span.FinishTimestamp - span.StartTimestamp).GetMicroseconds()
            };

            spanContract.Baggages = context.Baggage?.Select(x => new BaggageContract { Key = x.Key, Value = x.Value }).ToList();
            spanContract.Logs = span.Logs?.Select(x =>
                new LogContract
                {
                    Timestamp = x.Timestamp,
                    Fields = x.Fields.Select(f => new LogFieldContract { Key = f.Key, Value = f.Value?.ToString() }).ToList()
                }).ToList();

            spanContract.Tags = span.Tags?.Select(x => new TagContract { Key = x.Key, Value = x.Value }).ToList();

            spanContract.References = context.References?.Select(x =>
                new SpanReferenceContract { ParentId = x.SpanContext.SpanId, Reference = x.SpanReferenceOptions.ToString() }).ToList();

            return spanContract;
        }
    }
}