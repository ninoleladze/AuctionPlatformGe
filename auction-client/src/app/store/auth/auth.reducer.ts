import { createReducer, on } from '@ngrx/store';
import { AuthUser } from '../../core/services/auth';
import * as AuthActions from './auth.actions';

export interface AuthState {
  user: AuthUser | null;
  loading: boolean;
  error: string | null;
}

const initialState: AuthState = { user: null, loading: false, error: null };

export const authReducer = createReducer(
  initialState,
  on(AuthActions.login, AuthActions.register, state => ({ ...state, loading: true, error: null })),
  on(AuthActions.loginSuccess, AuthActions.registerSuccess, (state, { user }) => ({ ...state, user, loading: false, error: null })),
  on(AuthActions.loginFailure, AuthActions.registerFailure, (state, { error }) => ({ ...state, error, loading: false })),
  on(AuthActions.logout, () => initialState),
  on(AuthActions.restoreSession, (state, { user }) => ({ ...state, user })),
  on(AuthActions.updateBidBalance, (state, { bidBalance }) => ({
    ...state,
    user: state.user ? { ...state.user, bidBalance } : null
  }))
);
