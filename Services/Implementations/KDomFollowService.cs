using KDomBackend.Models.DTOs.KDom;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Models.MongoEntities.KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Services.Interfaces;

public class KDomFollowService : IKDomFollowService
{
    private readonly IKDomRepository _kdomRepository;
    private readonly IKDomFollowRepository _followRepository;

    public KDomFollowService(IKDomRepository kdomRepository, IKDomFollowRepository followRepository)
    {
        _kdomRepository = kdomRepository;
        _followRepository = followRepository;
    }

    public async Task FollowAsync(string kdomId, int userId)
    {
        var kdom = await _kdomRepository.GetByIdAsync(kdomId);
        if (kdom == null)
            throw new Exception("K-Dom not found.");

        var alreadyFollowed = await _followRepository.ExistsAsync(userId, kdomId);
        if (alreadyFollowed)
            throw new Exception("Already following this K-Dom.");

        var follow = new KDomFollow
        {
            UserId = userId,
            KDomId = kdomId
        };

        await _followRepository.CreateAsync(follow);
    }

    public async Task UnfollowAsync(string kdomId, int userId)
    {
        var kdom = await _kdomRepository.GetByIdAsync(kdomId);
        if (kdom == null)
            throw new Exception("K-Dom not found.");

        await _followRepository.UnfollowAsync(userId, kdomId);
    }

    public async Task<List<KDomTagSearchResultDto>> GetFollowedKDomsAsync(int userId)
    {
        var followedIds = await _followRepository.GetFollowedKDomIdsAsync(userId);

        var all = await _kdomRepository.GetByIdsAsync(followedIds);

        return all.Select(k => new KDomTagSearchResultDto
        {
            Id = k.Id,
            Title = k.Title,
            Slug = k.Slug
        }).ToList();
    }

    // verifica daca un utilizator urmareste un k-dom pt butonul de follow/unfollow
    public async Task<bool> IsFollowingAsync(string kdomId, int userId)
    {
        return await _followRepository.ExistsAsync(userId, kdomId);
    }


}
