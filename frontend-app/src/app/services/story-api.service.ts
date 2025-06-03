import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { catchError, Observable, of } from 'rxjs';
import { FindStoryResponse, StoryItem } from '../models/StoryItem';
import { API_BASE_URL } from './api.tokens';

@Injectable({
  providedIn: 'root'
})
export class StoryApiService {
  

  constructor(private http: HttpClient,  @Inject(API_BASE_URL) private baseUrl: string) {}

  getNewStories(amount: number, page: number): Observable<FindStoryResponse> {
    return this.http.get<FindStoryResponse>(`${this.baseUrl}/stories?amount=${amount}&page=${page}`)
    .pipe(
      catchError(err => {
        console.error('API search error:', err);
        return of({ stories: [], numberOfStories: 0 }); // return fallback response
      })
    );
  }

  //call the search endpoint
  //encodeURIComponent is used because I was having issues when doing a search with spaces in it
  searchStories(query: string): Observable<FindStoryResponse> {
  return this.http.get<FindStoryResponse>(`${this.baseUrl}/stories/search?queryString=${encodeURIComponent(query)}`)
    .pipe(
      catchError(err => {
        console.error('API search error:', err);
        return of({ stories: [], numberOfStories: 0 }); // return fallback response
      })
    );
}
  
}
