using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LoodsmanTaskManagingConnectorObjects.Components.Processing.Generics;
using LoodsmanTaskManagingConnectorObjects.Services;
using UMP.Loodsman.Dtos;
using UMP.Loodsman.Dtos.SUPR;
using UMP.Loodsman.Dtos.SUPR.Exchange;

namespace LoodsmanTaskManagingConnectorObjects.Components.Processing
{
    public class NewDocumentBasedTasksCreationRequestProcessor : TaskManagingRequestProcessor<
        NewDocumentBasedTasksCreationRequest, NewDocumentBasedTasksCreationReply>
    {
        private readonly ILoodsmanDocumentService documentService;

        public NewDocumentBasedTasksCreationRequestProcessor(ILoodsmanDocumentService documentService,
            ILoodsmanTaskService taskService) : base(taskService)
        {
            this.documentService = documentService;
        }

        public override NewDocumentBasedTasksCreationReply ProcessRequest(NewDocumentBasedTasksCreationRequest request)
        {
            return ProcessRequestAsync(request).Result;
        }

        public override async Task<NewDocumentBasedTasksCreationReply> ProcessRequestAsync(NewDocumentBasedTasksCreationRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var documentId = await GetOrCreateDocumentAsync(request.DocumentFullInfo, cancellationToken);

                await CreateTasksByDocumentAttachmentAsync(request.NewTasks, documentId);

                return new NewDocumentBasedTasksCreationReply
                {
                    DocumentFullInfo = request.DocumentFullInfo,
                    RequestId = request.Id,
                    Result = TaskManagingRequestProcessingResult.Success
                };
            }

            catch (Exception ex)
            {
                return new NewDocumentBasedTasksCreationReply
                {
                    Errors = new List<string> { ex.Message },
                    RequestId = request.Id,
                    Result = TaskManagingRequestProcessingResult.Failed
                };
            }
        }

        private Task<long> GetOrCreateDocumentAsync(DocumentFullInfoDto documentFullInfo,
            CancellationToken cancellationToken)
        {
            return documentService.GetOrCreateDocumentAsync(documentFullInfo, cancellationToken);
        }

        private async Task CreateTasksByDocumentAttachmentAsync(IEnumerable<NewTaskDto> taskInfos, long documentId)
        {
            await taskService.CreateTasksByDocumentAttachmentAsync(taskInfos, documentId);
        }
    }
}