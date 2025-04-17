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
        //doesn't ever change, used up top for benifit of unit testing
        const string cacheKey = "new-story-ids";

        public StoryService(ILogger<StoryService> logger, IMemoryCache memoryCache, IRestClient restClient)
        {
            this.logger = logger;
            this.cache = memoryCache;
            this.restClient = restClient;
        }

        
        public async Task<FindStoryResponse> SearchStories(string query)
        {
                var allIds = await GetNewStoryIDs();//uses the already built in way of finding the newest stories
                var allStories = await GetStoryItems(500, 1);//gets all the story items that are aviable, this takes the longest
                var storyResponse = new FindStoryResponse();
                storyResponse.stories = allStories.stories
                    .Where(s => !string.IsNullOrWhiteSpace(s.Title) && s.Title.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();//filters the stories to only get the ones that contain info that matches the query in the title
            storyResponse.numberOfStories = storyResponse.stories.Count();
            return storyResponse;
        }

        public async Task<FindStoryResponse> GetStoryItems(int amountPerPage, int page)
        {
            try
            {
                var allIds = await GetNewStoryIDs();
                var pagedIds = allIds.Skip((page - 1) * amountPerPage).Take(amountPerPage).ToList(); // creates a paginated list of ids. skips page - 1 times number of items, takes the number of itmes, and turns it into a list.  
                                                                                                     //first page is (1-1) * 10, so skips 0 items, then takes 10 items from allIds
                var tasks = pagedIds.Select(async id =>
                {
                    var request = new RestRequest($"v0/item/{id}.json", Method.Get);
                    //unfortunally using the non-generic version of ExecuteAsync so that I can unit test it, wanted to use ExecuteAsync<StoryItem> instead
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
                var storyResponse = new FindStoryResponse();
                storyResponse.stories = [.. results.Where(r => r != null)];
                storyResponse.numberOfStories = allIds.Count();
                return storyResponse;
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Network Error");
                return new FindStoryResponse();
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Deserialization error");
                return new FindStoryResponse();
            }
        }


      
        //Fetchs the storyid list, caches it, returns the list
        //could be used in the future to call the other endpoints to get top stories, comments, all that
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
                //prevents null from ever being returned by the deserializer, so if null is somehow returned, I just get an empty array in response
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
