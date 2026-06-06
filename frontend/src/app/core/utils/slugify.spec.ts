import { slugify } from './slugify';

describe('slugify', () => {
  it('should return empty string for empty input', () => {
    expect(slugify('')).toBe('');
  });

  it('should handle nullish input gracefully', () => {
    expect(slugify(null as unknown as string)).toBe('');
    expect(slugify(undefined as unknown as string)).toBe('');
  });

  it('should convert to lowercase', () => {
    expect(slugify('HELLO')).toBe('hello');
  });

  it('should replace spaces with hyphens', () => {
    expect(slugify('hello world')).toBe('hello-world');
  });

  it('should remove accents', () => {
    expect(slugify('José')).toBe('jose');
    expect(slugify('Málaga')).toBe('malaga');
    expect(slugify('François')).toBe('francois');
  });

  it('should remove non-word characters', () => {
    expect(slugify('hello!world?')).toBe('helloworld');
    expect(slugify('foo@bar')).toBe('foobar');
  });

  it('should collapse multiple hyphens', () => {
    expect(slugify('hello   world')).toBe('hello-world');
    expect(slugify('hello---world')).toBe('hello-world');
  });

  it('should trim leading and trailing hyphens', () => {
    expect(slugify('--hello-world--')).toBe('hello-world');
  });

  it('should handle real-world movie titles', () => {
    expect(slugify('The Lord of the Rings: The Return of the King')).toBe('the-lord-of-the-rings-the-return-of-the-king');
    expect(slugify('Spider-Man: No Way Home')).toBe('spider-man-no-way-home');
    expect(slugify('Everything Everywhere All at Once')).toBe('everything-everywhere-all-at-once');
  });
});
