using Backend_Challenge.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RestSharp;
using System.Text.Json;

namespace Backend_Challenge.Services
{
    public class StoryService : IStoryService
    {
        private readonly IMemoryCache cache;
        private readonly IRestClient restClient;
        private readonly ILogger<StoryService> logger;
        private const string cacheKey = "new-story-ids";

        public StoryService(ILogger<StoryService> logger, IMemoryCache memoryCache, IRestClient restClient)
        {
            this.logger = logger;
            this.cache = memoryCache;
            this.restClient = restClient;
        }

        public async Task<FindStoryResponse> SearchStories(string query)
        {
            var allIds = await GetNewStoryIDs();
            var allStories = await GetStoryItems(500, 1);

            var filteredStories = allStories.stories
                .Where(s => !string.IsNullOrWhiteSpace(s.Title) && s.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return new FindStoryResponse
            {
                stories = filteredStories,
                numberOfStories = filteredStories.Count
            };
        }

        public async Task<FindStoryResponse> GetStoryItems(int amountPerPage, int page)
        {
            var allIds = await GetNewStoryIDs();

            var pagedIds = allIds
                .Skip((page - 1) * amountPerPage)
                .Take(amountPerPage)
                .ToList();

            var tasks = pagedIds.Select(async id =>
            {
                var request = new RestRequest($"v0/item/{id}.json", Method.Get);
                var response = await restClient.ExecuteAsync(request);

                if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(response.Content))
                    return null;

                return JsonSerializer.Deserialize<StoryItem>(response.Content);
            });

            var results = await Task.WhenAll(tasks);

            return new FindStoryResponse
            {
                stories = results.Where(r => r != null).ToList(),
                numberOfStories = allIds.Count
            };
        }

        private async Task<List<long>> GetNewStoryIDs()
        {
            if (cache.TryGetValue(cacheKey, out List<long> cachedIds))
            {
                logger.LogInformation("Pulling IDs from cache");
                return cachedIds;
            }

            var request = new RestRequest("v0/newstories.json", Method.Get);
            var response = await restClient.ExecuteAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException("Failed to fetch new story IDs.");

            var ids = JsonSerializer.Deserialize<List<long>>(response.Content ?? "[]") ?? new List<long>();

            if (ids.Any())
            {
                logger.LogInformation("Caching new story IDs");
                cache.Set(cacheKey, ids, TimeSpan.FromMinutes(5));
            }

            return ids;
        }
    }
}
