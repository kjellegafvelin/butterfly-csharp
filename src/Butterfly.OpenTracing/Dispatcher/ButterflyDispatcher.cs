using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SpanContract = Butterfly.DataContract.Tracing.Span;
using Microsoft.Extensions.Logging;

namespace Butterfly.OpenTracing.Dispatcher
{
    internal class ButterflyDispatcher : IButterflyDispatcher
    {
        private const int DefaultBoundedCapacity = 1000000;
        private const int DefaultInterval = 5;
        private readonly int DefaultConsumerCount = Environment.ProcessorCount;
        private readonly int _boundedCapacity;
        private readonly int _consumerCount;
        private readonly int _flushInterval;
        private readonly BlockingCollection<IDispatchable> _limitCollection;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IEnumerable<IDispatchCallback> _callbacks;
        private readonly ICollection<Task> _consumerTasks;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        public ButterflyDispatcher(IEnumerable<IDispatchCallback> callbacks, ILoggerFactory loggerFactory, int flushInterval, int boundedCapacity, int consumerCount)
        {
            _callbacks = callbacks;
            _consumerCount = consumerCount <= 0 ? DefaultConsumerCount : consumerCount;
            _boundedCapacity = boundedCapacity <= 0 ? DefaultBoundedCapacity : boundedCapacity;
            _flushInterval = flushInterval <= 0 ? DefaultInterval : flushInterval;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger(typeof(ButterflyDispatcher));
            _cancellationTokenSource = new CancellationTokenSource();
            _limitCollection = InitializationLimitCollection(_boundedCapacity);
            _consumerTasks = InitializationConsumer(_consumerCount);
        }

        private BlockingCollection<IDispatchable> InitializationLimitCollection(int boundedCapacity)
        {
            _logger.LogInformation($"Butterfly Client initialized LimitQueue with options: BoundedCapacity={boundedCapacity}");
            return new BlockingCollection<IDispatchable>(boundedCapacity);
        }

        private ICollection<Task> InitializationConsumer(int consumerCount)
        {
            var consumerList = new List<Task>(consumerCount);
            for (var i = 0; i < _consumerCount; i++)
            {
                consumerList.Add(CreateConsumer());
            }
            _logger.LogInformation($"Butterfly Client initialized ConsumerTasks with options: ConsumerCount={_consumerCount} FlushInterval={_flushInterval}");
            return consumerList;
        }

        private Task CreateConsumer()
        {
            return Task.Factory.StartNew(
                Consumer, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public bool Dispatch(SpanContract span)
        {
            if (_limitCollection.IsAddingCompleted)
            {
                return false;
            }

            _limitCollection.Add(new Dispatchable<SpanContract>(DispatchableToken.SpanToken, span));
            return true;
        }

        private void Consumer()
        {
            using (var handler = new TimerDispatchHandler(_callbacks, _loggerFactory, _flushInterval))
            {
                foreach (var consumingItem in _limitCollection.GetConsumingEnumerable(_cancellationTokenSource.Token))
                    handler.Post(consumingItem);
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _limitCollection?.CompleteAdding();
        }
    }
}