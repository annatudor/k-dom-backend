using KDomBackend.Repositories.Interfaces;

namespace KDomBackend.Helpers
{
    public class KDomTagHelper
    {
        private readonly IKDomRepository _kdomRepository;

        public KDomTagHelper(IKDomRepository kdomRepository)
        {
            _kdomRepository = kdomRepository;
        }

        public async Task<List<string>> GetTagsFromKDomIdAsync(string? kdomId)
        {
            var tags = new List<string>();

            if (string.IsNullOrEmpty(kdomId))
                return tags;

            var kdom = await _kdomRepository.GetByIdAsync(kdomId);
            if (kdom == null)
                throw new Exception("K-Dom asociat inexistent.");

            tags.Add(kdom.Slug);

            if (!string.IsNullOrEmpty(kdom.ParentId))
            {
                var parent = await _kdomRepository.GetByIdAsync(kdom.ParentId);
                if (parent != null)
                    tags.Add(parent.Slug);
            }

            return tags;
        }
    }
}
