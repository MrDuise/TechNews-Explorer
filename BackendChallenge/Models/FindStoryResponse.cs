namespace Backend_Challenge.Models
{
    public class FindStoryResponse
    {
        public List<StoryItem> stories { get; set; }
        public int numberOfStories { get; set; }
    }
}