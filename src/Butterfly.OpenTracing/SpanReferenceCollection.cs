using System.Collections.ObjectModel;

namespace Butterfly.OpenTracing
{
    internal class SpanReferenceCollection : Collection<SpanReference>
    {
        public static readonly SpanReferenceCollection Empty = new SpanReferenceCollection();
    }
}