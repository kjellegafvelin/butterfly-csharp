﻿using System.Collections.Generic;
using System.Linq;
using Butterfly.Client.Tracing;
using Butterfly.OpenTracing;

namespace Butterfly.Client.Benckmark.Formatter.Mock
{
    class CollectionSpanRecorder : ISpanRecorder
    {
        private readonly ICollection<Span> collection = new List<Span>();

        public void Record(Span span)
        {
            collection.Add(span);
        }

        public ICollection<DataContract.Tracing.Span> GetSpans()
        {
            return collection.Select(x => SpanContractUtils.CreateFromSpan(x)).ToList();
        }
    }
}