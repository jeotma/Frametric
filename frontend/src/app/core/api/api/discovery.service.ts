import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SelectionResultDto } from '../model/selection-result-dto';
import { MysteryBoxDto } from '../model/mystery-box-dto';
import { BingoGridDto } from '../model/bingo-grid-dto';
import { DiceRollResultDto } from '../model/dice-roll-result-dto';
import { SlotMachineResultDto } from '../model/slot-machine-result-dto';
import { RouletteRequest } from '../model/roulette-request';
import { MysteryBoxRequest } from '../model/mystery-box-request';
import { DiceRollRequest } from '../model/dice-roll-request';
import { SlotMachineRequest } from '../model/slot-machine-request';

@Injectable({
  providedIn: 'root'
})
export class DiscoveryService {
  private http = inject(HttpClient);
  private basePath = 'http://localhost:5168';

  public apiV1DiscoveryRoulettePost(request: RouletteRequest): Observable<SelectionResultDto> {
    return this.http.post<SelectionResultDto>(`${this.basePath}/api/v1/discovery/roulette`, request);
  }

  public apiV1DiscoveryDicePost(request: DiceRollRequest): Observable<DiceRollResultDto> {
    return this.http.post<DiceRollResultDto>(`${this.basePath}/api/v1/discovery/dice`, request);
  }

  public apiV1DiscoverySlotMachinePost(request: SlotMachineRequest): Observable<SlotMachineResultDto> {
    return this.http.post<SlotMachineResultDto>(`${this.basePath}/api/v1/discovery/slot-machine`, request);
  }

  public apiV1DiscoveryMysteryBoxPost(request: MysteryBoxRequest): Observable<MysteryBoxDto> {
    return this.http.post<MysteryBoxDto>(`${this.basePath}/api/v1/discovery/mystery-box`, request);
  }

  public apiV1DiscoveryMysteryBoxBoxIdRevealGet(boxId: string): Observable<SelectionResultDto> {
    return this.http.get<SelectionResultDto>(`${this.basePath}/api/v1/discovery/mystery-box/${boxId}/reveal`);
  }

  public apiV1DiscoveryBingoGet(gridSize?: number): Observable<BingoGridDto> {
    const params = gridSize ? `?gridSize=${gridSize}` : '';
    return this.http.get<BingoGridDto>(`${this.basePath}/api/v1/discovery/bingo${params}`);
  }
}
