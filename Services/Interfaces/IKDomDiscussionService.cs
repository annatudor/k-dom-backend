using KDomBackend.Models.DTOs.KDom;

namespace KDomBackend.Services.Interfaces
{
    public interface IKDomDiscussionService
    {
        /// <summary>
        /// Obține discussion-ul complet pentru un K-Dom cu paginare
        /// </summary>
        Task<KDomDiscussionReadDto> GetKDomDiscussionAsync(string slug, KDomDiscussionFilterDto filter);

        /// <summary>
        /// Obține doar statisticile discussion-ului pentru un K-Dom
        /// </summary>
        Task<KDomDiscussionStatsDto> GetDiscussionStatsAsync(string slug);

        /// <summary>
        /// Verifică dacă un K-Dom are discussion activ
        /// </summary>
        Task<bool> HasActiveDiscussionAsync(string slug);
    }
}