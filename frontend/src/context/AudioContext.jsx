import React, { createContext, useContext, useRef, useState } from 'react';

const AudioContext = createContext();

export function AudioProvider({ children }) {
    const backgroundMusicRef = useRef(null);
    const ambientSoundRef = useRef(null);
    const choiceAudioRef = useRef(null);
    const fadeIntervalRef = useRef(null);

    const [currentBackgroundUrl, setCurrentBackgroundUrl] = useState(null);
    const [currentAmbientUrl, setCurrentAmbientUrl] = useState(null);
    const [isMuted, setIsMuted] = useState(false);

    const playBackgroundMusic = (url) => {
        // Don't change if same URL or no URL provided
        if (!url || url === currentBackgroundUrl) return;

        // Stop existing background music
        if (backgroundMusicRef.current) {
            backgroundMusicRef.current.pause();
            backgroundMusicRef.current = null;
        }

        // Play new background music
        backgroundMusicRef.current = new Audio(url);
        backgroundMusicRef.current.loop = true;
        backgroundMusicRef.current.volume = isMuted ? 0 : 0.3;
        backgroundMusicRef.current.play().catch(err => {
            console.error('Error playing background music:', err);
        });

        setCurrentBackgroundUrl(url);
    };

    const playAmbientSound = async (url, loop = false, fadeDuration = 800) => {
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

    const stopAmbientSound = () => {
        if (ambientSoundRef.current) {
            ambientSoundRef.current.pause();
            ambientSoundRef.current = null;
        }
        setCurrentAmbientUrl(null);
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