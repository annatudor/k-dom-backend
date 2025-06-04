namespace KDomBackend.Models.DTOs.Collaboration
{
    public class CollaboratorReadDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public DateTime AddedAt { get; set; }
        public int EditCount { get; set; } 
        public DateTime? LastActivity { get; set; }
    }

}
