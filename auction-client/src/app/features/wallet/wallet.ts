import { Component, OnInit, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AsyncPipe } from '@angular/common';
import { Store } from '@ngrx/store';
import { BehaviorSubject } from 'rxjs';
import { selectCurrentUser } from '../../store/auth/auth.selectors';
import { environment } from '../../../environments/environment';

interface BidPackage { id: number; bidCount: number; priceGel: number; name: string; }

@Component({
  selector: 'app-wallet',
  standalone: true,
  imports: [AsyncPipe],
  template: `
    <div class="container">
      <h1>My Wallet</h1>

      @if (user$ | async; as user) {
        <div class="wallet-stats">
          <div class="stat-card">
            <span class="stat-label">Bid Credits</span>
            <span class="stat-value">{{ user.bidBalance }}</span>
          </div>
        </div>
      }

      @if (successMessage) {
        <div class="bid-success">{{ successMessage }}</div>
      }

      <h2>Buy Bid Credits</h2>
      <div class="packages-grid">
        @for (pkg of packages$ | async; track pkg.id) {
          <div class="package-card">
            <h3>{{ pkg.name }}</h3>
            <div class="package-bids">{{ pkg.bidCount }} bids</div>
            <div class="package-price">{{ pkg.priceGel }} &#8382;</div>
            <button (click)="buyPackage(pkg.id)" [disabled]="loading">
              {{ loading ? 'Processing...' : 'Buy Now' }}
            </button>
          </div>
        }
      </div>

      <div class="otp-section">
        <h2>Phone Verification</h2>
        @if (!otpSent) {
          <div class="otp-form">
            <input type="tel" [value]="phone" placeholder="+995 5XX XXX XXX"
                   (input)="phone = $any($event.target).value" />
            <button (click)="sendOtp()" [disabled]="otpLoading">Send OTP</button>
          </div>
        } @else {
          <div class="otp-form">
            <input type="text" [value]="otpCode" placeholder="6-digit code"
                   (input)="otpCode = $any($event.target).value" maxlength="6" />
            <button (click)="verifyOtp()" [disabled]="otpLoading">Verify</button>
            <button (click)="otpSent = false">Resend</button>
          </div>
          @if (otpMessage) {
            <div class="bid-success">{{ otpMessage }}</div>
          }
        }
      </div>
    </div>
  `,
})
export class WalletComponent implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly store = inject(Store);

  private readonly api = environment.apiUrl;
  private readonly packagesSubject = new BehaviorSubject<BidPackage[]>([]);

  user$ = this.store.select(selectCurrentUser);
  packages$ = this.packagesSubject.asObservable();
  loading = false;
  successMessage = '';
  phone = '';
  otpCode = '';
  otpSent = false;
  otpLoading = false;
  otpMessage = '';

  ngOnInit(): void {
    this.http.get<BidPackage[]>(`${this.api}/payments/packages`)
      .subscribe(pkgs => this.packagesSubject.next(pkgs));

    const params = new URLSearchParams(window.location.search);
    if (params.get('status') === 'success') {
      this.successMessage = 'Payment successful! Your bid credits have been added.';
    }
  }

  buyPackage(packageId: number): void {
    this.loading = true;
    this.http.post<{ transactionId: string; paymentUrl: string }>(
      `${this.api}/payments/initiate`, { packageId })
      .subscribe({
        next: res => { window.location.href = res.paymentUrl; },
        error: () => { this.loading = false; }
      });
  }

  sendOtp(): void {
    this.otpLoading = true;
    this.http.post(`${this.api}/auth/phone/send-otp`, { phoneNumber: this.phone })
      .subscribe({
        next: () => { this.otpSent = true; this.otpLoading = false; },
        error: () => { this.otpLoading = false; }
      });
  }

  verifyOtp(): void {
    this.otpLoading = true;
    this.http.post(`${this.api}/auth/phone/verify-otp`, { code: this.otpCode })
      .subscribe({
        next: () => { this.otpMessage = 'Phone verified!'; this.otpLoading = false; },
        error: () => { this.otpMessage = 'Invalid OTP.'; this.otpLoading = false; }
      });
  }
}
