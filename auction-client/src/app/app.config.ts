import { APP_INITIALIZER, ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideStore, Store } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { authReducer } from './store/auth/auth.reducer';
import { auctionsReducer } from './store/auctions/auctions.reducer';
import { AuthEffects } from './store/auth/auth.effects';
import { AuctionsEffects } from './store/auctions/auctions.effects';
import { AuthUser } from './core/services/auth';
import * as AuthActions from './store/auth/auth.actions';

function restoreSession(store: Store) {
  return () => {
    const stored = localStorage.getItem('user');
    if (stored) {
      try {
        const user: AuthUser = JSON.parse(stored);
        store.dispatch(AuthActions.restoreSession({ user }));
      } catch {
        // ignore malformed stored data
      }
    }
  };
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideStore({ auth: authReducer, auctions: auctionsReducer }),
    provideEffects([AuthEffects, AuctionsEffects]),
    provideStoreDevtools({ maxAge: 25, logOnly: false }),
    {
      provide: APP_INITIALIZER,
      useFactory: restoreSession,
      deps: [Store],
      multi: true
    }
  ]
};
