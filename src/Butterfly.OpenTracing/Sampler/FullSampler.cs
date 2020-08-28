using Butterfly.OpenTracing;

namespace Butterfly.OpenTracing.Sampler
{
    public class FullSampler : ISampler
    {
        public bool ShouldSample()
        {
            return true;
        }
    }
}