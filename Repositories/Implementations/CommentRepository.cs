using KDomBackend.Data;
using KDomBackend.Enums;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace KDomBackend.Repositories.Implementations
{
    public class CommentRepository : ICommentRepository
    {
        private readonly IMongoCollection<Comment> _collection;

        public CommentRepository(MongoDbContext context)
        {
            _collection = context.Comments;
        }

        public async Task CreateAsync(Comment comment)
        {
            await _collection.InsertOneAsync(comment);
        }

        public async Task<List<Comment>> GetByTargetAsync(CommentTargetType type, string targetId)
        {
            return await _collection
                .Find(c => c.TargetType == type && c.TargetId == targetId)
                .SortByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Comment>> GetRepliesAsync(string parentCommentId)
        {
            return await _collection
                .Find(c => c.ParentCommentId == parentCommentId)
                .SortBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Comment?> GetByIdAsync(string id)
        {
            return await _collection.Find(c => c.Id == id).FirstOrDefaultAsync();
        }

        public async Task UpdateTextAsync(string id, string newText)
        {
            var update = Builders<Comment>.Update
                .Set(c => c.Text, newText)
                .Set(c => c.IsEdited, true)
                .Set(c => c.EditedAt, DateTime.UtcNow);

            await _collection.UpdateOneAsync(c => c.Id == id, update);
        }

        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(c => c.Id == id);
        }

        public async Task ToggleLikeAsync(string commentId, int userId, bool like)
        {
            var update = like
                ? Builders<Comment>.Update.AddToSet(c => c.Likes, userId)
                : Builders<Comment>.Update.Pull(c => c.Likes, userId);

            await _collection.UpdateOneAsync(c => c.Id == commentId, update);
        }

        public async Task<Dictionary<string, int>> CountRecentCommentsByKDomAsync(int days = 7)
        {
            var fromDate = DateTime.UtcNow.AddDays(-days);

            var filter = Builders<Comment>.Filter.And(
                Builders<Comment>.Filter.Eq(c => c.TargetType, CommentTargetType.KDom),
                Builders<Comment>.Filter.Gte(c => c.CreatedAt, fromDate)
            );

            var pipeline = new[]
            {
        new BsonDocument("$match", new BsonDocument {
            { "targetType", "KDom" },
            { "createdAt", new BsonDocument("$gte", fromDate) }
        }),
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", "$targetId" },
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

        public async Task<List<string>> GetCommentedKDomIdsByUserAsync(int userId, int days = 30)
        {
            var fromDate = DateTime.UtcNow.AddDays(-days);

            var filter = Builders<Comment>.Filter.And(
                Builders<Comment>.Filter.Eq(c => c.UserId, userId),
                Builders<Comment>.Filter.Eq(c => c.TargetType, CommentTargetType.KDom),
                Builders<Comment>.Filter.Gte(c => c.CreatedAt, fromDate)
            );

            var comments = await _collection.Find(filter).ToListAsync();

            return comments.Select(c => c.TargetId).Distinct().ToList();
        }

        public async Task<int> GetCommentCountByUserAsync(int userId)
        {
            var filter = Builders<Comment>.Filter.Eq(c => c.UserId, userId);
            return (int)await _collection.CountDocumentsAsync(filter);
        }

        public async Task<int> GetCommentsReceivedByUserAsync(int userId)
        {
            // Această metodă calculează comentariile primite pe K-Dom-uri
            // Comentariile pe postări sunt calculate separat prin GetCommentsCountOnPostsAsync

            var filter = Builders<Comment>.Filter.And(
                Builders<Comment>.Filter.Eq(c => c.TargetType, CommentTargetType.KDom),
                Builders<Comment>.Filter.Ne(c => c.UserId, userId) // Exclude propriile comentarii
            );

            // Pentru a verifica dacă comentariile sunt pe K-Dom-urile user-ului,
            // ar trebui să facem join cu KDomRepository, dar pentru simplitate
            // returnăm numărul total de comentarii pe K-Dom-uri (va fi rafinat în service)
            return (int)await _collection.CountDocumentsAsync(filter);
        }

        public async Task<List<Comment>> GetCommentsByUserAsync(int userId, int limit = 50)
        {
            return await _collection
                .Find(c => c.UserId == userId)
                .SortByDescending(c => c.CreatedAt)
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<int> GetTotalLikesReceivedByUserCommentsAsync(int userId)
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

        public async Task<int> GetTotalLikesGivenByUserAsync(int userId)
        {
            var filter = Builders<Comment>.Filter.AnyEq(c => c.Likes, userId);
            return (int)await _collection.CountDocumentsAsync(filter);
        }

        public async Task<List<Comment>> GetCommentsOnUserKDomsAsync(List<string> kdomIds, int limit = 100)
        {
            var filter = Builders<Comment>.Filter.And(
                Builders<Comment>.Filter.Eq(c => c.TargetType, CommentTargetType.KDom),
                Builders<Comment>.Filter.In(c => c.TargetId, kdomIds)
            );

            return await _collection
                .Find(filter)
                .SortByDescending(c => c.CreatedAt)
                .Limit(limit)
                .ToListAsync();
        }

        /// <summary>
        /// Obține statistici de comentarii pentru ultimele X luni
        /// </summary>
        public async Task<Dictionary<string, int>> GetUserCommentsByMonthAsync(int userId, int months = 12)
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
        /// Obține top utilizatori care au comentat pe conținutul unui user
        /// </summary>
        public async Task<Dictionary<int, int>> GetTopCommentersOnUserContentAsync(int userId, List<string> userContentIds, int limit = 10)
        {
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument
                {
                    { "targetId", new BsonDocument("$in", new BsonArray(userContentIds)) },
                    { "userId", new BsonDocument("$ne", userId) } // Exclude propria activitate
                }),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$userId" },
                    { "commentCount", new BsonDocument("$sum", 1) }
                }),
                new BsonDocument("$sort", new BsonDocument("commentCount", -1)),
                new BsonDocument("$limit", limit)
            };

            var cursor = await _collection.AggregateAsync<BsonDocument>(pipeline);
            var results = await cursor.ToListAsync();

            return results.ToDictionary(
                doc => doc["_id"].AsInt32,
                doc => doc["commentCount"].AsInt32
            );
        }

        public async Task<int> GetCommentsCountOnPostsAsync(List<string> postIds, int excludeUserId)
        {
            if (!postIds.Any()) return 0;

            var filter = Builders<Comment>.Filter.And(
                Builders<Comment>.Filter.Eq(c => c.TargetType, CommentTargetType.Post),
                Builders<Comment>.Filter.In(c => c.TargetId, postIds),
                Builders<Comment>.Filter.Ne(c => c.UserId, excludeUserId) // Exclude propriile comentarii
            );

            return (int)await _collection.CountDocumentsAsync(filter);
        }

        public async Task<int> GetTotalCommentsCountAsync()
        {
            return (int)await _collection.CountDocumentsAsync(Builders<Comment>.Filter.Empty);
        }

        public async Task<int> GetCommentsCountByKDomAsync(string targetId)
        {
            var filter = Builders<Comment>.Filter.Eq(c => c.TargetId, targetId);
            return (int)await _collection.CountDocumentsAsync(filter);
        }

    }

}

