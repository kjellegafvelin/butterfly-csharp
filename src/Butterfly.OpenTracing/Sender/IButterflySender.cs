using System.Threading;
using System.Threading.Tasks;
using SpanContract = Butterfly.DataContract.Tracing.Span;

namespace Butterfly.OpenTracing.Sender
{
    public interface IButterflySender
    {
        Task SendSpanAsync(SpanContract[] spans, CancellationToken cancellationToken = default(CancellationToken));
    }
}