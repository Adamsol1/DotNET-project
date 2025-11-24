import darius from './darius.png';
import hero from './hero.png';
import irene from './irene.png';
import narrator from './narrator.png';
import protagonist from './protagonist.png';
import systemAI from './systemAI.png';

const fallbackCharacter = hero;

const characterLookup = {
  'assets/char/hero.png': hero,
  'assets/characters/hero.png': hero,
  'assets/characters/darius.png': darius,
  'assets/characters/irene.png': irene,
  'assets/characters/narrator.png': narrator,
  'assets/characters/protagonist.png': protagonist,
  'assets/characters/systemAI.png': systemAI,
  'images/characters/darius.png': darius,
  'images/characters/irene.png': irene,
  'images/characters/narrator.png': narrator,
  'images/characters/protagonist.png': protagonist,
  'images/characters/systemAI.png': systemAI,
};

export function resolveCharacterAsset(path) {
  if (!path) {
    return fallbackCharacter;
  }

  if (/^https?:\/\//i.test(path)) {
    return path;
  }

  const normalized = path.trim().replace(/\\/g, '/').replace(/^\/+/, '');
  return characterLookup[normalized] ?? fallbackCharacter;
}

export { fallbackCharacter };
