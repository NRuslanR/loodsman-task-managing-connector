using System.Threading;
using System.Threading.Tasks;
using LoodsmanTaskManagingConnectorObjects.Services;
using UMP.Loodsman.Dtos.SUPR.Exchange;

namespace LoodsmanTaskManagingConnectorObjects.Components.Processing.Generics
{
    public abstract class
        TaskManagingRequestProcessor<TRequest, TReply> : ITaskManagingRequestProcessor<TRequest, TReply>
        where TRequest : TaskManagingRequest
        where TReply : TaskManagingReply
    {
        protected readonly ILoodsmanTaskService taskService;

        protected TaskManagingRequestProcessor(ILoodsmanTaskService taskService)
        {
            this.taskService = taskService;
        }

        public TaskManagingReply ProcessRequest(TaskManagingRequest request)
        {
            if (request is TRequest targetRequest)
                return ProcessRequest(targetRequest);

            return TaskManagingReply.WithUnprocessableRequest(request.Id);
        }

        public async Task<TaskManagingReply> ProcessRequestAsync(TaskManagingRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request is TRequest targetRequest)
                return await ProcessRequestAsync(targetRequest, cancellationToken);

            return TaskManagingReply.WithUnprocessableRequest(request.Id);
        }

        public abstract TReply ProcessRequest(TRequest request);

        public abstract Task<TReply> ProcessRequestAsync(TRequest request,
            CancellationToken cancellationToken = default);
    }
}