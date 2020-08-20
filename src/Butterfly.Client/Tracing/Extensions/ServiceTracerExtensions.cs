using System;
using System.Threading.Tasks;
using Butterfly.OpenTracing;
using OpenTracing;

namespace Butterfly.Client.Tracing
{
    public static class ServiceTracerExtensions
    {
        public static async Task ChildTraceAsync(this IServiceTracer tracer, string operationName, DateTimeOffset? startTimestamp, Func<Span, Task> operation)
        {
            if (tracer == null)
            {
                throw new ArgumentNullException(nameof(tracer));
            }

            var spanBuilder = CreateChildSpanBuilder(tracer, operationName, startTimestamp);

            using (var span = (Span)tracer.Start(spanBuilder))
            {
                try
                {
                    await operation?.Invoke(span);
                }
                catch (Exception exception)
                {
                    span.Tags.Error(true);
                    span.Log(LogField.CreateNew().EventError().ErrorKind(exception).Message(exception.Message));

                    throw;
                }
            }
        }

        public static async Task<TResult> ChildTraceAsync<TResult>(this IServiceTracer tracer, string operationName, DateTimeOffset? startTimestamp, Func<Span, Task<TResult>> operation)
        {
            if (tracer == null)
            {
                throw new ArgumentNullException(nameof(tracer));
            }

            var spanBuilder = CreateChildSpanBuilder(tracer, operationName, startTimestamp);

            using (var span = (Span)tracer.Start(spanBuilder))
            {
                try
                {
                    return await operation?.Invoke(span);
                }
                catch (Exception exception)
                {
                    span.Tags.Error(true);
                    span.Log(LogField.CreateNew().EventError().ErrorKind(exception).Message(exception.Message));

                    throw;
                }
            }
        }

        public static ISpan StartChild(this IServiceTracer tracer, string operationName, DateTimeOffset? startTimestamp = null)
        {
            return tracer.Start(CreateChildSpanBuilder(tracer, operationName, startTimestamp));
        }

        public static ISpan Start(this IServiceTracer tracer, string operationName, DateTimeOffset? startTimestamp = null)
        {
            var spanBuilder = tracer.Tracer.BuildSpan(operationName);
            if (startTimestamp != null)
            {
                spanBuilder.WithStartTimestamp(startTimestamp.Value);
            }
            return spanBuilder.Start();
        }


        private static ISpanBuilder CreateChildSpanBuilder(IServiceTracer tracer, string operationName, DateTimeOffset? startTimestamp = null)
        {
            var spanBuilder = tracer.Tracer.BuildSpan(operationName);
            
            if (startTimestamp != null)
            {
                spanBuilder.WithStartTimestamp(startTimestamp.Value);
            }

            var spanContext = tracer.Tracer.GetCurrentSpan()?.Context;

            if (spanContext != null)
            {
                spanBuilder.AsChildOf(spanContext);
            }

            return spanBuilder;
        }
    }
}