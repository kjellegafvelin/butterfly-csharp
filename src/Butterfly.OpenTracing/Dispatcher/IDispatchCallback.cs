using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Butterfly.OpenTracing.Dispatcher
{
    internal interface IDispatchCallback
    {
        Func<DispatchableToken, bool> Filter { get; }

        Task Accept(IEnumerable<IDispatchable> dispatchables);
    }
}
