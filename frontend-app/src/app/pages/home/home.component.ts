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

  pageSize = 10; // Default page size
  currentPage: number = 0; // Start on page 1 (0-based index)
  maxStories: number = 0;
  response!: FindStoryResponse;
  storyId: number = this.currentPage * this.pageSize;
   // Search-related properties
   searchedFlag = false;
   allSearchedStories: StoryItem[] = [];
   paginatedSearchResults: StoryItem[] = [];
  constructor(private storyApiService: StoryApiService) {}

  ngOnInit() {
    this.getNewStories();
  }

  onPageChange(event: PageEvent): void {
    this.pageSize = event.pageSize;
    this.currentPage = event.pageIndex;
    this.storyId = this.currentPage * this.pageSize;

    if (this.searchedFlag) {
      this.paginateSearchResults();
    } else {
      this.getNewStories();
    }
  }

  performSearch(query: string): void {
    this.storyApiService.searchStories(query).subscribe((data) => {
      this.searchedFlag = true;
      this.allSearchedStories = data.stories;
      this.maxStories = data.numberOfStories;
      this.currentPage = 0;
      this.paginateSearchResults();
    });
  }

  paginateSearchResults(): void {
    const start = this.currentPage * this.pageSize;
    const end = start + this.pageSize;
    this.paginatedSearchResults = this.allSearchedStories.slice(start, end);

    // Map to the response shape if needed
    this.response = {
      numberOfStories: this.maxStories,
      stories: this.paginatedSearchResults
    };
  }

  clearSearch(): void {
    this.searchedFlag = false;
    this.allSearchedStories = [];
    this.currentPage = 0;
    this.storyId = this.currentPage * this.pageSize;
    this.getNewStories();
  }

  

  getNewStories(){
    this.storyApiService.getNewStories(this.pageSize,this.currentPage+1).subscribe((data) => {
      this.response = data;
      this.maxStories = this.response.numberOfStories;
    });
  }
}
