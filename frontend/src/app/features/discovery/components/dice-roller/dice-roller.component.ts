// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

import { Component, Input, OnChanges, SimpleChanges, Output, EventEmitter, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dice-roller',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dice-roller.html',
  styleUrl: './dice-roller.scss'
})
export class DiceRollerComponent implements OnChanges, OnDestroy {
  @Input() values: number[] = [0, 0, 0, 0, 0];
  @Input() labels: string[] = ['', '', '', '', ''];
  @Input() settled: boolean[] = [false, false, false, false, false];
  @Input() rolling: boolean[] = [false, false, false, false, false];
  @Input() pendingCriticalChoice: boolean = false;

  @Output() dieClicked = new EventEmitter<number>();

  public diceTypes = ['Quality (D3)', 'Rarity (D4)', 'Risk (D6)', 'Complexity (D12)', 'Exploration (D20)'];
  public maxValues = [3, 4, 6, 12, 20];
  
  public displayValues: number[] = [0, 0, 0, 0, 0];
  private intervals: any[] = [null, null, null, null, null];

  public onDieClick(idx: number): void {
    this.dieClicked.emit(idx);
  }

  ngOnChanges(changes: SimpleChanges): void {
    for (let i = 0; i < 5; i++) {
      const isCurrentlyRolling = this.rolling && this.rolling[i];
      if (isCurrentlyRolling) {
        if (!this.intervals[i]) {
          const max = this.maxValues[i];
          this.intervals[i] = setInterval(() => {
            this.displayValues[i] = Math.floor(Math.random() * max) + 1;
          }, 60 + i * 10);
        }
      } else {
        if (this.intervals[i]) {
          clearInterval(this.intervals[i]);
          this.intervals[i] = null;
        }
        this.displayValues[i] = this.values ? this.values[i] : 0;
      }
    }
  }

  private clearAllIntervals(): void {
    this.intervals.forEach((interval, idx) => {
      if (interval) {
        clearInterval(interval);
        this.intervals[idx] = null;
      }
    });
  }

  ngOnDestroy(): void {
    this.clearAllIntervals();
  }
}
