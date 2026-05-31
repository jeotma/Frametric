// Final Cut Smart Component
import { Component, OnInit, inject, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { AnalyticsService } from '../../core/api/api/analytics.service';
import { WrappedSummaryDto } from '../../core/api/model/wrapped-summary-dto';

// Slides
import { IntroSlideComponent } from './slides/intro-slide/intro-slide';
import { GenreSlideComponent } from './slides/genre-slide/genre-slide';
import { DirectorSlideComponent } from './slides/director-slide/director-slide';
import { DecadeSlideComponent } from './slides/decade-slide/decade-slide';
import { SummarySlideComponent } from './slides/summary-slide/summary-slide';

@Component({
  selector: 'app-final-cut',
  standalone: true,
  imports: [
    CommonModule,
    IntroSlideComponent,
    GenreSlideComponent,
    DirectorSlideComponent,
    DecadeSlideComponent,
    SummarySlideComponent
  ],
  templateUrl: './final-cut.html',
  styleUrl: './final-cut.scss'
})
export class FinalCutComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private analytics = inject(AnalyticsService);

  public year = signal<number>(new Date().getFullYear() - 1);
  public data = signal<WrappedSummaryDto | null>(null);
  public loading = signal<boolean>(true);
  public activeSlide = signal<number>(0);

  public readonly SLIDE_COUNT = 5;

  constructor() {
    effect(() => {
      // Whenever year changes, fetch data
      const y = this.year();
      this.loadData(y);
    });
  }

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const yearParam = params.get('year');
      if (yearParam) {
        this.year.set(parseInt(yearParam, 10));
      }
    });
  }

  private loadData(year: number) {
    this.loading.set(true);
    this.activeSlide.set(0);
    this.analytics.apiAnalyticsWrappedYearGet(year).subscribe({
      next: (res) => {
        this.data.set(res);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load Final Cut data', err);
        this.loading.set(false);
      }
    });
  }

  public nextSlide() {
    if (this.activeSlide() < this.SLIDE_COUNT - 1) {
      this.activeSlide.update(v => v + 1);
    }
  }

  public prevSlide() {
    if (this.activeSlide() > 0) {
      this.activeSlide.update(v => v - 1);
    }
  }

  public changeYear(newYear: number) {
    this.router.navigate(['/final-cut', newYear]);
  }

  public onYearChange(event: any) {
    this.changeYear(Number(event.target.value));
  }
}
