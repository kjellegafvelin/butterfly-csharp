
namespace Butterfly.OpenTracing
{
    public interface ISpanRecorder
    {
        void Record(Span span);
    }
}