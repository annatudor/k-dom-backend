using KDomBackend.Models.DTOs.User;
using KDomBackend.Models.MongoEntities;

namespace KDomBackend.Services.Interfaces
{
    public interface IKDomPermissionService
    {

       
        Task<bool> CanUserEditKDomAsync(KDom kdom, int userId);
        Task<bool> CanUserEditKDomByIdAsync(string kdomId, int userId);
        Task<bool> CanUserEditKDomBySlugAsync(string slug, int userId);
        Task<bool> CanUserViewSensitiveInfoAsync(KDom kdom, int userId);
        Task<bool> CanUserViewSensitiveInfoByIdAsync(string kdomId, int userId);
        Task<bool> CanUserViewSensitiveInfoBySlugAsync(string slug, int userId);
        bool IsUserOwner(KDom kdom, int userId);
        Task<bool> IsUserOwnerByIdAsync(string kdomId, int userId);
        Task<bool> IsUserOwnerBySlugAsync(string slug, int userId);
        bool IsUserCollaborator(KDom kdom, int userId);
        Task<bool> IsUserCollaboratorByIdAsync(string kdomId, int userId);
        Task<bool> IsUserCollaboratorBySlugAsync(string slug, int userId);
        Task<bool> IsUserAdminOrModeratorAsync(int userId);
        Task EnsureUserCanEditKDomAsync(KDom kdom, int userId, string action = "edit");
        Task EnsureUserCanEditKDomByIdAsync(string kdomId, int userId, string action = "edit");
        Task EnsureUserCanEditKDomBySlugAsync(string slug, int userId, string action = "edit");
        Task EnsureUserCanViewSensitiveInfoAsync(KDom kdom, int userId, string info = "view this information");
        Task EnsureUserCanViewSensitiveInfoByIdAsync(string kdomId, int userId, string info = "view this information");
        Task EnsureUserCanViewSensitiveInfoBySlugAsync(string slug, int userId, string info = "view this information");
        void EnsureUserIsOwner(KDom kdom, int userId, string action = "perform this action");
        Task EnsureUserIsOwnerByIdAsync(string kdomId, int userId, string action = "perform this action");
        Task EnsureUserIsOwnerBySlugAsync(string slug, int userId, string action = "perform this action");
        Task<UserPermissionsDto> GetUserPermissionsByIdAsync(string kdomId, int userId);
        Task<UserPermissionsDto> GetUserPermissionsBySlugAsync(string slug, int userId);
        Task<bool> CanUserCreateSubKDomByIdAsync(string parentKDomId, int userId);
        Task<bool> CanUserCreateSubKDomBySlugAsync(string parentSlug, int userId);
        Task<bool> CanUserEditMetadataAsync(KDom kdom, int userId);
        Task<bool> CanUserEditMetadataByIdAsync(string kdomId, int userId);
        Task EnsureUserCanEditMetadataAsync(KDom kdom, int userId, string action = "edit metadata for");
        Task EnsureUserCanEditMetadataByIdAsync(string kdomId, int userId, string action = "edit metadata for");
        Task EnsureUserCanEditMetadataBySlugAsync(string slug, int userId, string action = "edit metadata for");
    }
}