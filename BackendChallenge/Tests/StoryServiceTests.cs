using Backend_Challenge.Models;
using Backend_Challenge.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using RestSharp;
using System.Net;
using System.Text.Json;
using Xunit;

namespace Backend_Challenge.UnitTests
{
    public class StoryServiceTests
    {
        private readonly Mock<IRestClient> restClientMock;
        private readonly IMemoryCache memoryCache;
        private readonly ILogger<StoryService> logger;

        public StoryServiceTests()
        {
            restClientMock = new Mock<IRestClient>();
            memoryCache = new MemoryCache(new MemoryCacheOptions());
            logger = new LoggerFactory().CreateLogger<StoryService>();
        }

        #region Get Story Items - Happy Path Tests

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetStoryItems_ReturnsExpectedResults()
        {
            // Arrange
            var ids = new List<long> { 1, 2 };
            var idsJson = JsonSerializer.Serialize(ids);

            // Mock the "get IDs" call
            restClientMock
                .Setup(client => client.ExecuteAsync(It.Is<RestRequest>(r => r.Resource == "v0/newstories.json"), default))
                .ReturnsAsync(new RestResponse
                {
                    Content = idsJson,
                    StatusCode = HttpStatusCode.OK,
                    IsSuccessStatusCode = true
                });

            var storyItem1 = new StoryItem
            {
                By = "nobody9999",
                Descendants = 2,
                Id = 1,
                Kids = new List<int> { 43699924, 43699922 },
                Score = 43700918,
                Time = 1744762056,
                Title = "Prototaxites Don't Belong to Living Lineage – Distinct Branch of Multicellular",
                Type = "story",
                Url = "https://astrobiology.com/2025/03/ancient-prototaxites-dont-belong-to-any-living-lineage-possibly-a-distinct-branch-of-multicellular-earth-life.html"
            };
            var storyItem2 = new StoryItem
            {
                By = "kjeetgill",
                Descendants = 0,
                Id = 2,
                Kids = null,
                Score = 2,
                Time = 1744761701,
                Title = "An Interactive Introduction to Rotors from Geometric Algebra",
                Type = "story",
                Url = "https://marctenbosch.com/quaternions/"
            };

            // Mock individual story item fetches
            SetupStoryItemMocks(storyItem1, storyItem2);

            var service = new StoryService(logger, memoryCache, restClientMock.Object);

            // Act
            var storyResponse = await service.GetStoryItems(2, 1);

            // Assert
            Assert.Equal(2, storyResponse.stories.Count());
            Assert.Contains(storyResponse.stories, story => story.Id == storyItem1.Id && story.By == storyItem1.By && story.Title == storyItem1.Title);
            Assert.Contains(storyResponse.stories, story => story.Id == storyItem2.Id && story.By == storyItem2.By && story.Title == storyItem2.Title);
            restClientMock.Verify(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetStoryItems_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var ids = new List<long> { 1, 2, 3, 4, 5, 6 };
            var idsJson = JsonSerializer.Serialize(ids);

            restClientMock
                .Setup(client => client.ExecuteAsync(It.Is<RestRequest>(r => r.Resource == "v0/newstories.json"), default))
                .ReturnsAsync(new RestResponse
                {
                    Content = idsJson,
                    StatusCode = HttpStatusCode.OK,
                    IsSuccessStatusCode = true
                });

            var storyItem3 = new StoryItem
            {
                By = "author3",
                Id = 3,
                Title = "Third Story",
                Type = "story",
                Score = 100,
                Time = 1744762000
            };
            var storyItem4 = new StoryItem
            {
                By = "author4",
                Id = 4,
                Title = "Fourth Story",
                Type = "story",
                Score = 200,
                Time = 1744763000
            };

            SetupStoryItemMocks(storyItem3, storyItem4);

            var service = new StoryService(logger, memoryCache, restClientMock.Object);

            // Act - Get page 2 with 2 items per page (should get items 3 and 4)
            var storyResponse = await service.GetStoryItems(2, 2);

            // Assert
            Assert.Equal(2, storyResponse.stories.Count());
            Assert.Contains(storyResponse.stories, story => story.Id == 3);
            Assert.Contains(storyResponse.stories, story => story.Id == 4);
            Assert.DoesNotContain(storyResponse.stories, story => story.Id == 1 || story.Id == 2);
        }

        #endregion

        #region Get Story Items - Failure Tests

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetStoryItems_ThrowsHttpRequestExceptionWhenApiFails()
        {
            // Arrange: throw exception on fetch
            restClientMock
                .Setup(client => client.ExecuteAsync(It.IsAny<RestRequest>(), default))
                .ThrowsAsync(new HttpRequestException("API unreachable"));

            var service = new StoryService(logger, memoryCache, restClientMock.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => service.GetStoryItems(10, 1));
            Assert.Contains("API unreachable", exception.Message);
            restClientMock.Verify(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetStoryItems_ThrowsHttpRequestExceptionWhenApiReturnsNonSuccessStatusCode()
        {
            // Arrange
            restClientMock
                .Setup(client => client.ExecuteAsync(It.IsAny<RestRequest>(), default))
                .ReturnsAsync(new RestResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = "Internal Server Error",
                    IsSuccessStatusCode = false
                });

            var service = new StoryService(logger, memoryCache, restClientMock.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => service.GetStoryItems(5, 1));
            Assert.Contains("Failed to fetch new story IDs", exception.Message);
            restClientMock.Verify(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region Caching - Happy Path Tests

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetStoryItems_UsesCacheIfAvailable()
        {
            // Arrange
            var cache = new MemoryCache(new MemoryCacheOptions());
            SetupStoryItemMocks();

            var expectedIds = new List<long> { 1, 2 };

            // Manually add to cache
            cache.Set("new-story-ids", expectedIds);

            var service = new StoryService(logger, cache, restClientMock.Object);

            // Act
            var storyResponse = await service.GetStoryItems(2, 1);

            // Assert
            Assert.Equal(2, storyResponse.stories.Count());
            Assert.Equal("Prototaxites Don't Belong to Living Lineage – Distinct Branch of Multicellular", storyResponse.stories[0].Title);
            // Should only call API for individual items, not for the IDs list
            restClientMock.Verify(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetStoryItems_CachesIdsAfterFirstCall()
        {
            // Arrange
            var cache = new MemoryCache(new MemoryCacheOptions());
            var ids = new List<long> { 1, 2 };
            var idsJson = JsonSerializer.Serialize(ids);

            restClientMock
            .Setup(client => client.ExecuteAsync(It.Is<RestRequest>(r => r.Resource == "v0/newstories.json"), default))
            .ReturnsAsync(new RestResponse
            {
                Content = idsJson,
                StatusCode = HttpStatusCode.OK,
                IsSuccessStatusCode = true
            });

            SetupStoryItemMocks();

            var service = new StoryService(logger, cache, restClientMock.Object);

            // Act - First call
            await service.GetStoryItems(2, 1);

            // Act - Second call
            await service.GetStoryItems(2, 1);

            // Assert
            Assert.True(cache.TryGetValue("new-story-ids", out var cachedIds));
            Assert.Equal(ids, cachedIds);
            // Should call newstories.json only once, then use cache
            restClientMock.Verify(c => c.ExecuteAsync(It.Is<RestRequest>(r => r.Resource == "v0/newstories.json"), It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region Caching - Failure Tests

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetStoryItems_ThrowsHttpRequestExceptionWhenCacheExpiredAndApiFails()
        {
            // Arrange
            var cache = new MemoryCache(new MemoryCacheOptions());
            var ids = new List<long> { 1, 2 };

            // Add expired cache entry
            cache.Set("new-story-ids", ids, TimeSpan.FromMilliseconds(1));
            await Task.Delay(10); // Wait for cache to expire

            restClientMock
                .Setup(client => client.ExecuteAsync(It.Is<RestRequest>(r => r.Resource == "v0/newstories.json"), default))
                .ReturnsAsync(new RestResponse
                {
                    StatusCode = HttpStatusCode.ServiceUnavailable,
                    IsSuccessStatusCode = false
                });

            var service = new StoryService(logger, cache, restClientMock.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => service.GetStoryItems(2, 1));
            Assert.Contains("Failed to fetch new story IDs", exception.Message);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetStoryItems_WhenCacheExpired_RefetchesFromApi()
        {
            // Arrange
            var cache = new MemoryCache(new MemoryCacheOptions());
            var ids = new List<long> { 1, 2 };
            var idsJson = JsonSerializer.Serialize(ids);

            // Add expired cache entry
            cache.Set("new-story-ids", ids, TimeSpan.FromMilliseconds(1));
            await Task.Delay(10); // Wait for cache to expire

            restClientMock
                .Setup(client => client.ExecuteAsync(It.Is<RestRequest>(r => r.Resource == "v0/newstories.json"), default))
                .ReturnsAsync(new RestResponse
                {
                    Content = idsJson,
                    StatusCode = HttpStatusCode.OK,
                    IsSuccessStatusCode = true
                });

            SetupStoryItemMocks();

            var service = new StoryService(logger, cache, restClientMock.Object);

            // Act
            var storyResponse = await service.GetStoryItems(2, 1);

            // Assert
            Assert.Equal(2, storyResponse.stories.Count());
            // Should call API for both IDs and individual items since cache expired
            restClientMock.Verify(c => c.ExecuteAsync(It.Is<RestRequest>(r => r.Resource == "v0/newstories.json"), It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region Individual Story Fetching - Happy Path Tests

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetStoryItems_FetchesIndividualStoriesSuccessfully()
        {
            // Arrange
            var ids = new List<long> { 1, 2 };
            var idsJson = JsonSerializer.Serialize(ids);

            // Mock the "get IDs" call
            restClientMock
                .Setup(client => client.ExecuteAsync(It.Is<RestRequest>(r => r.Resource == "v0/newstories.json"), default))
                .ReturnsAsync(new RestResponse
                {
                    Content = idsJson,
                    StatusCode = HttpStatusCode.OK,
                    IsSuccessStatusCode = true
                });

            SetupStoryItemMocks();

              var service = new StoryService(logger, memoryCache, restClientMock.Object);

            // Act
            var storyResponse = await service.GetStoryItems(1, 1);

            // Assert
            Assert.Single(storyResponse.stories);
            var story = storyResponse.stories.First();
            Assert.Equal(1, story.Id);
            Assert.Equal("nobody9999", story.By);
            Assert.Equal("Prototaxites Don't Belong to Living Lineage – Distinct Branch of Multicellular", story.Title);
            Assert.Equal(43700918, story.Score);
            Assert.Equal("https://astrobiology.com/2025/03/ancient-prototaxites-dont-belong-to-any-living-lineage-possibly-a-distinct-branch-of-multicellular-earth-life.html", story.Url);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetStoryItems_HandlesStoryWithNullUrl()
        {
            // Arrange
            var ids = new List<long> { 200 };
            var idsJson = JsonSerializer.Serialize(ids);

            restClientMock
                .Setup(client => client.ExecuteAsync(It.Is<RestRequest>(r => r.Resource == "v0/newstories.json"), default))
                .ReturnsAsync(new RestResponse
                {
                    Content = idsJson,
                    StatusCode = HttpStatusCode.OK,
                    IsSuccessStatusCode = true
                });

            var storyItem = new StoryItem
            {
                By = "askhn",
                Id = 200,
                Title = "Ask HN: How do you handle stress?",
                Type = "story",
                Score = 25,
                Time = 1744762000,
                Url = null, // Ask HN posts don't have URLs
                Descendants = 10
            };

            restClientMock
                .Setup(client => client.ExecuteAsync(It.Is<RestRequest>(r => r.Resource == "v0/item/200.json"), default))
                .ReturnsAsync(new RestResponse
                {
                    Content = JsonSerializer.Serialize(storyItem),
                    StatusCode = HttpStatusCode.OK,
                    IsSuccessStatusCode = true
                });

            var service = new StoryService(logger, memoryCache, restClientMock.Object);

            // Act
            var storyResponse = await service.GetStoryItems(1, 1);

            // Assert
            Assert.Single(storyResponse.stories);
            var story = storyResponse.stories.First();
            Assert.Equal(200, story.Id);
            Assert.Equal("Ask HN: How do you handle stress?", story.Title);
            Assert.Null(story.Url);
        }

        #endregion

        #region Individual Story Fetching - Failure Tests

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetStoryItems_SkipsStoryWhenIndividualFetchFails()
        {
            // Arrange
            var ids = new List<long> { 1, 2 };
            var idsJson = JsonSerializer.Serialize(ids);

            restClientMock
                .Setup(client => client.ExecuteAsync(It.Is<RestRequest>(r => r.Resource == "v0/newstories.json"), default))
                .ReturnsAsync(new RestResponse
                {
                    Content = idsJson,
                    StatusCode = HttpStatusCode.OK,
                    IsSuccessStatusCode = true
                });

            var storyItem1 = new StoryItem { Id = 1, Title = "Working Story", By = "author1", Type = "story" };

            // First story succeeds
            restClientMock
                .Setup(client => client.ExecuteAsync(It.Is<RestRequest>(r => r.Resource == "v0/item/1.json"), default))
                .ReturnsAsync(new RestResponse
                {
                    Content = JsonSerializer.Serialize(storyItem1),
                    StatusCode = HttpStatusCode.OK,
                    IsSuccessStatusCode = true
                });

            // Second story fails
            restClientMock
                .Setup(client => client.ExecuteAsync(It.Is<RestRequest>(r => r.Resource == "v0/item/2.json"), default))
                .ReturnsAsync(new RestResponse
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccessStatusCode = false
                });

            var service = new StoryService(logger, memoryCache, restClientMock.Object);

            // Act
            var storyResponse = await service.GetStoryItems(2, 1);

            // Assert
            Assert.Single(storyResponse.stories); // Only the successful one
            Assert.Equal(1, storyResponse.stories.First().Id);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetStoryItems_HandlesInvalidJsonInStoryResponse()
        {
            // Arrange
            var ids = new List<long> { 1 };
            var idsJson = JsonSerializer.Serialize(ids);

            restClientMock
                .Setup(client => client.ExecuteAsync(It.Is<RestRequest>(r => r.Resource == "v0/newstories.json"), default))
                .ReturnsAsync(new RestResponse
                {
                    Content = idsJson,
                    StatusCode = HttpStatusCode.OK,
                    IsSuccessStatusCode = true
                });

            // Return invalid JSON for story item
            restClientMock
                .Setup(client => client.ExecuteAsync(It.Is<RestRequest>(r => r.Resource == "v0/item/1.json"), default))
                .ReturnsAsync(new RestResponse
                {
                    Content = "{ invalid json }",
                    StatusCode = HttpStatusCode.OK,
                    IsSuccessStatusCode = true
                });

            var service = new StoryService(logger, memoryCache, restClientMock.Object);

            // Act
            await Assert.ThrowsAsync<JsonException>(async () =>
            {
                await service.GetStoryItems(10, 1);
            });
        }

        #endregion

        #region Edge Cases

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetStoryItems_WhenCacheContainsInvalidData_FallsBackToApi()
        {
            // Arrange
            var cache = new MemoryCache(new MemoryCacheOptions());

            // Add invalid data to cache - this should cause TryGetValue to return false or cast to fail
            cache.Set("new-story-ids", "invalid-data");

            var ids = new List<long> { 1, 2 };
            var idsJson = JsonSerializer.Serialize(ids);

            restClientMock
                .Setup(client => client.ExecuteAsync(It.Is<RestRequest>(r => r.Resource == "v0/newstories.json"), default))
                .ReturnsAsync(new RestResponse
                {
                    Content = idsJson,
                    StatusCode = HttpStatusCode.OK,
                    IsSuccessStatusCode = true
                });

            SetupStoryItemMocks();

            var service = new StoryService(logger, cache, restClientMock.Object);

            // Act
            var storyResponse = await service.GetStoryItems(2, 1);

            // Assert
            Assert.Equal(2, storyResponse.stories.Count());
            // Should fallback to API call when cache contains invalid data
            restClientMock.Verify(c => c.ExecuteAsync(It.Is<RestRequest>(r => r.Resource == "v0/newstories.json"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetStoryItems_WithZeroCount_StillFetchesIdsButReturnsEmptyList()
        {
            // Arrange
            var ids = new List<long> { 1, 2, 3 };
            var idsJson = JsonSerializer.Serialize(ids);

            restClientMock
                .Setup(client => client.ExecuteAsync(It.Is<RestRequest>(r => r.Resource == "v0/newstories.json"), default))
                .ReturnsAsync(new RestResponse
                {
                    Content = idsJson,
                    StatusCode = HttpStatusCode.OK,
                    IsSuccessStatusCode = true
                });

            var service = new StoryService(logger, memoryCache, restClientMock.Object);

            // Act
            var storyResponse = await service.GetStoryItems(0, 1);

            // Assert
            Assert.Empty(storyResponse.stories);
            Assert.Equal(3, storyResponse.numberOfStories); // Total count should still be available
            // Should still call to get IDs but not individual stories
            restClientMock.Verify(c => c.ExecuteAsync(It.Is<RestRequest>(r => r.Resource == "v0/newstories.json"), It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region Helper Methods

        private void SetupStoryItemMocks(params StoryItem[] storyItems)
        {
            if (storyItems.Length == 0)
            {
                // Default setup for backwards compatibility
                var storyItem1 = new StoryItem
                {
                    By = "nobody9999",
                    Descendants = 2,
                    Id = 1,
                    Kids = new List<int> { 43699924, 43699922 },
                    Score = 43700918,
                    Time = 1744762056,
                    Title = "Prototaxites Don't Belong to Living Lineage – Distinct Branch of Multicellular",
                    Type = "story",
                    Url = "https://astrobiology.com/2025/03/ancient-prototaxites-dont-belong-to-any-living-lineage-possibly-a-distinct-branch-of-multicellular-earth-life.html"
                };
                var storyItem2 = new StoryItem
                {
                    By = "kjeetgill",
                    Descendants = 0,
                    Id = 2,
                    Kids = null,
                    Score = 2,
                    Time = 1744761701,
                    Title = "An Interactive Introduction to Rotors from Geometric Algebra",
                    Type = "story",
                    Url = "https://marctenbosch.com/quaternions/"
                };
                storyItems = new[] { storyItem1, storyItem2 };
            }

            foreach (var storyItem in storyItems)
            {
                restClientMock
                    .Setup(client => client.ExecuteAsync(It.Is<RestRequest>(r => r.Resource == $"v0/item/{storyItem.Id}.json"), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new RestResponse
                    {
                        Content = JsonSerializer.Serialize(storyItem),
                        StatusCode = HttpStatusCode.OK,
                        ResponseStatus = ResponseStatus.Completed,
                        IsSuccessStatusCode = true
                    });
            }
        }

        #endregion
    }
}