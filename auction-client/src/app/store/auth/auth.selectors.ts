import { createSelector, createFeatureSelector } from '@ngrx/store';
import { AuthState } from './auth.reducer';

export const selectAuthState = createFeatureSelector<AuthState>('auth');
export const selectCurrentUser = createSelector(selectAuthState, s => s.user);
export const selectIsLoggedIn = createSelector(selectCurrentUser, user => !!user);
export const selectAuthLoading = createSelector(selectAuthState, s => s.loading);
export const selectAuthError = createSelector(selectAuthState, s => s.error);
export const selectToken = createSelector(selectCurrentUser, user => user?.token ?? null);
export const selectBidBalance = createSelector(selectCurrentUser, user => user?.bidBalance ?? 0);
export const selectUsername = createSelector(selectCurrentUser, user => user?.username ?? null);
export const selectUserId = createSelector(selectCurrentUser, user => user?.userId ?? null);
export const selectIsAdmin = createSelector(selectCurrentUser, user => user?.role === 'Admin');
