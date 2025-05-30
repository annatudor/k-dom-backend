using KDomBackend.Models.DTOs.KDom;
using KDomBackend.Repositories.Interfaces;

namespace KDomBackend.Services.Validation
{
    public class KDomMetadataValidator
    {
        private readonly IKDomRepository _kdomRepository;

        public KDomMetadataValidator(IKDomRepository kdomRepository)
        {
            _kdomRepository = kdomRepository;
        }

        public async Task ValidateParentAsync(string kdomId, string? newParentId)
        {
            // Dacă nu se specifică parent, este OK
            if (string.IsNullOrWhiteSpace(newParentId))
                return;

            // Validează formatul ObjectId
            if (!IsValidObjectId(newParentId))
                throw new Exception("Invalid parent ID format.");

            if (newParentId == kdomId)
                throw new Exception("A K-Dom cannot have itself as a parent.");

            var current = await _kdomRepository.GetByIdAsync(newParentId);
            if (current == null)
                throw new Exception("Parent K-Dom does not exist.");

            // Verifică ciclurile
            while (current != null)
            {
                if (current.Id == kdomId)
                    throw new Exception("Hierarchical cycle detected: K-Dom cannot become its own descendant.");

                if (string.IsNullOrEmpty(current.ParentId))
                    break;

                current = await _kdomRepository.GetByIdAsync(current.ParentId);
            }
        }

        private static bool IsValidObjectId(string id)
        {
            return !string.IsNullOrWhiteSpace(id) &&
                   id.Length == 24 &&
                   id.All(c => "0123456789abcdefABCDEF".Contains(c));
        }
    }
}
