import { EasterEggPipe } from './easter-egg.pipe';
import { EasterEggService } from './easter-egg.service';

describe('EasterEggPipe', () => {
  let pipe: EasterEggPipe;
  let mockService: jasmine.SpyObj<EasterEggService>;

  beforeEach(() => {
    mockService = jasmine.createSpyObj('EasterEggService', ['getEasterEgg']);
    pipe = new EasterEggPipe(mockService);
  });

  it('should be created', () => {
    expect(pipe).toBeTruthy();
  });

  it('should delegate to EasterEggService.getEasterEgg', () => {
    const result = { text: '🎬', className: 'easter-egg', tooltip: 'lucky!' };
    mockService.getEasterEgg.and.returnValue(result);

    const output = pipe.transform('test-value', 'actor-name');
    expect(mockService.getEasterEgg).toHaveBeenCalledWith('test-value', 'actor-name');
    expect(output).toBe(result);
  });

  it('should return null when service returns null', () => {
    mockService.getEasterEgg.and.returnValue(null);
    expect(pipe.transform('anything', 'context')).toBeNull();
  });

  it('should work with numeric values', () => {
    mockService.getEasterEgg.and.returnValue({ text: '42!', className: '', tooltip: '' });
    const output = pipe.transform(42, 'rating');
    expect(mockService.getEasterEgg).toHaveBeenCalledWith(42, 'rating');
    expect(output).toEqual({ text: '42!', className: '', tooltip: '' });
  });
});
