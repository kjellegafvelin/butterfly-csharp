using Butterfly.OpenTracing;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Util;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddButterfly(this IServiceCollection services, Action<ButterflyOptions> options)
        {
            var bfOptions = new ButterflyOptions();

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Invoke(bfOptions);

            services.AddSingleton<ITracer>(sp =>
            {
                var loggerFactory = sp.GetService<ILoggerFactory>();
                var tracer = new Configuration(bfOptions.Service, loggerFactory)
                                    .WithOptions(bfOptions)
                                    .BuildTracer();

                GlobalTracer.Register(tracer);

                return tracer;
            });

            return services;
        }
    }
}
