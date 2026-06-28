// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

import { Component, Input, OnChanges, SimpleChanges, Output, EventEmitter, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CinematicTooltipComponent } from '../../../../components/cinematic-tooltip/cinematic-tooltip.component';

@Component({
  selector: 'app-dice-roller',
  standalone: true,
  imports: [CommonModule, CinematicTooltipComponent],
  templateUrl: './dice-roller.html',
  styleUrl: './dice-roller.scss'
})
export class DiceRollerComponent implements OnChanges, OnDestroy {
  @Input() values: number[] = [0, 0, 0, 0, 0];
  @Input() labels: string[] = ['', '', '', '', ''];
  @Input() settled: boolean[] = [false, false, false, false, false];
  @Input() rolling: boolean[] = [false, false, false, false, false];
  @Input() pendingCriticalChoice: boolean = false;
  @Input() muted: boolean = false;

  @Output() dieClicked = new EventEmitter<number>();

  public diceTypes = ['Duration (D3)', 'Popularity (D4)', 'Risk (D6)', 'Quality (D12)', 'Genre (D20)'];
  public maxValues = [3, 4, 6, 12, 20];
  
  public displayValues: number[] = [0, 0, 0, 0, 0];
  private intervals: any[] = [null, null, null, null, null];
  private timeouts: any[] = [null, null, null, null, null];
  private audioCtx: AudioContext | null = null;

  public onDieClick(idx: number): void {
    this.dieClicked.emit(idx);
  }

  public playClackSound(pitch = 1.0, volume = 0.5): void {
    if (this.muted) return;
    try {
      if (!this.audioCtx) {
        this.audioCtx = new (window.AudioContext || (window as any).webkitAudioContext)();
      }
      if (this.audioCtx.state === 'suspended') {
        this.audioCtx.resume();
      }
      
      const ctx = this.audioCtx;
      const now = ctx.currentTime;
      
      // Node creation
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();
      
      // Noise buffer for the physical impact transient
      const bufferSize = ctx.sampleRate * 0.04; // 40ms transient
      const buffer = ctx.createBuffer(1, bufferSize, ctx.sampleRate);
      const data = buffer.getChannelData(0);
      for (let i = 0; i < bufferSize; i++) {
        data[i] = Math.random() * 2 - 1;
      }
      
      const noise = ctx.createBufferSource();
      noise.buffer = buffer;
      
      const noiseFilter = ctx.createBiquadFilter();
      noiseFilter.type = 'bandpass';
      noiseFilter.frequency.setValueAtTime(1200 * pitch, now);
      noiseFilter.Q.setValueAtTime(3.0, now);
      
      const noiseGain = ctx.createGain();
      noiseGain.gain.setValueAtTime(0.2 * volume, now);
      noiseGain.gain.exponentialRampToValueAtTime(0.005, now + 0.025);
      
      // Tone oscillator for body resonance
      osc.type = 'triangle';
      osc.frequency.setValueAtTime(140 * pitch, now);
      osc.frequency.exponentialRampToValueAtTime(70 * pitch, now + 0.07);
      
      gain.gain.setValueAtTime(0.4 * volume, now);
      gain.gain.exponentialRampToValueAtTime(0.005, now + 0.075);
      
      // Connections
      noise.connect(noiseFilter);
      noiseFilter.connect(noiseGain);
      noiseGain.connect(ctx.destination);
      
      osc.connect(gain);
      gain.connect(ctx.destination);
      
      // Play
      noise.start(now);
      osc.start(now);
      
      noise.stop(now + 0.04);
      osc.stop(now + 0.09);
    } catch (e) {
      // Audio Context blocked by autoplay or unsupported
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    for (let i = 0; i < 5; i++) {
      const isCurrentlyRolling = this.rolling && this.rolling[i];
      if (isCurrentlyRolling) {
        if (!this.intervals[i]) {
          const max = this.maxValues[i];
          this.intervals[i] = setInterval(() => {
            this.displayValues[i] = Math.floor(Math.random() * max) + 1;
            // Quiet, higher pitch micro-clacks for tumbling
            if (Math.random() < 0.22) {
              this.playClackSound(1.1 + Math.random() * 0.5, 0.15);
            }
          }, 60 + i * 10);
          // Safety timeout: force stop after 15 seconds
          this.timeouts[i] = setTimeout(() => {
            if (this.intervals[i]) {
              clearInterval(this.intervals[i]);
              this.intervals[i] = null;
            }
            this.displayValues[i] = this.values ? this.values[i] : 1;
          }, 15000);
        }
      } else {
        if (this.intervals[i]) {
          clearInterval(this.intervals[i]);
          this.intervals[i] = null;
          if (this.timeouts[i]) {
            clearTimeout(this.timeouts[i]);
            this.timeouts[i] = null;
          }
          // Hard, heavier pitch clack for landing
          const landingPitch = 0.5 + (4 - i) * 0.12;
          this.playClackSound(landingPitch, 0.6);
        }
        this.displayValues[i] = this.values ? this.values[i] : 0;
      }
    }
    if (this.settled && this.settled.every(s => s)) {
      this.clearAllIntervals();
    }
  }

  private clearAllIntervals(): void {
    this.intervals.forEach((interval, idx) => {
      if (interval) {
        clearInterval(interval);
        this.intervals[idx] = null;
      }
    });
    this.timeouts.forEach((t, idx) => {
      if (t) {
        clearTimeout(t);
        this.timeouts[idx] = null;
      }
    });
  }

  ngOnDestroy(): void {
    this.clearAllIntervals();
  }
}
