import { Component, EventEmitter, Output } from '@angular/core';
import {MatInputModule} from '@angular/material/input';
import {FormsModule} from '@angular/forms';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';

@Component({
  selector: 'app-search-bar',
  templateUrl: './search-bar.component.html',
  styleUrls: ['./search-bar.component.scss'],
  imports: [FormsModule, MatInputModule, MatIconModule, MatFormFieldModule],
})
export class SearchBarComponent {
  searchText: string = '';

  @Output() search = new EventEmitter<string>();

  onSearch() {
    this.search.emit(this.searchText);
  }

  clearSearch() {
    this.searchText = '';
    this.search.emit(this.searchText);
  }
}
