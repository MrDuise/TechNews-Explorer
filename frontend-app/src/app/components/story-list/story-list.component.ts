import { Component, Input, SimpleChanges } from '@angular/core';
import {MatCardModule} from '@angular/material/card';
import {MatPaginatorModule, PageEvent} from '@angular/material/paginator';
import {ProgressSpinnerMode, MatProgressSpinnerModule} from '@angular/material/progress-spinner';
import { StoryItem } from '../../models/StoryItem';
import { StoryApiService } from '../../services/story-api.service';

@Component({
  selector: 'app-story-list',
  imports: [MatCardModule, MatPaginatorModule, MatProgressSpinnerModule],
  templateUrl: './story-list.component.html',
  styleUrl: './story-list.component.scss'
})
export class StoryListComponent {
  @Input() stories: StoryItem[] = [];
  @Input() storyNumber: number = 0;

  
  loaded = false;
  constructor(){}

  ngOnChanges(changes: SimpleChanges): void {
    // Check if the 'movies' input has changed and is not empty
    if (changes['stories'] && this.stories.length > 0) {
      this.loaded = true;
    }
  }
}
