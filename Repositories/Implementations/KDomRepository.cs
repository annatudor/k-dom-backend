using KDomBackend.Data;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace KDomBackend.Repositories.Implementations
{
    public class KDomRepository : IKDomRepository
    {
        private readonly IMongoCollection<KDom> _collection;

        private readonly MongoDbContext _context;

        public KDomRepository(MongoDbContext context)
        {
            _context = context;
            _collection = context.KDoms;
        }


        public async Task CreateAsync(KDom kdom)
        {
            await _collection.InsertOneAsync(kdom);
        }

        public async Task<KDom?> GetByIdAsync(string id)
        {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task UpdateContentAsync(string id, string newContentHtml)
        {
            var update = Builders<KDom>.Update
                .Set(x => x.ContentHtml, newContentHtml)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            await _collection.UpdateOneAsync(x => x.Id == id, update);
        }

        public async Task SaveEditAsync(KDomEdit edit)
        {
            await _context.KDomEdits.InsertOneAsync(edit);
        }

        public async Task UpdateMetadataAsync(KDomUpdateMetadataDto dto)
        {
            var update = Builders<KDom>.Update
                .Set(x => x.Title, dto.Title)
                .Set(x => x.Description, dto.Description)
                .Set(x => x.Hub, dto.Hub)
                .Set(x => x.Language, dto.Language)
                .Set(x => x.IsForKids, dto.IsForKids)
                .Set(x => x.Theme, dto.Theme)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            // Gestionează ParentId - poate fi null, string gol sau ObjectId valid
            if (dto.ParentId == null || string.IsNullOrWhiteSpace(dto.ParentId))
            {
                update = update.Set(x => x.ParentId, null);
            }
            else
            {
                // Verifică că parentId este un ObjectId valid
                if (ObjectId.TryParse(dto.ParentId, out _))
                {
                    update = update.Set(x => x.ParentId, dto.ParentId);
                }
                else
                {
                    throw new ArgumentException($"Invalid ParentId format: {dto.ParentId}");
                }
            }

            await _collection.UpdateOneAsync(
                x => x.Id == dto.KDomSlug, // Folosește KDomSlug care conține ID-ul
                update
            );
        }

        public async Task SaveMetadataEditAsync(KDomMetadataEdit edit)
        {
            await _context.KDomMetadataEdits.InsertOneAsync(edit);
        }

        public async Task<List<KDomEdit>> GetEditsByKDomIdAsync(string kdomId)
        {
            return await _context.KDomEdits
                .Find(e => e.KDomId == kdomId)
                .SortByDescending(e => e.EditedAt)
                .ToListAsync();
        }

        public async Task<List<KDomMetadataEdit>> GetMetadataEditsByKDomIdAsync(string kdomId)
        {
            return await _context.KDomMetadataEdits
                .Find(e => e.KDomId == kdomId)
                .SortByDescending(e => e.EditedAt)
                .ToListAsync();
        }

        public async Task<List<KDom>> GetPendingKdomsAsync()
        {
            Console.WriteLine("[DEBUG] KDomRepository - GetPendingKdomsAsync called");

            // Încearcă mai multe criterii pentru a găsi K-DOM-urile pending
            var filters = new List<FilterDefinition<KDom>>();

            // Criteriu 1: Status = "Pending"
            filters.Add(Builders<KDom>.Filter.Eq(k => k.Status, "Pending"));

            // Criteriu 2: isApproved = false ȘI isRejected = false
            filters.Add(Builders<KDom>.Filter.And(
                Builders<KDom>.Filter.Eq(k => k.IsApproved, false),
                Builders<KDom>.Filter.Eq(k => k.IsRejected, false)
            ));

            // Criteriu 3: Status nu este setat sau este null (K-DOM-uri vechi)
            filters.Add(Builders<KDom>.Filter.Or(
                Builders<KDom>.Filter.Eq(k => k.Status, null),
                Builders<KDom>.Filter.Eq(k => k.Status, ""),
                Builders<KDom>.Filter.Exists(k => k.Status, false)
            ));

            // Combină toate criteriile cu OR
            var finalFilter = Builders<KDom>.Filter.Or(filters);

            var result = await _collection.Find(finalFilter)
                .SortBy(k => k.CreatedAt)
                .ToListAsync();

            Console.WriteLine($"[DEBUG] KDomRepository - Found {result.Count} pending K-DOMs");

            // Debug: afișează primul K-DOM găsit
            if (result.Any())
            {
                var first = result.First();
                Console.WriteLine($"[DEBUG] First pending K-DOM: Title='{first.Title}', Status='{first.Status}', IsApproved={first.IsApproved}, IsRejected={first.IsRejected}");
            }
            else
            {
                Console.WriteLine("[DEBUG] No pending K-DOMs found, checking total count...");
                var totalCount = await _collection.CountDocumentsAsync(Builders<KDom>.Filter.Empty);
                Console.WriteLine($"[DEBUG] Total K-DOMs in collection: {totalCount}");

                // Afișează primul K-DOM din colecție pentru debug
                var firstAny = await _collection.Find(Builders<KDom>.Filter.Empty).FirstOrDefaultAsync();
                if (firstAny != null)
                {
                    Console.WriteLine($"[DEBUG] Sample K-DOM: Title='{firstAny.Title}', Status='{firstAny.Status}', IsApproved={firstAny.IsApproved}, IsRejected={firstAny.IsRejected}");
                }
            }

            return result;
        }

        public async Task ApproveAsync(string kdomId)
        {
            var update = Builders<KDom>.Update
                .Set(k => k.Status, "Approved")
                .Set(k => k.IsApproved, true)
                .Set(k => k.IsRejected, false) 
                .Set(k => k.ModeratedAt, DateTime.UtcNow)
                .Set(k => k.UpdatedAt, DateTime.UtcNow);

            await _collection.UpdateOneAsync(k => k.Id == kdomId, update);
        }

        public async Task RejectAsync(string kdomId, string reason)
        {
            var update = Builders<KDom>.Update
                .Set(k => k.Status, "Rejected")
                .Set(k => k.IsApproved, false) 
                .Set(k => k.IsRejected, true)
                .Set(k => k.RejectionReason, reason)
                .Set(k => k.ModeratedAt, DateTime.UtcNow)
                .Set(k => k.UpdatedAt, DateTime.UtcNow);

            await _collection.UpdateOneAsync(k => k.Id == kdomId, update);
        }

        public async Task<bool> ExistsByTitleOrSlugAsync(string title, string slug)
        {
            var filter = Builders<KDom>.Filter.Or(
                Builders<KDom>.Filter.Eq(k => k.Title, title),
                Builders<KDom>.Filter.Eq(k => k.Slug, slug)
            );

            return await _collection.Find(filter).AnyAsync();
        }

        public async Task<List<KDom>> FindSimilarByTitleAsync(string title)
        {
            var regex = new BsonRegularExpression($".*{Regex.Escape(title)}.*", "i");

            var filter = Builders<KDom>.Filter.Regex(k => k.Title, regex);

            return await _collection.Find(filter).Limit(5).ToListAsync();
        }

        public async Task<List<KDom>> GetChildrenByParentIdAsync(string parentId)
        {
            var filter = Builders<KDom>.Filter.Eq(k => k.ParentId, parentId);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<KDom?> GetParentAsync(string childId)
        {
            var child = await _collection.Find(k => k.Id == childId).FirstOrDefaultAsync();
            if (child?.ParentId == null) return null;

            return await _collection.Find(k => k.Id == child.ParentId).FirstOrDefaultAsync();
        }
        public async Task<List<KDom>> GetSiblingsAsync(string kdomId)
        {
            var current = await _collection.Find(k => k.Id == kdomId).FirstOrDefaultAsync();
            if (current == null || current.ParentId == null)
                return new List<KDom>();

            var filter = Builders<KDom>.Filter.And(
                Builders<KDom>.Filter.Eq(k => k.ParentId, current.ParentId),
                Builders<KDom>.Filter.Ne(k => k.Id, kdomId) // exclude pe sine
            );

            return await _collection.Find(filter).ToListAsync();
        }
        public async Task UpdateCollaboratorsAsync(string kdomId, List<int> collaborators)
        {
            var update = Builders<KDom>.Update.Set(k => k.Collaborators, collaborators);
            await _collection.UpdateOneAsync(k => k.Id == kdomId, update);
        }
        public async Task<List<KDom>> SearchTitleOrSlugByQueryAsync(string query)
        {
            var filter = Builders<KDom>.Filter.Or(
                Builders<KDom>.Filter.Regex(k => k.Title, new MongoDB.Bson.BsonRegularExpression(query, "i")),
                Builders<KDom>.Filter.Regex(k => k.Slug, new MongoDB.Bson.BsonRegularExpression(query, "i"))
            );

            return await _collection.Find(filter)
                                    .Limit(10)
                                    .SortBy(k => k.Title)
                                    .ToListAsync();
        }

        public async Task<List<KDom>> GetByIdsAsync(IEnumerable<string> ids)
        {
            var filter = Builders<KDom>.Filter.In(k => k.Id, ids);
            return await _collection.Find(filter).ToListAsync();
        }
        public async Task<List<KDom>> GetBySlugsAsync(IEnumerable<string> slugs)
        {
            var filter = Builders<KDom>.Filter.In(k => k.Slug, slugs);
            return await _collection.Find(filter).ToListAsync();
        }


        public async Task<Dictionary<string, int>> CountRecentEditsAsync(int days = 7)
        {
            var fromDate = DateTime.UtcNow.AddDays(-days);

            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("editedAt",
                new BsonDocument("$gte", fromDate))),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$kdomId" },
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

        public async Task<List<KDom>> SearchByQueryAsync(string query)
        {
            var filter = Builders<KDom>.Filter.Or(
                Builders<KDom>.Filter.Regex(k => k.Title, new MongoDB.Bson.BsonRegularExpression(query, "i")),
                Builders<KDom>.Filter.Regex(k => k.Slug, new MongoDB.Bson.BsonRegularExpression(query, "i"))
            );

            return await _collection.Find(filter).Limit(10).ToListAsync();
        }

        public async Task<List<KDom>> GetOwnedOrCollaboratedByUserAsync(int userId)
        {
            var filter = Builders<KDom>.Filter.Or(
                Builders<KDom>.Filter.Eq(k => k.UserId, userId),
                Builders<KDom>.Filter.AnyEq(k => k.Collaborators, userId)
            );

            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<KDom?> GetBySlugAsync(string slug)
        {
            return await _collection.Find(x => x.Slug == slug).FirstOrDefaultAsync();
        }


     
        public async Task<int> GetCreatedKDomsCountByUserAsync(int userId)
        {
            var filter = Builders<KDom>.Filter.Eq(k => k.UserId, userId);
            return (int)await _collection.CountDocumentsAsync(filter);
        }

        public async Task<int> GetCollaboratedKDomsCountByUserAsync(int userId)
        {
            var filter = Builders<KDom>.Filter.And(
                Builders<KDom>.Filter.Ne(k => k.UserId, userId),
                Builders<KDom>.Filter.AnyEq(k => k.Collaborators, userId)
            );
            return (int)await _collection.CountDocumentsAsync(filter);
        }

        public async Task<List<KDom>> GetKDomsByUserAsync(int userId, bool includeCollaborated = true)
        {
            FilterDefinition<KDom> filter;

            if (includeCollaborated)
            {
                filter = Builders<KDom>.Filter.Or(
                    Builders<KDom>.Filter.Eq(k => k.UserId, userId),
                    Builders<KDom>.Filter.AnyEq(k => k.Collaborators, userId)
                );
            }
            else
            {
                filter = Builders<KDom>.Filter.Eq(k => k.UserId, userId);
            }

            return await _collection
                .Find(filter)
                .SortByDescending(k => k.CreatedAt)
                .ToListAsync();
        }

   
        public async Task<List<string>> GetUserKDomIdsAsync(int userId, bool includeCollaborated = true)
        {
            FilterDefinition<KDom> filter;

            if (includeCollaborated)
            {
                filter = Builders<KDom>.Filter.Or(
                    Builders<KDom>.Filter.Eq(k => k.UserId, userId),
                    Builders<KDom>.Filter.AnyEq(k => k.Collaborators, userId)
                );
            }
            else
            {
                filter = Builders<KDom>.Filter.Eq(k => k.UserId, userId);
            }

            var projection = Builders<KDom>.Projection.Include(k => k.Id);
            var cursor = await _collection.Find(filter).Project(projection).ToCursorAsync();
            var results = await cursor.ToListAsync();

            return results.Select(doc => doc["_id"].AsString).ToList();
        }

   
        public async Task<int> GetUserKDomEditsCountAsync(int userId)
        {
            // Folosește KDomEditRepository prin MongoDB context
            var filter = Builders<KDomEdit>.Filter.Eq(e => e.UserId, userId);
            return (int)await _context.KDomEdits.CountDocumentsAsync(filter);
        }

        public async Task<Dictionary<string, int>> GetUserKDomsByHubAsync(int userId)
        {
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("$or", new BsonArray
                {
                    new BsonDocument("userId", userId),
                    new BsonDocument("collaborators", userId)
                })),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$hub" },
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

        public async Task<Dictionary<string, int>> GetUserKDomsByLanguageAsync(int userId)
        {
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("$or", new BsonArray
                {
                    new BsonDocument("userId", userId),
                    new BsonDocument("collaborators", userId)
                })),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$language" },
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


        public async Task<List<KDom>> GetUserTopCollaborativeKDomsAsync(int userId, int limit = 5)
        {
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("userId", userId)),
                new BsonDocument("$addFields", new BsonDocument
                {
                    { "collaboratorCount", new BsonDocument("$size", "$collaborators") }
                }),
                new BsonDocument("$sort", new BsonDocument("collaboratorCount", -1)),
                new BsonDocument("$limit", limit)
            };

            var cursor = await _collection.AggregateAsync<KDom>(pipeline);
            return await cursor.ToListAsync();
        }

        public async Task<List<KDom>> GetUserRecentlyEditedKDomsAsync(int userId, int limit = 10)
        {
            // Găsește ultimele editări ale user-ului
            var recentEdits = await _context.KDomEdits
                .Find(e => e.UserId == userId)
                .SortByDescending(e => e.EditedAt)
                .Limit(limit)
                .ToListAsync();

            var kdomIds = recentEdits.Select(e => e.KDomId).Distinct().ToList();

            return await _collection
                .Find(k => kdomIds.Contains(k.Id))
                .ToListAsync();
        }

        public async Task<KDomEdit?> GetFirstEditByUserAsync(string kdomId, int userId)
        {
            return await _context.KDomEdits
                .Find(e => e.KDomId == kdomId && e.UserId == userId)
                .SortBy(e => e.EditedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<KDomEdit>> GetEditsByUserAsync(string kdomId, int userId)
        {
            return await _context.KDomEdits
                .Find(e => e.KDomId == kdomId && e.UserId == userId)
                .SortByDescending(e => e.EditedAt)
                .ToListAsync();
        }

        public async Task<int> GetEditCountByUserAsync(string kdomId, int userId)
        {
            return (int)await _context.KDomEdits
                .CountDocumentsAsync(e => e.KDomId == kdomId && e.UserId == userId);
        }

        // moderation tasks

        public async Task DeleteAsync(string kdomId)
        {
            await _collection.DeleteOneAsync(k => k.Id == kdomId);
        }

        public async Task<List<KDom>> GetKDomsByStatusAsync(bool isApproved, bool isRejected)
        {
            var filter = Builders<KDom>.Filter.And(
                Builders<KDom>.Filter.Eq(k => k.IsApproved, isApproved),
                Builders<KDom>.Filter.Eq(k => k.IsRejected, isRejected)
            );

            return await _collection.Find(filter)
                .SortByDescending(k => k.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<KDom>> GetUserKDomsWithStatusAsync(int userId)
        {
            var filter = Builders<KDom>.Filter.Eq(k => k.UserId, userId);

            return await _collection.Find(filter)
                .SortByDescending(k => k.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetPendingCountAsync()
        {
            var filter = Builders<KDom>.Filter.And(
                Builders<KDom>.Filter.Eq(k => k.IsApproved, false),
                Builders<KDom>.Filter.Eq(k => k.IsRejected, false)
            );

            return (int)await _collection.CountDocumentsAsync(filter);
        }

        public async Task<List<KDom>> GetApprovedKdomsAsync()
        {
            var filter = Builders<KDom>.Filter.Eq(k => k.IsApproved, true);
            return await _collection.Find(filter).SortByDescending(k => k.CreatedAt).ToListAsync();
        }

        public async Task<List<KDom>> GetRejectedKdomsAsync()
        {
            var filter = Builders<KDom>.Filter.Eq(k => k.IsRejected, true);
            return await _collection.Find(filter).SortByDescending(k => k.ModeratedAt).ToListAsync();
        }


        public async Task<int> GetApprovedCountAsync(DateTime? fromDate = null)
        {
            var filter = Builders<KDom>.Filter.Eq(k => k.IsApproved, true);

            if (fromDate.HasValue)
            {
                filter = Builders<KDom>.Filter.And(
                    filter,
                    Builders<KDom>.Filter.Gte(k => k.UpdatedAt, fromDate.Value)
                );
            }

            return (int)await _collection.CountDocumentsAsync(filter);
        }

        public async Task<int> GetRejectedCountAsync(DateTime? fromDate = null)
        {
            var filter = Builders<KDom>.Filter.Eq(k => k.IsRejected, true);

            if (fromDate.HasValue)
            {
                filter = Builders<KDom>.Filter.And(
                    filter,
                    Builders<KDom>.Filter.Gte(k => k.UpdatedAt, fromDate.Value)
                );
            }

            return (int)await _collection.CountDocumentsAsync(filter);
        }

        public async Task<List<KDom>> GetRecentlyModeratedAsync(int limit = 10)
        {
            var filter = Builders<KDom>.Filter.Or(
                Builders<KDom>.Filter.Eq(k => k.IsApproved, true),
                Builders<KDom>.Filter.Eq(k => k.IsRejected, true)
            );

            return await _collection.Find(filter)
                .SortByDescending(k => k.UpdatedAt)
                .Limit(limit)
                .ToListAsync();
        }

    

        public async Task<TimeSpan> GetAverageProcessingTimeAsync()
        {
            var pipeline = new[]
            {
        new BsonDocument("$match", new BsonDocument("$or", new BsonArray
        {
            new BsonDocument("isApproved", true),
            new BsonDocument("isRejected", true)
        })),
        new BsonDocument("$addFields", new BsonDocument("processingTime",
            new BsonDocument("$subtract", new BsonArray { "$updatedAt", "$createdAt" }))),
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", BsonNull.Value },
            { "avgProcessingTime", new BsonDocument("$avg", "$processingTime") }
        })
    };

            var cursor = await _collection.AggregateAsync<BsonDocument>(pipeline);
            var result = await cursor.FirstOrDefaultAsync();

            if (result != null && result["avgProcessingTime"] != BsonNull.Value)
            {
                var avgMilliseconds = result["avgProcessingTime"].AsDouble;
                return TimeSpan.FromMilliseconds(avgMilliseconds);
            }

            return TimeSpan.Zero;
        }

        public async Task<DateTime?> GetModerationDateAsync(string kdomId)
        {
            var kdom = await _collection.Find(k => k.Id == kdomId).FirstOrDefaultAsync();
            return kdom?.UpdatedAt;
        }

        public async Task SetModeratorAsync(string kdomId, int moderatorUserId)
        {
            var update = Builders<KDom>.Update
                .Set(k => k.ModeratedByUserId, moderatorUserId);

            await _collection.UpdateOneAsync(k => k.Id == kdomId, update);
        }
    }
}
