using OpenTracing;
using System.Threading;

namespace Butterfly.OpenTracing
{
    internal static class SpanLocal
    {
        private static readonly AsyncLocal<ISpan> AsyncLocal = new AsyncLocal<ISpan>();

        public static ISpan Current
        {
            get
            {
                return AsyncLocal.Value;
            }
            set
            {
                AsyncLocal.Value = value;
            }
        }
    }
}