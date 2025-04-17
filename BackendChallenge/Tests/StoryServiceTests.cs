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
                    StatusCode = HttpStatusCode.OK
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
            clientSetup();

            var service = new StoryService(logger, memoryCache, restClientMock.Object);

            // Act
            var result = await service.GetStoryItems(2, 1);

            // Assert
            Assert.Equal(2, result.stories.Count());

            Assert.Contains(result.stories, x => x.Id == storyItem1.Id && x.By == storyItem1.By && x.Title == storyItem1.Title);
            Assert.Contains(result.stories, x => x.Id == storyItem2.Id && x.By == storyItem2.By && x.Title == storyItem2.Title);
            restClientMock.Verify(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(3));

        }

        [Fact]
        public async Task GetStoryItems_UsesCacheIfAvailable()
        {
            // Arrange
            var cache = new MemoryCache(new MemoryCacheOptions());
            //var mockLogger = new LoggerFactory().CreateLogger<StoryService>();
            //var mockClient = new Mock<IRestClient>();

            clientSetup();

             var expectedIds = new List<long>
            {
                1,
                2
            };

            // Manually add to cache
            cache.Set("new-story-ids", expectedIds);

            var service = new StoryService(logger, cache, restClientMock.Object);

            // Act
            var result = await service.GetStoryItems(2, 1);

            // Assert
            Assert.Equal(2, result.stories.Count());
            Assert.Equal("Prototaxites Don't Belong to Living Lineage – Distinct Branch of Multicellular", result.stories[0].Title);
            restClientMock.Verify(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }


        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetStoryItems_HandlesApiFailureGracefully()
        {
            // Arrange: throw exception on fetch
            restClientMock
                .Setup(client => client.ExecuteAsync(It.IsAny<RestRequest>(), default))
                .ThrowsAsync(new HttpRequestException("API unreachable"));

            var service = new StoryService(logger, memoryCache, restClientMock.Object);

            // Act
            var result = await service.GetStoryItems(10, 1);

            // Assert
            Assert.Empty(result.stories);
            restClientMock.Verify(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        private void clientSetup()
        {
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

            restClientMock
               .Setup(client => client.ExecuteAsync(It.Is<RestRequest>(r => r.Resource == "v0/item/1.json"), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new RestResponse
               {
                   Content = JsonSerializer.Serialize(storyItem1),
                   StatusCode = HttpStatusCode.OK,
                   ResponseStatus = ResponseStatus.Completed,
                   IsSuccessStatusCode = true
               });

            restClientMock
                .Setup(client => client.ExecuteAsync(It.Is<RestRequest>(r => r.Resource == "v0/item/2.json"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RestResponse
                {
                    Content = JsonSerializer.Serialize(storyItem2),
                    StatusCode = HttpStatusCode.OK,
                    ResponseStatus = ResponseStatus.Completed,
                    IsSuccessStatusCode = true
                });
        }
    }
}

