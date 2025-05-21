using KDomBackend.Helpers;
using KDomBackend.Repositories.Interfaces;

namespace KDomBackend.Services.Validation
{
    public class KDomValidator
    {
        private readonly IKDomRepository _repository;

        public KDomValidator(IKDomRepository repository)
        {
            _repository = repository;
        }

        public async Task CheckDuplicateOrSuggestAsync(string title)
        {
            var slug = SlugHelper.GenerateSlug(title);
            var exists = await _repository.ExistsByTitleOrSlugAsync(title, slug);

            if (exists)
            {
                var suggestions = await _repository.FindSimilarByTitleAsync(title);
                var titles = suggestions.Select(k => k.Title).ToList();

                throw new Exception($"A K-Dom with a similar title or slug similar already exists. Try editing or creating a new page for: {string.Join(", ", titles)}?");
            }
        }
    }
}
