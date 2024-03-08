using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MessagingServicing;
using UMP.Loodsman.Dtos.SUPR.Exchange;

namespace LoodsmanTaskManagingConnectorObjects.Components.Polling
{
    public class MessagingTaskManagingRequestPoller : ITaskManagingRequestPoller
    {
        private readonly IMessagingService messagingService;

        public MessagingTaskManagingRequestPoller(IMessagingService messagingService)
        {
            this.messagingService = messagingService;
        }

        public async IAsyncEnumerable<TaskManagingRequest> StartTaskManagingRequestStream(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await ConnectMessagingServiceIfNecessaryAsync(cancellationToken);

            await foreach (var message in messagingService.StartMessageStreamReceiving(
                               AcknowledgeOptions.ManualAcknowledge, cancellationToken))
                yield return CreateTaskManagingRequestFrom(message);
        }

        private Task ConnectMessagingServiceIfNecessaryAsync(CancellationToken cancellationToken) =>
            !messagingService.Connected ? messagingService.OpenConnectionAsync(cancellationToken) : Task.CompletedTask;

        private TaskManagingRequest CreateTaskManagingRequestFrom(IMessage message)
        {
            var taskManagingRequest =
                message.TryGetContent<NewDocumentBasedTasksCreationRequest>(
                    out var newDocumentBasedTasksCreationRequest)
                    ?
                    (TaskManagingRequest)newDocumentBasedTasksCreationRequest
                    : message.TryGetContent<DocumentBasedTasksRemovingRequest>(
                        out var documentBasedTasksRemovingRequest)
                        ? documentBasedTasksRemovingRequest
                        : throw new InvalidOperationException("Unknown message's body type was encountered");

            taskManagingRequest.Id = message.Id;

            return taskManagingRequest;
        }
    }
}