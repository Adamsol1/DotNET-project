import React, { createContext, useContext, useRef, useState } from 'react';

const AudioContext = createContext();

export function AudioProvider({ children }) {
    const backgroundMusicRef = useRef(null);
    const ambientSoundRef = useRef(null);
    const choiceAudioRef = useRef(null);
    const fadeIntervalRef = useRef(null);
    const pendingBackgroundRef = useRef(null);
    const pendingAmbientRef = useRef(null);
    const isUnlockedRef = useRef(false);

    const [currentBackgroundUrl, setCurrentBackgroundUrl] = useState(null);
    const [currentAmbientUrl, setCurrentAmbientUrl] = useState(null);
    const [isMuted, setIsMuted] = useState(false);

    const unlockAudio = () => {
        if (isUnlockedRef.current) return;
        isUnlockedRef.current = true;

        // If there were pending requests before user interaction, start them now
        if (pendingBackgroundRef.current) {
            playBackgroundMusic(pendingBackgroundRef.current);
            pendingBackgroundRef.current = null;
        }
        if (pendingAmbientRef.current) {
            playAmbientSound(pendingAmbientRef.current.url, pendingAmbientRef.current.loop);
            pendingAmbientRef.current = null;
        }
    };

    React.useEffect(() => {
        // Wait for first user gesture to satisfy autoplay policies
        const handler = () => unlockAudio();
        window.addEventListener('pointerdown', handler, { once: true });
        window.addEventListener('keydown', handler, { once: true });
        window.addEventListener('touchstart', handler, { once: true });

        return () => {
            window.removeEventListener('pointerdown', handler);
            window.removeEventListener('keydown', handler);
            window.removeEventListener('touchstart', handler);
        };
    });

    const startBackgroundMusic = async (url) => {
        // Stop existing background music and wait for it to pause
        if (backgroundMusicRef.current) {
            try {
                backgroundMusicRef.current.pause();
                await new Promise(resolve => {
                    if (backgroundMusicRef.current) {
                        backgroundMusicRef.current.addEventListener('pause', resolve, { once: true });
                        setTimeout(resolve, 100);
                    } else {
                        resolve();
                    }
                });
            } catch (err) {
                // Ignore pause errors
            }
            backgroundMusicRef.current = null;
        }

        // Play new background music
        backgroundMusicRef.current = new Audio(url);
        backgroundMusicRef.current.loop = true;
        backgroundMusicRef.current.volume = isMuted ? 0 : 0.3;
        
        try {
            await backgroundMusicRef.current.play();
        } catch (err) {
            // AbortError is expected when audio is interrupted - don't log as error
            if (err.name !== 'AbortError') {
                console.error('Error playing background music:', err);
            }
        }

        setCurrentBackgroundUrl(url);
    };

    const playBackgroundMusic = (url) => {
        // Don't change if same URL or no URL provided
        if (!url || url === currentBackgroundUrl) return;

        if (!isUnlockedRef.current) {
            // Defer until the first user gesture unlocks audio
            pendingBackgroundRef.current = url;
            return;
        }

        startBackgroundMusic(url).catch(err => {
            if (err.name !== 'AbortError') {
                console.error('Error in playBackgroundMusic:', err);
            }
        });
    };

    const playAmbientSound = async (url, loop = false, fadeDuration = 800) => {
        if (!isUnlockedRef.current) {
            // Defer ambient until unlock
            pendingAmbientRef.current = { url, loop };
            return;
        }

        // Fade out existing ambient sound
        if (ambientSoundRef.current) {
            await fadeAudio(ambientSoundRef.current, 0, fadeDuration);
            ambientSoundRef.current.pause();
            ambientSoundRef.current = null;
        }

        if (!url) {
            setCurrentAmbientUrl(null);
            return;
        }

        // Play new ambient sound with fade in
        ambientSoundRef.current = new Audio(url);
        ambientSoundRef.current.loop = loop;
        ambientSoundRef.current.volume = 0; // Start at 0

        try {
            await ambientSoundRef.current.play();
            const targetVolume = isMuted ? 0 : 0.5;
            await fadeAudio(ambientSoundRef.current, targetVolume, fadeDuration);
        } catch (err) {
            console.error('Error playing ambient sound:', err);
        }

        setCurrentAmbientUrl(url);
    };

    const fadeAudio = (audioElement, targetVolume, duration = 1000) => {
        if (!audioElement) return Promise.resolve();

        return new Promise((resolve) => {
            const startVolume = audioElement.volume;
            const volumeChange = targetVolume - startVolume;
            const startTime = Date.now();

            if (fadeIntervalRef.current) {
                clearInterval(fadeIntervalRef.current);
            }

            fadeIntervalRef.current = setInterval(() => {
                const elapsed = Date.now() - startTime;
                const progress = Math.min(elapsed / duration, 1);

                // Exponential easing for more natural sound fade
                const easedProgress = progress < 0.5
                    ? 2 * progress * progress
                    : 1 - Math.pow(-2 * progress + 2, 2) / 2;

                audioElement.volume = startVolume + (volumeChange * easedProgress);

                if (progress >= 1) {
                    clearInterval(fadeIntervalRef.current);
                    audioElement.volume = targetVolume;
                    resolve();
                }
            }, 16); // ~60fps
        });
    };

    const playChoiceAudio = async (url, fadeDuration = 400) => {
        if (!url) return;

        // Play choice audio (doesn't loop, can overlap)
        const audio = new Audio(url);
        audio.volume = 0; // Start at 0

        try {
            await audio.play();
            const targetVolume = isMuted ? 0 : 0.5;
            await fadeAudio(audio, targetVolume, fadeDuration);
        } catch (err) {
            console.error('Error playing choice audio:', err);
        }

        choiceAudioRef.current = audio;
    };
    

    const stopAllAudio = () => {
        if (backgroundMusicRef.current) {
            backgroundMusicRef.current.pause();
            backgroundMusicRef.current = null;
        }
        if (ambientSoundRef.current) {
            ambientSoundRef.current.pause();
            ambientSoundRef.current = null;
        }
        if (choiceAudioRef.current) {
            choiceAudioRef.current.pause();
            choiceAudioRef.current = null;
        }

        pendingBackgroundRef.current = null;
        pendingAmbientRef.current = null;

        setCurrentBackgroundUrl(null);
        setCurrentAmbientUrl(null);
    };

    const toggleMute = () => {
        const newMutedState = !isMuted;
        setIsMuted(newMutedState);

        // Update volume for all active audio
        if (backgroundMusicRef.current) {
            backgroundMusicRef.current.volume = newMutedState ? 0 : 0.3;
        }
        if (ambientSoundRef.current) {
            ambientSoundRef.current.volume = newMutedState ? 0 : 0.7;
        }
    };

    const setBackgroundVolume = (volume) => {
        if (backgroundMusicRef.current && !isMuted) {
            backgroundMusicRef.current.volume = Math.max(0, Math.min(1, volume));
        }
    };

    const setAmbientVolume = (volume) => {
        if (ambientSoundRef.current && !isMuted) {
            ambientSoundRef.current.volume = Math.max(0, Math.min(1, volume));
        }
    };

    const values = {
        playBackgroundMusic,
        playAmbientSound,
        playChoiceAudio,
        stopAllAudio,
        toggleMute,
        setBackgroundVolume,
        setAmbientVolume,
        currentBackgroundUrl,
        currentAmbientUrl,
        isMuted,
    };

    return (
        <AudioContext.Provider value={values}>
            {children}
        </AudioContext.Provider>
    );
}

export function useAudio() {
    const context = useContext(AudioContext);
    if (!context) {
        throw new Error('useAudio must be used within an AudioProvider');
    }
    return context;
}

export default AudioContext;
