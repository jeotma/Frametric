import { Pipe, PipeTransform, inject } from '@angular/core';
import { EasterEggService, EasterEggResult } from './easter-egg.service';

@Pipe({
  name: 'easterEgg',
  standalone: true,
  pure: true
})
export class EasterEggPipe implements PipeTransform {
  private readonly service = inject(EasterEggService);

  transform(value: any, context: string): EasterEggResult | null {
    return this.service.getEasterEgg(value, context);
  }
}
