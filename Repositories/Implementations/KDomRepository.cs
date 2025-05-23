﻿using KDomBackend.Data;
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
                .Set(x => x.ParentId, dto.ParentId)
                .Set(x => x.Title, dto.Title)
                .Set(x => x.Description, dto.Description)
                .Set(x => x.Hub, dto.Hub)
                .Set(x => x.Language, dto.Language)
                .Set(x => x.IsForKids, dto.IsForKids)
                .Set(x => x.Theme, dto.Theme)
                .Set(x => x.UpdatedAt, DateTime.UtcNow); 

            await _collection.UpdateOneAsync(
                x => x.Id == dto.KDomId,
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
            var filter = Builders<KDom>.Filter.And(
                Builders<KDom>.Filter.Eq(k => k.IsApproved, false),
                Builders<KDom>.Filter.Eq(k => k.IsRejected, false)
            );

            return await _collection.Find(filter).SortBy(k => k.CreatedAt).ToListAsync();
        }

        public async Task ApproveAsync(string kdomId)
        {
            var update = Builders<KDom>.Update
                .Set(k => k.IsApproved, true);

            await _collection.UpdateOneAsync(k => k.Id == kdomId, update);
        }

        public async Task RejectAsync(string kdomId, string reason)
        {
            var update = Builders<KDom>.Update
                .Set(k => k.IsRejected, true)
                .Set(k => k.RejectionReason, reason);

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


    }
}
