
namespace Butterfly.OpenTracing.Recorder
{
    public interface ISpanRecorder
    {
        void Record(Span span);
    }
}