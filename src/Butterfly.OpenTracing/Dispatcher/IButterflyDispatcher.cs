using System;
using SpanContract = Butterfly.DataContract.Tracing.Span;

namespace Butterfly.OpenTracing.Dispatcher
{
    internal interface IButterflyDispatcher : IDisposable
    {
        bool Dispatch(SpanContract span);
    }
}