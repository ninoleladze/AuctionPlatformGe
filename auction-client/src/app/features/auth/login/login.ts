import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { AsyncPipe } from '@angular/common';
import { selectAuthLoading, selectAuthError } from '../../../store/auth/auth.selectors';
import * as AuthActions from '../../../store/auth/auth.actions';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, RouterLink, AsyncPipe],
  template: `
    <div class="auth-container">
      <div class="auth-card">
        <h2>Login</h2>
        @if (error$ | async; as error) {
          <div class="error">{{ error }}</div>
        }
        <input type="email" [(ngModel)]="email" placeholder="Email" />
        <input type="password" [(ngModel)]="password" placeholder="Password" />
        <button (click)="login()" [disabled]="loading$ | async">
          {{ (loading$ | async) ? 'Logging in...' : 'Login' }}
        </button>
        <p>Don't have an account? <a routerLink="/register">Register</a></p>
      </div>
    </div>
  `
})
export class Login {
  private store = inject(Store);
  email = '';
  password = '';
  loading$ = this.store.select(selectAuthLoading);
  error$ = this.store.select(selectAuthError);

  login() {
    this.store.dispatch(AuthActions.login({ email: this.email, password: this.password }));
  }
}
