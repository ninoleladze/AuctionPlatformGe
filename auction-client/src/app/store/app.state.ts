import { AuthState } from './auth/auth.reducer';
import { AuctionsState } from './auctions/auctions.reducer';

export interface AppState {
  auth: AuthState;
  auctions: AuctionsState;
}
