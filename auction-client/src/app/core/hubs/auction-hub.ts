import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface BidUpdate {
  auctionId: string;
  newBid: number;
  bidderId: string;
  bidCount: number;
  endsAt: string;
  extended: boolean;
}

export interface AuctionEnded {
  auctionId: string;
  finalBid: number;
  highestBidderId: string | null;
  reserveNotMet: boolean;
}

@Injectable({ providedIn: 'root' })
export class AuctionHubService {
  private hub!: signalR.HubConnection;

  private bidUpdate       = new BehaviorSubject<BidUpdate | null>(null);
  private bidError        = new BehaviorSubject<string | null>(null);
  private auctionEnded    = new BehaviorSubject<AuctionEnded | null>(null);
  private auctionActivated = new BehaviorSubject<string | null>(null);

  bidUpdate$        = this.bidUpdate.asObservable();
  bidError$         = this.bidError.asObservable();
  auctionEnded$     = this.auctionEnded.asObservable();
  auctionActivated$ = this.auctionActivated.asObservable();

  connect(token: string) {
    if (this.hub?.state === signalR.HubConnectionState.Connected ||
        this.hub?.state === signalR.HubConnectionState.Connecting) {
      return Promise.resolve();
    }
    this.hub = new signalR.HubConnectionBuilder()
      .withUrl(environment.hubUrl, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    this.hub.on('BidPlaced', (data: BidUpdate) => {
      this.bidUpdate.next(data);
    });

    this.hub.on('BidError', (message: string) => {
      this.bidError.next(message);
    });

    this.hub.on('AuctionEnded', (data: AuctionEnded) => {
      this.auctionEnded.next(data);
    });

    this.hub.on('AuctionActivated', (data: { auctionId: string }) => {
      this.auctionActivated.next(data.auctionId);
    });

    return this.hub.start();
  }

  joinAuction(auctionId: string) {
    return this.hub.invoke('JoinAuction', auctionId);
  }

  leaveAuction(auctionId: string) {
    return this.hub.invoke('LeaveAuction', auctionId);
  }

  placeBid(auctionId: string, amount: number) {
    return this.hub.invoke('PlaceBid', auctionId, amount);
  }

  disconnect() {
    return this.hub?.stop();
  }
}
