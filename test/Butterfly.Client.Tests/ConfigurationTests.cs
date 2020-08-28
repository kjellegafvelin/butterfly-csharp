using Butterfly.OpenTracing;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Butterfly.Client.Tests
{
    public class ConfigurationTests
    {
        [Fact]
        public void SimpleConfigurationTest()
        {
            var tracer = new Configuration("test", new NullLoggerFactory()).BuildTracer();
            
            Assert.NotNull(tracer);
        }
    }
}