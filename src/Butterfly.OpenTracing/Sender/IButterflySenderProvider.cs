namespace Butterfly.OpenTracing.Sender
{
    public interface IButterflySenderProvider
    {
        IButterflySender GetSender();
    }
}