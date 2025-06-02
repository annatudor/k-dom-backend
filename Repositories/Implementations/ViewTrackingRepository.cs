// Repositories/Implementations/ViewTrackingRepository.cs - Versiunea esențială
using KDomBackend.Data;
using KDomBackend.Enums;
using KDomBackend.Models.DTOs.ViewTracking;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace KDomBackend.Repositories.Implementations
{
    public class ViewTrackingRepository : IViewTrackingRepository
    {
        private readonly IMongoCollection<ViewTracking> _collection;

        public ViewTrackingRepository(MongoDbContext context)
        {
            _collection = context.ViewTrackings;
        }

        #region Basic Operations

        public async Task CreateAsync(ViewTracking viewTracking)
        {
            await _collection.InsertOneAsync(viewTracking);
        }

        public async Task<int> GetViewCountAsync(ContentType contentType, string contentId)
        {
            var filter = Builders<ViewTracking>.Filter.And(
                Builders<ViewTracking>.Filter.Eq(v => v.ContentType, contentType),
                Builders<ViewTracking>.Filter.Eq(v => v.ContentId, contentId)
            );

            return (int)await _collection.CountDocumentsAsync(filter);
        }

        public async Task<bool> HasRecentViewAsync(ContentType contentType, string contentId, int? userId, string? ipAddress, int minutesWindow = 30)
        {
            var fromTime = DateTime.UtcNow.AddMinutes(-minutesWindow);

            var filterBuilder = Builders<ViewTracking>.Filter.And(
                Builders<ViewTracking>.Filter.Eq(v => v.ContentType, contentType),
                Builders<ViewTracking>.Filter.Eq(v => v.ContentId, contentId),
                Builders<ViewTracking>.Filter.Gte(v => v.ViewedAt, fromTime)
            );

            // Verifică după user ID dacă este logat, altfel după IP
            if (userId.HasValue)
            {
                filterBuilder = Builders<ViewTracking>.Filter.And(
                    filterBuilder,
                    Builders<ViewTracking>.Filter.Eq(v => v.ViewerId, userId.Value)
                );
            }
            else if (!string.IsNullOrEmpty(ipAddress))
            {
                filterBuilder = Builders<ViewTracking>.Filter.And(
                    filterBuilder,
                    Builders<ViewTracking>.Filter.Eq(v => v.IpAddress, ipAddress)
                );
            }

            return await _collection.Find(filterBuilder).AnyAsync();
        }


       
        public async Task<int> GetRecentViewsCountAsync(ContentType contentType, string contentId, int hours = 24)
        {
            var fromTime = DateTime.UtcNow.AddHours(-hours);

            var filter = Builders<ViewTracking>.Filter.And(
                Builders<ViewTracking>.Filter.Eq(v => v.ContentType, contentType),
                Builders<ViewTracking>.Filter.Eq(v => v.ContentId, contentId),
                Builders<ViewTracking>.Filter.Gte(v => v.ViewedAt, fromTime)
            );

            return (int)await _collection.CountDocumentsAsync(filter);
        }

        public async Task<int> GetUniqueViewersCountAsync(ContentType contentType, string contentId)
        {
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument
                {
                    { "contentType", contentType.ToString() },
                    { "contentId", contentId }
                }),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", new BsonDocument
                        {
                            { "viewerId", "$viewerId" },
                            { "ipAddress", "$ipAddress" }
                        }
                    }
                }),
                new BsonDocument("$count", "uniqueViewers")
            };

            var cursor = await _collection.AggregateAsync<BsonDocument>(pipeline);
            var result = await cursor.FirstOrDefaultAsync();

            return result?["uniqueViewers"]?.AsInt32 ?? 0;
        }

       
        public async Task<DateTime?> GetLastViewedDateAsync(ContentType contentType, string contentId)
        {
            var filter = Builders<ViewTracking>.Filter.And(
                Builders<ViewTracking>.Filter.Eq(v => v.ContentType, contentType),
                Builders<ViewTracking>.Filter.Eq(v => v.ContentId, contentId)
            );

            var sort = Builders<ViewTracking>.Sort.Descending(v => v.ViewedAt);

            var latestView = await _collection.Find(filter)
                .Sort(sort)
                .FirstOrDefaultAsync();

            return latestView?.ViewedAt;
        }

      

        public async Task<Dictionary<string, int>> GetTopViewedContentAsync(ContentType contentType, int limit = 10)
        {
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("contentType", contentType.ToString())),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$contentId" },
                    { "viewCount", new BsonDocument("$sum", 1) }
                }),
                new BsonDocument("$sort", new BsonDocument("viewCount", -1)),
                new BsonDocument("$limit", limit)
            };

            var cursor = await _collection.AggregateAsync<BsonDocument>(pipeline);
            var results = await cursor.ToListAsync();

            return results.ToDictionary(
                doc => doc["_id"].AsString,
                doc => doc["viewCount"].AsInt32
            );
        }

       
        public async Task<List<TrendingContentDto>> GetTrendingContentAsync(ContentType? contentType = null, int hours = 24, int limit = 10)
        {
            var fromTime = DateTime.UtcNow.AddHours(-hours);

            var matchFilter = new BsonDocument("viewedAt", new BsonDocument("$gte", fromTime));

            if (contentType.HasValue)
            {
                matchFilter.Add("contentType", contentType.Value.ToString());
            }

            var pipeline = new[]
            {
                new BsonDocument("$match", matchFilter),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", new BsonDocument
                        {
                            { "contentId", "$contentId" },
                            { "contentType", "$contentType" }
                        }
                    },
                    { "viewCount", new BsonDocument("$sum", 1) },
                    { "lastViewed", new BsonDocument("$max", "$viewedAt") },
                    { "firstViewed", new BsonDocument("$min", "$viewedAt") }
                }),
                new BsonDocument("$addFields", new BsonDocument
                {
                    { "trendingScore", new BsonDocument("$multiply", new BsonArray
                        {
                            "$viewCount",
                            new BsonDocument("$divide", new BsonArray
                            {
                                "$viewCount",
                                new BsonDocument("$add", new BsonArray
                                {
                                    new BsonDocument("$divide", new BsonArray
                                    {
                                        new BsonDocument("$subtract", new BsonArray { DateTime.UtcNow, "$firstViewed" }),
                                        3600000 // Convert to hours
                                    }),
                                    1 // Add 1 to avoid division by zero
                                })
                            })
                        })
                    }
                }),
                new BsonDocument("$sort", new BsonDocument("trendingScore", -1)),
                new BsonDocument("$limit", limit)
            };

            var cursor = await _collection.AggregateAsync<BsonDocument>(pipeline);
            var results = await cursor.ToListAsync();

            return results.Select(doc => new TrendingContentDto
            {
                ContentId = doc["_id"]["contentId"].AsString,
                ContentType = Enum.Parse<ContentType>(doc["_id"]["contentType"].AsString),
                ViewCount = doc["viewCount"].AsInt32,
                TrendingScore = doc["trendingScore"].AsDouble,
                LastViewed = doc["lastViewed"].ToUniversalTime()
            }).ToList();
        }

       
        public async Task<int> GetTotalViewsForPeriodAsync(int days, int offsetDays = 0)
        {
            var endDate = DateTime.UtcNow.AddDays(-offsetDays);
            var startDate = endDate.AddDays(-days);

            var filter = Builders<ViewTracking>.Filter.And(
                Builders<ViewTracking>.Filter.Gte(v => v.ViewedAt, startDate),
                Builders<ViewTracking>.Filter.Lt(v => v.ViewedAt, endDate)
            );

            return (int)await _collection.CountDocumentsAsync(filter);
        }

        public async Task<Dictionary<string, int>> GetViewsByContentTypeAsync(int days = 30)
        {
            var fromDate = DateTime.UtcNow.AddDays(-days);

            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("viewedAt", new BsonDocument("$gte", fromDate))),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$contentType" },
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

        public async Task<Dictionary<string, int>> GetDailyViewsAsync(int days = 30)
        {
            var fromDate = DateTime.UtcNow.AddDays(-days);

            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("viewedAt", new BsonDocument("$gte", fromDate))),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", new BsonDocument("$dateToString", new BsonDocument
                        {
                            { "format", "%Y-%m-%d" },
                            { "date", "$viewedAt" }
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

       
        public async Task<List<string>> GetUserPostIdsAsync(int userId)
        {
            // Această metodă va fi apelată din service care are acces la PostRepository
            // Repository-ul de ViewTracking nu are acces direct la Posts
            throw new NotImplementedException("This method should be called through the service layer");
        }

        /// <summary>
        /// Obține ID-urile K-Dom-urilor unui user pentru calculul view-urilor
        /// </summary>
        public async Task<List<string>> GetUserKDomIdsAsync(int userId)
        {
            // Această metodă va fi apelată din service care are acces la KDomRepository
            // Repository-ul de ViewTracking nu are acces direct la KDoms
            throw new NotImplementedException("This method should be called through the service layer");
        }

        #endregion
    }
}