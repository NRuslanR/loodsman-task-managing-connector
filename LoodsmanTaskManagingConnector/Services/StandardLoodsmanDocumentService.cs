using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UMP.Loodsman.Adapters;
using UMP.Loodsman.Dtos;
using UMP.Loodsman.Dtos.Names.Attributes.Documents;

namespace LoodsmanTaskManagingConnectorObjects.Services
{
    // refactor: extract api calls to separeate apiadapter's methods
    public class StandardLoodsmanDocumentService : ILoodsmanDocumentService
    {
        private readonly IApiAdapter apiAdapter;
        private readonly IBaseDocumentAttributeNames baseDocumentAttributeNames;

        public StandardLoodsmanDocumentService(IApiAdapter apiAdapter,
            IBaseDocumentAttributeNames baseDocumentAttributeNames)
        {
            this.apiAdapter = apiAdapter;
            this.baseDocumentAttributeNames = baseDocumentAttributeNames;
        }

        public long GetOrCreateDocument(DocumentFullInfoDto documentFullInfo)
        {
            return GetOrCreateDocumentAsync(documentFullInfo).Result;
        }

        public async Task<long> GetOrCreateDocumentAsync(DocumentFullInfoDto documentFullInfo,
            CancellationToken cancellationToken = default)
        {
            var documentId = await FindDocument(documentFullInfo.Document, cancellationToken);

            if (documentId.HasValue)
            {
                documentFullInfo.Document.VersionId = documentId.Value;

                return documentId.Value;
            }

            return await CreateDocumentAsync(documentFullInfo, cancellationToken);
        }

        private async Task<long?> FindDocument(DocumentDto documentInfo, CancellationToken cancellationToken)
        {
            var documentDataSet = await apiAdapter.GetDataSetAsync(cancellationToken, "FindObjects",
                documentInfo.TypeName, documentInfo.ProductValue, documentInfo.Version, documentInfo.State, "", "", "");

            if (documentDataSet.RecordCount > 1)
                throw new InvalidOperationException(
                    $"Найдено более одного документа {documentInfo.TypeName}:{documentInfo.ProductValue}");

            var documentId = !documentDataSet.IsEmpty() ? (int)documentDataSet.FieldValue["_ID_VERSION"] : (long?)null;

            return documentId;
        }

        private async Task<long> CreateDocumentAsync(DocumentFullInfoDto documentFullInfo,
            CancellationToken cancellationToken)
        {
            var parentObjectLink = GetDocumentParentObjectLink(documentFullInfo.Links);

            var checkoutId =
                await CreateObjectCheckOut(
                    parentObjectLink.ParentTypeName,
                    parentObjectLink.ParentProductValue,
                    parentObjectLink.ParentVersion,
                    cancellationToken
                );

            long documentId;

            try
            {
                await ConnectToCheckOut(checkoutId, cancellationToken);

                documentId = await CreateDocumentObjectAsync(documentFullInfo, cancellationToken);

                await CreateObjectLink(documentId, parentObjectLink, cancellationToken);

                await CheckIn(checkoutId, cancellationToken);
            }

            catch
            {
                await CancelCheckOut(checkoutId, cancellationToken);

                throw;
            }

            return documentId;
        }

        private LinkDto GetDocumentParentObjectLink(LinkDtos links)
        {
            var link = links.FirstOrDefault();

            if (link is null)
                throw new ArgumentException("Document's parent object link wasn't specified");

            return link;
        }

        private async Task<string> CreateObjectCheckOut(string typeName, string productValue, string version,
            CancellationToken cancellationToken)
        {
            return await apiAdapter.RunMethodAsync<string>(cancellationToken, "CheckOut", typeName, productValue,
                version, 0);
        }

        private async Task<int> CreateDocumentObjectAsync(DocumentFullInfoDto documentFullInfo,
            CancellationToken cancellationToken)
        {
            var document = documentFullInfo.Document;

            var documentId = await apiAdapter.RunMethodAsync<int>(cancellationToken, "NewObject", document.TypeName,
                document.State, document.ProductValue, 0);

            await UpdateDocumentAttributesAsync(documentId, documentFullInfo.AttributeValueList, cancellationToken);
            await UpdateDocumentFilesAsync(documentId, documentFullInfo.Files, cancellationToken);

            return documentId;
        }

        private async Task UpdateDocumentAttributesAsync(int documentId, AttributeValueDtos attributeValues,
            CancellationToken cancellationToken)
        {
            foreach (var attributeName in baseDocumentAttributeNames)
            {
                var attributeValue = attributeValues.Contains(attributeName) ? attributeValues[attributeName] : null;

                await apiAdapter.RunMethodAsync(cancellationToken, "UpAttrValueById", documentId, attributeName,
                    attributeValue);
            }
        }

        private async Task UpdateDocumentFilesAsync(int documentId, FileDtos fileInfos,
            CancellationToken cancellationToken)
        {
            var currentUserInfoDataSet = await apiAdapter.GetDataSetAsync(cancellationToken, "GetInfoAboutCurrentUser");

            var currentUserFileDir = currentUserInfoDataSet.FieldValue["_FILEDIR"] as string;

            foreach (var fileInfo in fileInfos)
            {
                var currentUserDocumentFilePath = currentUserFileDir + Path.DirectorySeparatorChar + fileInfo.Name;

                File.Copy(fileInfo.LocalName, currentUserDocumentFilePath, true);

                await apiAdapter.RunMethodAsync(cancellationToken, "UpFileById", documentId, fileInfo.Name,
                    currentUserDocumentFilePath, null, DateTime.Now, false);
            }
        }

        private async Task<int> CreateObjectLink(long objectId, LinkDto parentObjectLink,
            CancellationToken cancellationToken)
        {
            return await apiAdapter.RunMethodAsync<int>(
                cancellationToken,
                "NewLink",
                0,
                parentObjectLink.ParentTypeName,
                parentObjectLink.ParentProductValue,
                parentObjectLink.ParentVersion,
                (int)objectId,
                "", "", "",
                -1,
                -1,
                "",
                parentObjectLink.Type.Name
            );
        }

        private async Task CheckIn(string checkoutId, CancellationToken cancellationToken)
        {
            await DisconnectCheckOut(checkoutId, cancellationToken);

            await apiAdapter.RunMethodAsync(cancellationToken, "CheckIn", checkoutId, apiAdapter.Api.DBName);
        }

        private async Task CancelCheckOut(string checkoutId, CancellationToken cancellationToken)
        {
            await DisconnectCheckOut(checkoutId, cancellationToken);

            await apiAdapter.RunMethodAsync(cancellationToken, "CancelCheckOut", checkoutId, apiAdapter.Api.DBName);
        }

        private async Task ConnectToCheckOut(string checkoutId, CancellationToken cancellationToken)
        {
            await Task.Run(
                async () => await apiAdapter.RunMethodAsync(cancellationToken, "ConnectToCheckOut", checkoutId,
                    apiAdapter.Api.DBName), cancellationToken);
        }

        private async Task DisconnectCheckOut(string checkoutId, CancellationToken cancellationToken)
        {
            await Task.Run(() => apiAdapter.RunMethod("DisconnectCheckOut", checkoutId, apiAdapter.Api.DBName),
                cancellationToken);
        }
    }
}