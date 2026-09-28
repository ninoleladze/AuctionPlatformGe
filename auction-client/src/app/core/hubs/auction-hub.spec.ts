import { TestBed } from '@angular/core/testing';

import { AuctionHub } from './auction-hub';

describe('AuctionHub', () => {
  let service: AuctionHub;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AuctionHub);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
