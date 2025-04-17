import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { FindStoryResponse, StoryItem } from '../models/StoryItem';
import { API_BASE_URL } from './api.tokens';

@Injectable({
  providedIn: 'root'
})
export class StoryApiService {
  

  constructor(private http: HttpClient,  @Inject(API_BASE_URL) private baseUrl: string) {}

  getNewStories(amount: number, page: number): Observable<FindStoryResponse> {
    return this.http.get<FindStoryResponse>(`${this.baseUrl}/stories?amount=${amount}&page=${page}`);
  }

  searchStories(query: string): Observable<FindStoryResponse> {
    return this.http.get<FindStoryResponse>(`${this.baseUrl}/stories/search?query=${encodeURIComponent(query)}`);
  }
  
}
