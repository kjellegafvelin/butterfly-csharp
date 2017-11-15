﻿using System.Linq;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Injector;
using StackExchange.Redis;

#pragma warning disable 4014

namespace AspectCore.APM.RedisProfiler
{
    public sealed class RedisProfiledInterceptor : AbstractInterceptor
    {
        public async override Task Invoke(AspectContext context, AspectDelegate next)
        {
            var connectionMultiplexer = context.ServiceProvider.ResolveRequired<IConnectionMultiplexer>();
            var redisProfilerCallbackHandler = context.ServiceProvider.ResolveRequired<IRedisProfilerCallbackHandler>();
            var profilerContext = new object();
            AspectRedisDatabaseProfilerContext.Context = profilerContext;
            connectionMultiplexer.BeginProfiling(profilerContext);
            await context.Invoke(next);
            var profiledCommands = connectionMultiplexer.FinishProfiling(profilerContext);
            redisProfilerCallbackHandler.HandleAsync(new RedisProfilerCallbackHandlerContext(
                profiledCommands.Select(x => 
                RedisProfiledCommand.Create(x.Command, x.EndPoint, x.Db, x.CommandCreated, x.ElapsedTime)).ToArray()));
            AspectRedisDatabaseProfilerContext.Context = null;
        }
    }
}