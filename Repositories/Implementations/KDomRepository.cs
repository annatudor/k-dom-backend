using KDomBackend.Data;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Interfaces;
using MongoDB.Driver;

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

    }
}
