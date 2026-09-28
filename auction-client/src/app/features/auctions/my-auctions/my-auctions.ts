import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe, CurrencyPipe, AsyncPipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';
import { Auction } from '../../../core/services/auction';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-my-auctions',
  standalone: true,
  imports: [RouterLink, DatePipe, CurrencyPipe, AsyncPipe],
  template: `
    <div class="container">
      <h1>My Auctions</h1>
      <div class="auctions-grid">
        @for (auction of auctions$ | async; track auction.id) {
          <div class="auction-card" [routerLink]="['/auctions', auction.id]">
            @if (auction.imageUrl) {
              <img class="card-image" [src]="auction.imageUrl" [alt]="auction.title" />
            }
            <div class="auction-type">{{ auction.itemType }}</div>
            <h2>{{ auction.title }}</h2>
            <div class="auction-info">
              <div class="bid-info">
                <span class="label">Current Bid</span>
                <span class="amount">{{ auction.currentBid | currency }}</span>
              </div>
              <div class="bid-info">
                <span class="label">Bids</span>
                <span class="amount">{{ auction.bidCount }}</span>
              </div>
            </div>
            <div class="ends-at">
              {{ auction.status === 'Draft' ? 'Starts' : 'Ends' }}:
              {{ (auction.status === 'Draft' ? auction.startsAt : auction.endsAt) | date:'medium' }}
            </div>
            <div class="status" [class.active]="auction.status === 'Active'">{{ auction.status }}</div>
          </div>
        } @empty {
          <div class="empty">
            You haven't created any auctions yet.
            <a routerLink="/auctions/create">Create one!</a>
          </div>
        }
      </div>
    </div>
  `
})
export class MyAuctionsComponent implements OnInit {
  private http = inject(HttpClient);
  private auctionsSubject = new BehaviorSubject<Auction[]>([]);
  auctions$ = this.auctionsSubject.asObservable();

  ngOnInit() {
    this.http.get<Auction[]>(`${environment.apiUrl}/auctions/mine`).subscribe({
      next: auctions => this.auctionsSubject.next(auctions),
      error: err => console.error('Failed to load my auctions', err)
    });
  }
}
