import { Component, Input, SimpleChanges } from '@angular/core';
import {MatCardModule} from '@angular/material/card';
import { MatProgressSpinnerModule} from '@angular/material/progress-spinner';
import { StoryItem } from '../../models/StoryItem';


@Component({
  selector: 'app-story-list',
  standalone: true,
  imports: [MatCardModule, MatProgressSpinnerModule],
  templateUrl: './story-list.component.html',
  styleUrl: './story-list.component.scss'
})
export class StoryListComponent {
  @Input() stories: StoryItem[] = [];
  @Input() storyNumber: number = 0;
  @Input() loading: boolean = false;

  constructor(){}
  ngOnInit() {
    console.log("StoryListComponent mounted");
  }
  

  ngOnChanges(changes: SimpleChanges): void {
    console.log("Loading input changed:", changes['loading']);
    // if (changes['stories'] && this.stories.length > 0) {
    //   console.log(this.loaded);
      
    //   this.loaded = true;
    // }
  }
}
