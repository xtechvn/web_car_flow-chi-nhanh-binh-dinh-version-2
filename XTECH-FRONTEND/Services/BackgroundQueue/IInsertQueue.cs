using System.Threading.Channels;
using XTECH_FRONTEND.Model;
namespace XTECH_FRONTEND.Services.BackgroundQueue
{
    public interface IInsertQueue
    {
        ValueTask EnqueueAsync(InsertJob job);
        ValueTask<InsertJob> DequeueAsync(CancellationToken cancellationToken);
    }
}
