import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface BidItem {
  id: string;
  bidderId: string;
  amount: number;
  placedAt: string;
}

export interface Auction {
  id: string;
  title: string;
  description: string;
  itemType: string;
  startingPrice: number;
  currentBid: number;
  buyNowPrice: number;
  reservePrice: number;
  status: string;
  startsAt: string;
  endsAt: string;
  sellerId: string;
  highestBidderId: string | null;
  bidCount: number;
  bids: BidItem[];
  imageUrl: string | null;
}

export interface CreateAuction {
  title: string;
  description: string;
  itemType: string;
  startingPrice: number;
  reservePrice?: number;
  buyNowPrice?: number;
  startsAt: string;
  endsAt: string;
  imageUrl?: string;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

@Injectable({ providedIn: 'root' })
export class AuctionService {
  private apiUrl = `${environment.apiUrl}/auctions`;
  private imagesUrl = `${environment.apiUrl}/images`;

  constructor(private http: HttpClient) { }

  getAll(page = 1, pageSize = 12) {
    return this.http.get<PaginatedResult<Auction>>(`${this.apiUrl}?page=${page}&pageSize=${pageSize}`);
  }

  getById(id: string) {
    return this.http.get<Auction>(`${this.apiUrl}/${id}`);
  }

  create(auction: CreateAuction) {
    return this.http.post<Auction>(this.apiUrl, auction);
  }

  uploadImage(file: File) {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<{ url: string }>(`${this.imagesUrl}/upload`, form);
  }
}
