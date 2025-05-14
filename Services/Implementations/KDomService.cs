using KDomBackend.Helpers;
using KDomBackend.Models.DTOs.KDom;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Implementations;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Services.Interfaces;

namespace KDomBackend.Services.Implementations
{
    public class KDomService : IKDomService
    {
        private readonly IKDomRepository _kdomRepository;
        private readonly IUserRepository _userRepository;

        public KDomService(IKDomRepository kdomRepository, IUserRepository userRepository)
        {
            _kdomRepository = kdomRepository;
            _userRepository = userRepository;
        }

        public async Task CreateKDomAsync(KDomCreateDto dto, int userId)
        {
            var sanitizedHtml = HtmlSanitizerHelper.Sanitize(dto.ContentHtml);

            var kdom = new KDom
            {
                Title = dto.Title,
                Slug = dto.Slug,
                Description = dto.Description,
                Hub = dto.Hub,
                Language = dto.Language,
                IsForKids = dto.IsForKids,
                Theme = dto.Theme,
                ContentHtml = sanitizedHtml,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _kdomRepository.CreateAsync(kdom);
        }

        public async Task<bool> EditKDomAsync(KDomEditDto dto, int userId, bool isAutoSave = true)
        {
            var kdom = await _kdomRepository.GetByIdAsync(dto.KDomId);
            if (kdom == null)
                throw new Exception("KDom not found.");

            var sanitizedHtml = HtmlSanitizerHelper.Sanitize(dto.ContentHtml);

            // nu salva daca e identic
            if (sanitizedHtml == kdom.ContentHtml)
                return false; // nimic de salvat

            var edit = new KDomEdit
            {
                KDomId = dto.KDomId,
                UserId = userId,
                PreviousContentHtml = kdom.ContentHtml,
                NewContentHtml = sanitizedHtml,
                EditNote = dto.EditNote,
                IsMinor = dto.IsMinor,
                IsAutoSave = isAutoSave,
                EditedAt = DateTime.UtcNow
            };

            await _kdomRepository.SaveEditAsync(edit); 
            await _kdomRepository.UpdateContentAsync(dto.KDomId, sanitizedHtml);

            return true;
        }

        public async Task<bool> UpdateKDomMetadataAsync(KDomUpdateMetadataDto dto, int userId)
        {
            var kdom = await _kdomRepository.GetByIdAsync(dto.KDomId);
            if (kdom == null)
                throw new Exception("KDom not found.");

            if (kdom.UserId != userId)
                throw new UnauthorizedAccessException("You are not the author of this KDom.");

            if (kdom.Title == dto.Title &&
                kdom.Description == dto.Description &&
                kdom.Language == dto.Language &&
                kdom.Hub == dto.Hub &&
                kdom.IsForKids == dto.IsForKids &&
                kdom.Theme == dto.Theme)
            {
                return false;
            }

            
            var metadataEdit = new KDomMetadataEdit
            {
                KDomId = kdom.Id,
                UserId = userId,
                PreviousTitle = kdom.Title,
                PreviousDescription = kdom.Description,
                PreviousLanguage = kdom.Language,
                PreviousHub = kdom.Hub,
                PreviousIsForKids = kdom.IsForKids,
                PreviousTheme = kdom.Theme,
                EditedAt = DateTime.UtcNow
            };

            await _kdomRepository.SaveMetadataEditAsync(metadataEdit);
            await _kdomRepository.UpdateMetadataAsync(dto);

            return true;
        }


        public async Task<KDomReadDto> GetKDomByIdAsync(string id)
        {
            var kdom = await _kdomRepository.GetByIdAsync(id);
            if (kdom == null)
                throw new Exception("KDom not found.");

            var user = await _userRepository.GetByIdAsync(kdom.UserId); 
            var username = user?.Username ?? "unknown";

            return new KDomReadDto
            {
                Id = kdom.Id,
                Title = kdom.Title,
                Slug = kdom.Slug,
                Description = kdom.Description,
                Hub = kdom.Hub,
                Theme = kdom.Theme,
                ContentHtml = kdom.ContentHtml,
                Language = kdom.Language,
                IsForKids = kdom.IsForKids,
                UserId = kdom.UserId,
                AuthorUsername = username,
                CreatedAt = kdom.CreatedAt,
                UpdatedAt = kdom.UpdatedAt,
                LastEditedAt = kdom.LastEditedtAt
            };
        }

        public async Task<List<KDomEditReadDto>> GetEditHistoryAsync(string kdomId, int userId)
        {
            var kdom = await _kdomRepository.GetByIdAsync(kdomId);
            if (kdom == null)
                throw new Exception("KDom not found.");

            if (kdom.UserId != userId)
                throw new UnauthorizedAccessException("You are not the author of this KDom.");

            var edits = await _kdomRepository.GetEditsByKDomIdAsync(kdomId);

            return edits.Select(e => new KDomEditReadDto
            {
                Id = e.Id,
                EditNote = e.EditNote ?? "",
                IsMinor = e.IsMinor,
                EditedAt = e.EditedAt
            }).ToList();
        }

        public async Task<List<KDomMetadataEditReadDto>> GetMetadataEditHistoryAsync(string kdomId, int userId)
        {
            var kdom = await _kdomRepository.GetByIdAsync(kdomId);
            if (kdom == null)
                throw new Exception("KDom not found.");

            if (kdom.UserId != userId)
                throw new UnauthorizedAccessException("You are not the owner of this KDom.");

            var edits = await _kdomRepository.GetMetadataEditsByKDomIdAsync(kdomId);

            return edits.Select(e => new KDomMetadataEditReadDto
            {
                Id = e.Id,
                PreviousTitle = e.PreviousTitle,
                PreviousDescription = e.PreviousDescription,
                PreviousLanguage = e.PreviousLanguage,
                PreviousHub = e.PreviousHub,
                PreviousIsForKids = e.PreviousIsForKids,
                PreviousTheme = e.PreviousTheme,
                EditedAt = e.EditedAt
            }).ToList();
        }


    }
}
