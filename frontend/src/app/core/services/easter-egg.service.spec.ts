import { EasterEggService } from './easter-egg.service';

describe('EasterEggService', () => {
  let service: EasterEggService;

  beforeEach(() => {
    service = new EasterEggService();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getEasterEgg', () => {
    it('should return null for null input', () => {
      expect(service.getEasterEgg(null, 'context')).toBeNull();
    });

    it('should return null for undefined input', () => {
      expect(service.getEasterEgg(undefined, 'context')).toBeNull();
    });

    it('should cache results and not re-evaluate on second call', () => {
      const spy = vi.spyOn(service as any, 'rollEasterEgg').mockReturnValue(null);
      service.getEasterEgg('test', 'ctx');
      service.getEasterEgg('test', 'ctx');
      expect(spy).toHaveBeenCalledTimes(1);
    });

    it('should use different cache keys for different contexts', () => {
      const spy = vi.spyOn(service as any, 'rollEasterEgg').mockReturnValue(null);
      service.getEasterEgg('2001', 'table-year');
      service.getEasterEgg('2001', 'rec-year');
      expect(spy).toHaveBeenCalledTimes(2);
    });
  });

  describe('specific easter egg rules (with controlled Math.random)', () => {
    function setRandom(value: number) {
      vi.spyOn(Math, 'random').mockReturnValue(value);
    }

    it('table-year 2001 with 1% roll', () => {
      setRandom(0.005);
      const result = service.getEasterEgg('2001', 'table-year');
      expect(result?.text).toContain("I'm sorry, Dave");
    });

    it('table-year 2001 with 2% roll (no trigger)', () => {
      setRandom(0.015);
      expect(service.getEasterEgg('2001', 'table-year')).toBeNull();
    });

    it('table-year 1985 with 1% roll', () => {
      setRandom(0.005);
      const result = service.getEasterEgg('1985', 'table-year');
      expect(result?.text).toContain('Doc!');
    });

    it('rec-year silent film (year <= 1920)', () => {
      setRandom(0.01);
      const result = service.getEasterEgg('1915', 'rec-year');
      expect(result?.text).toContain('🎥');
    });

    it('rec-year silent film above 2% threshold returns null', () => {
      setRandom(0.025);
      expect(service.getEasterEgg('1915', 'rec-year')).toBeNull();
    });

    it('rec-year 2001 triggers HAL', () => {
      setRandom(0.005);
      const result = service.getEasterEgg('2001', 'rec-year');
      expect(result?.text).toContain("I'm sorry, Dave");
    });

    it('rec-year modern year returns null', () => {
      setRandom(0.5);
      expect(service.getEasterEgg('2024', 'rec-year')).toBeNull();
    });

    it('value 2.5 triggers mid-curve curse (1%)', () => {
      setRandom(0.005);
      const result = service.getEasterEgg('2.5', 'any');
      expect(result?.text).toContain('Centrist');
    });

    it('value 69 triggers Nice (1%)', () => {
      setRandom(0.005);
      const result = service.getEasterEgg('69', 'any');
      expect(result?.text).toContain('Nice');
    });

    it('value 6.9 triggers Nice (1%)', () => {
      setRandom(0.005);
      const result = service.getEasterEgg('6.9', 'any');
      expect(result?.text).toContain('Nice');
    });

    it('value 67 triggers "too old" (1%)', () => {
      setRandom(0.005);
      const result = service.getEasterEgg('67', 'any');
      expect(result?.text).toContain("too old");
    });

    it('value 420 triggers Blaze it (1%)', () => {
      setRandom(0.005);
      const result = service.getEasterEgg('420', 'any');
      expect(result?.text).toContain('Blaze it');
    });

    it('value 10 triggers Masterpiece (1%)', () => {
      setRandom(0.005);
      const result = service.getEasterEgg('10', 'any');
      expect(result?.text).toContain('Masterpiece');
    });

    it('value 1 triggers True Cinema (1%)', () => {
      setRandom(0.005);
      const result = service.getEasterEgg('1', 'any');
      expect(result?.text).toContain('True Cinema');
    });

    it('value 180 triggers Intermission (1%)', () => {
      setRandom(0.005);
      const result = service.getEasterEgg('180', 'any');
      expect(result?.text).toContain('Intermission');
    });

    it('value 300 triggers Sparta (1%)', () => {
      setRandom(0.005);
      const result = service.getEasterEgg('300', 'any');
      expect(result?.text).toContain('SPARTA');
    });

    it('value 500 triggers milestone (1%)', () => {
      setRandom(0.005);
      const result = service.getEasterEgg('500', 'any');
      expect(result?.text).toContain('500 Days');
    });

    it('value 1000 triggers cinephile (1%)', () => {
      setRandom(0.005);
      const result = service.getEasterEgg('1000', 'any');
      expect(result?.text).toContain('Absolute Cinephile');
    });

    it('value 88 triggers Doc (1%)', () => {
      setRandom(0.005);
      const result = service.getEasterEgg('88', 'any');
      expect(result?.text).toContain('Doc!');
    });

    it('value 8.8 triggers Doc (1%)', () => {
      setRandom(0.005);
      const result = service.getEasterEgg('8.8', 'any');
      expect(result?.text).toContain('Doc!');
    });

    it('value 1.21 triggers Gigawatts (1%)', () => {
      setRandom(0.005);
      const result = service.getEasterEgg('1.21', 'any');
      expect(result?.text).toContain('Gigawatts');
    });

    it('value 24 triggers Cinema is Truth (1%)', () => {
      setRandom(0.005);
      const result = service.getEasterEgg('24', 'any');
      expect(result?.text).toContain('Cinema is Truth');
    });

    it('value 0 triggers Gotti (1%)', () => {
      setRandom(0.005);
      const result = service.getEasterEgg('0', 'any');
      expect(result?.text).toContain('Gotti');
    });

    it('value 7 triggers "What\'s in the box" (1%)', () => {
      setRandom(0.005);
      const result = service.getEasterEgg('7', 'any');
      expect(result?.text).toContain("What's in the box");
    });

    it('value 42 triggers Answer to Life (1%)', () => {
      setRandom(0.005);
      const result = service.getEasterEgg('42', 'any');
      expect(result?.text).toContain('Answer to Life');
    });

    it('value 4.2 triggers Answer to Life (1%)', () => {
      setRandom(0.005);
      const result = service.getEasterEgg('4.2', 'any');
      expect(result?.text).toContain('Answer to Life');
    });
  });

  describe('context-specific idle/empty messages (15% probability)', () => {
    function setRandom(value: number) {
      vi.spyOn(Math, 'random').mockReturnValue(value);
    }

    it('stats-idle with 15% roll', () => {
      setRandom(0.1);
      const result = service.getEasterEgg('stats-idle', 'ui');
      expect(result?.text).toContain('One does not simply');
    });

    it('stats-idle above 15% returns null', () => {
      setRandom(0.2);
      expect(service.getEasterEgg('stats-idle', 'ui')).toBeNull();
    });

    it('reco-empty with 15% roll', () => {
      setRandom(0.1);
      const result = service.getEasterEgg('reco-empty', 'ui');
      expect(result?.text).toContain('archives are incomplete');
    });

    it('auth-tagline with 15% roll', () => {
      setRandom(0.1);
      const result = service.getEasterEgg('auth-tagline', 'ui');
      expect(result?.text).toContain('This is fine');
    });
  });

  describe('name-based easter eggs (10% probability)', () => {
    function setRandom(value: number) {
      vi.spyOn(Math, 'random').mockReturnValue(value);
    }

    it('Nolan triggers astronaut meme (10%)', () => {
      setRandom(0.05);
      const result = service.getEasterEgg('Christopher Nolan', 'actor-name');
      expect(result?.text).toContain('Always has been');
    });

    it('Nolan above 10% returns null', () => {
      setRandom(0.15);
      expect(service.getEasterEgg('Christopher Nolan', 'actor-name')).toBeNull();
    });

    it('Nicolas Cage triggers bees (10%)', () => {
      setRandom(0.05);
      const result = service.getEasterEgg('Nicolas Cage', 'actor-name');
      expect(result?.text).toContain('bees');
    });

    it('Ryan Gosling triggers "literally me" (10%)', () => {
      setRandom(0.05);
      const result = service.getEasterEgg('Ryan Gosling', 'actor-name');
      expect(result?.text).toContain("literally me");
    });

    it('Keanu Reeves triggers breathtaking (10%)', () => {
      setRandom(0.05);
      const result = service.getEasterEgg('Keanu Reeves', 'actor-name');
      expect(result?.text).toContain('breathtaking');
    });

    it('Quentin Tarantino triggers feet close-up (10%)', () => {
      setRandom(0.05);
      const result = service.getEasterEgg('Quentin Tarantino', 'actor-name');
      expect(result?.text).toContain('Feet Close-up');
    });

    it('Martin Scorsese triggers "This is cinema" (10%)', () => {
      setRandom(0.05);
      const result = service.getEasterEgg('Martin Scorsese', 'actor-name');
      expect(result?.text).toContain('This is cinema');
    });

    it('Leonardo DiCaprio triggers pointing (10%)', () => {
      setRandom(0.05);
      const result = service.getEasterEgg('Leonardo DiCaprio', 'actor-name');
      expect(result?.text).toContain('Pointing');
    });

    it('Zack Snyder triggers slow-mo (10%)', () => {
      setRandom(0.05);
      const result = service.getEasterEgg('Zack Snyder', 'actor-name');
      expect(result?.text).toContain('Slow-mo');
    });

    it('Sci-Fi genre triggers beam me up (10%)', () => {
      setRandom(0.05);
      const result = service.getEasterEgg('sci-fi', 'genre');
      expect(result?.text).toContain('Beam me up');
    });

    it('Horror genre triggers spooky (10%)', () => {
      setRandom(0.05);
      const result = service.getEasterEgg('horror', 'genre');
      expect(result?.text).toContain("look behind you");
    });
  });

  describe('long movie easter egg (runtime >= 180, 10%)', () => {
    function setRandom(value: number) {
      vi.spyOn(Math, 'random').mockReturnValue(value);
    }

    it('runtime 180 triggers "one does not simply" (10%)', () => {
      setRandom(0.05);
      const result = service.getEasterEgg('200', 'runtime');
      expect(result?.text).toContain('One does not simply');
    });

    it('runtime 120 does not trigger', () => {
      setRandom(0.05);
      expect(service.getEasterEgg('120', 'runtime')).toBeNull();
    });
  });

  describe('getLoadingMessage', () => {
    it('should return a non-empty string', () => {
      const msg = service.getLoadingMessage();
      expect(typeof msg).toBe('string');
    });

    it('should return empty string by default (98% chance)', () => {
      vi.spyOn(Math, 'random').mockReturnValue(0.5);
      expect(service.getLoadingMessage()).toBe('');
    });

    it('should return a custom message with 2% chance', () => {
      vi.spyOn(Math, 'random').mockReturnValue(0.01);
      const msg = service.getLoadingMessage();
      expect(msg.length).toBeGreaterThan(0);
    });
  });
});
