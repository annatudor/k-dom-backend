using KDomBackend.Data;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace KDomBackend.Repositories.Implementations
{
    public class PostRepository : IPostRepository
    {
        private readonly IMongoCollection<Post> _collection;

        public PostRepository(MongoDbContext context)
        {
            _collection = context.Posts;
        }

        public async Task CreateAsync(Post post)
        {
            await _collection.InsertOneAsync(post);
        }

        public async Task<List<Post>> GetAllAsync()
        {
            return await _collection
                .Find(_ => true)
                .SortByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Post?> GetByIdAsync(string id)
        {
            return await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task ToggleLikeAsync(string postId, int userId, bool like)
        {
            var update = like
                ? Builders<Post>.Update.AddToSet(p => p.Likes, userId)
                : Builders<Post>.Update.Pull(p => p.Likes, userId);

            await _collection.UpdateOneAsync(p => p.Id == postId, update);
        }

        public async Task UpdateAsync(string postId, string newHtml, List<string> newTags)
        {
            var update = Builders<Post>.Update
                .Set(p => p.ContentHtml, newHtml)
                .Set(p => p.Tags, newTags)
                .Set(p => p.IsEdited, true)
                .Set(p => p.EditedAt, DateTime.UtcNow);

            await _collection.UpdateOneAsync(p => p.Id == postId, update);
        }
        public async Task DeleteAsync(string postId)
        {
            await _collection.DeleteOneAsync(p => p.Id == postId);
        }

        public async Task<List<Post>> GetByUserIdAsync(int userId)
        {
            return await _collection
                .Find(p => p.UserId == userId)
                .SortByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Post>> GetFeedPostsAsync(List<int> followedUserIds, List<string> followedTags, int limit = 30)
        {
            var filterUser = Builders<Post>.Filter.In(p => p.UserId, followedUserIds);
            var filterTag = Builders<Post>.Filter.AnyIn(p => p.Tags, followedTags);

            var combinedFilter = Builders<Post>.Filter.Or(filterUser, filterTag);

            return await _collection.Find(combinedFilter)
                .SortByDescending(p => p.CreatedAt)
                .Limit(limit)
                .ToListAsync();
        }


        public async Task<List<Post>> GetPublicPostsAsync(int limit = 30)
        {
            return await _collection
                .Find(_ => true)
                .SortByDescending(p => p.CreatedAt)
                .Limit(limit)
                .ToListAsync();
        }
        public async Task<List<Post>> GetByTagAsync(string tag)
        {
            var filter = Builders<Post>.Filter.AnyEq(p => p.Tags, tag.ToLower());
            return await _collection.Find(filter)
                                    .SortByDescending(p => p.CreatedAt)
                                    .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetRecentTagCountsAsync(int days = 7)
        {
            var fromDate = DateTime.UtcNow.AddDays(-days);

            var pipeline = new[]
            {
        new BsonDocument("$match", new BsonDocument("createdAt", new BsonDocument("$gte", fromDate))),
        new BsonDocument("$unwind", "$tags"),
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", "$tags" },
            { "count", new BsonDocument("$sum", 1) }
        })
    };

            var cursor = await _collection.AggregateAsync<BsonDocument>(pipeline);
            var list = await cursor.ToListAsync();

            return list.ToDictionary(
                doc => doc["_id"].AsString,
                doc => doc["count"].AsInt32
            );

        }

        public async Task<List<Post>> GetRecentPostsByUserAsync(int userId, int limit = 30)
        {
            var filter = Builders<Post>.Filter.Eq(p => p.UserId, userId);

            return await _collection
                .Find(filter)
                .SortByDescending(p => p.CreatedAt)
                .Limit(limit)
                .ToListAsync();
        }

        /// <summary>
        /// Obține numărul total de postări ale unui utilizator
        /// </summary>
        public async Task<int> GetPostCountByUserAsync(int userId)
        {
            var filter = Builders<Post>.Filter.Eq(p => p.UserId, userId);
            return (int)await _collection.CountDocumentsAsync(filter);
        }

        /// <summary>
        /// Obține postările unui utilizator cu limit
        /// </summary>
        public async Task<List<Post>> GetPostsByUserAsync(int userId, int limit = 50)
        {
            return await _collection
                .Find(p => p.UserId == userId)
                .SortByDescending(p => p.CreatedAt)
                .Limit(limit)
                .ToListAsync();
        }

        /// <summary>
        /// Calculează totalul de like-uri primite pe postările unui user
        /// </summary>
        public async Task<int> GetTotalLikesReceivedByUserPostsAsync(int userId)
        {
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("userId", userId)),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", BsonNull.Value },
                    { "totalLikes", new BsonDocument("$sum", new BsonDocument("$size", "$likes")) }
                })
            };

            var cursor = await _collection.AggregateAsync<BsonDocument>(pipeline);
            var result = await cursor.FirstOrDefaultAsync();

            return result?["totalLikes"]?.AsInt32 ?? 0;
        }

        /// <summary>
        /// Calculează totalul de like-uri date de un user pe postări
        /// </summary>
        public async Task<int> GetTotalLikesGivenByUserAsync(int userId)
        {
            var filter = Builders<Post>.Filter.AnyEq(p => p.Likes, userId);
            return (int)await _collection.CountDocumentsAsync(filter);
        }

        /// <summary>
        /// Obține statistici de postări pentru ultimele X luni
        /// </summary>
        public async Task<Dictionary<string, int>> GetUserPostsByMonthAsync(int userId, int months = 12)
        {
            var fromDate = DateTime.UtcNow.AddMonths(-months);

            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument
                {
                    { "userId", userId },
                    { "createdAt", new BsonDocument("$gte", fromDate) }
                }),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", new BsonDocument("$dateToString", new BsonDocument
                        {
                            { "format", "%Y-%m" },
                            { "date", "$createdAt" }
                        })
                    },
                    { "count", new BsonDocument("$sum", 1) }
                }),
                new BsonDocument("$sort", new BsonDocument("_id", 1))
            };

            var cursor = await _collection.AggregateAsync<BsonDocument>(pipeline);
            var results = await cursor.ToListAsync();

            return results.ToDictionary(
                doc => doc["_id"].AsString,
                doc => doc["count"].AsInt32
            );
        }

        /// <summary>
        /// Obține ID-urile postărilor unui user (pentru calculul comentariilor)
        /// </summary>
        public async Task<List<string>> GetUserPostIdsAsync(int userId)
        {
            var filter = Builders<Post>.Filter.Eq(p => p.UserId, userId);
            var projection = Builders<Post>.Projection.Include(p => p.Id);

            var cursor = await _collection.Find(filter).Project(projection).ToCursorAsync();
            var results = await cursor.ToListAsync();

            return results.Select(doc => doc["_id"].AsString).ToList();
        }

        /// <summary>
        /// Obține postările cu cel mai mare engagement pentru un user
        /// </summary>
        public async Task<List<Post>> GetUserTopPostsByEngagementAsync(int userId, int limit = 5)
        {
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("userId", userId)),
                new BsonDocument("$addFields", new BsonDocument
                {
                    { "engagement", new BsonDocument("$size", "$likes") }
                }),
                new BsonDocument("$sort", new BsonDocument("engagement", -1)),
                new BsonDocument("$limit", limit)
            };

            var cursor = await _collection.AggregateAsync<Post>(pipeline);
            return await cursor.ToListAsync();
        }

        /// <summary>
        /// Obține distribuția tag-urilor pentru postările unui user
        /// </summary>
        public async Task<Dictionary<string, int>> GetUserTagDistributionAsync(int userId)
        {
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("userId", userId)),
                new BsonDocument("$unwind", "$tags"),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$tags" },
                    { "count", new BsonDocument("$sum", 1) }
                }),
                new BsonDocument("$sort", new BsonDocument("count", -1))
            };

            var cursor = await _collection.AggregateAsync<BsonDocument>(pipeline);
            var results = await cursor.ToListAsync();

            return results.ToDictionary(
                doc => doc["_id"].AsString,
                doc => doc["count"].AsInt32
            );
        }

        public async Task<List<Post>> GetByTagAsync(string tag, int skip, int limit)
        {
            var filter = Builders<Post>.Filter.AnyEq(p => p.Tags, tag.ToLower());
            return await _collection.Find(filter)
                                    .SortByDescending(p => p.CreatedAt)
                                    .Skip(skip)
                                    .Limit(limit)
                                    .ToListAsync();
        }

        public async Task<int> GetCountByTagAsync(string tag)
        {
            var filter = Builders<Post>.Filter.AnyEq(p => p.Tags, tag.ToLower());
            return (int)await _collection.CountDocumentsAsync(filter);
        }

        public async Task<List<Post>> SearchPostsByTagAsync(string tag, string? contentQuery = null,
    string? username = null, string sortBy = "newest", bool? onlyLiked = null,
    int? lastDays = null, int skip = 0, int limit = 20)
        {
            var filterBuilder = Builders<Post>.Filter;
            var filters = new List<FilterDefinition<Post>>();

            // Filtru pentru tag
            filters.Add(filterBuilder.AnyEq(p => p.Tags, tag.ToLower()));

            // Filtru pentru conținut
            if (!string.IsNullOrWhiteSpace(contentQuery))
            {
                var contentRegex = new BsonRegularExpression(contentQuery, "i");
                filters.Add(filterBuilder.Regex(p => p.ContentHtml, contentRegex));
            }

            // Filtru pentru perioada
            if (lastDays.HasValue)
            {
                var fromDate = DateTime.UtcNow.AddDays(-lastDays.Value);
                filters.Add(filterBuilder.Gte(p => p.CreatedAt, fromDate));
            }

            // Filtru pentru like-uri
            if (onlyLiked == true)
            {
                filters.Add(filterBuilder.SizeGt(p => p.Likes, 0));
            }

            var combinedFilter = filterBuilder.And(filters);

            // Determinăm sortarea
            SortDefinition<Post> sortDefinition = sortBy.ToLower() switch
            {
                "oldest" => Builders<Post>.Sort.Ascending(p => p.CreatedAt),
                "most-liked" => Builders<Post>.Sort.Descending(p => p.Likes),
                _ => Builders<Post>.Sort.Descending(p => p.CreatedAt) // newest (default)
            };

            var posts = await _collection
                .Find(combinedFilter)
                .Sort(sortDefinition)
                .Skip(skip)
                .Limit(limit)
                .ToListAsync();

            // Filtrăm după username dacă este specificat (nu putem face asta direct în MongoDB)
            if (!string.IsNullOrWhiteSpace(username))
            {
                // Va trebui să obținem username-urile prin UserService în service layer
                // Aici doar returnăm toate postările, filtrarea se va face în service
            }

            return posts;
        }

        public async Task<int> CountSearchPostsByTagAsync(string tag, string? contentQuery = null,
            string? username = null, bool? onlyLiked = null, int? lastDays = null)
        {
            var filterBuilder = Builders<Post>.Filter;
            var filters = new List<FilterDefinition<Post>>();

            // Filtru pentru tag
            filters.Add(filterBuilder.AnyEq(p => p.Tags, tag.ToLower()));

            // Filtru pentru conținut
            if (!string.IsNullOrWhiteSpace(contentQuery))
            {
                var contentRegex = new BsonRegularExpression(contentQuery, "i");
                filters.Add(filterBuilder.Regex(p => p.ContentHtml, contentRegex));
            }

            // Filtru pentru perioada
            if (lastDays.HasValue)
            {
                var fromDate = DateTime.UtcNow.AddDays(-lastDays.Value);
                filters.Add(filterBuilder.Gte(p => p.CreatedAt, fromDate));
            }

            // Filtru pentru like-uri
            if (onlyLiked == true)
            {
                filters.Add(filterBuilder.SizeGt(p => p.Likes, 0));
            }

            var combinedFilter = filterBuilder.And(filters);
            return (int)await _collection.CountDocumentsAsync(combinedFilter);
        }

        public async Task<int> GetTotalPostsCountAsync()
        {
            return (int)await _collection.CountDocumentsAsync(Builders<Post>.Filter.Empty);
        }

        public async Task<int> GetPostCountByTagAsync(string tag)
        {
            var filter = Builders<Post>.Filter.AnyEq(p => p.Tags, tag.ToLower());
            return (int)await _collection.CountDocumentsAsync(filter);
        }
    }
}
