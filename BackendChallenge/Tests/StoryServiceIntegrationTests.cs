using Backend_Challenge.Models;
using Backend_Challenge.Services;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using RestSharp;
using System.Net;
using Xunit;

namespace Backend_Challenge.IntegrationTests
{
    public class StoryServiceIntegrationTests
    {

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetStoryItems_ReturnsItemsFromLiveApi()
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var logger = new LoggerFactory().CreateLogger<StoryService>();
            var restClient = new RestClient("https://hacker-news.firebaseio.com/");

            var service = new StoryService(logger, cache, restClient);

            var results = await service.GetStoryItems(5, 1);

            Assert.NotEmpty(results.stories);
            Assert.All(results.stories, item => Assert.False(string.IsNullOrEmpty(item.Title)));
            Assert.Equal(5, results.stories.Count());
        }
    }
}
