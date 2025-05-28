import { ComponentFixture, TestBed, waitForAsync, fakeAsync, tick } from '@angular/core/testing';
import { HomeComponent } from './home.component';
import { StoryApiService } from '../../services/story-api.service';
import { of, throwError } from 'rxjs';
import { FindStoryResponse, StoryItem } from '../../models/StoryItem';
import { SearchBarComponent } from '../../components/search-bar/search-bar.component';
import { StoryListComponent } from '../../components/story-list/story-list.component';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';

describe('HomeComponent', () => {
  let component: HomeComponent;
  let fixture: ComponentFixture<HomeComponent>;
  let mockStoryApiService: jasmine.SpyObj<StoryApiService>;

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

  const mockResponse: FindStoryResponse = {
    numberOfStories: mockStories.length,
    stories: mockStories
  };

  beforeEach(waitForAsync(() => {
    mockStoryApiService = jasmine.createSpyObj('StoryApiService', ['getNewStories', 'searchStories']);

    TestBed.configureTestingModule({
      imports: [MatPaginatorModule, SearchBarComponent, StoryListComponent, HomeComponent],
      providers: [
        { provide: StoryApiService, useValue: mockStoryApiService }
      ]
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(HomeComponent);
    component = fixture.componentInstance;
  });

  describe('Component Creation', () => {
    it('Should create (Happy Path)', () => {
      expect(component).toBeTruthy();
    });

    it('Should initialize with correct default values (Happy Path)', () => {
      expect(component.pageSize).toBe(10);
      expect(component.currentPage).toBe(0);
      expect(component.maxStories).toBe(0);
      expect(component.isLoading).toBe(true);
      expect(component.searchedFlag).toBe(false);
      expect(component.allSearchedStories).toEqual([]);
      expect(component.paginatedSearchResults).toEqual([]);
    });
  });

  describe('ngOnInit', () => {
    it('Should call getNewStories on init (Happy Path)', () => {
      mockStoryApiService.getNewStories.and.returnValue(of(mockResponse));
      fixture.detectChanges(); // triggers ngOnInit
      expect(mockStoryApiService.getNewStories).toHaveBeenCalledWith(10, 1);
      expect(component.response).toEqual(mockResponse);
      expect(component.isLoading).toBe(false);
    });

    it('Should handle error when getNewStories fails on init (Error Path)', () => {
      const errorResponse = { status: 500, message: 'Server Error' };
      mockStoryApiService.getNewStories.and.returnValue(throwError(() => errorResponse));
      
      spyOn(console, 'error');
      fixture.detectChanges();
      
      expect(mockStoryApiService.getNewStories).toHaveBeenCalled();
      expect(component.isLoading).toBe(true); // Should remain true on error
    });
  });

  describe('getNewStories', () => {
    it('Should fetch new stories successfully (Happy Path)', () => {
      mockStoryApiService.getNewStories.and.returnValue(of(mockResponse));
      
      component.getNewStories();
      
      expect(component.response).toEqual(mockResponse);
      expect(component.maxStories).toBe(mockResponse.numberOfStories);
      expect(component.isLoading).toBe(false);
    });

    it('Should handle error when fetching new stories (Error Path)', () => {
      const errorResponse = { status: 404, message: 'Not Found' };
      mockStoryApiService.getNewStories.and.returnValue(of({stories: [], numberOfStories: 0}));
      
      component.isLoading = true;
      component.getNewStories();
      
      expect(component.isLoading).toBe(false); // Should remain true on error
      expect(component.response.stories.length).toBe(0);
    });

    it('Should call with correct parameters based on current page and size (Happy Path)', () => {
      mockStoryApiService.getNewStories.and.returnValue(of(mockResponse));
      component.currentPage = 2;
      component.pageSize = 5;
      
      component.getNewStories();
      
      expect(mockStoryApiService.getNewStories).toHaveBeenCalledWith(5, 3); // page is 0-based, so +1
    });
  });

  describe('performSearch', () => {
    it('Should perform search successfully (Happy Path)', () => {
      const searchResponse: FindStoryResponse = {
        stories: mockStories.slice(0, 5),
        numberOfStories: 5
      };
      mockStoryApiService.searchStories.and.returnValue(of(searchResponse));
      
      component.performSearch('test query');
      
      expect(component.searchedFlag).toBeTrue();
      expect(component.allSearchedStories).toEqual(searchResponse.stories);
      expect(component.maxStories).toBe(5);
      expect(component.currentPage).toBe(0);
      expect(component.isLoading).toBe(false);
    });

    it('Should handle search error (Error Path)', () => {
        const errorResponse = new HttpErrorResponse({
        status: 400,
        statusText: 'Bad Request',
        url: 'api/search',
        error: { message: 'Bad Request' }
      });

      mockStoryApiService.searchStories.and.returnValue(of({stories: [], numberOfStories: 0}));
      
      component.isLoading = false;
      component.performSearch('invalid query');
      
      expect(component.isLoading).toBe(false); // Should be set to true initially
      expect(component.searchedFlag).toBe(true); // Should remain false on error
});

    it('Should enable loading spinner when search starts (Happy Path)', () => {
      mockStoryApiService.searchStories.and.returnValue(of(mockResponse));
      component.isLoading = false;
      
      component.performSearch('test');
      
      expect(component.isLoading).toBe(false); // Should be false after successful search
    });

    it('Should handle empty search query (Happy Path)', () => {
      const emptyResponse: FindStoryResponse = { stories: [], numberOfStories: 0 };
      mockStoryApiService.searchStories.and.returnValue(of(emptyResponse));
      
      component.performSearch('');
      
      expect(component.allSearchedStories).toEqual([]);
      expect(component.maxStories).toBe(0);
    });

    it('Should reset pagination on new search (Happy Path)', () => {
      mockStoryApiService.searchStories.and.returnValue(of(mockResponse));
      component.currentPage = 5;
      
      component.performSearch('new search');
      
      expect(component.currentPage).toBe(0);
    });
  });

  describe('paginateSearchResults', () => {
    beforeEach(() => {
      component.allSearchedStories = mockStories;
      component.maxStories = mockStories.length;
    });

    it('Should paginate search results correctly (Happy Path)', () => {
      component.currentPage = 1;
      component.pageSize = 5;
      
      component.paginateSearchResults();
      
      expect(component.paginatedSearchResults.length).toBe(5);
      expect(component.paginatedSearchResults[0].title).toBe('Story 5');
      expect(component.response.stories).toEqual(component.paginatedSearchResults);
    });

    it('Should handle last page with fewer items (Happy Path)', () => {
      component.currentPage = 2;
      component.pageSize = 7;
      component.allSearchedStories = mockStories.slice(0, 22); // Only 22 items
      
      component.paginateSearchResults();
      
      expect(component.paginatedSearchResults.length).toBe(6); // current page starts at item 14
    });

    it('Should handle empty search results (Happy Path)', () => {
      component.allSearchedStories = [];
      component.currentPage = 0;
      component.pageSize = 10;
      
      component.paginateSearchResults();
      
      expect(component.paginatedSearchResults.length).toBe(0);
      expect(component.response.stories).toEqual([]);
    });

    it('Should handle page beyond available results (Error Path)', () => {
      component.currentPage = 10; // Way beyond available pages
      component.pageSize = 5;
      
      component.paginateSearchResults();
      
      expect(component.paginatedSearchResults.length).toBe(0);
    });
  });

  describe('clearSearch', () => {
    it('Should clear search and return to new stories (Happy Path)', () => {
      mockStoryApiService.getNewStories.and.returnValue(of(mockResponse));
      
      // Set up search state
      component.searchedFlag = true;
      component.allSearchedStories = mockStories;
      component.currentPage = 3;
      
      component.clearSearch();
      
      expect(component.searchedFlag).toBe(false);
      expect(component.allSearchedStories).toEqual([]);
      expect(component.currentPage).toBe(0);
      expect(component.storyId).toBe(0);
      expect(mockStoryApiService.getNewStories).toHaveBeenCalled();
    });

    it('Should handle error when clearing search (Error Path)', () => {
      const errorResponse = { status: 500, message: 'Server Error' };
      mockStoryApiService.getNewStories.and.returnValue(of({stories: [], numberOfStories: 0}));
      
      component.searchedFlag = true;
      component.allSearchedStories = mockStories;
      
      component.clearSearch();
      
      expect(component.searchedFlag).toBe(false);
      expect(component.allSearchedStories).toEqual([]);
      expect(mockStoryApiService.getNewStories).toHaveBeenCalled();
    });
  });

  describe('onPageChange', () => {
    it('Should handle page change for new stories (Happy Path)', () => {
      mockStoryApiService.getNewStories.and.returnValue(of(mockResponse));
      component.searchedFlag = false;
      const event: PageEvent = { pageIndex: 1, pageSize: 5, length: 20 };
      
      component.onPageChange(event);
      
      expect(component.currentPage).toBe(1);
      expect(component.pageSize).toBe(5);
      expect(component.storyId).toBe(5);
      expect(mockStoryApiService.getNewStories).toHaveBeenCalledWith(5, 2);
    });

    it('Should handle page change for search results (Happy Path)', () => {
      component.searchedFlag = true;
      component.allSearchedStories = mockStories;
      const event: PageEvent = { pageIndex: 2, pageSize: 5, length: 20 };
      
      spyOn(component, 'paginateSearchResults');
      
      component.onPageChange(event);
      
      expect(component.currentPage).toBe(2);
      expect(component.pageSize).toBe(5);
      expect(component.storyId).toBe(10);
      expect(component.paginateSearchResults).toHaveBeenCalled();
    });

    it('Should handle error during page change for new stories (Error Path)', () => {
      const errorResponse = { status: 500, message: 'Server Error' };
      mockStoryApiService.getNewStories.and.returnValue(of({stories: [], numberOfStories: 0}));
      component.searchedFlag = false;
      const event: PageEvent = { pageIndex: 1, pageSize: 10, length: 100 };
      
      component.onPageChange(event);
      
      expect(component.currentPage).toBe(1);
      expect(component.pageSize).toBe(10);
      expect(mockStoryApiService.getNewStories).toHaveBeenCalled();
    });

    it('Should update storyId correctly based on page and size (Happy Path)', () => {
      component.searchedFlag = true;
      component.allSearchedStories = mockStories;
      const event: PageEvent = { pageIndex: 3, pageSize: 7, length: 50 };
      
      component.onPageChange(event);
      
      expect(component.storyId).toBe(21); // 3 * 7 = 21
    });
  });

  describe('UI Integration', () => {
    it('Should display clear search button when searching (Happy Path)', () => {
      spyOn(component, 'getNewStories');
      component.searchedFlag = true;
      fixture.detectChanges();
      
      const clearButton = fixture.debugElement.query(By.css('.clearSearch'));
      expect(clearButton).toBeTruthy();
    });

    it('Should not display clear search button when not searching (Happy Path)', () => {
      spyOn(component, 'getNewStories');
      component.searchedFlag = false;
      fixture.detectChanges();
      
      const clearButton = fixture.debugElement.query(By.css('.clearSearch'));
      expect(clearButton).toBeFalsy();
    });

    it('Should pass correct props to story-list component (Happy Path)', () => {
      mockStoryApiService.getNewStories.and.returnValue(of(mockResponse));
      fixture.detectChanges();
      
      const storyListElement = fixture.debugElement.query(By.css('app-story-list'));
      const storyListComponent = storyListElement.componentInstance;
      
      expect(storyListComponent.stories).toEqual(component.response.stories);
      expect(storyListComponent.storyNumber).toBe(component.storyId);
      expect(storyListComponent.loading).toBe(component.isLoading);
    });

    it('Should handle search bar events correctly (Happy Path)', () => {
      mockStoryApiService.searchStories.and.returnValue(of(mockResponse));
      spyOn(component, 'performSearch');
      spyOn(component, 'getNewStories');
      fixture.detectChanges();
      
      const searchBarElement = fixture.debugElement.query(By.css('app-search-bar'));
      searchBarElement.triggerEventHandler('search', 'test query');
      
      expect(component.performSearch).toHaveBeenCalledWith('test query');
    });
  });
});