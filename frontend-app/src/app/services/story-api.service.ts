import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { FindStoryResponse, StoryItem } from '../models/StoryItem';

@Injectable({
  providedIn: 'root'
})
export class StoryApiService {
  private baseUrl = 'http://localhost:5037/api';

  constructor(private http: HttpClient) {}

  getNewStories(amount: number, page: number): Observable<FindStoryResponse> {
    return this.http.get<FindStoryResponse>(`${this.baseUrl}/stories?amount=${amount}&page=${page}`);
  }

  searchStories(query: string): Observable<FindStoryResponse> {
    return this.http.get<FindStoryResponse>(`${this.baseUrl}/stories/search?query=${encodeURIComponent(query)}`);
  }
  
}
