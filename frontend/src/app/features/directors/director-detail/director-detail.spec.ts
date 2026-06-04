import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DirectorDetail } from './director-detail';

describe('DirectorDetail', () => {
  let component: DirectorDetail;
  let fixture: ComponentFixture<DirectorDetail>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DirectorDetail],
    }).compileComponents();

    fixture = TestBed.createComponent(DirectorDetail);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
