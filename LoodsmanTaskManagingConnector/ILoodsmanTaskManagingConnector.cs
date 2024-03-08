using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LoodsmanTaskManagingConnectorObjects.Components.Notifications;
using LoodsmanTaskManagingConnectorObjects.Components.Polling;
using LoodsmanTaskManagingConnectorObjects.Components.Processing;
using Serilog;

namespace LoodsmanTaskManagingConnectorObjects
{
    public interface ILoodsmanTaskManagingConnector
    {
        ILogger Logger { get; set; }

        ITaskManagingRequestPoller RequestPoller { get; set; }

        ICollection<ITaskManagingRequestProcessor> RequestsProcessors { get; set; }

        ITaskManagingReplyNotificator ReplyNotificator { get; set; }

        void Run();

        Task RunAsync(CancellationToken cancellationToken = default);
    }
}