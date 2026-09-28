import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { AsyncPipe } from '@angular/common';
import { selectAuthLoading, selectAuthError } from '../../../store/auth/auth.selectors';
import * as AuthActions from '../../../store/auth/auth.actions';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, RouterLink, AsyncPipe],
  template: `
    <div class="auth-container">
      <div class="auth-card">
        <h2>Register</h2>
        @if (error$ | async; as error) {
          <div class="error">{{ error }}</div>
        }
        <input type="text" [(ngModel)]="username" placeholder="Username" />
        <input type="email" [(ngModel)]="email" placeholder="Email" />
        <input type="password" [(ngModel)]="password" placeholder="Password" />
        <button (click)="register()" [disabled]="loading$ | async">
          {{ (loading$ | async) ? 'Creating account...' : 'Register' }}
        </button>
        <p>Already have an account? <a routerLink="/login">Login</a></p>
      </div>
    </div>
  `
})
export class Register {
  private store = inject(Store);
  username = '';
  email = '';
  password = '';
  loading$ = this.store.select(selectAuthLoading);
  error$ = this.store.select(selectAuthError);

  register() {
    this.store.dispatch(AuthActions.register({ username: this.username, email: this.email, password: this.password }));
  }
}
