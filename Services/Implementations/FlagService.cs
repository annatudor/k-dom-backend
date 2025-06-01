using KDomBackend.Models.Entities;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Models.DTOs.Flag;
using KDomBackend.Services.Interfaces;
using KDomBackend.Enums;

namespace KDomBackend.Services.Implementations
{
    public class FlagService : IFlagService
    {
        private readonly IFlagRepository _repository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IKDomRepository _kdomRepository; 
        private readonly IPostRepository _postRepository; 
        private readonly ICommentRepository _commentRepository;

        public FlagService(
            IFlagRepository repository,
            IAuditLogRepository auditLogRepository,
            IKDomRepository kdomRepository,
            IPostRepository postRepository,
            ICommentRepository commentRepository)
        {
            _repository = repository;
            _auditLogRepository = auditLogRepository;
            _kdomRepository = kdomRepository;
            _postRepository = postRepository;
            _commentRepository = commentRepository;
        }

        public async Task CreateFlagAsync(int userId, FlagCreateDto dto)
        {

            await ValidateContentExistsAsync(dto.ContentType, dto.ContentId);

            var flag = new Flag
            {
                UserId = userId,
                ContentType = dto.ContentType,
                ContentId = dto.ContentId,
                Reason = dto.Reason,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.CreateAsync(flag);
        }

        private async Task ValidateContentExistsAsync(ContentType contentType, string contentId)
        {
            switch (contentType)
            {
                case ContentType.KDom:
                    var kdom = await _kdomRepository.GetByIdAsync(contentId);
                    if (kdom == null)
                        throw new Exception("K-Dom not found.");
                    break;

                case ContentType.Post:
                    var post = await _postRepository.GetByIdAsync(contentId);
                    if (post == null)
                        throw new Exception("Post not found.");
                    break;

                case ContentType.Comment:
                    var comment = await _commentRepository.GetByIdAsync(contentId);
                    if (comment == null)
                        throw new Exception("Comment not found.");
                    break;

                default:
                    throw new ArgumentException($"Unsupported content type: {contentType}");
            }
        }

        public async Task<List<FlagReadDto>> GetAllAsync()
        {
            var flags = await _repository.GetAllAsync();

            return flags.Select(f => new FlagReadDto
            {
                Id = f.Id,
                UserId = f.UserId,
                ContentType = f.ContentType,
                ContentId = f.ContentId,
                Reason = f.Reason,
                CreatedAt = f.CreatedAt,
                IsResolved = f.IsResolved
            }).ToList();
        }

        public async Task ResolveAsync(int flagId, int userId)
        {
            await _repository.MarkResolvedAsync(flagId);
            await _auditLogRepository.CreateAsync(new AuditLog
            {
                UserId = userId,
                Action = AuditAction.ResolveFlag,
                TargetType = AuditTargetType.Flag,
                TargetId = flagId.ToString(),
                CreatedAt = DateTime.UtcNow
            });
        }

        public async Task DeleteAsync(int flagId, int userId)
        {
            await _repository.DeleteAsync(flagId);
            await _auditLogRepository.CreateAsync(new AuditLog
            {
                UserId = userId,
                Action = AuditAction.DeleteFlag,
                TargetType = AuditTargetType.Flag,
                TargetId = flagId.ToString(),
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}