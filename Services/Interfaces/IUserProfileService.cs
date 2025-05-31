using KDomBackend.Models.DTOs.User;

namespace KDomBackend.Services.Interfaces
{
    public interface IUserProfileService
    {
        /// <summary>
        /// Obține profilul unui utilizator
        /// </summary>
        Task<UserProfileReadDto> GetUserProfileAsync(int userId);

        /// <summary>
        /// Actualizează profilul unui utilizator
        /// IMPORTANT: Nu verifică permisiunile - trebuie verificate în controller
        /// </summary>
        Task UpdateProfileAsync(int userId, UserProfileUpdateDto dto);

        /// <summary>
        /// Adaugă un K-Dom în lista celor vizionate recent
        /// </summary>
        Task AddRecentlyViewedKDomAsync(int userId, string kdomId);

        /// <summary>
        /// Obține lista K-Dom-urilor vizionate recent
        /// </summary>
        Task<List<string>> GetRecentlyViewedKDomIdsAsync(int userId);

        /// <summary>
        /// Verifică dacă un utilizator poate actualiza profilul altui utilizator
        /// </summary>
        Task<bool> CanUserUpdateProfileAsync(int currentUserId, int targetUserId);

        /// <summary>
        /// Validează permisiunile înainte de actualizare
        /// </summary>
        Task ValidateUpdatePermissionsAsync(int currentUserId, int targetUserId);
    }
}