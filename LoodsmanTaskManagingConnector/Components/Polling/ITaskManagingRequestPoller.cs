using System.Collections.Generic;
using System.Threading;
using UMP.Loodsman.Dtos.SUPR.Exchange;

namespace LoodsmanTaskManagingConnectorObjects.Components.Polling
{
    public interface ITaskManagingRequestPoller
    {
        IAsyncEnumerable<TaskManagingRequest> StartTaskManagingRequestStream(
            CancellationToken cancellationToken = default);
    }
}