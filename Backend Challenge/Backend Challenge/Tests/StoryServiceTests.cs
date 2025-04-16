using Backend_Challenge.Models;
using Backend_Challenge.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using RestSharp;
using System.Net;
using System.Text.Json;
using Xunit;

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
            Id = 43700903,
            Kids = null,
            Score = 2,
            Time = 1744761701,
            Title = "An Interactive Introduction to Rotors from Geometric Algebra",
            Type = "story",
            Url = "https://marctenbosch.com/quaternions/"
        };

        // Mock individual story item fetches
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

        var service = new StoryService(logger, memoryCache, restClientMock.Object);

        // Act
        var results = await service.GetStoryItems(2, 1);

        // Assert
        Assert.Equal(2, results.Count);
        //Assert.Contains(results, s => s.Id == 43700918 && s.Title == "Prototaxites Don't Belong to Living Lineage – Distinct Branch of Multicellular");
        Assert.True(results.Any(x => x.Id == storyItem1.Id && x.By == storyItem1.By && x.Title == storyItem1.Title));

        //Assert.Contains(results, s => s.Id == 43700903 && s.Title == "An Interactive Introduction to Rotors from Geometric Algebra");
    }

    [Fact]
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
        Assert.Empty(result);
    }
}
