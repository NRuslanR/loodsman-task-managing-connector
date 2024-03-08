using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SUPR;
using UMP.Loodsman.Dtos.SUPR;

namespace LoodsmanTaskManagingConnectorObjects.Services
{
    public interface ILoodsmanTaskService
    {
        IEnumerable<ITask> CreateTasksByDocumentAttachment(IEnumerable<NewTaskDto> taskInfos, long documentId);

        Task<IEnumerable<ITask>> CreateTasksByDocumentAttachmentAsync(IEnumerable<NewTaskDto> taskInfos,
            long documentId, CancellationToken cancellationToken = default);
    }
}