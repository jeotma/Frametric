import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { DiscoveryService } from './discovery.service';

describe('DiscoveryService', () => {
  let service: DiscoveryService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        DiscoveryService,
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
      ]
    });
    service = TestBed.inject(DiscoveryService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should POST roulette and return SelectionResultDto', () => {
    const mockResponse = { movieId: '1', title: 'Test', directorName: 'Dir', releaseYear: 2020, selectionMechanismMetadata: 'random' };
    let result: any;

    service.apiV1DiscoveryRoulettePost({ scope: 1 }).subscribe(r => result = r);

    const req = httpMock.expectOne('http://localhost:5168/api/v1/discovery/roulette');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ scope: 1 });
    req.flush(mockResponse);
    expect(result).toEqual(mockResponse);
  });

  it('should POST dice and return DiceRollResultDto', () => {
    const mockResponse = { movieId: '1', title: 'Test', directorName: 'Dir', releaseYear: 2020, selectionMechanismMetadata: 'roll', diceResults: [], specialEvent: null };
    let result: any;

    service.apiV1DiscoveryDicePost({ scope: 1 }).subscribe(r => result = r);

    const req = httpMock.expectOne('http://localhost:5168/api/v1/discovery/dice');
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);
    expect(result).toEqual(mockResponse);
  });

  it('should POST slot-machine and return SlotMachineResultDto', () => {
    const mockResponse = { movieId: '1', title: 'Test', directorName: 'Dir', releaseYear: 2020, selectionMechanismMetadata: 'spin', reelResults: [], isJackpot: false };
    let result: any;

    service.apiV1DiscoverySlotMachinePost({ scope: 1 }).subscribe(r => result = r);

    const req = httpMock.expectOne('http://localhost:5168/api/v1/discovery/slot-machine');
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);
    expect(result).toEqual(mockResponse);
  });

  it('should POST mystery-box and return MysteryBoxDto', () => {
    const mockResponse = { boxIds: ['1', '2'], variant: 0, generatedAt: '2026-01-01T00:00:00Z' };
    let result: any;

    service.apiV1DiscoveryMysteryBoxPost({ scope: 1, variant: 0 }).subscribe(r => result = r);

    const req = httpMock.expectOne('http://localhost:5168/api/v1/discovery/mystery-box');
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);
    expect(result).toEqual(mockResponse);
  });

  it('should GET mystery-box reveal and return SelectionResultDto', () => {
    const mockResponse = { movieId: '1', title: 'Revealed', directorName: 'Dir', releaseYear: 2020, selectionMechanismMetadata: 'reveal' };
    let result: any;

    service.apiV1DiscoveryMysteryBoxBoxIdRevealGet('box-123').subscribe(r => result = r);

    const req = httpMock.expectOne('http://localhost:5168/api/v1/discovery/mystery-box/box-123/reveal');
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
    expect(result).toEqual(mockResponse);
  });

  it('should POST bingo with params', () => {
    const mockResponse = { gridSize: 3, squares: [] };
    let result: any;

    service.apiV1DiscoveryBingoPost({ gridSize: 3 }).subscribe((r: any) => result = r);

    const req = httpMock.expectOne('http://localhost:5168/api/v1/discovery/bingo');
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);
    expect(result).toEqual(mockResponse);
  });

  it('should POST bingo without params when gridSize omitted', () => {
    const mockResponse = { gridSize: 3, squares: [] };
    let result: any;

    service.apiV1DiscoveryBingoPost({}).subscribe((r: any) => result = r);

    const req = httpMock.expectOne('http://localhost:5168/api/v1/discovery/bingo');
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);
    expect(result).toEqual(mockResponse);
  });
});
