import { Injectable } from '@angular/core';

export interface EasterEggResult {
  text: string;
  tooltip: string;
  className?: string;
}

@Injectable({
  providedIn: 'root',
})
export class EasterEggService {
  private cache = new Map<string, EasterEggResult | null>();

  public getEasterEgg(value: any, context: string): EasterEggResult | null {
    if (value === null || value === undefined) return null;
    
    // Normalize value to a string key
    const valStr = String(value).trim();
    const cacheKey = `${context}:${valStr}`;
    
    if (this.cache.has(cacheKey)) {
      return this.cache.get(cacheKey)!;
    }
    
    const result = this.rollEasterEgg(valStr, context);
    this.cache.set(cacheKey, result);
    return result;
  }

  public getLoadingMessage(): string {
    const messages = [
      'Calibrating cinematic vectors...',
      'Popcorn machine warming up...',
      'Rewinding VHS tapes...',
      'Projector bulb warming up...',
      'Decrypting your current obsessions...',
      'Bribing the Academy voters...',
      'Fetching coffee for the director...',
      'Running simulation of a 10-star movie...',
      'One does not simply walk into Mordor... or watch a 4-hour director\'s cut without a bathroom break. 💍',
      'Wait, it\'s all Nolan? Always has been. 🔫👩‍🚀',
      'This is fine. 🔥 (Me watching a devastating 3-hour indie drama at 2 AM)',
      'Am I a joke to you? (Your watchlist, when you generate Discover recommendations instead)',
      '*Visible Confusion* (When the Marvel movie ends and there is no post-credits scene)',
      'Distracted Boyfriend: Me looking at brand new releases instead of my 200+ movie watchlist.'
    ];
    // Roll with 2% probability to show a custom message, otherwise return standard loading
    if (Math.random() < 0.02) {
      const idx = Math.floor(Math.random() * messages.length);
      return messages[idx];
    }
    return '';
  }

  private rollEasterEgg(val: string, context?: string): EasterEggResult | null {
    const num = parseFloat(val);
    const hasNum = !isNaN(num);
    const rand = Math.random() * 100;

    if (context === 'table-year') {
      if (val === '2001' || (hasNum && num === 2001)) {
        if (rand < 1) {
          return {
            text: "(I'm sorry, Dave 🔴)",
            tooltip: '2001: "I\'m afraid I can\'t let you watch that, Dave." (1% trigger rate)',
            className: 'space-badge'
          };
        }
      }
      if (val === '1985' || val === '1955' || (hasNum && (num === 1985 || num === 1955))) {
        if (rand < 1) {
          return {
            text: '(Doc! ⚡)',
            tooltip: 'Back to the Future: "Great Scott! We are going back to the future!" (1% trigger rate)',
            className: 'future-badge'
          };
        }
      }
      return null;
    }

    if (context === 'rec-year') {
      // Silent-film era (<= 1920) — 2% chance
      if (hasNum && num <= 1920) {
        if (rand < 2) {
          return {
            text: '🎥',
            tooltip: 'A silent film! Make sure to put on your monocle and complain about how "talkies" ruined the purity of the medium. (2% trigger rate)',
            className: 'silent-film-warn'
          };
        }
        return null;
      }
      // 2001: HAL — 1% chance
      if (hasNum && num === 2001) {
        if (rand < 1) {
          return {
            text: "(I'm sorry, Dave 🔴)",
            tooltip: '2001: "I\'m afraid I can\'t let you watch that, Dave." (1% trigger rate)',
            className: 'space-badge'
          };
        }
      }
      // 1985 / 1955: Back to the Future — 1% chance
      if (hasNum && (num === 1985 || num === 1955)) {
        if (rand < 1) {
          return {
            text: '(Doc! ⚡)',
            tooltip: 'Back to the Future: "Great Scott! We are going back to the future!" (1% trigger rate)',
            className: 'future-badge'
          };
        }
      }
      return null;
    }

    // Mid-Curve Curse (2.5 / 5.0) - Probability 1%
    if (val === '2.5' || val === '5.0' || (hasNum && (num === 2.5 || num === 5.0))) {
      if (rand < 1) {
        return {
          text: '(Centrist ⚠️)',
          tooltip: 'Ah, the ultimate centrist. Not good enough to love, not bad enough to hate. Indifference is the real killer here. (1% trigger rate)',
          className: 'mid-curve-warn'
        };
      }
      return null;
    }

    // 1. Nice (69 / 6.9) - Probability 1%
    if (val === '69' || val === '6.9' || (hasNum && (num === 69 || num === 6.9))) {
      if (rand < 1) {
        return {
          text: '(Nice)',
          tooltip: '69: The legendary internet number. You hit a 69/6.9 in your stats/recommendations! (1% trigger rate)',
          className: 'nice-badge'
        };
      }
      return null;
    }

    // 2. Old for this (67 / 6.7) - Probability 1%
    if (val === '67' || val === '6.7' || (hasNum && (num === 67 || num === 6.7))) {
      if (rand < 1) {
        return {
          text: "(I'm too old for this)",
          tooltip: "SIX SEVEN SIX SEV... I'm too old for this (67/6.7: Danny Glover's iconic line. 1% trigger rate)",
          className: 'old-badge'
        };
      }
      return null;
    }

    // 3. 420 (Blaze it) - Probability 1%
    if (val === '420' || (hasNum && num === 420)) {
      if (rand < 1) {
        return {
          text: '(Blaze it 🌿)',
          tooltip: '420: The universal symbol. Grab some "snacks" for this run! (1% trigger rate)',
          className: 'weed-badge'
        };
      }
      return null;
    }

    // 4. 10 / 10.0 (Masterpiece) - Probability 1%
    if (val === '10' || val === '10.0' || (hasNum && num === 10)) {
      if (rand < 1) {
        return {
          text: '(Masterpiece?)',
          tooltip: '10/10: A perfect score! But is anything truly perfect? (1% trigger rate)',
          className: 'masterpiece-badge'
        };
      }
      return null;
    }

    // 5. 1 / 1.0 (True Cinema 🗑️) - Probability 1%
    if (val === '1' || val === '1.0' || (hasNum && num === 1)) {
      if (rand < 1) {
        return {
          text: '(True Cinema 🗑️)',
          tooltip: '1/10: Certified garbage or misunderstood avant-garde genius? (1% trigger rate)',
          className: 'trash-badge'
        };
      }
      return null;
    }

    // 6. 180 (Intermission when?) - Probability 1%
    if (val === '180' || (hasNum && num === 180)) {
      if (rand < 1) {
        return {
          text: '(Intermission when?)',
          tooltip: '180m: Exactly three hours. Prepare your bladder! (1% trigger rate)',
          className: 'long-badge'
        };
      }
      return null;
    }

    // 7. 300 (Sparta) - Probability 1%
    if (val === '300' || (hasNum && num === 300)) {
      if (rand < 1) {
        return {
          text: '(THIS IS SPARTA!)',
          tooltip: '300: Prepare for glory! (1% trigger rate)',
          className: 'sparta-badge'
        };
      }
      return null;
    }

    // 8. 500 (500 Days of Cinema) - Probability 1%
    if (val === '500' || (hasNum && num === 500)) {
      if (rand < 1) {
        return {
          text: '(500 Days of Cinema)',
          tooltip: '500 watches: A milestone! Halfway to a thousand. (1% trigger rate)',
          className: 'milestone-badge'
        };
      }
      return null;
    }

    // 9. 1000 (Absolute Cinephile) - Probability 1%
    if (val === '1000' || (hasNum && num === 1000)) {
      if (rand < 1) {
        return {
          text: '(Absolute Cinephile 🏆)',
          tooltip: '1000 watches: You have spent weeks of your life staring at screens. (1% trigger rate)',
          className: 'legend-badge'
        };
      }
      return null;
    }

    // 10. 88 / 8.8 (Back to the Future) - Probability 1%
    if (val === '88' || val === '8.8' || (hasNum && (num === 88 || num === 8.8))) {
      if (rand < 1) {
        return {
          text: '(Doc! ⚡)',
          tooltip: '88: "Great Scott! We are going back to the future!" (1% trigger rate)',
          className: 'future-badge'
        };
      }
      return null;
    }

    // 11. 1.21 (Gigawatts BTTF) - Probability 1%
    if (val === '1.21' || (hasNum && num === 1.21)) {
      if (rand < 1) {
        return {
          text: '(1.21 Gigawatts! ⚡)',
          tooltip: '1.21 Gigawatts: "What the hell is a gigawatt?!" (1% trigger rate)',
          className: 'future-badge'
        };
      }
      return null;
    }

    // 12. 2001 (Space Odyssey Hal) - Probability 1%
    if (val === '2001' || (hasNum && num === 2001)) {
      if (rand < 1) {
        return {
          text: "(I'm sorry, Dave 🔴)",
          tooltip: '2001: "I\'m afraid I can\'t let you watch that, Dave." (1% trigger rate)',
          className: 'space-badge'
        };
      }
      return null;
    }

    // 13. 24 / 2.4 (Godard FPS Cinema is Truth) - Probability 1%
    if (val === '24' || val === '2.4' || (hasNum && (num === 24 || num === 2.4))) {
      if (rand < 1) {
        return {
          text: '(Cinema is Truth 🎥)',
          tooltip: '24: "Cinema is truth 24 frames-per-second." - Jean-Luc Godard. (1% trigger rate)',
          className: 'godard-badge'
        };
      }
      return null;
    }

    // 14. 0 / 0.0 (Rotten Tomatoes Gotti 0% score) - Probability 1%
    if (val === '0' || val === '0.0' || (hasNum && num === 0)) {
      if (rand < 1) {
        return {
          text: '(Literally Gotti 🍅)',
          tooltip: '0%: Audience loved it, critics put out a hit. (1% trigger rate)',
          className: 'gotti-badge'
        };
      }
      return null;
    }

    // 15. 7 / 7.0 (Se7en "What's in the box?!") - Probability 1%
    if (val === '7' || val === '7.0' || (hasNum && (num === 7 || num === 7.0))) {
      if (rand < 1) {
        return {
          text: "(What's in the box? 📦)",
          tooltip: '7: "Oh, what\'s in the box?!" (1% trigger rate)',
          className: 'seven-badge'
        };
      }
      return null;
    }

    // 16. 42 / 4.2 (Hitchhiker's Guide to the Galaxy) - Probability 1%
    if (val === '42' || val === '4.2' || (hasNum && (num === 42 || num === 4.2))) {
      if (rand < 1) {
        return {
          text: '(The Answer to Life 🌌)',
          tooltip: '42: The Answer to the Ultimate Question of Life, the Universe, and Everything. (1% trigger rate)',
          className: 'galaxy-badge'
        };
      }
      return null;
    }

    const lowerVal = val.toLowerCase();

    // Stats idle state meme
    if (lowerVal === 'stats-idle') {
      if (rand < 15) {
        return {
          text: "No statistics are loaded yet. (One does not simply analyze metrics without running a query first 💍)",
          tooltip: 'One does not simply walk into Mordor... or view stats without querying. (15% trigger rate)',
          className: 'boromir-badge'
        };
      }
      return null;
    }

    // Recommendations empty state meme
    if (lowerVal === 'reco-empty') {
      if (rand < 15) {
        return {
          text: "No recommendations match your criteria. (Impossible. Perhaps the archives are incomplete)",
          tooltip: 'Obi-Wan Kenobi when a planet is missing from the Jedi Archives. (15% trigger rate)',
          className: 'archives-incomplete-badge'
        };
      }
      return null;
    }

    // Auth tagline meme
    if (lowerVal === 'auth-tagline') {
      if (rand < 15) {
        return {
          text: "This is fine. 🔥 (Analyzing your Letterboxd obsession)",
          tooltip: 'This is fine. (15% trigger rate)',
          className: 'fine-badge'
        };
      }
      return null;
    }

    // Watchlist trigger "Am I a joke to you?"
    if (lowerVal === 'watchlist') {
      if (rand < 15) {
        return {
          text: '(Am I a joke to you? 👁️)',
          tooltip: 'Your watchlist when you choose to discover new movies instead of clearing it. (15% trigger rate)',
          className: 'watchlist-joke-badge'
        };
      }
      return null;
    }

    // Nolan astronaut meme
    if (lowerVal.includes('nolan')) {
      if (rand < 10) {
        return {
          text: '(Always has been 🔫)',
          tooltip: 'Wait, it\'s all Nolan? Always has been. (10% trigger rate)',
          className: 'nolan-astro-badge'
        };
      }
    }

    // Epic / Long movie meme
    if (hasNum && num >= 180) {
      if (rand < 10) {
        return {
          text: '(One does not simply... 💍)',
          tooltip: 'One does not simply walk into Mordor... or watch a 3+ hour epic without a bathroom break. (10% trigger rate)',
          className: 'boromir-badge'
        };
      }
    }

    // 17. Nicolas Cage
    if (lowerVal.includes('nicolas cage')) {
      if (rand < 10) {
        return {
          text: '(Not the bees! 🐝)',
          tooltip: 'Nicolas Cage: "NOT THE BEES! NOT THE BEES!" (10% trigger rate)',
          className: 'cage-badge'
        };
      }
      return null;
    }

    // 18. Ryan Gosling
    if (lowerVal.includes('ryan gosling')) {
      if (rand < 10) {
        return {
          text: "(He's literally me 🕶️)",
          tooltip: 'Ryan Gosling: The ultimate "literally me" character driver. (10% trigger rate)',
          className: 'gosling-badge'
        };
      }
      return null;
    }

    // 19. Keanu Reeves
    if (lowerVal.includes('keanu reeves')) {
      if (rand < 10) {
        return {
          text: "(You're breathtaking! 👉)",
          tooltip: 'Keanu Reeves: "You\'re breathtaking!" (10% trigger rate)',
          className: 'keanu-badge'
        };
      }
      return null;
    }

    // 20. Christopher Nolan
    if (lowerVal.includes('christopher nolan')) {
      if (rand < 10) {
        return {
          text: '(BWAAAAM 🔊)',
          tooltip: 'Christopher Nolan: Loud horns and complex temporal paradoxes. (10% trigger rate)',
          className: 'nolan-badge'
        };
      }
      return null;
    }

    // 21. Quentin Tarantino
    if (lowerVal.includes('quentin tarantino')) {
      if (rand < 10) {
        return {
          text: '(Feet Close-up 🦶)',
          tooltip: 'Quentin Tarantino: Famous for foot shots, trunk angles, and intense dialog. (10% trigger rate)',
          className: 'tarantino-badge'
        };
      }
      return null;
    }

    // 22. Martin Scorsese
    if (lowerVal.includes('martin scorsese')) {
      if (rand < 10) {
        return {
          text: '(This is cinema 🍿)',
          tooltip: 'Martin Scorsese: "This is cinema. It is not theme parks." (10% trigger rate)',
          className: 'scorsese-badge'
        };
      }
      return null;
    }

    // 23. Leonardo DiCaprio
    if (lowerVal.includes('leonardo dicaprio')) {
      if (rand < 10) {
        return {
          text: '(Pointing at screen 👉)',
          tooltip: 'Leonardo DiCaprio: *Points at screen recognizing cinematic details* (10% trigger rate)',
          className: 'dicaprio-badge'
        };
      }
      return null;
    }

    // 24. Zack Snyder
    if (lowerVal.includes('zack snyder')) {
      if (rand < 10) {
        return {
          text: '(Slow-mo active ⏱️)',
          tooltip: 'Zack Snyder: Expect high-contrast colors and heavy slow-motion. (10% trigger rate)',
          className: 'snyder-badge'
        };
      }
      return null;
    }

    // 25. Sci-Fi genre
    if (lowerVal === 'sci-fi' || lowerVal === 'science fiction') {
      if (rand < 10) {
        return {
          text: '(Beam me up 🛸)',
          tooltip: 'Sci-Fi: Space, aliens, and neon lights. (10% trigger rate)',
          className: 'scifi-badge'
        };
      }
      return null;
    }

    // 26. Horror genre
    if (lowerVal === 'horror') {
      if (rand < 10) {
        return {
          text: '(Don\'t look behind you 👻)',
          tooltip: 'Horror: A spooky palette cleanser. (10% trigger rate)',
          className: 'horror-badge'
        };
      }
      return null;
    }

    return null;
  }
}
