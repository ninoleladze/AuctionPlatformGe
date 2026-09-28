import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap, tap } from 'rxjs';
import { AuthService } from '../../core/services/auth';
import { AuctionHubService } from '../../core/hubs/auction-hub';
import * as AuthActions from './auth.actions';

@Injectable()
export class AuthEffects {
  private actions$    = inject(Actions);
  private authService = inject(AuthService);
  private hubService  = inject(AuctionHubService);
  private router      = inject(Router);

  login$ = createEffect(() => this.actions$.pipe(
    ofType(AuthActions.login),
    switchMap(({ email, password }) =>
      this.authService.login(email, password).pipe(
        map(user => AuthActions.loginSuccess({ user })),
        catchError(err => of(AuthActions.loginFailure({ error: extractError(err) })))
      )
    )
  ));

  register$ = createEffect(() => this.actions$.pipe(
    ofType(AuthActions.register),
    switchMap(({ username, email, password }) =>
      this.authService.register(username, email, password).pipe(
        map(user => AuthActions.registerSuccess({ user })),
        catchError(err => of(AuthActions.registerFailure({ error: extractError(err) })))
      )
    )
  ));

  persistAndNavigate$ = createEffect(() => this.actions$.pipe(
    ofType(AuthActions.loginSuccess, AuthActions.registerSuccess),
    tap(({ user }) => {
      localStorage.setItem('user', JSON.stringify(user));
      this.router.navigate(['/auctions']);
    })
  ), { dispatch: false });

  // Connect SignalR hub as soon as a token is available
  connectHub$ = createEffect(() => this.actions$.pipe(
    ofType(AuthActions.loginSuccess, AuthActions.registerSuccess, AuthActions.restoreSession),
    tap(({ user }) => {
      if (user.token) {
        this.hubService.connect(user.token)
          .catch(err => console.warn('SignalR connect failed:', err));
      }
    })
  ), { dispatch: false });

  logout$ = createEffect(() => this.actions$.pipe(
    ofType(AuthActions.logout),
    tap(() => {
      localStorage.removeItem('user');
      this.hubService.disconnect();
      this.router.navigate(['/login']);
    })
  ), { dispatch: false });
}

function extractError(err: { error?: { error?: string; errors?: unknown } }): string {
  if (err?.error?.error) return err.error.error;
  if (err?.error?.errors) return JSON.stringify(err.error.errors);
  return 'An error occurred.';
}
