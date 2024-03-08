using System.Threading;
using System.Threading.Tasks;
using MessagingServicing;
using UMP.Loodsman.Dtos.SUPR.Exchange;

namespace LoodsmanTaskManagingConnectorObjects.Components.Notifications
{
    public class MessagingTaskManagingReplyNotificator : ITaskManagingReplyNotificator
    {
        private readonly IMessagingService messagingService;

        public MessagingTaskManagingReplyNotificator(IMessagingService messagingService)
        {
            this.messagingService = messagingService;
        }

        public void SendNewDocumentBasedTasksCreationReply(TaskManagingReply tasksCreationReply)
        {
            ConnectMessagingServiceIfNecessary();

            messagingService.AcknowledgeMessage(tasksCreationReply.RequestId);
            messagingService.Send(tasksCreationReply);
        }

        private void ConnectMessagingServiceIfNecessary()
        {
            if (!messagingService.Connected)
                messagingService.Connected = true;
        }

        public async Task SendNewDocumentBasedTasksCreationReplyAsync(TaskManagingReply tasksCreationReply, CancellationToken cancellationToken = default)
        {
            await ConnectMessagingServiceIfNecessaryAsync(cancellationToken);

            await messagingService.AcknowledgeMessageAsync(tasksCreationReply.RequestId, cancellationToken);
            await messagingService.SendAsync(tasksCreationReply, cancellationToken);
        }

        private Task ConnectMessagingServiceIfNecessaryAsync(CancellationToken cancellationToken = default) =>
            !messagingService.Connected ? messagingService.OpenConnectionAsync(cancellationToken) : Task.CompletedTask;
    }
}