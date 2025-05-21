using KDomBackend.Enums;
using KDomBackend.Models.DTOs.Collaboration;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Services.Implementations;
using KDomBackend.Services.Interfaces;

public class CollaborationRequestService : ICollaborationRequestService
{
    private readonly ICollaborationRequestRepository _repository;
    private readonly IKDomRepository _kdomRepository;
    private readonly IUserService _userService;

    public CollaborationRequestService(ICollaborationRequestRepository repository, IKDomRepository kdomRepository)
    {
        _repository = repository;
        _kdomRepository = kdomRepository;
    }

    public async Task CreateRequestAsync(string kdomId, int userId, CollaborationRequestCreateDto dto)
    {
        var kdom = await _kdomRepository.GetByIdAsync(kdomId);
        if (kdom == null)
            throw new Exception("K-Dom not found.");

        if (kdom.UserId == userId || kdom.Collaborators.Contains(userId))
            throw new Exception("You are already an editor of this K-Dom.");

        var alreadyRequested = await _repository.HasPendingAsync(kdomId, userId);
        if (alreadyRequested)
            throw new Exception("You already have a pending request.");

        var request = new KDomCollaborationRequest
        {
            KDomId = kdomId,
            UserId = userId,
            Message = dto.Message
        };

        await _repository.CreateAsync(request);
    }

    public async Task ApproveAsync(string kdomId, string requestId, int reviewerId)
    {
        var request = await _repository.GetByIdAsync(requestId);
        if (request == null || request.KDomId != kdomId)
            throw new Exception("Invalid request.");

        var kdom = await _kdomRepository.GetByIdAsync(kdomId);
        if (kdom == null || kdom.UserId != reviewerId)
            throw new UnauthorizedAccessException("Only the K-Dom owner can approve.");

        if (request.Status != CollaborationRequestStatus.Pending)
            throw new Exception("Request already reviewed.");

        request.Status = CollaborationRequestStatus.Approved;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewedBy = reviewerId;

        await _repository.UpdateAsync(request);

        if (!kdom.Collaborators.Contains(request.UserId))
        {
            kdom.Collaborators.Add(request.UserId);
            await _kdomRepository.UpdateCollaboratorsAsync(kdom.Id, kdom.Collaborators);
        }
    }

    public async Task RejectAsync(string kdomId, string requestId, int reviewerId, string? reason)
    {
        var request = await _repository.GetByIdAsync(requestId);
        if (request == null || request.KDomId != kdomId)
            throw new Exception("Invalid request.");

        var kdom = await _kdomRepository.GetByIdAsync(kdomId);
        if (kdom == null || kdom.UserId != reviewerId)
            throw new UnauthorizedAccessException("Only the K-Dom owner can reject.");

        if (request.Status != CollaborationRequestStatus.Pending)
            throw new Exception("Request already reviewed.");

        request.Status = CollaborationRequestStatus.Rejected;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewedBy = reviewerId;
        request.RejectionReason = reason;

        await _repository.UpdateAsync(request);
    }

    public async Task<List<CollaborationRequestReadDto>> GetRequestsAsync(string kdomId, int userId)
    {
        var kdom = await _kdomRepository.GetByIdAsync(kdomId);
        if (kdom == null)
            throw new Exception("K-Dom not found.");

        if (kdom.UserId != userId)
            throw new UnauthorizedAccessException("Only the owner can view collaboration requests.");

        var requests = await _repository.GetByKDomIdAsync(kdomId);

        var result = new List<CollaborationRequestReadDto>();

        foreach (var req in requests)
        {
            var username = await _userService.GetUsernameByUserIdAsync(req.UserId);

            result.Add(new CollaborationRequestReadDto
            {
                Id = req.Id,
                UserId = req.UserId,
                Username = username,
                Status = req.Status,
                Message = req.Message,
                RejectionReason = req.RejectionReason,
                CreatedAt = req.CreatedAt,
                ReviewedAt = req.ReviewedAt
            });
        }

        return result;
    }


}
