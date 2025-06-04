namespace KDomBackend.Models.DTOs.Collaboration
{
    public class KDomCollaborationStatsDto
    {
        public int TotalCollaborators { get; set; }
        public int ActiveCollaborators { get; set; }
        public DateTime? LastCollaboratorActivity { get; set; }
        public List<CollaboratorEditStatsDto> TopCollaborators { get; set; } = new();
        public CollaborationDistributionDto EditDistribution { get; set; } = new();
    }

    public class CollaboratorEditStatsDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int EditCount { get; set; }
        public DateTime? LastEdit { get; set; }
        public double ContributionPercentage { get; set; }
    }

    public class CollaborationDistributionDto
    {
        public int OwnerEdits { get; set; }
        public int CollaboratorEdits { get; set; }
        public double OwnerPercentage { get; set; }
        public double CollaboratorPercentage { get; set; }
    }
}