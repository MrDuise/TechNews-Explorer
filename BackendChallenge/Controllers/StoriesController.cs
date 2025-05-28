using Backend_Challenge.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Challenge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoriesController : ControllerBase
    {
        private readonly ILogger<StoriesController> logger;
        private readonly IStoryService storyService;

        public StoriesController(ILogger<StoriesController> logger, IStoryService service)
        {
            this.logger = logger;
            this.storyService = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetStories(int amount, int page)
        {
            var equipment = await storyService.GetStoryItems(amount, page);
            return Ok(equipment);
        }


        [HttpGet("search")]
        public async Task<IActionResult> SearchStories([FromQuery] string queryString)
        {
            var results = await storyService.SearchStories(queryString);
            return Ok(results);
        }
    }
}
