import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { API_BASE_URL } from './services/api.tokens';

export const appConfig: ApplicationConfig = {
  providers: [provideZoneChangeDetection({ eventCoalescing: true }), provideHttpClient(), { provide: API_BASE_URL, useValue: 'https://nextechbackendchallenge-gebgfjh6arh6a5g3.westus-01.azurewebsites.net/api' }]
};
