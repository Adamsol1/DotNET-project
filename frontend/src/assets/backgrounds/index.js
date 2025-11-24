import afterLogin from './afterLogin.png';
import airlock from './airlock.png';
import awakening from './awakening.png';
import background from './background.png';
import corridor from './corridor.png';
import encounter from './encounter.png';
import exposedWires from './exposedWires.png';
import hallway from './hallway.png';
import homePage from './homePage.png';
import homePageImage from './HomePageImage.png';
import logInImage from './LogInImage.png';
import medkit from './medkit.png';
import outsideCryopod from './outsideCryopod.png';
import planet from './planet.png';
import releaseHatch from './releaseHatch.png';
import spaceTunnel from './space-tunnel.png';
import spaceship from './spaceship.png';
import ventilationShaft from './ventilationShaft.png';

const fallbackBackground = spaceTunnel;

const backgroundLookup = {
  'assets/bg/afterLogin.png': afterLogin,
  'assets/bg/background.png': background,
  'assets/bg/homePage.png': homePage,
  'assets/bg/planet.png': planet,
  'assets/bg/space-tunnel.png': spaceTunnel,
  'assets/bg/spaceship.png': spaceship,
  'assets/backgrounds/afterLogin.png': afterLogin,
  'assets/backgrounds/background.png': background,
  'assets/backgrounds/airlock.png': airlock,
  'assets/backgrounds/awakening.png': awakening,
  'assets/backgrounds/corridor.png': corridor,
  'assets/backgrounds/encounter.png': encounter,
  'assets/backgrounds/homePage.png': homePage,
  'assets/backgrounds/hallway.png': hallway,
  'assets/backgrounds/exposedWires.png': exposedWires,
  'assets/backgrounds/medkit.png': medkit,
  'assets/backgrounds/planet.png': planet,
  'assets/backgrounds/outsideCryopod.png': outsideCryopod,
  'assets/backgrounds/releaseHatch.png': releaseHatch,
  'assets/backgrounds/space-tunnel.png': spaceTunnel,
  'assets/backgrounds/spaceship.png': spaceship,
  'assets/backgrounds/ventilationShaft.png': ventilationShaft,
  'assets/backgrounds/HomePageImage.png': homePageImage,
  'assets/backgrounds/LogInImage.png': logInImage,
  'images/backgrounds/airlock.png': airlock,
  'images/backgrounds/awakening.png': awakening,
  'images/backgrounds/corridor.png': corridor,
  'images/backgrounds/encounter.png': encounter,
  'images/backgrounds/exposedWires.png': exposedWires,
  'images/backgrounds/hallway.png': hallway,
  'images/backgrounds/maintenenceCorridor.png': corridor,
  'images/backgrounds/medkit.png': medkit,
  'images/backgrounds/outsideCryopod.png': outsideCryopod,
  'images/backgrounds/releaseHatch.png': releaseHatch,
  'images/backgrounds/ventilationShaft.png': ventilationShaft,
  'images/backgrounds/HomePageImage.png': homePageImage,
  'images/backgrounds/LogInImage.png': logInImage,
};

export function resolveBackgroundAsset(path) {
  if (!path) {
    return fallbackBackground;
  }

  if (/^https?:\/\//i.test(path)) {
    return path;
  }

  const normalized = path.trim().replace(/\\/g, '/').replace(/^\/+/, '');
  return backgroundLookup[normalized] ?? fallbackBackground;
}

export { fallbackBackground };
