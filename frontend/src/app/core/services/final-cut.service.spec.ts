import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { FinalCutService } from './final-cut.service';
import { BASE_PATH } from '../api/variables';

describe('FinalCutService', () => {
  let service: FinalCutService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        FinalCutService,
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
        { provide: BASE_PATH, useValue: '' }
      ]
    });
    service = TestBed.inject(FinalCutService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('loadSummary', () => {
    it('should GET wrapped summary for a specific year', (done) => {
      const mockResponse = { totalWatches: 100, uniqueMoviesCount: 50, totalWatchtimeMinutes: 5000 };

      service.loadSummary(2025).subscribe({
        next: (data) => {
          expect(data).toEqual(mockResponse);
          done();
        }
      });

      const req = httpMock.expectOne('/api/analytics/wrapped?year=2025');
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });

    it('should GET wrapped summary for global year', (done) => {
      const mockResponse = { totalWatches: 500, uniqueMoviesCount: 200, totalWatchtimeMinutes: 25000 };

      service.loadSummary('global').subscribe({
        next: (data) => {
          expect(data).toEqual(mockResponse);
          done();
        }
      });

      const req = httpMock.expectOne('/api/analytics/wrapped');
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });
  });

  describe('loadAllData', () => {
    const year = 2025;
    const mockDto = (overrides: any = {}) => ({
      primeTime: { peakDay: 'Friday', peakDayCount: 20, peakMonth: 'October', peakMonthCount: 15, slumpDay: 'Monday', slumpDayCount: 3, slumpMonth: 'February', slumpMonthCount: 2 },
      cinemaFatigue: { avgRatingLightDays: 7.5, avgRatingHeavyDays: 6.2, slumpDay: 'Monday', slumpDayWatchCount: 3, slumpMonth: 'February', slumpMonthWatchCount: 2 },
      weekendWarrior: { weekendWatches: 40, weekdayWatches: 60 },
      genreLandscape: [{ genreName: 'Drama', count: 30, averageRating: 7.0 }],
      castingPairs: [{ actor1: 'Actor A', actor2: 'Actor B', count: 3 }],
      directorActorPairs: [{ directorName: 'Director A', actorName: 'Actor A', collaborationCount: 5 }],
      bestRookies: { newDirectors: [], newActors: [] },
      bookends: { openingScene: { id: '1', title: 'First' }, fadeToBlack: { id: '2', title: 'Last' } },
      shortestMovie: { id: '3', title: 'Short', runtimeMinutes: 80 },
      longestMovie: { id: '4', title: 'Long', runtimeMinutes: 200 },
      monthlyExtremes: [{ month: 1, monthName: 'January', bestMovie: undefined, worstMovie: undefined }],
      mostRewatched: { title: 'Rewatch', posterPath: '', releaseYear: 2020, rewatchCount: 5 },
      topBottom: { topRated: [], bottomRated: [] },
      hiddenGems: [],
      ratingEvolution: [],
      genreStreaks: [],
      directors: [],
      actors: [],
      decadeBreakdown: [],
      eraBreakdown: undefined,
      ...overrides
    });

    it('should fire 21 parallel GET requests', (done) => {
      const mock = mockDto();

      service.loadAllData(year).subscribe({
        next: () => done()
      });

      const reqs = httpMock.match(req => req.url.startsWith('/api/analytics/advanced/'));
      expect(reqs.length).toBe(21);
      reqs.forEach(r => r.flush({}));
    });

    it('should map null arrays to empty arrays', (done) => {
      const mock = mockDto({
        genreLandscape: null,
        castingPairs: null,
        directorActorPairs: null,
        monthlyExtremes: null,
        hiddenGems: null,
        ratingEvolution: null,
        genreStreaks: null,
        directors: null,
        actors: null,
        decadeBreakdown: null,
      });

      service.loadAllData(year).subscribe({
        next: (data) => {
          expect(data.genreLandscape).toEqual([]);
          expect(data.castingPairs).toEqual([]);
          expect(data.directorActorPairs).toEqual([]);
          expect(data.monthlyExtremes).toEqual([]);
          expect(data.hiddenGems).toEqual([]);
          expect(data.ratingEvolution).toEqual([]);
          expect(data.genreStreaks).toEqual([]);
          expect(data.directors).toEqual([]);
          expect(data.actors).toEqual([]);
          expect(data.decadeBreakdown).toEqual([]);
          done();
        }
      });

      httpMock.expectOne(req => req.url.startsWith('/api/analytics/advanced/')).flush(mock.primeTime, { status: 200, statusText: 'OK' });
      httpMock.expectOne(req => req.url.startsWith('/api/analytics/advanced/')).flush(mock.cinemaFatigue, { status: 200, statusText: 'OK' });
      const remaining = 19;
      for (let i = 0; i < remaining; i++) {
        httpMock.expectOne(req => req.url.startsWith('/api/analytics/advanced/')).flush(mock.weekendWarrior, { status: 200, statusText: 'OK' });
      }
    });

    it('should combine shortest+longest into davidAndGoliath', (done) => {
      const mock = mockDto();

      service.loadAllData(year).subscribe({
        next: (data) => {
          expect(data.davidAndGoliath?.shortest?.title).toBe('Short');
          expect(data.davidAndGoliath?.longest?.title).toBe('Long');
          done();
        }
      });

      httpMock.expectOne(req => req.url.startsWith('/api/analytics/advanced/')).flush(mock.primeTime, { status: 200, statusText: 'OK' });
      httpMock.expectOne(req => req.url.startsWith('/api/analytics/advanced/')).flush(mock.cinemaFatigue, { status: 200, statusText: 'OK' });
      const remaining = 19;
      for (let i = 0; i < remaining; i++) {
        httpMock.expectOne(req => req.url.startsWith('/api/analytics/advanced/')).flush(mock.weekendWarrior, { status: 200, statusText: 'OK' });
      }
    });

    it('should pass undefined eraBreakdown when null', (done) => {
      const mock = mockDto({ eraBreakdown: undefined });

      service.loadAllData(year).subscribe({
        next: (data) => {
          expect(data.eraBreakdown).toBeUndefined();
          done();
        }
      });

      httpMock.expectOne(req => req.url.startsWith('/api/analytics/advanced/')).flush(mock.primeTime, { status: 200, statusText: 'OK' });
      httpMock.expectOne(req => req.url.startsWith('/api/analytics/advanced/')).flush(mock.cinemaFatigue, { status: 200, statusText: 'OK' });
      const remaining = 19;
      for (let i = 0; i < remaining; i++) {
        httpMock.expectOne(req => req.url.startsWith('/api/analytics/advanced/')).flush(mock.weekendWarrior, { status: 200, statusText: 'OK' });
      }
    });
  });
});
