import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { DirectorDetailComponent } from './director-detail';
import { DirectorsService } from '../../../core/api';

describe('DirectorDetailComponent', () => {
  let component: DirectorDetailComponent;
  let fixture: ComponentFixture<DirectorDetailComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DirectorDetailComponent],
      providers: [
        provideRouter([]),
        { provide: DirectorsService, useValue: { getDirector: () => null } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DirectorDetailComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
