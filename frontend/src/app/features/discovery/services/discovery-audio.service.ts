import { Injectable, signal, effect } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class DiscoveryAudioService {
  public muted = signal<boolean>(false);
  private audioCtx: AudioContext | null = null;

  constructor() {
    // Restore mute state from localStorage
    const saved = localStorage.getItem('frametric_discovery_muted');
    if (saved !== null) {
      this.muted.set(saved === 'true');
    }

    // Persist mute state changes
    effect(() => {
      localStorage.setItem('frametric_discovery_muted', String(this.muted()));
    });
  }

  public toggleMute(): void {
    this.muted.update(m => !m);
  }

  private initCtx(): AudioContext {
    if (!this.audioCtx) {
      this.audioCtx = new (window.AudioContext || (window as any).webkitAudioContext)();
    }
    if (this.audioCtx.state === 'suspended') {
      this.audioCtx.resume();
    }
    return this.audioCtx;
  }

  public playTick(pitch = 1.0, volume = 0.4): void {
    if (this.muted()) return;
    try {
      const ctx = this.initCtx();
      const now = ctx.currentTime;

      const osc = ctx.createOscillator();
      const gain = ctx.createGain();

      osc.type = 'triangle';
      osc.frequency.setValueAtTime(1500 * pitch, now);

      gain.gain.setValueAtTime(0.15 * volume, now);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.015);

      osc.connect(gain);
      gain.connect(ctx.destination);

      osc.start(now);
      osc.stop(now + 0.02);
    } catch (e) {
      // Audio context blocked
    }
  }

  public playClack(pitch = 1.0, volume = 0.5): void {
    if (this.muted()) return;
    try {
      const ctx = this.initCtx();
      const now = ctx.currentTime;

      // 40ms noise transient
      const bufferSize = ctx.sampleRate * 0.04;
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

      // Triangle body tone
      const osc = ctx.createOscillator();
      const bodyGain = ctx.createGain();

      osc.type = 'triangle';
      osc.frequency.setValueAtTime(140 * pitch, now);
      osc.frequency.exponentialRampToValueAtTime(70 * pitch, now + 0.07);

      bodyGain.gain.setValueAtTime(0.4 * volume, now);
      bodyGain.gain.exponentialRampToValueAtTime(0.005, now + 0.075);

      // Connections
      noise.connect(noiseFilter);
      noiseFilter.connect(noiseGain);
      noiseGain.connect(ctx.destination);

      osc.connect(bodyGain);
      bodyGain.connect(ctx.destination);

      noise.start(now);
      osc.start(now);

      noise.stop(now + 0.04);
      osc.stop(now + 0.09);
    } catch (e) {
      // Audio context blocked
    }
  }

  public playFlip(pitch = 1.0, volume = 0.4): void {
    if (this.muted()) return;
    try {
      const ctx = this.initCtx();
      const now = ctx.currentTime;

      const osc = ctx.createOscillator();
      const gain = ctx.createGain();

      osc.type = 'sine';
      osc.frequency.setValueAtTime(250 * pitch, now);
      osc.frequency.exponentialRampToValueAtTime(750 * pitch, now + 0.15);

      gain.gain.setValueAtTime(0.01, now);
      gain.gain.linearRampToValueAtTime(0.3 * volume, now + 0.05);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.18);

      // Add a soft bandpass filter
      const filter = ctx.createBiquadFilter();
      filter.type = 'bandpass';
      filter.frequency.setValueAtTime(1000, now);
      filter.Q.setValueAtTime(1.0, now);

      osc.connect(filter);
      filter.connect(gain);
      gain.connect(ctx.destination);

      osc.start(now);
      osc.stop(now + 0.18);
    } catch (e) {
      // Audio context blocked
    }
  }

  public playLeverPull(pitch = 1.0, volume = 0.4): void {
    if (this.muted()) return;
    try {
      const ctx = this.initCtx();
      const now = ctx.currentTime;

      const osc = ctx.createOscillator();
      const gain = ctx.createGain();

      osc.type = 'sawtooth';
      osc.frequency.setValueAtTime(320 * pitch, now);
      osc.frequency.linearRampToValueAtTime(90 * pitch, now + 0.35);

      const filter = ctx.createBiquadFilter();
      filter.type = 'lowpass';
      filter.frequency.setValueAtTime(400, now);

      gain.gain.setValueAtTime(0.2 * volume, now);
      gain.gain.linearRampToValueAtTime(0.05 * volume, now + 0.2);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.36);

      osc.connect(filter);
      filter.connect(gain);
      gain.connect(ctx.destination);

      osc.start(now);
      osc.stop(now + 0.36);
    } catch (e) {
      // Audio context blocked
    }
  }

  public playSuccess(pitch = 1.0, volume = 0.4): void {
    if (this.muted()) return;
    try {
      const ctx = this.initCtx();
      const now = ctx.currentTime;

      // C5, E5, G5, C6 notes for a rising major chord arpeggio
      const notes = [523.25, 659.25, 783.99, 1046.50];
      const delays = [0, 0.08, 0.16, 0.24];

      notes.forEach((freq, idx) => {
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();

        osc.type = 'triangle';
        osc.frequency.setValueAtTime(freq * pitch, now + delays[idx]);

        gain.gain.setValueAtTime(0.001, now);
        gain.gain.linearRampToValueAtTime(0.2 * volume, now + delays[idx] + 0.02);
        gain.gain.exponentialRampToValueAtTime(0.001, now + delays[idx] + 0.35);

        osc.connect(gain);
        gain.connect(ctx.destination);

        osc.start(now + delays[idx]);
        osc.stop(now + delays[idx] + 0.4);
      });
    } catch (e) {
      // Audio context blocked
    }
  }
}
