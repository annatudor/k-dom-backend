using KDomBackend.Enums;
using KDomBackend.Models.DTOs.KDom;
using KDomBackend.Models.DTOs.Post;

namespace KDomBackend.Models.DTOs.User
{
    public class UserProfileReadDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public ProfileTheme ProfileTheme { get; set; } = ProfileTheme.Default;

        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }

        public DateTime JoinedAt { get; set; }

        public bool? IsFollowedByCurrentUser { get; set; }

        public int CreatedKDomsCount { get; set; }
        public int CollaboratedKDomsCount { get; set; }
        public int TotalPostsCount { get; set; }
        public int TotalCommentsCount { get; set; }
        public DateTime? LastActivityAt { get; set; }


        public List<KDomDisplayDto> OwnedKDoms { get; set; } = new();
        public List<KDomDisplayDto> CollaboratedKDoms { get; set; } = new();
        public List<KDomTagSearchResultDto> FollowedKDoms { get; set; } = new();
        public List<KDomDisplayDto> RecentlyViewedKDoms { get; set; } = new();


        public List<PostReadDto> RecentPosts { get; set; } = new();

        public bool IsOwnProfile { get; set; }
        public bool CanEdit { get; set; }
    }
}
