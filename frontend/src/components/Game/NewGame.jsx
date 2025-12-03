import React, { useState, useEffect, useCallback } from 'react';
import { useGame } from '../../context/GameContext';
import { useAudio } from '../../context/AudioContext';
import { motion } from 'framer-motion';
import { useAuth } from '../../context/Authentication';

export function StartGame({ onGameStart, onBack }) {
    const [formData, setFormData] = useState({
        saveName: ''
    });
    const [errors, setErrors] = useState({});
    const [saveCount, setSaveCount] = useState(0);
    const [isLoadingSaveCount, setIsLoadingSaveCount] = useState(true);

    const { startGame, loading, error, clearError, getAllSaves } = useGame();
    const { playBackgroundMusic } = useAudio();
    const { user } = useAuth();

    // Function to count user's saves
  const countUserSaves = useCallback(async () => {
    try {
        setIsLoadingSaveCount(true);
        const userId = Number(localStorage.getItem('user_id'));
        
        // Use the existing getAllSaves function from GameContext
        const saves = await getAllSaves(userId);
        
        console.log('[StartGame] User saves:', saves);
        
        const count = saves ? saves.length : 0;
        setSaveCount(count);
        return count;
    } catch (error) {
        console.error('Failed to count saves:', error);
        return 0;
    } finally {
        setIsLoadingSaveCount(false);
    }
    }, [getAllSaves]);

    // Count saves when component mounts
    useEffect(() => {
        playBackgroundMusic('/assets/audio/menu-music.mp3');
        countUserSaves();

        return () => {
            // Don't stop audio here - let it continue to the game
        };
    }, [playBackgroundMusic, countUserSaves]);

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: value
        }));
        if (errors[name]) {
            setErrors(prev => ({
                ...prev,
                [name]: ''
            }));
        }
    };

        const handleSubmit = async (e) => {
        e.preventDefault();
        console.log("gamecontext user in startgame:", user)
        setErrors({});
        clearError();

        const newErrors = {};
        if (!formData.saveName.trim()) {
            newErrors.saveName = 'Save name is required';
        }

        if (Object.keys(newErrors).length > 0) {
            setErrors(newErrors);
            return;
        }

        // Check save limit before starting game
        console.log('[StartGame] Checking save count...');
        const currentSaveCount = await countUserSaves();
        console.log('[StartGame] Current save count:', currentSaveCount);
        
        if (currentSaveCount >= 3) {
            setErrors({
                saveName: 'Maximum 3 saves reached. Please delete an existing save before creating a new one.'
            });
            return;
        }

        try {
            console.log('[StartGame] Creating new save...');
            const gameSave = await startGame({
                UserId: Number(localStorage.getItem('user_id')),
                SaveName: formData.saveName
            });
            console.log('[StartGame] Save created:', gameSave);
            
            // Recount after creating save
            const newCount = await countUserSaves();
            console.log('[StartGame] New save count:', newCount);
            
            if (onGameStart) {
                onGameStart(gameSave);
            }
        } catch (error) {
            console.error('Failed to start game:', error);
            const errorMessage = error.response?.data?.message || error.response?.data || error.message || 'Failed to start game';
            setErrors({
                saveName: typeof errorMessage === 'string' ? errorMessage : 'Failed to start game'
            });
        }
    };

    return (
        <div
            className="min-h-screen text-white font-mono relative overflow-hidden bg-cover bg-center bg-no-repeat"
            style={{
                backgroundImage: `url('/assets/backgrounds/afterLogin.png')`,
            }}
        >
            <div className="relative z-10 min-h-screen flex flex-col items-center justify-center p-6">
                <motion.div
                    initial={{ opacity: 0, scale: 0.9 }}
                    animate={{ opacity: 1, scale: 1 }}
                    transition={{ duration: 0.5 }}
                    className="w-full max-w-md bg-gray-800 bg-opacity-90 p-8 rounded-lg border-2 border-gray-600"
                >
                    <div className="flex justify-between items-center mb-8">
                        <motion.button
                            whileHover={{ scale: 1.05 }}
                            whileTap={{ scale: 0.95 }}
                            onClick={onBack}
                            className="px-4 py-2 bg-transparent text-white font-bold border-2 border-white hover:bg-white hover:text-black transition-colors"
                        >
                            ← BACK
                        </motion.button>
                        <div className="text-center flex-1">
                            <h2 className="text-3xl font-bold text-white tracking-wider" style={{ textShadow: '3px 3px 0px rgba(0, 0, 0, 0.8)' }}>
                                NEW GAME
                            </h2>
                            <p className="text-gray-300 mt-2">
                                Name your save file ({saveCount}/3 saves)
                            </p>
                        </div>
                        <div className="w-20"></div>
                    </div>

                    <form onSubmit={handleSubmit}>
                        <div className="space-y-4">
                            {/* Save Name Field */}
                            <div>
                                <input
                                    type="text"
                                    name="saveName"
                                    value={formData.saveName}
                                    onChange={handleChange}
                                    className="w-full px-4 py-3 bg-gray-200 text-black text-center font-bold border-2 border-black focus:outline-none focus:ring-2 focus:ring-blue-500"
                                    placeholder="SAVE NAME"
                                    disabled={isLoadingSaveCount}
                                />
                                {errors.saveName && (
                                    <p className="text-red-500 text-sm mt-2 text-center">
                                        {errors.saveName}
                                    </p>
                                )}
                            </div>

                            {error && (
                                <div className="bg-red-500 bg-opacity-20 border-2 border-red-500 rounded p-4">
                                    <p className="text-red-500 text-sm text-center">
                                        {typeof error === 'string' ? error : error?.title || error?.message || 'An error occurred'}
                                    </p>
                                </div>
                            )}

                            {/* Submit Button */}
                            <motion.button
                                whileHover={{ scale: saveCount >= 3 ? 1 : 1.05 }}
                                whileTap={{ scale: saveCount >= 3 ? 1 : 0.95 }}
                                type="submit"
                                disabled={loading || isLoadingSaveCount || saveCount >= 3}
                                className={`w-full px-4 py-3 font-bold border-2 transition-colors disabled:cursor-not-allowed ${
                                    saveCount >= 3
                                        ? 'bg-red-600 text-white border-red-800 opacity-90'
                                        : 'bg-gray-200 text-black border-black hover:bg-gray-300 disabled:opacity-60'
                                }`}
                            >
                                {loading ? 'STARTING GAME...' : 
                                saveCount >= 3 ? 'DELETE A SAVE FIRST' :
                                'BEGIN ADVENTURE'}
                            </motion.button>
                        </div>
                    </form>
                </motion.div>
            </div>
        </div>
    );
}