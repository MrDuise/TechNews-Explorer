import { StoryApiService } from './story-api.service';
import { provideHttpClient, HttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { FindStoryResponse, StoryItem } from '../models/StoryItem';

describe('StoryApiService', () => {
  let service: StoryApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        StoryApiService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(StoryApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

 const mockStories: StoryItem[] = Array.from({ length: 20 }, (_, i) => ({
    id: i,
    title: `Story ${i}`,
    url: `http://example.com/${i}`,
    by: `Author ${i}`,
    time: Date.now(),
    descendants: 0,
    kids: [],
    score: 1, 
    type: "story"
  }));

  it('should fetch new stories', () => {
    const dummyResponse: FindStoryResponse = {
      stories: mockStories,
      numberOfStories: 20
    };

    service.getNewStories(5, 1).subscribe((response) => {
      expect(response).toEqual(dummyResponse);
    });

    const req = httpMock.expectOne('http://localhost:5037/api/stories?amount=5&page=1');
    expect(req.request.method).toBe('GET');
    req.flush(dummyResponse);
  });

  it('should search for stories', () => {
    const dummyResponse: FindStoryResponse = {
      stories: [{
        id: 2,
        title: "Story 1",
        url: "http://example.com/2",
        by: "Author 2",
        time: Date.now(),
        descendants: 0,
        kids: [],
        score: 1,
        type: "story"
      }],
      numberOfStories: 1
    };

    const query = 'test';
    service.searchStories(query).subscribe((response) => {
      expect(response).toEqual(dummyResponse);
    });

    const req = httpMock.expectOne(`http://localhost:5037/api/stories/search?query=${encodeURIComponent(query)}`);
    expect(req.request.method).toBe('GET');
    req.flush(dummyResponse);
  });
});

