using BenchmarkDotNet.Attributes;
using Butterfly.Client.Benckmark.Formatter.Mock;
using Butterfly.OpenTracing;
using MessagePack;
using Microsoft.Extensions.Logging.Abstractions;
using OpenTracing;
using OpenTracing.Tag;
using System.Text.Json;

namespace Butterfly.Client.Benckmark.Formatter
{
    [MemoryDiagnoser]
    [AllStatisticsColumn]
    public class SerializationBenchmarks
    {
        private readonly CollectionSpanRecorder recorder = new CollectionSpanRecorder();

        public SerializationBenchmarks()
        {
            Initialization();
        }

        private void Initialization()
        {
            ITracer tracer = new Configuration("benchmark", new NullLoggerFactory())
                .WithRecorder(recorder)
                .BuildTracer();

            for (var i = 0; i < 250; i++)
            {
                using (var parentScope = tracer.BuildSpan("parent").StartActive(finishSpanOnDispose: true))
                {
                    var span = parentScope.Span;

                    span.Log(LogField.CreateNew().ServerReceive());

                    span.SetTag(Tags.SpanKind, Tags.SpanKindServer)
                        .SetTag(Tags.Component, "AspNetCore")
                        .SetTag(Tags.HttpMethod, "method")
                        .SetTag(Tags.HttpUrl, "url")
                        .SetTag("http.host", "host")
                        .SetTag("http.path", "Path")
                        .SetTag(Tags.HttpStatus, 200)
                        .SetTag(Tags.PeerHostIpv4, "ip")
                        .SetTag(Tags.PeerPort, 8080);

                    span.Log(LogField.CreateNew().ServerSend());

                    using (var childScope = tracer.BuildSpan("child").StartActive(finishSpanOnDispose: true))
                    {
                    }
                }
            }
        }

        [Benchmark]
        public void JsonFormatter()
        {
            JsonSerializer.Serialize(recorder.GetSpans());
        }

        [Benchmark]
        public void MessagePackFormatter()
        {
            MessagePackSerializer.Serialize(recorder.GetSpans());
        }
    }
}