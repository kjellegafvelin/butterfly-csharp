using Butterfly.OpenTracing.Dispatcher;
using System;

namespace Butterfly.OpenTracing.Recorder
{
    internal class AsyncSpanRecorder : ISpanRecorder
    {
        private readonly IButterflyDispatcher _dispatcher;

        internal AsyncSpanRecorder(IButterflyDispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public void Record(Span span)
        {
            _dispatcher.Dispatch(SpanContractUtils.CreateFromSpan(span));
        }
    }
}