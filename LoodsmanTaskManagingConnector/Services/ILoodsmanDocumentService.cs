using System.Threading;
using System.Threading.Tasks;
using UMP.Loodsman.Dtos;

namespace LoodsmanTaskManagingConnectorObjects.Services
{
    public interface ILoodsmanDocumentService
    {
        long GetOrCreateDocument(DocumentFullInfoDto documentFullInfo);

        Task<long> GetOrCreateDocumentAsync(DocumentFullInfoDto documentFullInfo,
            CancellationToken cancellationToken = default);
    }
}