const fs = require('fs');
const path = require('path');

const slidesDir = path.join('c:', 'Users', 'Jeotm', 'Documents', 'PersonalProjects', 'Frametric', 'frontend', 'src', 'app', 'features', 'final-cut', 'slides');

const proseMap = {
  'big-numbers-slide': "Every frame watched is a moment lived. The raw scale of your cinematic journey.",
  'prime-time-slide': "When the lights go down. Your rhythm and preferred screening times.",
  'slump-slide': "Even the greatest directors call 'cut'. The moments you stepped away from the screen.",
  'weekday-warrior-slide': "Escaping reality on a Tuesday or saving the blockbusters for the weekend.",
  'genre-landscape-slide': "The colors of your palette. The genres that defined your personal film festival.",
  'a-list-slide': "Your leading stars. The actors who captured the screen and your attention the most.",
  'auteurs-slide': "The visionaries behind the lens. The directors whose craft you couldn't look away from.",
  'dynamic-duos-slide': "Iconic collaborations. The director-actor pairings that sparked magic.",
  'best-rookies-slide': "Fresh faces and new voices. The talents you recently discovered for the very first time.",
  'generational-divide-slide': "Time traveling through cinema. How your watches spanned the decades.",
  'obsession-streak-slide': "Binge-watching masterclass. When one genre took complete control of the narrative.",
  'david-goliath-slide': "From epic sagas to brief encounters. The extremes of your runtime.",
  'cinematic-fatigue-slide': "Quality vs. Quantity. How your ratings shifted during your movie marathons.",
  'bookends-slide': "The opening sequence and the final fade to black. The Alpha and Omega of your journey.",
  'monthly-extremes-slide': "The highs and lows. The masterpieces and the box office bombs of every month.",
  'return-of-the-king-slide': "Encore performances. The stories you just had to experience one more time.",
  'hall-of-fame-slide': "Five stars. Pure cinema. The absolute masterpieces that left a lasting mark.",
  'golden-raspberry-slide': "The cutting room floor. The films that completely missed the mark.",
  'intro-slide': "Grab your popcorn and take a seat. The show is about to begin."
};

function processDir(dir) {
  const files = fs.readdirSync(dir);
  for (const f of files) {
    const fullPath = path.join(dir, f);
    if (fs.statSync(fullPath).isDirectory()) {
      processDir(fullPath);
    } else if (f.endsWith('.ts') && f !== 'summary-slide.ts') {
      let content = fs.readFileSync(fullPath, 'utf8');
      
      // Fix @Input() year
      content = content.replace(/@Input\(\)\s+year!:\s+number;/g, "@Input() year!: number | 'global';");

      // Replace {{ year }} in template to handle 'global'
      content = content.replace(/\{\{\s*year\s*\}\}/g, "{{ year === 'global' ? 'All-Time' : year }}");
      
      const folderName = path.basename(dir);
      if (proseMap[folderName]) {
        const prose = proseMap[folderName];
        // Inject <p class="slide-explainer"> after slide-title or slide-subtitle
        if (!content.includes('slide-explainer')) {
           if (content.includes('class="slide-subtitle"')) {
             content = content.replace(/(class="slide-subtitle"[^>]*>.*?<\/(p|div)>)/s, "$1\n      <p class=\"slide-explainer\">" + prose + "</p>");
           } else if (content.includes('class="slide-title"')) {
             content = content.replace(/(class="slide-title"[^>]*>.*?<\/(h2|div)>)/s, "$1\n      <p class=\"slide-explainer\">" + prose + "</p>");
           }
        }
      }
      
      // Inject CSS for slide-explainer if not there
      if (content.includes('slide-explainer') && !content.includes('.slide-explainer')) {
         content = content.replace(/styles:\s*\[`/, "styles: [`\n    .slide-explainer {\n      font-size: 0.95rem;\n      color: rgba(255,255,255,0.7);\n      margin-bottom: 32px;\n      font-style: italic;\n      max-width: 600px;\n      text-align: center;\n    }");
      }
      
      fs.writeFileSync(fullPath, content);
      console.log('Updated ' + fullPath);
    }
  }
}

processDir(slidesDir);
