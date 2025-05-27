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
        private readonly ILogger<StoryService> logger;
        private const string BaseUrl = "https://hacker-news.firebaseio.com/";

        public StoryServiceIntegrationTests()
        {
            logger = new LoggerFactory().CreateLogger<StoryService>();
        }

        #region Happy Path Integration Tests

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetStoryItems_ReturnsItemsFromLiveApi()
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var restClient = new RestClient(BaseUrl);

            var storyService = new StoryService(logger, cache, restClient);

            var storyResponse = await storyService.GetStoryItems(5, 1);

            Assert.NotEmpty(storyResponse.stories);
            Assert.All(storyResponse.stories, item => Assert.False(string.IsNullOrEmpty(item.Title)));
            Assert.Equal(5, storyResponse.stories.Count());
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetStoryItems_WithPagination_ReturnsCorrectPageFromLiveApi()
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var restClient = new RestClient(BaseUrl);
            var storyService = new StoryService(logger, cache, restClient);

            // Get first page
            var firstPageResponse = await storyService.GetStoryItems(3, 1);

            // Get second page
            var secondPageResponse = await storyService.GetStoryItems(3, 2);

            Assert.Equal(3, firstPageResponse.stories.Count());
            Assert.Equal(3, secondPageResponse.stories.Count());

            // Ensure different stories on different pages
            var firstPageIds = firstPageResponse.stories.Select(s => s.Id).ToHashSet();
            var secondPageIds = secondPageResponse.stories.Select(s => s.Id).ToHashSet();

            Assert.False(firstPageIds.Overlaps(secondPageIds), "Pages should contain different stories");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetStoryItems_CachingWorksWithLiveApi()
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var restClient = new RestClient(BaseUrl);
            var storyService = new StoryService(logger, cache, restClient);

            // First call - should populate cache
            var firstResponse = await storyService.GetStoryItems(2, 1);

            // Verify cache is populated
            Assert.True(cache.TryGetValue("new-story-ids", out var cachedIds));

            // Second call - should use cache
            var secondResponse = await storyService.GetStoryItems(2, 1);

            Assert.Equal(2, firstResponse.stories.Count());
            Assert.Equal(2, secondResponse.stories.Count());

            // Both responses should have the same story IDs since cached
            var firstIds = firstResponse.stories.Select(s => s.Id).OrderBy(x => x);
            var secondIds = secondResponse.stories.Select(s => s.Id).OrderBy(x => x);
            Assert.Equal(firstIds, secondIds);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetStoryItems_HandlesLargePageSize()
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var restClient = new RestClient(BaseUrl);
            var storyService = new StoryService(logger, cache, restClient);

            var storyResponse = await storyService.GetStoryItems(20, 1);

            Assert.Equal(20, storyResponse.stories.Count());
            Assert.All(storyResponse.stories, story =>
            {
                Assert.True(story.Id > 0);
                Assert.False(string.IsNullOrWhiteSpace(story.Title));
                Assert.False(string.IsNullOrWhiteSpace(story.By));
                Assert.True(story.Score >= 0);
                Assert.True(story.Time > 0);
            });
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetStoryItems_ValidatesStoryDataQuality()
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var restClient = new RestClient(BaseUrl);
            var storyService = new StoryService(logger, cache, restClient);

            var storyResponse = await storyService.GetStoryItems(10, 1);

            Assert.Equal(10, storyResponse.stories.Count());

            foreach (var story in storyResponse.stories)
            {
                // Validate required fields
                Assert.True(story.Id > 0, $"Story ID should be positive: {story.Id}");
                Assert.False(string.IsNullOrWhiteSpace(story.Title), "Story title should not be empty");
                Assert.False(string.IsNullOrWhiteSpace(story.By), "Story author should not be empty");
                Assert.Equal("story", story.Type);

                // Validate reasonable values
                Assert.True(story.Score >= 0, "Story score should be non-negative");
                Assert.True(story.Time > 0, "Story time should be positive");
                Assert.True(story.Descendants >= 0, "Story descendants should be non-negative");

                // URL can be null for Ask HN posts, but if present should be valid
                if (!string.IsNullOrEmpty(story.Url))
                {
                    Assert.True(Uri.TryCreate(story.Url, UriKind.Absolute, out _),
                        $"Story URL should be valid: {story.Url}");
                }
            }
        }

        #endregion

        #region Failure Scenario Integration Tests

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetStoryItems_WithInvalidBaseUrl_HandlesFailureGracefully()
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var restClient = new RestClient("https://invalid-hacker-news-api.com/");
            var storyService = new StoryService(logger, cache, restClient);

            await Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                await storyService.GetStoryItems(10, 1);
            });
        }
    }
}
#endregion