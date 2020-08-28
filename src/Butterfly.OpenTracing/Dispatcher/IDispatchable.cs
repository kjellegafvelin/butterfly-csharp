using System;

namespace Butterfly.OpenTracing.Dispatcher
{
    internal interface IDispatchable
    {
        DispatchableToken Token { get; }

        object RawInstance { get; }

        SendState State { get; set; }

        int ErrorCount { get; }

        int Error();

        DateTimeOffset Timestamp { get; }
    }

    internal class Dispatchable<T> : IDispatchable
    {
        public DispatchableToken Token { get; }

        public object RawInstance { get; }

        public T Instance { get; }

        public Dispatchable(string token, T instance)
        {
            Token = token;
            RawInstance = Instance = instance;
        }

        private int _counter = 0;

        public SendState State { get; set; } = SendState.Untreated;

        public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;

        public int ErrorCount
        {
            get { return _counter; }
        }

        public int Error()
        {
            return System.Threading.Interlocked.Increment(ref _counter);
        }
    }

    internal enum SendState
    {
        Untreated = 0,
        Sending = 1,
        Sended = 2
    }
}