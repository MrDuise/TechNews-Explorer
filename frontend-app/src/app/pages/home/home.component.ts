import { Component } from '@angular/core';
import { StoryListComponent } from '../../components/story-list/story-list.component';
import {MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { StoryApiService } from '../../services/story-api.service';
import { FindStoryResponse, StoryItem } from '../../models/StoryItem';
import { SearchBarComponent } from '../../components/search-bar/search-bar.component';

@Component({
  selector: 'app-home',
  imports: [StoryListComponent,MatPaginatorModule, SearchBarComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {

  //paginator properties
  pageSize = 10; // Default page size
  currentPage: number = 0; // Start on page 1. paginator is 0 based index, but all other instances of using this increase the page number by 1 for visual
  maxStories: number = 0; //the total number of stories

  //story display properties
  response: FindStoryResponse = {
    stories: [],
    numberOfStories: 0
  };
  storyId: number = this.currentPage * this.pageSize; //this allows to have a story number next to each story, and for it to change as I paginate 
  isLoading : boolean = true;

   //search-related properties
   searchedFlag = false;
   allSearchedStories: StoryItem[] = [];
   paginatedSearchResults: StoryItem[] = [];
   
  constructor(private storyApiService: StoryApiService) {}

  ngOnInit() {
    console.log("Loading state before API:", this.isLoading);
    this.getNewStories();
  }

  //handles the paginator page change events
  onPageChange(event: PageEvent): void {
    this.pageSize = event.pageSize;
    this.currentPage = event.pageIndex;
    this.storyId = this.currentPage * this.pageSize;

    //check to not call the newstories api and send in the wrong data when doing a search
    if (this.searchedFlag) {
      this.paginateSearchResults();
    } else {
      this.getNewStories();
    }
  }

  //this gets called by the searchbar component using an event with the query string passed in
  performSearch(query: string): void {
    this.isLoading = true;//reenables the spinner when a search is started
    this.storyApiService.searchStories(query).subscribe((data) => {
      this.searchedFlag = true;
      this.allSearchedStories = data.stories;
      this.maxStories = data.numberOfStories;
      this.currentPage = 0;
      this.paginateSearchResults();
      this.isLoading = false;
    });
  }

  //create a custom pagination for the search results specfically
  paginateSearchResults(): void {
    const start = this.currentPage * this.pageSize;
    const end = start + this.pageSize;
    this.paginatedSearchResults = this.allSearchedStories.slice(start, end);

    //updates the response value
    this.response = {
      numberOfStories: this.maxStories,
      stories: this.paginatedSearchResults
    };
  }

  //wipes out the search results, going back to the newest stories default
  clearSearch(): void {
    this.searchedFlag = false;
    this.allSearchedStories = [];
    this.currentPage = 0;
    this.storyId = this.currentPage * this.pageSize;
    this.getNewStories();
  }

  
//calls the story service to get the newest stories
  getNewStories(){
    this.storyApiService.getNewStories(this.pageSize,this.currentPage+1).subscribe((data) => {
      this.response = data;
      this.maxStories = this.response.numberOfStories;
      this.isLoading = false;
    });
  }
}
