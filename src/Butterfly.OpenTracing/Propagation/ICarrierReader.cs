using OpenTracing;
using OpenTracing.Propagation;

namespace Butterfly.OpenTracing.Propagation
{
    public interface ICarrierReader
    {
        ISpanContext Read(ITextMap carrier);
    }
}