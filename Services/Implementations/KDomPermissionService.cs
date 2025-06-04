using KDomBackend.Models.DTOs.User;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Services.Interfaces;

namespace KDomBackend.Services.Implementations
{
    public class KDomPermissionService : IKDomPermissionService
    {
        private readonly IKDomRepository _kdomRepository;
        private readonly IUserRepository _userRepository;

        public KDomPermissionService(
            IKDomRepository kdomRepository,
            IUserRepository userRepository)
        {
            _kdomRepository = kdomRepository;
            _userRepository = userRepository;
        }

        // BASIC PERMISSION CHECKS WITH K-DOM OBJECT

        
        /// Checks if a user can edit a K-Dom (owner, collaborator, or admin/moderator)
        public async Task<bool> CanUserEditKDomAsync(KDom kdom, int userId)
        {
            // Owner can always edit
            if (IsUserOwner(kdom, userId))
                return true;

            // Collaborators can edit
            if (IsUserCollaborator(kdom, userId))
                return true;

            // Admins and moderators can edit
            if (await IsUserAdminOrModeratorAsync(userId))
                return true;

            return false;
        }

        
        /// Checks if a user can view sensitive information like edit history (owner, collaborator, or admin/moderator)
        public async Task<bool> CanUserViewSensitiveInfoAsync(KDom kdom, int userId)
        {
            // Same permissions as editing for now
            return await CanUserEditKDomAsync(kdom, userId);
        }

        
        /// Checks if a user is the owner of a K-Dom
        public bool IsUserOwner(KDom kdom, int userId)
        {
            return kdom.UserId == userId;
        }

        
        /// Checks if a user is a collaborator on a K-Dom
        public bool IsUserCollaborator(KDom kdom, int userId)
        {
            return kdom.Collaborators.Contains(userId);
        }

        // PERMISSION CHECKS BY K-DOM ID

        
        /// Checks if a user can edit a K-Dom by ID
        public async Task<bool> CanUserEditKDomByIdAsync(string kdomId, int userId)
        {
            var kdom = await _kdomRepository.GetByIdAsync(kdomId);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            return await CanUserEditKDomAsync(kdom, userId);
        }

        
        /// Checks if a user can view sensitive information by K-Dom ID
        public async Task<bool> CanUserViewSensitiveInfoByIdAsync(string kdomId, int userId)
        {
            var kdom = await _kdomRepository.GetByIdAsync(kdomId);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            return await CanUserViewSensitiveInfoAsync(kdom, userId);
        }

        
        /// Checks if a user is the owner of a K-Dom by ID
        public async Task<bool> IsUserOwnerByIdAsync(string kdomId, int userId)
        {
            var kdom = await _kdomRepository.GetByIdAsync(kdomId);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            return IsUserOwner(kdom, userId);
        }

        
        /// Checks if a user is a collaborator on a K-Dom by ID
        public async Task<bool> IsUserCollaboratorByIdAsync(string kdomId, int userId)
        {
            var kdom = await _kdomRepository.GetByIdAsync(kdomId);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            return IsUserCollaborator(kdom, userId);
        }

        // PERMISSION CHECKS BY K-DOM SLUG

        
        /// Checks if a user can edit a K-Dom by slug
        public async Task<bool> CanUserEditKDomBySlugAsync(string slug, int userId)
        {
            var kdom = await _kdomRepository.GetBySlugAsync(slug);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            return await CanUserEditKDomAsync(kdom, userId);
        }

        
        /// Checks if a user can view sensitive information by K-Dom slug
        public async Task<bool> CanUserViewSensitiveInfoBySlugAsync(string slug, int userId)
        {
            var kdom = await _kdomRepository.GetBySlugAsync(slug);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            return await CanUserViewSensitiveInfoAsync(kdom, userId);
        }

        
        /// Checks if a user is the owner of a K-Dom by slug
        public async Task<bool> IsUserOwnerBySlugAsync(string slug, int userId)
        {
            var kdom = await _kdomRepository.GetBySlugAsync(slug);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            return IsUserOwner(kdom, userId);
        }

        
        /// Checks if a user is a collaborator on a K-Dom by slug
        public async Task<bool> IsUserCollaboratorBySlugAsync(string slug, int userId)
        {
            var kdom = await _kdomRepository.GetBySlugAsync(slug);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            return IsUserCollaborator(kdom, userId);
        }

        // ADMIN/MODERATOR CHECK

        
        /// Checks if user is admin or moderator
        public async Task<bool> IsUserAdminOrModeratorAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user?.Role == "admin" || user?.Role == "moderator";
        }

        // ENSURE METHODS (THROW EXCEPTIONS) WITH K-DOM OBJECT

        
        /// Throws UnauthorizedAccessException if user cannot edit the K-Dom
        public async Task EnsureUserCanEditKDomAsync(KDom kdom, int userId, string action = "edit")
        {
            if (!await CanUserEditKDomAsync(kdom, userId))
            {
                throw new UnauthorizedAccessException($"You don't have permission to {action} this K-Dom.");
            }
        }

        
        /// Throws UnauthorizedAccessException if user cannot view sensitive info for the K-Dom
        public async Task EnsureUserCanViewSensitiveInfoAsync(KDom kdom, int userId, string info = "view this information")
        {
            if (!await CanUserViewSensitiveInfoAsync(kdom, userId))
            {
                throw new UnauthorizedAccessException($"You don't have permission to {info} for this K-Dom.");
            }
        }

        
        /// Throws UnauthorizedAccessException if user is not the owner
        public void EnsureUserIsOwner(KDom kdom, int userId, string action = "perform this action")
        {
            if (!IsUserOwner(kdom, userId))
            {
                throw new UnauthorizedAccessException($"Only the K-Dom owner can {action}.");
            }
        }

        // ENSURE METHODS BY K-DOM ID

        
        /// Throws UnauthorizedAccessException if user cannot edit the K-Dom by ID
        public async Task EnsureUserCanEditKDomByIdAsync(string kdomId, int userId, string action = "edit")
        {
            var kdom = await _kdomRepository.GetByIdAsync(kdomId);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            await EnsureUserCanEditKDomAsync(kdom, userId, action);
        }

        
        /// Throws UnauthorizedAccessException if user cannot view sensitive info by K-Dom ID
        public async Task EnsureUserCanViewSensitiveInfoByIdAsync(string kdomId, int userId, string info = "view this information")
        {
            var kdom = await _kdomRepository.GetByIdAsync(kdomId);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            await EnsureUserCanViewSensitiveInfoAsync(kdom, userId, info);
        }

        
        /// Throws UnauthorizedAccessException if user is not the owner by K-Dom ID
        public async Task EnsureUserIsOwnerByIdAsync(string kdomId, int userId, string action = "perform this action")
        {
            var kdom = await _kdomRepository.GetByIdAsync(kdomId);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            EnsureUserIsOwner(kdom, userId, action);
        }

        // ENSURE METHODS BY K-DOM SLUG

        
        /// Throws UnauthorizedAccessException if user cannot edit the K-Dom by slug
        public async Task EnsureUserCanEditKDomBySlugAsync(string slug, int userId, string action = "edit")
        {
            var kdom = await _kdomRepository.GetBySlugAsync(slug);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            await EnsureUserCanEditKDomAsync(kdom, userId, action);
        }

        
        /// Throws UnauthorizedAccessException if user cannot view sensitive info by K-Dom slug
        public async Task EnsureUserCanViewSensitiveInfoBySlugAsync(string slug, int userId, string info = "view this information")
        {
            var kdom = await _kdomRepository.GetBySlugAsync(slug);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            await EnsureUserCanViewSensitiveInfoAsync(kdom, userId, info);
        }

        
        /// Throws UnauthorizedAccessException if user is not the owner by K-Dom slug
        public async Task EnsureUserIsOwnerBySlugAsync(string slug, int userId, string action = "perform this action")
        {
            var kdom = await _kdomRepository.GetBySlugAsync(slug);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            EnsureUserIsOwner(kdom, userId, action);
        }

        public async Task<UserPermissionsDto> GetUserPermissionsByIdAsync(string kdomId, int userId)
        {
            var kdom = await _kdomRepository.GetByIdAsync(kdomId);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            return await BuildUserPermissionsDto(kdom, userId);
        }

        /// <summary>
        /// Obține toate permisiunile unui utilizator pentru un K-Dom prin Slug
        /// </summary>
        public async Task<UserPermissionsDto> GetUserPermissionsBySlugAsync(string slug, int userId)
        {
            var kdom = await _kdomRepository.GetBySlugAsync(slug);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            return await BuildUserPermissionsDto(kdom, userId);
        }

        public async Task<bool> CanUserCreateSubKDomByIdAsync(string parentKDomId, int userId)
        {
            // Pentru a crea o sub-pagină, utilizatorul trebuie să aibă permisiuni de editare pe parent
            return await CanUserEditKDomByIdAsync(parentKDomId, userId);
        }

        /// <summary>
        /// Verifică dacă un utilizator poate crea o sub-pagină pentru un K-Dom prin Slug
        /// </summary>
        public async Task<bool> CanUserCreateSubKDomBySlugAsync(string parentSlug, int userId)
        {
            // Pentru a crea o sub-pagină, utilizatorul trebuie să aibă permisiuni de editare pe parent
            return await CanUserEditKDomBySlugAsync(parentSlug, userId);
        }

        private async Task<UserPermissionsDto> BuildUserPermissionsDto(KDom kdom, int userId)
        {
            var isOwner = IsUserOwner(kdom, userId);
            var isCollaborator = IsUserCollaborator(kdom, userId);
            var isAdminOrMod = await IsUserAdminOrModeratorAsync(userId);

            var canEdit = await CanUserEditKDomAsync(kdom, userId);
            var canViewSensitive = await CanUserViewSensitiveInfoAsync(kdom, userId);

            return new UserPermissionsDto
            {
                IsOwner = isOwner,
                IsCollaborator = isCollaborator,
                IsAdminOrModerator = isAdminOrMod,
                CanEdit = canEdit,
                CanViewSensitive = canViewSensitive,
                CanCreateSubPages = canEdit, // Dacă poate edita, poate crea sub-pagini
                CanManageCollaborators = isOwner || isAdminOrMod, // Doar owner și admin/mod
                CanViewEditHistory = canViewSensitive, // Același cu sensitive info
                CanApproveReject = isAdminOrMod, // Doar admin/moderator
                
            };
        }

        public async Task<bool> CanUserEditMetadataAsync(KDom kdom, int userId)
        {
            // Only owner can edit metadata
            if (IsUserOwner(kdom, userId))
                return true;

            // Admins and moderators can edit metadata
            if (await IsUserAdminOrModeratorAsync(userId))
                return true;

            return false;
        }

        public async Task<bool> CanUserEditMetadataByIdAsync(string kdomId, int userId)
        {
            var kdom = await _kdomRepository.GetByIdAsync(kdomId);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            return await CanUserEditMetadataAsync(kdom, userId);
        }

        public async Task<bool> CanUserEditMetadataBySlugAsync(string slug, int userId)
        {
            var kdom = await _kdomRepository.GetBySlugAsync(slug);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            return await CanUserEditMetadataAsync(kdom, userId);
        }

      
        public async Task EnsureUserCanEditMetadataAsync(KDom kdom, int userId, string action = "edit metadata for")
        {
            if (!await CanUserEditMetadataAsync(kdom, userId))
            {
                throw new UnauthorizedAccessException($"Only the owner can {action} this K-Dom. Collaborators can edit content but not metadata.");
            }
        }

        public async Task EnsureUserCanEditMetadataByIdAsync(string kdomId, int userId, string action = "edit metadata for")
        {
            var kdom = await _kdomRepository.GetByIdAsync(kdomId);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            await EnsureUserCanEditMetadataAsync(kdom, userId, action);
        }

        public async Task EnsureUserCanEditMetadataBySlugAsync(string slug, int userId, string action = "edit metadata for")
        {
            var kdom = await _kdomRepository.GetBySlugAsync(slug);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            await EnsureUserCanEditMetadataAsync(kdom, userId, action);
        }


    }
}