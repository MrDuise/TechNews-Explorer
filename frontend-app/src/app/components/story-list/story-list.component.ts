import { Component, Input, SimpleChanges } from '@angular/core';
import {MatCardModule} from '@angular/material/card';
import {ProgressSpinnerMode, MatProgressSpinnerModule} from '@angular/material/progress-spinner';
import { StoryItem } from '../../models/StoryItem';


@Component({
  selector: 'app-story-list',
  imports: [MatCardModule, MatProgressSpinnerModule],
  templateUrl: './story-list.component.html',
  styleUrl: './story-list.component.scss'
})
export class StoryListComponent {
  @Input() stories: StoryItem[] = [];
  @Input() storyNumber: number = 0;

  
  loaded = false;
  constructor(){}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['stories'] && this.stories?.length > 0) {
      this.loaded = true;
    }
  }
}
