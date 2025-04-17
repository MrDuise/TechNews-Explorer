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
        //these tests are disabled in the pipeline
        //but can easily be run from vs
        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetStoryItems_ReturnsItemsFromLiveApi()
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var logger = new LoggerFactory().CreateLogger<StoryService>();
            var restClient = new RestClient("https://hacker-news.firebaseio.com/");

            var storyService = new StoryService(logger, cache, restClient);

            var storyResponse = await storyService.GetStoryItems(5, 1);

            Assert.NotEmpty(storyResponse.stories);
            Assert.All(storyResponse.stories, item => Assert.False(string.IsNullOrEmpty(item.Title)));
            Assert.Equal(5, storyResponse.stories.Count());
        }
    }
}
