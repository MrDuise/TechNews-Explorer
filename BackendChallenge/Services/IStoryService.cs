using Backend_Challenge.Models;

namespace Backend_Challenge.Services
{
    public interface IStoryService
    {
        //Returns a list of paginated story objects
        Task<FindStoryResponse> GetStoryItems(int amount, int page);
        Task<FindStoryResponse> SearchStoriesAsync(string query);

    }
}
