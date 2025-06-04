namespace KDomBackend.Models.DTOs.User
{
    public class UserPermissionsDto
    {
        public bool CanEdit { get; set; }
        public bool CanEditMetadata { get; set; }
        public bool CanViewSensitive { get; set; }
        public bool IsOwner { get; set; }
        public bool IsCollaborator { get; set; }
        public bool IsAdminOrModerator { get; set; }
        public bool CanCreateSubPages { get; set; }
        public bool CanManageCollaborators { get; set; }
        public bool CanViewEditHistory { get; set; }
        public bool CanApproveReject { get; set; }
    }
}
