using System.Threading;
using System.Threading.Tasks;
using UMP.Loodsman.Dtos.SUPR.Exchange;

namespace LoodsmanTaskManagingConnectorObjects.Components.Notifications
{
    public interface ITaskManagingReplyNotificator
    {
        void SendNewDocumentBasedTasksCreationReply(TaskManagingReply tasksCreationReply);

        Task SendNewDocumentBasedTasksCreationReplyAsync(TaskManagingReply tasksCreationReply, CancellationToken cancellationToken = default);
    }
}