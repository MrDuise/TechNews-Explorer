import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { StoryItem } from '../models/StoryItem';

@Injectable({
  providedIn: 'root'
})
export class StoryApiService {
  private baseUrl = 'http://localhost:5037/api';

  constructor(private http: HttpClient) {}

  getNewStories(amount: number, page: number): Observable<StoryItem[]> {
    return this.http.get<StoryItem[]>(`${this.baseUrl}/stories?amount=${amount}&page=${page}`);
  }
  
}
