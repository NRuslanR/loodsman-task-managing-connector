using System.Threading;
using System.Threading.Tasks;
using UMP.Loodsman.Dtos.SUPR.Exchange;

namespace LoodsmanTaskManagingConnectorObjects.Components.Processing.Generics
{
    public interface ITaskManagingRequestProcessor<in TRequest, TReply> : ITaskManagingRequestProcessor
        where TRequest : TaskManagingRequest
        where TReply : TaskManagingReply
    {
        TReply ProcessRequest(TRequest request);

        Task<TReply> ProcessRequestAsync(TRequest request, CancellationToken cancellationToken = default);
    }
}