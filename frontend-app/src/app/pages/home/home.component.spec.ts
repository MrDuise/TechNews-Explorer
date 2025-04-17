import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { HomeComponent } from './home.component';
import { StoryApiService } from '../../services/story-api.service';
import { of } from 'rxjs';
import { FindStoryResponse, StoryItem } from '../../models/StoryItem';
import { SearchBarComponent } from '../../components/search-bar/search-bar.component';
import { StoryListComponent } from '../../components/story-list/story-list.component';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { By } from '@angular/platform-browser';

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
      imports: [MatPaginatorModule, SearchBarComponent, StoryListComponent,HomeComponent],
      providers: [
        { provide: StoryApiService, useValue: mockStoryApiService }
      ]
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(HomeComponent);
    component = fixture.componentInstance;
  });

  it('Should create', () => {
    expect(component).toBeTruthy();
  });

  it('Should call getNewStories on init', () => {
    mockStoryApiService.getNewStories.and.returnValue(of(mockResponse));
    fixture.detectChanges(); // triggers ngOnInit
    expect(mockStoryApiService.getNewStories).toHaveBeenCalledWith(10, 1);
    expect(component.response).toEqual(mockResponse);
  });

  it('Should paginate correctly when searchedFlag is true', () => {
    component.searchedFlag = true;
    component.pageSize = 5;
    component.currentPage = 1;
    component.allSearchedStories = mockStories;
    component.paginateSearchResults();

    expect(component.paginatedSearchResults.length).toBe(5);
    expect(component.response.stories[0].title).toBe('Story 5');
  });

  it('Should call performSearch and set search results', () => {
    mockStoryApiService.searchStories.and.returnValue(of(mockResponse));
    component.performSearch('test');

    expect(component.searchedFlag).toBeTrue();
    expect(component.allSearchedStories.length).toBe(mockStories.length);
    expect(component.response.stories.length).toBe(component.pageSize);
  });

  it('Should reset state when clearSearch is called', () => {
    mockStoryApiService.getNewStories.and.returnValue(of(mockResponse));
    component.searchedFlag = true;
    component.allSearchedStories = mockStories;

    component.clearSearch();

    expect(component.searchedFlag).toBeFalse();
    expect(component.allSearchedStories).toEqual([]);
    expect(mockStoryApiService.getNewStories).toHaveBeenCalled();
  });

  it('Should call getNewStories on page change when not searching', () => {
    mockStoryApiService.getNewStories.and.returnValue(of(mockResponse));
    component.searchedFlag = false;
    const event: PageEvent = { pageIndex: 1, pageSize: 5, length: 20 };

    component.onPageChange(event);

    expect(component.currentPage).toBe(1);
    expect(component.pageSize).toBe(5);
    expect(mockStoryApiService.getNewStories).toHaveBeenCalledWith(5, 2);
  });

  it('Should paginate search results on page change when searching', () => {
    component.searchedFlag = true;
    component.allSearchedStories = mockStories;
    const event: PageEvent = { pageIndex: 2, pageSize: 5, length: 20 };

    component.onPageChange(event);

    expect(component.currentPage).toBe(2);
    expect(component.paginatedSearchResults[0].title).toBe('Story 10');
  });
});
