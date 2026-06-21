import { Component, Input, Output, EventEmitter, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-cinematic-toggle',
  standalone: true,
  imports: [CommonModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => CinematicToggleComponent),
      multi: true
    }
  ],
  template: `
    <label class="toggle-wrapper">
      <input type="checkbox" [checked]="value" (change)="onToggle($event)" [disabled]="disabled">
      <span class="slider"></span>
      <span class="label-text" *ngIf="label">{{ label }}</span>
    </label>
  `,
  styles: [`
    .toggle-wrapper {
      display: inline-flex;
      align-items: center;
      gap: 0.75rem;
      cursor: pointer;
      user-select: none;
      
      input {
        display: none;
      }

      .slider {
        position: relative;
        width: 40px;
        height: 20px;
        background-color: var(--bg-tertiary);
        border: 1px solid var(--border-color);
        border-radius: 20px;
        transition: all 0.3s ease;

        &::before {
          content: "";
          position: absolute;
          width: 14px;
          height: 14px;
          border-radius: 50%;
          background-color: var(--text-muted);
          top: 2px;
          left: 2px;
          transition: all 0.3s cubic-bezier(0.68, -0.55, 0.265, 1.55);
        }
      }

      input:checked + .slider {
        background-color: rgba(226, 186, 100, 0.15); /* Sepia accent tint */
        border-color: var(--accent-sepia);

        &::before {
          transform: translateX(20px);
          background-color: var(--accent-sepia);
          box-shadow: 0 0 10px rgba(226, 186, 100, 0.5);
        }
      }

      input:disabled + .slider {
        opacity: 0.5;
        cursor: not-allowed;
      }

      .label-text {
        color: var(--text-secondary);
        font-size: 0.9rem;
        transition: color 0.2s;
      }

      &:hover input:not(:disabled) + .slider {
        border-color: var(--accent-silver);
      }
      &:hover input:not(:disabled) ~ .label-text {
        color: var(--text-primary);
      }
    }
  `]
})
export class CinematicToggleComponent implements ControlValueAccessor {
  @Input() label: string = '';
  @Input() disabled: boolean = false;
  
  public value: boolean = false;

  private onChange: (value: boolean) => void = () => {};
  private onTouched: () => void = () => {};

  public onToggle(event: Event): void {
    if (this.disabled) return;
    this.value = (event.target as HTMLInputElement).checked;
    this.onChange(this.value);
    this.onTouched();
  }

  // ControlValueAccessor implementation
  writeValue(obj: any): void {
    this.value = !!obj;
  }
  registerOnChange(fn: any): void {
    this.onChange = fn;
  }
  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }
  setDisabledState?(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }
}
