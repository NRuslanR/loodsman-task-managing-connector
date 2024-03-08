using System.Threading;
using System.Threading.Tasks;
using LoodsmanTaskManagingConnectorObjects.Components.Processing.Generics;
using LoodsmanTaskManagingConnectorObjects.Services;
using UMP.Loodsman.Dtos.SUPR.Exchange;

namespace LoodsmanTaskManagingConnectorObjects.Components.Processing
{
    public class
        DocumentBasedTasksRemovingRequestProcessor : TaskManagingRequestProcessor<DocumentBasedTasksRemovingRequest,
            DocumentBasedTasksRemovingReply>
    {
        public DocumentBasedTasksRemovingRequestProcessor(ILoodsmanTaskService loodsmanTaskService) : base(
            loodsmanTaskService)
        {
        }

        public override DocumentBasedTasksRemovingReply ProcessRequest(DocumentBasedTasksRemovingRequest request)
        {
            return new DocumentBasedTasksRemovingReply
                { Result = TaskManagingRequestProcessingResult.Success, RequestId = request.Id };
        }

        public override Task<DocumentBasedTasksRemovingReply> ProcessRequestAsync(DocumentBasedTasksRemovingRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new DocumentBasedTasksRemovingReply { Result = TaskManagingRequestProcessingResult.Success, RequestId = request.Id });
        }
    }
}