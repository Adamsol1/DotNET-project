import React, { useState, useEffect, useCallback } from 'react';
import { useGame } from '../../context/GameContext';
import { useAudio } from '../../context/AudioContext';
import { motion } from 'framer-motion';
import { useAuth } from '../../context/Authentication';


/**
 * Function component is used by players to create a new game save. 
 * It allows users to name their game save
 * 
 * StartGame componenet is used to create a new game save by players.
 * It allows the users to name their game saves, before starting it, which then, 
 * navigates them and starts the game save after it is created.
 * Users are limited to only have 3 active saves at a time.
 * 
 */

export function StartGame({ onGameStart, onBack }) {
    // state to capture form name from the user
    const [saveName, setSaveName] = useState('');
    // game api functions from api.js trough gameContext.
    const { startGame, loading, error, clearError, getAllSaves } = useGame();
    // audio play functions from audioContext
    const { playBackgroundMusic } = useAudio();
    // current user from Authentication
    const { user } = useAuth();


    // state management for handling and displaying errors
    const [errors, setErrors] = useState({});
    // state to track how many saves the user has
    const [saveCount, setSaveCount] = useState(0);
    // tracks if the save count is loading
    const [isLoadingSaveCount, setIsLoadingSaveCount] = useState(true);

    // Function to count user's saves
  const countUserSaves = useCallback(async () => {
    try {
        setIsLoadingSaveCount(true);
        const userId = Number(localStorage.getItem('user_id'));
        
        // Use the existing getAllSaves function from GameContext
        const saves = await getAllSaves(userId);
        
        //console.log('[StartGame] User saves:', saves);

        // get the count of the saves and set it to its state
        const count = saves ? saves.length : 0;
        setSaveCount(count);
        // return the count of the saves
        return count;
    } catch (error) {
        // if the count fails, return 0
        console.error('Failed to count saves:', error);
        return 0;
    } finally {
        // set the loading state to false
        setIsLoadingSaveCount(false);
    } // dependency to rerun the function saves changes
    }, [getAllSaves]);

    // Count saves when component mounts and play the background music
    useEffect(() => {
        //playBackgroundMusic('/assets/audio/music/menuMusic.mp3');
        countUserSaves();

        // return an empty function on unmount,
        return () => {
            // Don't stop audio here - let it continue to the game
        };
        // dependency array to rerun if user saves changes or background music changes.
    }, [playBackgroundMusic, countUserSaves]);

    // function to capture the save name from the user
    const handleChange = (e) => {
        // get the name and value from the event target
        const { name, value } = e.target;
        // set the save name to the value
        setSaveName(value);

        // if there are errors, add it to the errors object
        if (errors[name]) {
            setErrors(prev => ({
                ...prev,
                [name]: ''
            }));
        }
    };

    // function to submit the saveName to the backend.
    const handleSubmit = async (e) => {
        // Prevents default / unamed save creation.
        e.preventDefault();
        //console.log("gamecontext user in startgame:", user)

        // empties the errors object
        setErrors({});
        clearError();

        // create a new errors object
        const newErrors = {};
        // if the save name is empty, add an error to the errors object
        if (!saveName.trim()) {
            newErrors.saveName = 'Save name is required';
        }

        // if there are errors, add it to the errors object
        if (Object.keys(newErrors).length > 0) {
            setErrors(newErrors);
            return;
        }

        // gets and checks the save count user has before starting a new game save.
        const currentSaveCount = await countUserSaves();
        
        // checks if the user has 3 saves. if yes gives an error telling
        // the user that they cannot create a new save. 
        if (currentSaveCount >= 3) {
            setErrors({
                saveName: 'Maximum 3 saves reached. Please delete an existing save before creating a new one.'
            });
            return;
        }

        // try catch statement to create a new game save.
        try {

            // create a gameSave for the user based on their Id.
            const gameSave = await startGame({
                UserId: user.id,
                SaveName: saveName
            });
            
            //console.log('[StartGame] Save created:', gameSave);
            
            // recall to count the user's saves after creating a new save.
            await countUserSaves();
            
            // calls the start game event after a new save is created.
            if (onGameStart) {
                onGameStart(gameSave);
            }
        } catch (error) {
            // get the error message from the error message from the backend.
            // before displaying it to the user
            const errorMessage = error.response?.data?.message || error.response?.data || error.message || 'Failed to start game';
            setErrors({
                saveName: typeof errorMessage === 'string' ? errorMessage : 'Failed to start game'
            });
        }
    };


    // html and tailwind for design
    // uses a background image 

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
                                    value={saveName.saveName}
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