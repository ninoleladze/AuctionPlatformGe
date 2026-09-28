import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AuctionList } from './auction-list';

describe('AuctionList', () => {
  let component: AuctionList;
  let fixture: ComponentFixture<AuctionList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AuctionList],
    }).compileComponents();

    fixture = TestBed.createComponent(AuctionList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
