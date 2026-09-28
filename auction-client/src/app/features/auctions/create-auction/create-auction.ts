import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { AsyncPipe } from '@angular/common';
import { AuctionService } from '../../../core/services/auction';
import { selectAuctionsError } from '../../../store/auctions/auctions.selectors';
import * as AuctionsActions from '../../../store/auctions/auctions.actions';

@Component({
  selector: 'app-create-auction',
  standalone: true,
  imports: [FormsModule, RouterLink, AsyncPipe],
  template: `
    <div class="container">
      <div class="create-form">
        <h2>Create Auction</h2>
        @if (error$ | async; as error) {
          <div class="error">{{ error }}</div>
        }

        <label>Title</label>
        <input type="text" [(ngModel)]="form.title" placeholder="Item title" />

        <label>Description</label>
        <textarea [(ngModel)]="form.description" placeholder="Describe your item"></textarea>

        <label>Item Type</label>
        <select [(ngModel)]="form.itemType">
          <option value="Physical">Physical</option>
          <option value="Digital">Digital</option>
          <option value="GameItem">Game Item</option>
        </select>

        <label>Starting Price ($)</label>
        <input type="number" [(ngModel)]="form.startingPrice" min="1" />

        <label>Reserve Price ($) <small>optional</small></label>
        <input type="number" [(ngModel)]="form.reservePrice" min="0" />

        <label>Buy Now Price ($) <small>optional</small></label>
        <input type="number" [(ngModel)]="form.buyNowPrice" min="0" />

        <label>Start Date</label>
        <input type="datetime-local" [(ngModel)]="form.startsAt" />

        <label>End Date</label>
        <input type="datetime-local" [(ngModel)]="form.endsAt" />

        <label>Item Image <small>optional · max 10 MB</small></label>
        <input type="file" accept="image/*" (change)="onFileSelected($event)" />
        @if (imagePreview) {
          <img [src]="imagePreview" class="image-preview" alt="Preview" />
        }
        @if (uploading) {
          <div class="loading">Uploading image...</div>
        }

        <button (click)="create()" [disabled]="submitting || uploading">
          {{ submitting ? 'Creating...' : 'Create Auction' }}
        </button>
        <a routerLink="/auctions">Cancel</a>
      </div>
    </div>
  `
})
export class CreateAuction {
  private store = inject(Store);
  private auctionService = inject(AuctionService);

  form = {
    title: '',
    description: '',
    itemType: 'Physical',
    startingPrice: 1,
    reservePrice: 0,
    buyNowPrice: 0,
    startsAt: '',
    endsAt: '',
    imageUrl: undefined as string | undefined
  };
  submitting = false;
  uploading = false;
  imagePreview: string | null = null;
  error$ = this.store.select(selectAuctionsError);

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    // Local preview
    const reader = new FileReader();
    reader.onload = () => (this.imagePreview = reader.result as string);
    reader.readAsDataURL(file);

    // Upload to server
    this.uploading = true;
    this.auctionService.uploadImage(file).subscribe({
      next: res => { this.form.imageUrl = res.url; this.uploading = false; },
      error: () => { this.uploading = false; }
    });
  }

  create() {
    if (!this.form.title || !this.form.startsAt || !this.form.endsAt) return;
    this.submitting = true;
    this.store.dispatch(AuctionsActions.createAuction({
      dto: {
        ...this.form,
        reservePrice: this.form.reservePrice || undefined,
        buyNowPrice: this.form.buyNowPrice || undefined
      }
    }));
    setTimeout(() => (this.submitting = false), 2000);
  }
}
