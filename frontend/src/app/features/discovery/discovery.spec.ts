import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DiscoveryComponent } from './discovery';
import { DiscoveryService } from '../../core/api/api/discovery.service';
import { CustomListsService } from '../../core/api/api/custom-lists.service';
import { AuthService } from '../../core/services/auth.service';
import { of } from 'rxjs';
import { By } from '@angular/platform-browser';

describe('DiscoveryComponent', () => {
  let component: DiscoveryComponent;
  let fixture: ComponentFixture<DiscoveryComponent>;
  let mockService: any;
  let mockAuthService: any;
  let mockCustomListsService: any;

  const createMockService = () => ({
    apiV1DiscoveryRoulettePost: () => of({
      winner: { movieId: '1', title: 'Roulette Movie', directorName: 'Dir', releaseYear: 2020, selectionMechanismMetadata: 'random' },
      spinSequence: [{ movieId: '1', title: 'Roulette Movie', directorName: 'Dir', releaseYear: 2020, selectionMechanismMetadata: 'random' }]
    } as any),
    apiV1DiscoveryDicePost: () => of({ movieId: '2', title: 'Dice Movie', directorName: 'Dir', releaseYear: 2021, selectionMechanismMetadata: 'roll', diceResults: [], specialEvent: null } as any),
    apiV1DiscoverySlotMachinePost: () => of({ movieId: '3', title: 'Slot Movie', directorName: 'Dir', releaseYear: 2022, selectionMechanismMetadata: 'spin', reelResults: [], isJackpot: false } as any),
    apiV1DiscoveryMysteryBoxPost: () => of({ boxIds: ['a', 'b'], variant: 0, generatedAt: new Date().toISOString() } as any),
    apiV1DiscoveryMysteryBoxBoxIdRevealGet: () => of({ movieId: '4', title: 'Revealed Movie', directorName: 'Dir', releaseYear: 2023, selectionMechanismMetadata: 'reveal' } as any),
    apiV1DiscoveryBingoPost: (req: any) => of({ gridSize: req?.gridSize ?? 3, squares: [] } as any),
    apiV1DiscoveryAvailableCountriesGet: () => of(['USA', 'UK', 'Spain']),
    apiV1DiscoveryBingoBoardsGet: () => of([]),
  });

  beforeEach(async () => {
    mockService = createMockService();
    mockAuthService = {
      isAuthenticated: () => true,
    };
    mockCustomListsService = {
      apiV1CustomListsGet: () => of([]),
    };
    await TestBed.configureTestingModule({
      imports: [DiscoveryComponent],
      providers: [
        { provide: DiscoveryService, useValue: mockService },
        { provide: AuthService, useValue: mockAuthService },
        { provide: CustomListsService, useValue: mockCustomListsService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DiscoveryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  describe('creation', () => {
    it('should create the component', () => {
      expect(component).toBeTruthy();
    });

    it('should display 5 discovery modes as tabs', () => {
      const tabs = fixture.debugElement.queryAll(By.css('button'));
      const tabTexts = tabs.map(t => t.nativeElement.textContent.trim());
      expect(tabTexts).toContain('Bingo');
      expect(tabTexts).toContain('Roulette');
      expect(tabTexts).toContain('Mystery Box');
      expect(tabTexts).toContain('Dice');
      expect(tabTexts).toContain('Slot Machine');
    });

    it('should start with Roulette tab active', () => {
      expect(component.activeTab()).toBe('roulette');
    });

    it('should switch tabs when clicked', () => {
      const tabs = fixture.debugElement.queryAll(By.css('button'));
      const rouletteTab = tabs.find(t => t.nativeElement.textContent.trim() === 'Roulette');
      rouletteTab?.nativeElement.click();
      fixture.detectChanges();
      expect(component.activeTab()).toBe('roulette');
    });
  });

  describe('bingo', () => {
    it('should load bingo board on loadBingo', () => {
      component.loadBingo();
      expect(component.bingoResultSig()).toBeTruthy();
      expect(component.bingoResultSig()?.gridSize).toBe(3);
    });
  });

  describe('roulette', () => {
    it('should spell roulette and set result', () => {
      component.spellRoulette();
      component.onRouletteFinished(); // Simulate animation finish
      expect(component.rouletteWinnerSig()).toBeTruthy();
      expect(component.rouletteWinnerSig()?.title).toBe('Roulette Movie');
    });
  });

  describe('mystery box', () => {
    it('should generate boxes and set result', () => {
      component.generateMysteryBox();
      expect(component.mysteryResultSig()).toBeTruthy();
      expect(component.mysteryResultSig()?.boxIds.length).toBeGreaterThan(0);
    });

    it('should reveal a specific box', async () => {
      component.generateMysteryBox();
      component.revealBox('a');
      await new Promise(r => setTimeout(r, 1600));
      expect(component.revealedMovieSig()).toBeTruthy();
      expect(component.revealedMovieSig()?.title).toBe('Revealed Movie');
    });
  });

  describe('dice', () => {
    it('should roll dice and set result', () => {
      component.rollDice();
      component.onDiceFinished(); // Simulate animation finish
      expect(component.diceResultSig()).toBeTruthy();
      expect(component.diceResultSig()?.title).toBe('Dice Movie');
    });
  });

  describe('slot machine', () => {
    it('should spin slots and set result', () => {
      component.spinSlots();
      expect(component.slotResultSig()).toBeTruthy();
      expect(component.slotResultSig()?.title).toBe('Slot Movie');
    });
  });

  describe('loading states', () => {
    it('should set rouletteLoading to false after spell completes', () => {
      component.spellRoulette();
      expect(component.rouletteLoading()).toBe(false);
    });

    it('should set bingoLoading to false after load completes', () => {
      component.loadBingo();
      expect(component.bingoLoading()).toBe(false);
    });

    it('should set diceLoading to false after roll completes', () => {
      component.rollDice();
      component.onDiceFinished();
      expect(component.diceLoading()).toBe(false);
    });
  });

  describe('error handling', () => {
    let errorService: any;

    beforeEach(async () => {
      errorService = {
        apiV1DiscoveryRoulettePost: () => of({ movieId: '1', title: 'OK', directorName: 'Dir', releaseYear: 2020, selectionMechanismMetadata: 'random' } as any),
        apiV1DiscoveryDicePost: () => of({ movieId: '2', title: 'OK', directorName: 'Dir', releaseYear: 2021, selectionMechanismMetadata: 'roll', diceResults: [], specialEvent: null } as any),
        apiV1DiscoverySlotMachinePost: () => of({ movieId: '3', title: 'OK', directorName: 'Dir', releaseYear: 2022, selectionMechanismMetadata: 'spin', reelResults: [], isJackpot: false } as any),
        apiV1DiscoveryMysteryBoxPost: () => of({ boxIds: ['a', 'b'], variant: 0, generatedAt: new Date().toISOString() } as any),
        apiV1DiscoveryMysteryBoxBoxIdRevealGet: () => of({ movieId: '4', title: 'OK', directorName: 'Dir', releaseYear: 2023, selectionMechanismMetadata: 'reveal' } as any),
        apiV1DiscoveryBingoPost: (req: any) => of({ gridSize: req?.gridSize ?? 3, squares: [] } as any),
        apiV1DiscoveryAvailableCountriesGet: () => of(['USA', 'UK']),
        apiV1DiscoveryBingoBoardsGet: () => of([]),
      };
      TestBed.resetTestingModule();
      await TestBed.configureTestingModule({
        imports: [DiscoveryComponent],
        providers: [
          { provide: DiscoveryService, useValue: errorService },
          { provide: AuthService, useValue: mockAuthService },
          { provide: CustomListsService, useValue: mockCustomListsService }
        ]
      }).compileComponents();
      fixture = TestBed.createComponent(DiscoveryComponent);
      component = fixture.componentInstance;
      fixture.detectChanges();
    });

    it('should handle errors gracefully in roulette', () => {
      component.spellRoulette();
      expect(component.rouletteLoading()).toBe(false);
    });
  });

  describe('trackBy', () => {
    it('should return index for trackByIndex', () => {
      expect(component.trackByIndex(0)).toBe(0);
      expect(component.trackByIndex(5)).toBe(5);
    });

    it('should return id for trackById', () => {
      expect(component.trackById(0, { objectiveId: 'abc' })).toBe('abc');
      expect(component.trackById(0, { boxId: 'xyz' })).toBe('xyz');
      expect(component.trackById(0, {})).toBe('');
    });
  });
});
