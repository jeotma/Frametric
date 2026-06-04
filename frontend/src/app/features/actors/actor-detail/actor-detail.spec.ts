import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ActorDetail } from './actor-detail';

describe('ActorDetail', () => {
  let component: ActorDetail;
  let fixture: ComponentFixture<ActorDetail>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ActorDetail],
    }).compileComponents();

    fixture = TestBed.createComponent(ActorDetail);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
