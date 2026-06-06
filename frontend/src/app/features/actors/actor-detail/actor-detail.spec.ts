import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ActorDetailComponent } from './actor-detail';
import { ActorsService } from '../../../core/api';

describe('ActorDetailComponent', () => {
  let component: ActorDetailComponent;
  let fixture: ComponentFixture<ActorDetailComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ActorDetailComponent],
      providers: [
        provideRouter([]),
        { provide: ActorsService, useValue: { getActor: () => null } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ActorDetailComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
