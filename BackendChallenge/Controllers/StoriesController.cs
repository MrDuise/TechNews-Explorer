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
            try
            {
                var equipment = await storyService.GetStoryItems(amount, page);
                return Ok(equipment);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching stories");
                return StatusCode(500, new { message = "An error occurred while fetching stories." });
            }
        }


        [HttpGet("search")]
        public async Task<IActionResult> SearchStories([FromQuery] string queryString)
        {
            var results = await storyService.SearchStories(queryString);
            return Ok(results);
        }
    }
}
