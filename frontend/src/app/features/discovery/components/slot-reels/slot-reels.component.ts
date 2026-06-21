// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

import { Component, Input, OnChanges, SimpleChanges, ElementRef, ViewChildren, QueryList, Output, EventEmitter, OnInit, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SlotMachineResultDto } from '../../../../core/api/model/slot-machine-result-dto';

@Component({
  selector: 'app-slot-reels',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './slot-reels.html',
  styleUrl: './slot-reels.scss'
})
export class SlotReelsComponent implements OnChanges, OnInit, AfterViewInit {
  @Input() result: SlotMachineResultDto | null = null;
  @Input() isSpinning: boolean = false;
  
  @Output() finished = new EventEmitter<void>();
  @Output() interact = new EventEmitter<void>();

  @ViewChildren('reelBox') reelBoxes!: QueryList<ElementRef<HTMLDivElement>>;

  public genericReels = [
    { label: 'GENRE', values: ['?', 'HORROR', 'DRAMA', 'ACTION', 'COMEDY', 'THRILLER'] },
    { label: 'DECADE', values: ['?', '1980s', '1990s', '2000s', '2010s', '2020s'] },
    { label: 'POPULARITY', values: ['?', 'BLOCKBUSTER', 'MAINSTREAM', 'NICHE / CULT', 'HIDDEN GEM'] },
    { label: 'RATING', values: ['?', 'MASTERPIECE', 'GREAT', 'DECENT', 'UNDERDOG'] },
    { label: 'COUNTRY', values: ['?', 'USA', 'UK', 'FRANCE', 'JAPAN', 'SOUTH KOREA'] }
  ];

  public currentValues: string[] = ['?', '?', '?', '?', '?'];
  public currentLabels: string[] = ['GENRE', 'DECADE', 'POPULARITY', 'RATING', 'COUNTRY'];

  ngOnInit() {
    this.resetToIdle();
  }

  ngAfterViewInit() {
    setTimeout(() => {
      this.positionReelsToIdle();
    }, 150);
  }

  public pullLever(): void {
    if (!this.isSpinning) {
      this.interact.emit();
    }
  }

  private resetToIdle(): void {
    this.currentValues = ['?', '?', '?', '?', '?'];
    this.currentLabels = ['GENRE', 'DECADE', 'POPULARITY', 'RATING', 'COUNTRY'];
  }

  private positionReelsToIdle(): void {
    if (!this.reelBoxes) return;
    this.reelBoxes.forEach((box, i) => {
      const stripEl = box.nativeElement.querySelector('.reel-strip') as HTMLElement;
      if (stripEl) {
        const originalLength = this.genericReels[i].values.length;
        const itemHeight = 90;
        const initialOffset = - (originalLength * itemHeight);
        stripEl.style.transition = 'none';
        stripEl.style.transform = `translateY(${initialOffset}px)`;
      }
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['isSpinning'] && this.isSpinning) {
      this.startSpin();
    }
    if (changes['result'] && this.result) {
      this.stopSpin(this.result);
    }
  }

  private startSpin(): void {
    this.reelBoxes?.forEach((box, i) => {
      const el = box.nativeElement;
      el.classList.remove('snap-stop');
      el.classList.remove('matched');
      const stripEl = el.querySelector('.reel-strip') as HTMLElement;
      if (stripEl) {
        stripEl.style.transition = 'none';
        stripEl.style.transform = 'translateY(0)';
        void stripEl.offsetHeight; // force reflow
        
        stripEl.classList.add('spinning');
        stripEl.style.animationDelay = `${i * 80}ms`;
      }
    });
  }

  private stopSpin(r: SlotMachineResultDto): void {
    if (!this.reelBoxes) return;

    const boxes = this.reelBoxes.toArray();
    let delayAcc = 0;

    r.reelResults.forEach((reel, index) => {
      setTimeout(() => {
        const box = boxes[index];
        if (!box) return;
        
        const stripEl = box.nativeElement.querySelector('.reel-strip') as HTMLElement;
        if (!stripEl) return;

        const val = (reel.value || 'ANY').toUpperCase();
        
        const reelData = this.genericReels[index];
        let valIndex = reelData.values.indexOf(val);
        if (valIndex === -1) {
          reelData.values.push(val);
          valIndex = reelData.values.length - 1;
        }

        const originalLength = reelData.values.length;
        const targetIndex = valIndex + originalLength;
        const itemHeight = 90;
        const targetOffset = - (targetIndex * itemHeight);

        stripEl.classList.remove('spinning');
        stripEl.style.animationDelay = '';
        stripEl.style.transition = 'transform 1.3s cubic-bezier(0.175, 0.885, 0.32, 1.18)';
        stripEl.style.transform = `translateY(${targetOffset}px)`;
        
        box.nativeElement.classList.add('snap-stop');

        // Apply green glow if this reel matched
        const isMatched = r.matchedReels ? r.matchedReels[index] : false;
        if (isMatched) {
          box.nativeElement.classList.add('matched');
        }
        
        this.currentLabels[index] = reel.label;
        this.currentValues[index] = reel.value || 'ANY';
      }, delayAcc);
      
      delayAcc += 350; // Stagger stopped animation
    });

    setTimeout(() => {
      this.finished.emit();
    }, delayAcc + 1300);
  }
}
