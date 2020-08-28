namespace Butterfly.OpenTracing.Sampler
{
    public interface ISampler
    {
        bool ShouldSample();
    }
}