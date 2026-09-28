import { createAction, props } from '@ngrx/store';
import { AuthUser } from '../../core/services/auth';

export const login = createAction('[Auth] Login', props<{ email: string; password: string }>());
export const loginSuccess = createAction('[Auth] Login Success', props<{ user: AuthUser }>());
export const loginFailure = createAction('[Auth] Login Failure', props<{ error: string }>());

export const register = createAction('[Auth] Register', props<{ username: string; email: string; password: string }>());
export const registerSuccess = createAction('[Auth] Register Success', props<{ user: AuthUser }>());
export const registerFailure = createAction('[Auth] Register Failure', props<{ error: string }>());

export const logout = createAction('[Auth] Logout');
export const restoreSession = createAction('[Auth] Restore Session', props<{ user: AuthUser }>());
export const updateBidBalance = createAction('[Auth] Update Bid Balance', props<{ bidBalance: number }>());
