import { Component } from '@angular/core';
import { StoryListComponent } from '../../components/story-list/story-list.component';
import {MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { StoryApiService } from '../../services/story-api.service';
import { StoryItem } from '../../models/StoryItem';

@Component({
  selector: 'app-home',
  imports: [StoryListComponent,MatPaginatorModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {

  pageSize = 10; // Default page size
  currentPage: number = 0; // Start on page 1 (0-based index)
  stories: StoryItem[] = []
  storyId: number = this.currentPage * this.pageSize;
  constructor(private storyApiService: StoryApiService) {}


  onPageChange(event: PageEvent): void {
    console.log(event);
    // Capture the page change event and update page size and current page
    console.log('Page changed:', event);
    this.pageSize = event.pageSize;
    this.currentPage = event.pageIndex;
    // Fetch articles for the selected page (this is just an example, replace with actual API call)
    this.getNewStories();
    this.storyId = this.currentPage * this.pageSize;
    //this.performSearch();
  }

  ngOnInit() {
    this.getNewStories();
  }

  getNewStories(){
    this.storyApiService.getNewStories(this.pageSize,this.currentPage+1).subscribe((data) => {
      this.stories = data;
    });
  }
}
