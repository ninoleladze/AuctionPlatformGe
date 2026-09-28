import { createAction, props } from '@ngrx/store';
import { Auction, CreateAuction } from '../../core/services/auction';
import { BidUpdate, AuctionEnded } from '../../core/hubs/auction-hub';

export const loadAuctions = createAction('[Auctions] Load', props<{ page?: number; pageSize?: number }>());
export const loadAuctionsSuccess = createAction('[Auctions] Load Success', props<{
  auctions: Auction[];
  totalCount: number;
  page: number;
  pageSize: number;
}>());
export const loadAuctionsFailure = createAction('[Auctions] Load Failure', props<{ error: string }>());

export const loadAuction = createAction('[Auction] Load', props<{ id: string }>());
export const loadAuctionSuccess = createAction('[Auction] Load Success', props<{ auction: Auction }>());
export const loadAuctionFailure = createAction('[Auction] Load Failure', props<{ error: string }>());

export const createAuction = createAction('[Auction] Create', props<{ dto: CreateAuction }>());
export const createAuctionSuccess = createAction('[Auction] Create Success', props<{ auction: Auction }>());
export const createAuctionFailure = createAction('[Auction] Create Failure', props<{ error: string }>());

export const bidPlaced = createAction('[Auction] Bid Placed', props<{ update: BidUpdate }>());
export const auctionEnded = createAction('[Auction] Ended', props<{ data: AuctionEnded }>());
export const clearSelected = createAction('[Auction] Clear Selected');
