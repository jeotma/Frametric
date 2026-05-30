using System.Threading.Channels;
using Frametric.Application.Interfaces;

namespace Frametric.Infrastructure.BackgroundJobs;

public class TmdbEnrichmentTrigger : ITmdbEnrichmentTrigger
{
    private readonly Channel<bool> _channel;

    public TmdbEnrichmentTrigger()
    {
        // We only care about a single message: "wake up". 
        // Bounded channel of size 1, drop oldest if already signaled.
        _channel = Channel.CreateBounded<bool>(new BoundedChannelOptions(1)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });
    }

    public void TriggerEnrichment()
    {
        _channel.Writer.TryWrite(true);
    }

    public IAsyncEnumerable<bool> ReadAllAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
