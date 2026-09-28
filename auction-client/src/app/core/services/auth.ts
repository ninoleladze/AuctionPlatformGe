import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface AuthUser {
  token: string;
  username: string;
  email: string;
  userId: string;
  bidBalance: number;
  role: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  register(username: string, email: string, password: string) {
    return this.http.post<AuthUser>(`${this.apiUrl}/auth/register`, { username, email, password });
  }

  login(email: string, password: string) {
    return this.http.post<AuthUser>(`${this.apiUrl}/auth/login`, { email, password });
  }
}
