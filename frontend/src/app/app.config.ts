import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { APP_BASE_HREF } from '@angular/common';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { BASE_PATH } from './core/api/variables';
import { environment } from '../environments/environment';

import { RouteReuseStrategy } from '@angular/router';
import { CustomRouteReuseStrategy } from './core/strategies/custom-route-reuse.strategy';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZonelessChangeDetection(),
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])),
    { provide: BASE_PATH, useValue: environment.apiUrl },
    { provide: APP_BASE_HREF, useValue: '/' },
    { provide: RouteReuseStrategy, useClass: CustomRouteReuseStrategy }
  ],
};
