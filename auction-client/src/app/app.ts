import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { AsyncPipe } from '@angular/common';
import { selectCurrentUser, selectIsAdmin, selectIsLoggedIn } from './store/auth/auth.selectors';
import * as AuthActions from './store/auth/auth.actions';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, AsyncPipe],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private store = inject(Store);
  isLoggedIn$ = this.store.select(selectIsLoggedIn);
  isAdmin$    = this.store.select(selectIsAdmin);
  user$       = this.store.select(selectCurrentUser);

  logout() {
    this.store.dispatch(AuthActions.logout());
  }
}
