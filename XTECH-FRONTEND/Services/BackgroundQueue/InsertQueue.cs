using System.Threading.Channels;
using XTECH_FRONTEND.Model;
namespace XTECH_FRONTEND.Services.BackgroundQueue
{
    public class InsertQueue : IInsertQueue
    {
        private readonly Channel<InsertJob> _queue;

        public InsertQueue()
        {
            _queue = Channel.CreateUnbounded<InsertJob>();
        }

        public ValueTask EnqueueAsync(InsertJob job)
            => _queue.Writer.WriteAsync(job);

        public ValueTask<InsertJob> DequeueAsync(CancellationToken cancellationToken)
            => _queue.Reader.ReadAsync(cancellationToken);
    }
}
