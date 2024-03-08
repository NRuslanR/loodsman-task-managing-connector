using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using LoodsmanTaskManagingConnectorObjects.Components.Processing;
using UMP.Loodsman.Dtos.SUPR.Exchange;

namespace LoodsmanTaskManagingConnectorObjects
{
    public class DataFlowLoodsmanTaskManagingConnector : LoodsmanTaskManagingConnector
    {
        protected override async Task InternalRunAsync(CancellationToken cancellationToken = default)
        {
            var startPipelineBlock = CreateAsyncTaskManagingPipeline(cancellationToken);

            await RunAsyncTaskManagingPipeline(startPipelineBlock, cancellationToken);
        }

        private ITargetBlock<TaskManagingRequest> CreateAsyncTaskManagingPipeline(CancellationToken cancellationToken)
        {
            var taskManagingReplyNotificationBlock = CreateTaskManagingReplyNotificationBlock(cancellationToken);
            var taskManagingRequestProcessingBlock =
                CreateTaskManagingRequestProcessingGatewayBlock(taskManagingReplyNotificationBlock, cancellationToken);

            return taskManagingRequestProcessingBlock;
        }

        private ITargetBlock<TaskManagingReply> CreateTaskManagingReplyNotificationBlock(
            CancellationToken cancellationToken)
        {
            return new ActionBlock<TaskManagingReply>(
                async reply =>
                {
                    try
                    {
                        await ReplyNotificator.SendNewDocumentBasedTasksCreationReplyAsync(reply, cancellationToken);
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(exception, "Error occurred in reply notification block");
                    }
                },
                new ExecutionDataflowBlockOptions { CancellationToken = cancellationToken }
            );
        }

        private BroadcastBlock<TaskManagingRequest> CreateTaskManagingRequestProcessingGatewayBlock(
            ITargetBlock<TaskManagingReply> replyNotificationBlock, CancellationToken cancellationToken)
        {
            var requestProcessingGatewayBlock = new BroadcastBlock<TaskManagingRequest>(null);

            foreach (var taskManagingRequestProcessor in RequestsProcessors)
            {
                var taskManagingRequestProcessingBlock =
                    CreateTaskManagingRequestProcessingBlock(taskManagingRequestProcessor, replyNotificationBlock,
                        cancellationToken);

                requestProcessingGatewayBlock.LinkTo(taskManagingRequestProcessingBlock);
            }

            return requestProcessingGatewayBlock;
        }

        private ITargetBlock<TaskManagingRequest> CreateTaskManagingRequestProcessingBlock(
            ITaskManagingRequestProcessor taskManagingRequestProcessor,
            ITargetBlock<TaskManagingReply> replyNotificationBlock, CancellationToken cancellationToken)
        {
            var taskManagingRequestProcessingBlock =
                new TransformBlock<TaskManagingRequest, TaskManagingReply>(
                    async request =>
                    {
                        try
                        {
                            return await taskManagingRequestProcessor.ProcessRequestAsync(request, cancellationToken);
                        }
                        catch (Exception exception)
                        {
                            Logger.Error(exception, "Error occurred in processing request block");

                            return TaskManagingReply.FailedFromException(request.Id, exception);
                        }
                    },
                    new ExecutionDataflowBlockOptions { CancellationToken = cancellationToken }
                );

            taskManagingRequestProcessingBlock.LinkTo(replyNotificationBlock,
                reply => reply.Result == TaskManagingRequestProcessingResult.Success);

            return taskManagingRequestProcessingBlock;
        }

        private async Task RunAsyncTaskManagingPipeline(ITargetBlock<TaskManagingRequest> startPipelineBlock,
            CancellationToken cancellationToken)
        {
            Logger.Information("Start listen");

            await foreach (var taskManagingRequest in RequestPoller.StartTaskManagingRequestStream(cancellationToken))
                await startPipelineBlock.SendAsync(taskManagingRequest, cancellationToken);
        }
    }
}