using KDomBackend.Models.DTOs.KDom;
using KDomBackend.Models.MongoEntities;

namespace KDomBackend.Services.Interfaces
{
    public interface IKDomPermissionService
    {
       
        /// Checks if a user can edit a K-Dom (owner, collaborator, or admin/moderator)
       
        Task<bool> CanUserEditKDomAsync(KDom kdom, int userId);

        
        /// Checks if a user can edit a K-Dom by ID
     
        Task<bool> CanUserEditKDomByIdAsync(string kdomId, int userId);

     
        /// Checks if a user can edit a K-Dom by slug
   
        Task<bool> CanUserEditKDomBySlugAsync(string slug, int userId);

  
        /// Checks if a user can view sensitive information like edit history (owner, collaborator, or admin/moderator)
   
        Task<bool> CanUserViewSensitiveInfoAsync(KDom kdom, int userId);


        /// Checks if a user can view sensitive information by K-Dom ID
     
        Task<bool> CanUserViewSensitiveInfoByIdAsync(string kdomId, int userId);

        /// Checks if a user can view sensitive information by K-Dom slug

        Task<bool> CanUserViewSensitiveInfoBySlugAsync(string slug, int userId);


        /// Checks if a user is the owner of a K-Dom

        bool IsUserOwner(KDom kdom, int userId);


        /// Checks if a user is the owner of a K-Dom by ID

        Task<bool> IsUserOwnerByIdAsync(string kdomId, int userId);


        /// Checks if a user is the owner of a K-Dom by slug

        Task<bool> IsUserOwnerBySlugAsync(string slug, int userId);


        /// Checks if a user is a collaborator on a K-Dom

        bool IsUserCollaborator(KDom kdom, int userId);


        /// Checks if a user is a collaborator on a K-Dom by ID

        Task<bool> IsUserCollaboratorByIdAsync(string kdomId, int userId);


        /// Checks if a user is a collaborator on a K-Dom by slug

        Task<bool> IsUserCollaboratorBySlugAsync(string slug, int userId);


        /// Checks if a user is admin or moderator

        Task<bool> IsUserAdminOrModeratorAsync(int userId);


        /// Throws UnauthorizedAccessException if user cannot edit the K-Dom

        Task EnsureUserCanEditKDomAsync(KDom kdom, int userId, string action = "edit");


        /// Throws UnauthorizedAccessException if user cannot edit the K-Dom by ID

        Task EnsureUserCanEditKDomByIdAsync(string kdomId, int userId, string action = "edit");


        /// Throws UnauthorizedAccessException if user cannot edit the K-Dom by slug

        Task EnsureUserCanEditKDomBySlugAsync(string slug, int userId, string action = "edit");


        /// Throws UnauthorizedAccessException if user cannot view sensitive info for the K-Dom

        Task EnsureUserCanViewSensitiveInfoAsync(KDom kdom, int userId, string info = "view this information");


        /// Throws UnauthorizedAccessException if user cannot view sensitive info by K-Dom ID

        Task EnsureUserCanViewSensitiveInfoByIdAsync(string kdomId, int userId, string info = "view this information");


        /// Throws UnauthorizedAccessException if user cannot view sensitive info by K-Dom slug

        Task EnsureUserCanViewSensitiveInfoBySlugAsync(string slug, int userId, string info = "view this information");


        /// Throws UnauthorizedAccessException if user is not the owner

        void EnsureUserIsOwner(KDom kdom, int userId, string action = "perform this action");


        /// Throws UnauthorizedAccessException if user is not the owner by K-Dom ID

        Task EnsureUserIsOwnerByIdAsync(string kdomId, int userId, string action = "perform this action");


        /// Throws UnauthorizedAccessException if user is not the owner by K-Dom slug

        Task EnsureUserIsOwnerBySlugAsync(string slug, int userId, string action = "perform this action");

        Task<UserPermissionsDto> GetUserPermissionsByIdAsync(string kdomId, int userId);

        /// Obține toate permisiunile unui utilizator pentru un K-Dom prin Slug
   
        Task<UserPermissionsDto> GetUserPermissionsBySlugAsync(string slug, int userId);

        /// Verifică dacă un utilizator poate crea o sub-pagină pentru un K-Dom prin ID

        Task<bool> CanUserCreateSubKDomByIdAsync(string parentKDomId, int userId);

        /// Verifică dacă un utilizator poate crea o sub-pagină pentru un K-Dom prin Slug

        Task<bool> CanUserCreateSubKDomBySlugAsync(string parentSlug, int userId);
    }
}