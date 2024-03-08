using System.Threading;
using System.Threading.Tasks;
using UMP.Loodsman.Dtos.SUPR.Exchange;

namespace LoodsmanTaskManagingConnectorObjects.Components.Processing
{
    public interface ITaskManagingRequestProcessor
    {
        TaskManagingReply ProcessRequest(TaskManagingRequest request);

        Task<TaskManagingReply> ProcessRequestAsync(TaskManagingRequest request,
            CancellationToken cancellationToken = default);
    }
}