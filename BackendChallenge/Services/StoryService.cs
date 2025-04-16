using Backend_Challenge.Models;
using Microsoft.Extensions.Caching.Memory;
using RestSharp;
using System.Text.Json;
using System.Threading;

namespace Backend_Challenge.Services
{
    public class StoryService : IStoryService
    {
        private readonly IMemoryCache cache;
        private readonly IRestClient restClient;
        private readonly ILogger<StoryService> logger;
        const string cacheKey = "new-story-ids";

        public StoryService(ILogger<StoryService> logger, IMemoryCache memoryCache, IRestClient restClient)
        {
            this.logger = logger;
            this.cache = memoryCache;
            this.restClient = restClient;
        }

        public async Task<List<StoryItem>> GetStoryItems(int amountPerPage, int page)
        {
            try
            {
                var allIds = await GetNewStoryIDs();
                var pagedIds = allIds.Skip((page - 1) * amountPerPage).Take(amountPerPage).ToList(); // creates a paginated list of ids. skips page - 1 times number of items, takes the number of itmes, and turns it into a list.  
                                                                                                     //first page is (1-1) * 10, so skips 0 items, then takes 10 items from allIds
                var tasks = pagedIds.Select(async id =>
                {
                    var request = new RestRequest($"v0/item/{id}.json", Method.Get);
                    var response = await restClient.ExecuteAsync(request);

                    if (response.IsSuccessStatusCode && response.Content != null)
                    {
                       var story = JsonSerializer.Deserialize<StoryItem>(response.Content);
                       return story; 
                    }

                    return null; // Return null instead of an invalid object
                });
                // use LINQ to turn the paginated id list into a list of Task<StoryItem>s
                var results = await Task.WhenAll(tasks); // takes the list of async operations and returns when they are all done, every request runs in parallel, so only waiting for the slowest request to finish
                return results.Where(r => r != null).ToList();
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Network Error");
                return new List<StoryItem>();
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Deserialization error");
                return new List<StoryItem>();
            }
        }


      
        //Fetchs the storyid list, caches it, returns the list
        private async Task<List<long>> GetNewStoryIDs()
        {
            //const string cacheKey = "new-story-ids";

            if (cache.TryGetValue(cacheKey, out List<long> cachedIds))
            {
                logger.LogInformation("Pulling ids from cache");
                return cachedIds;
            }
            try
            {
                var request = new RestRequest("v0/newstories.json", Method.Get);
                var response = await restClient.ExecuteAsync(request);
                var ids = JsonSerializer.Deserialize<List<long>>(response.Content ?? "[]") ?? new List<long>();
                if (ids.Any())
                {
                    logger.LogInformation("Caching new story IDs");
                    cache.Set(cacheKey, ids, TimeSpan.FromMinutes(5));
                }
                return ids;
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Network Error");
                return new List<long>();
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Deserialization error");
                return new List<long>();
            }

        }


    }
}
