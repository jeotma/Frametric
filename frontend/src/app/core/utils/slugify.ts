export function slugify(text: string): string {
  if (!text) return '';
  return text
    .toString()
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '') // Remove accents/diacritics
    .replace(/\s+/g, '-')           // Replace spaces with hyphens
    .replace(/[^\w\-]+/g, '')       // Remove non-word characters
    .replace(/\-\-+/g, '-')         // Replace multiple hyphens with single
    .replace(/^-+/, '')             // Trim leading hyphens
    .replace(/-+$/, '');            // Trim trailing hyphens
}
