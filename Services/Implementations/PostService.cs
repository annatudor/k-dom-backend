using KDomBackend.Helpers;
using KDomBackend.Models.DTOs.Post;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Implementations;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Services.Interfaces;

namespace KDomBackend.Services.Implementations
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _repository;
        private readonly IUserService _userService;
        private readonly IFollowRepository _followRepository;

        public PostService(IPostRepository repository, IUserService userService, IFollowRepository followRepository)
        {
            _repository = repository;
            _userService = userService;
            _followRepository = followRepository;
        }


        public async Task CreatePostAsync(PostCreateDto dto, int userId)
        {
            var cleanHtml = HtmlSanitizerHelper.Sanitize(dto.ContentHtml);

            var post = new Post
            {
                UserId = userId,
                ContentHtml = cleanHtml,
                Tags = dto.Tags,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.CreateAsync(post);
        }

        public async Task<List<PostReadDto>> GetAllPostsAsync()
        {
            var posts = await _repository.GetAllAsync();
            var result = new List<PostReadDto>();

            foreach (var post in posts)
            {
                var username = await _userService.GetUsernameByUserIdAsync(post.UserId);

                result.Add(new PostReadDto
                {
                    Id = post.Id,
                    ContentHtml = post.ContentHtml,
                    Tags = post.Tags,
                    UserId = post.UserId,
                    Username = username,
                    CreatedAt = post.CreatedAt,
                    IsEdited = post.IsEdited,
                    EditedAt = post.EditedAt,
                    LikeCount = post.Likes.Count
                });

            }

            return result;
        }
        public async Task<PostReadDto> GetByIdAsync(string postId)
        {
            var post = await _repository.GetByIdAsync(postId);
            if (post == null)
                throw new Exception("Post not found.");

            var username = await _userService.GetUsernameByUserIdAsync(post.UserId);

            return new PostReadDto
            {
                Id = post.Id,
                ContentHtml = post.ContentHtml,
                Tags = post.Tags,
                UserId = post.UserId,
                Username = username,
                CreatedAt = post.CreatedAt,
                IsEdited = post.IsEdited,
                EditedAt = post.EditedAt,
                LikeCount = post.Likes.Count
            };
        }


        public async Task<PostLikeResponseDto> ToggleLikeAsync(string postId, int userId)
        {
            var post = await _repository.GetByIdAsync(postId);
            if (post == null)
                throw new Exception("Post not found.");

            var alreadyLiked = post.Likes.Contains(userId);
            var like = !alreadyLiked;

            await _repository.ToggleLikeAsync(postId, userId, like);

            // recalculam count dupa toggle
            var updatedPost = await _repository.GetByIdAsync(postId);
            var count = updatedPost?.Likes.Count ?? 0;

            return new PostLikeResponseDto
            {
                Liked = like,
                LikeCount = count
            };
        }
        public async Task EditPostAsync(string postId, PostEditDto dto, int userId)
        {
            var post = await _repository.GetByIdAsync(postId);
            if (post == null)
                throw new Exception("Post not found.");

            if (post.UserId != userId)
                throw new UnauthorizedAccessException("You cannot edit this post.");

            var cleanHtml = HtmlSanitizerHelper.Sanitize(dto.ContentHtml);

            if (cleanHtml == post.ContentHtml && dto.Tags.SequenceEqual(post.Tags))
                return; // nimic de modificat

            await _repository.UpdateAsync(postId, cleanHtml, dto.Tags);
        }

        public async Task DeletePostAsync(string postId, int userId)
        {
            var post = await _repository.GetByIdAsync(postId);
            if (post == null)
                throw new Exception("Post not found.");

            if (post.UserId != userId)
                throw new UnauthorizedAccessException("You cannot delete this post.");

            await _repository.DeleteAsync(postId);
        }

        public async Task<List<PostReadDto>> GetPostsByUserIdAsync(int userId)
        {
            var posts = await _repository.GetByUserIdAsync(userId);
            var username = await _userService.GetUsernameByUserIdAsync(userId);

            return posts.Select(p => new PostReadDto
            {
                Id = p.Id,
                ContentHtml = p.ContentHtml,
                Tags = p.Tags,
                UserId = p.UserId,
                Username = username,
                CreatedAt = p.CreatedAt,
                IsEdited = p.IsEdited,
                EditedAt = p.EditedAt,
                LikeCount = p.Likes.Count
            }).ToList();
        }

        public async Task<List<PostReadDto>> GetFeedAsync(int userId)
        {
            var followedUserIds = await _followRepository.GetFollowingAsync(userId);
            var posts = await _repository.GetFeedPostsAsync(followedUserIds);

            var result = new List<PostReadDto>();

            foreach (var post in posts)
            {
                var username = await _userService.GetUsernameByUserIdAsync(post.UserId);

                result.Add(new PostReadDto
                {
                    Id = post.Id,
                    ContentHtml = post.ContentHtml,
                    Tags = post.Tags,
                    UserId = post.UserId,
                    Username = username,
                    CreatedAt = post.CreatedAt,
                    IsEdited = post.IsEdited,
                    EditedAt = post.EditedAt,
                    LikeCount = post.Likes.Count
                });
            }

            return result;
        }

        public async Task<List<PostReadDto>> GetGuestFeedAsync(int limit = 30)
        {
            var posts = await _repository.GetPublicPostsAsync(limit);
            var result = new List<PostReadDto>();

            foreach (var post in posts)
            {
                var username = await _userService.GetUsernameByUserIdAsync(post.UserId);

                result.Add(new PostReadDto
                {
                    Id = post.Id,
                    ContentHtml = post.ContentHtml,
                    Tags = post.Tags,
                    UserId = post.UserId,
                    Username = username,
                    CreatedAt = post.CreatedAt,
                    IsEdited = post.IsEdited,
                    EditedAt = post.EditedAt,
                    LikeCount = post.Likes.Count
                });
            }

            return result;
        }



    }
}
