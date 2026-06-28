import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { FinalCutService } from './final-cut.service';
import { BASE_PATH } from '../api/variables';

const ENDPOINT = (path: string) => (req: any) => req.url.startsWith(path);

function expectAndFlush(httpMock: HttpTestingController, urlPrefix: string, data: any): void {
  httpMock.expectOne(ENDPOINT(urlPrefix)).flush(data);
}

function flushDefault(httpMock: HttpTestingController): void {
  expectAndFlush(httpMock, '/api/v1/analytics/advanced/final-cut/prime-time', { peakDay: 'Fri', peakDayCount: 1, peakMonth: 'Oct', peakMonthCount: 1, slumpDay: 'Mon', slumpDayCount: 1, slumpMonth: 'Feb', slumpMonthCount: 1 });
  expectAndFlush(httpMock, '/api/v1/analytics/advanced/bonus/cinematic-fatigue', { avgRatingLightDays: 7, avgRatingHeavyDays: 6, slumpDay: 'Mon', slumpDayWatchCount: 1, slumpMonth: 'Feb', slumpMonthWatchCount: 1 });
  expectAndFlush(httpMock, '/api/v1/analytics/advanced/bonus/weekend-warrior', { weekendWatches: 10, weekdayWatches: 20 });
  expectAndFlush(httpMock, '/api/v1/analytics/advanced/final-cut/genre-landscape', [{ genreName: 'Drama', count: 5, averageRating: 7 }]);
  expectAndFlush(httpMock, '/api/v1/analytics/advanced/watched/casting-repetitions', []);
  expectAndFlush(httpMock, '/api/v1/analytics/advanced/final-cut/director-actor-pairs', []);
  expectAndFlush(httpMock, '/api/v1/analytics/advanced/final-cut/best-rookies', { newDirectors: [], newActors: [] });
  expectAndFlush(httpMock, '/api/v1/analytics/advanced/final-cut/bookends', {});
  expectAndFlush(httpMock, '/api/v1/analytics/advanced/watched/longest-movie', { id: 'long', title: 'Long', runtimeMinutes: 200 });
  expectAndFlush(httpMock, '/api/v1/analytics/advanced/final-cut/shortest-movie', { id: 'short', title: 'Short', runtimeMinutes: 80 });
  expectAndFlush(httpMock, '/api/v1/analytics/advanced/final-cut/monthly-extremes', []);
  expectAndFlush(httpMock, '/api/v1/analytics/advanced/final-cut/most-rewatched', { title: 'Rewatch', rewatchCount: 5 });
  expectAndFlush(httpMock, '/api/v1/analytics/advanced/final-cut/top-bottom-rated', { topRated: [], bottomRated: [] });
  expectAndFlush(httpMock, '/api/v1/analytics/advanced/bonus/hidden-gems', []);
  expectAndFlush(httpMock, '/api/v1/analytics/advanced/watched/rating-evolution', []);
  expectAndFlush(httpMock, '/api/v1/analytics/advanced/watched/genre-streaks', []);
  expectAndFlush(httpMock, '/api/v1/analytics/advanced/watched/directors', []);
  expectAndFlush(httpMock, '/api/v1/analytics/advanced/watched/actors', []);
  expectAndFlush(httpMock, '/api/v1/analytics/advanced/watched/decades', []);
  expectAndFlush(httpMock, '/api/v1/analytics/advanced/watched/predominant-era', {});
}

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
    it('should GET wrapped summary for a specific year', async () => {
      const mockResponse = { totalWatches: 100, uniqueMoviesCount: 50, totalWatchtimeMinutes: 5000 };

      const promise = firstValueFrom(service.loadSummary(2025));

      const req = httpMock.expectOne('/api/v1/analytics/wrapped?year=2025');
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);

      const data = await promise;
      expect(data).toEqual(mockResponse);
    });

    it('should GET wrapped summary for global year', async () => {
      const mockResponse = { totalWatches: 500, uniqueMoviesCount: 200, totalWatchtimeMinutes: 25000 };

      const promise = firstValueFrom(service.loadSummary('global'));

      const req = httpMock.expectOne('/api/v1/analytics/wrapped');
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);

      const data = await promise;
      expect(data).toEqual(mockResponse);
    });
  });

  describe('loadAllData', () => {
    const year = 2025;

    it('should fire 20 parallel GET requests', async () => {
      const promise = firstValueFrom(service.loadAllData(year));

      const reqs = httpMock.match(ENDPOINT('/api/v1/analytics/advanced/'));
      expect(reqs.length).toBe(20);
      reqs.forEach(r => r.flush({}));

      await promise;
    });

    it('should map null arrays to empty arrays', async () => {
      const promise = firstValueFrom(service.loadAllData(year));

      expectAndFlush(httpMock, '/api/v1/analytics/advanced/final-cut/prime-time', { peakDay: 'Fri', peakDayCount: 1, peakMonth: 'Oct', peakMonthCount: 1, slumpDay: 'Mon', slumpDayCount: 1, slumpMonth: 'Feb', slumpMonthCount: 1 });
      expectAndFlush(httpMock, '/api/v1/analytics/advanced/bonus/cinematic-fatigue', { avgRatingLightDays: 7, avgRatingHeavyDays: 6, slumpDay: 'Mon', slumpDayWatchCount: 1, slumpMonth: 'Feb', slumpMonthWatchCount: 1 });
      // All array fields return null to test ?? []
      expectAndFlush(httpMock, '/api/v1/analytics/advanced/bonus/weekend-warrior', { weekendWatches: 10, weekdayWatches: 20 });
      expectAndFlush(httpMock, '/api/v1/analytics/advanced/final-cut/genre-landscape', null);
      expectAndFlush(httpMock, '/api/v1/analytics/advanced/watched/casting-repetitions', null);
      expectAndFlush(httpMock, '/api/v1/analytics/advanced/final-cut/director-actor-pairs', null);
      expectAndFlush(httpMock, '/api/v1/analytics/advanced/final-cut/best-rookies', { newDirectors: [], newActors: [] });
      expectAndFlush(httpMock, '/api/v1/analytics/advanced/final-cut/bookends', {});
      expectAndFlush(httpMock, '/api/v1/analytics/advanced/watched/longest-movie', { id: 'long', title: 'Long', runtimeMinutes: 200 });
      expectAndFlush(httpMock, '/api/v1/analytics/advanced/final-cut/shortest-movie', { id: 'short', title: 'Short', runtimeMinutes: 80 });
      expectAndFlush(httpMock, '/api/v1/analytics/advanced/final-cut/monthly-extremes', null);
      expectAndFlush(httpMock, '/api/v1/analytics/advanced/final-cut/most-rewatched', { title: 'Rewatch', rewatchCount: 5 });
      expectAndFlush(httpMock, '/api/v1/analytics/advanced/final-cut/top-bottom-rated', { topRated: [], bottomRated: [] });
      expectAndFlush(httpMock, '/api/v1/analytics/advanced/bonus/hidden-gems', null);
      expectAndFlush(httpMock, '/api/v1/analytics/advanced/watched/rating-evolution', null);
      expectAndFlush(httpMock, '/api/v1/analytics/advanced/watched/genre-streaks', null);
      expectAndFlush(httpMock, '/api/v1/analytics/advanced/watched/directors', null);
      expectAndFlush(httpMock, '/api/v1/analytics/advanced/watched/actors', null);
      expectAndFlush(httpMock, '/api/v1/analytics/advanced/watched/decades', null);
      expectAndFlush(httpMock, '/api/v1/analytics/advanced/watched/predominant-era', {});

      const data = await promise;
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
    });

    it('should combine shortest+longest into davidAndGoliath', async () => {
      const promise = firstValueFrom(service.loadAllData(year));
      flushDefault(httpMock);

      const data = await promise;
      expect(data.davidAndGoliath?.shortest?.title).toBe('Short');
      expect(data.davidAndGoliath?.longest?.title).toBe('Long');
    });
  });
});
