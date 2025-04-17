//interface to match the datamodel coming from C#, properties match the data structure of an item from the HackerNews API
export interface StoryItem {
    by: string;
    descendants: number;
    id: number;
    kids: number[];
    score: number;
    time: number;
    title: string;
    type: string;
    url: string;
  }

  //custom interface, holds a list of storyitems and the total number of them, helps with pagination
  export interface FindStoryResponse {
    stories: StoryItem[];
    numberOfStories: number;
  }
  