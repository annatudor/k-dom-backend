using KDomBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KDomBackend.Controllers
{
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new { error = "Query cannot be empty" });

            var result = await _searchService.GlobalSearchAsync(q.Trim());
            return Ok(result);
        }
    }

}
