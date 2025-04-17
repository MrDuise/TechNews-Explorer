import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AppComponent } from './app.component';
import { HomeComponent } from './pages/home/home.component';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { of } from 'rxjs';
import { StoryApiService } from './services/story-api.service';
import { Component } from '@angular/core';

class MockStoryApiService {
  getNewStories () {
    return of({ results: [] }); // Avoids null/undefined
  }
}

@Component({
  selector: 'app-home',
  standalone: true,
  template: ''
})
class MockHomeComponent {}

describe('AppComponent', () => {
  let component: AppComponent;
  let fixture: ComponentFixture<AppComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [

        AppComponent,            // Since AppComponent is standalone
        MockHomeComponent            // Also a standalone component
      ],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: StoryApiService, useClass: MockStoryApiService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AppComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create the app', () => {
    expect(component).toBeTruthy();
  });
});

