using OpenTracing.Propagation;

namespace Butterfly.OpenTracing.Propagation
{
    internal interface ICarrierWriter
    {
        void Write(SpanContextPackage spanContext, ITextMap carrier);
    }
}