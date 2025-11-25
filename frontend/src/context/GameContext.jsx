// react imports
import React, { createContext, useContext, useReducer, useEffect, useState } from 'react';
// api imports
import { auth, game, story, account } from '../endpoints/api';

/**
 * This context file is used to manage the game state and actions that can be taken.
 * as well used to store game states, that the backend, can use to get data.
 *
 */

/* Adding browser state management, so that the user isnt defaulted back to
home if he reloads or closes the browser. */
const STORAGE_KEY = {
    USER: 'user',
    AUTHENTICATED: 'authenticated',
    CURRENT_SAVE_ID: 'game_current_save_id',
    GAME_STATE: 'game_state',
}

// this function loads the state from the browser storage, if something is saved.
const loadStateFromStorage = () => {
    try {
        const user = localStorage.getItem(STORAGE_KEY.USER);
        const authenticated = localStorage.getItem(STORAGE_KEY.AUTHENTICATED);
        const currentSaveId = localStorage.getItem(STORAGE_KEY.CURRENT_SAVE_ID);
        const gameState = localStorage.getItem(STORAGE_KEY.GAME_STATE);

        // return the state object.
        return {
            // we check if the user is logged in, if not we set the user to null.
            user: user ? JSON.parse(user) : null,
            authenticated: authenticated === 'true',
            currentSaveId: currentSaveId ? parseInt(currentSaveId) : null,
        }
    } catch (error) {
        console.error('Error loading state from storage:', error);
        return {
            user: null,
            authenticated: false,
            currentSaveId: null
        }
    }
}

// function to save the user state to the browser storage.
const saveUserStateToStorage = (user, authenticated) => {
    try {
        if (user && authenticated) {
            localStorage.setItem(STORAGE_KEY.USER, JSON.stringify(user));
            localStorage.setItem(STORAGE_KEY.AUTHENTICATED, 'true');
        } else {
            localStorage.removeItem(STORAGE_KEY.USER);
            localStorage.setItem(STORAGE_KEY.AUTHENTICATED, 'false');
        }
    } catch (err) {
        console.error('Error saving user state to storage:', err);
    }
}

// save the current save id, as this changes a lot.
const saveCurrentSaveIdToStorage = (saveId) => {
    try {
        // if there is a save id. update it.
        // if not remove, the previous save id.
        if (saveId) {
            localStorage.setItem(STORAGE_KEY.CURRENT_SAVE_ID, saveId.toString());
        } else {
            localStorage.removeItem(STORAGE_KEY.CURRENT_SAVE_ID);
        }
    } catch (err) {
        console.error('Error saving current save id to storage:', err);
    }
}

// Clear all game state from localStorage
const clearGameStateFromStorage = () => {
    try {
        localStorage.removeItem(STORAGE_KEY.USER);
        localStorage.removeItem(STORAGE_KEY.AUTHENTICATED);
        localStorage.removeItem(STORAGE_KEY.CURRENT_SAVE_ID);
        localStorage.removeItem(STORAGE_KEY.GAME_STATE);
    } catch (err) {
        console.error('Error clearing game state from storage:', err);
    }
};

// Load initial state from localStorage
const savedState = loadStateFromStorage();

// this is the start / initial state, of the game before the user even has
// visited the application. 
const startState = {
    user: null,
    authenticated: false,
    currentSave: null,
    currentNode: null,
    availableChoices: [],
    playerState: null,
    visitedNodes: [],
    loading: false,
    error: null,
    dialogueIndex: 0,
    gameOver: false,
    allSaves: [], // store all previous saves
};

// Initial state to merge saved state with start state for initialization
const initialState = {
    ...startState,
    user: savedState.user,
    authenticated: savedState.authenticated,
    currentSaveId: savedState.currentSaveId,
};

/**
 * Action types are strings that describe commands that can be sent by the application.
 */

const ActionTypes = {
    //auth actions.
    LOGIN_START: 'LOGIN_START',
    LOGIN_SUCCESS: 'LOGIN_SUCCESS',
    LOGIN_ERROR: 'LOGIN_ERROR',
    REGISTER_START: 'REGISTER_START',
    REGISTER_SUCCESS: 'REGISTER_SUCCESS',
    REGISTER_ERROR: 'REGISTER_ERROR',
    LOGOUT_START: 'LOGOUT_START',
    LOGOUT_SUCCESS: 'LOGOUT_SUCCESS',
    LOGOUT_ERROR: 'LOGOUT_ERROR',
    LOGOUT: 'LOGOUT',

    // game actions.
    START_GAME: 'START_GAME',
    GAME_SUCCESS: 'GAME_SUCCESS',
    GAME_ERROR: 'GAME_ERROR',
    GET_ALL_SAVES: 'GET_ALL_SAVES',
    DELETE_SAVE_START: 'DELETE_SAVE_START',
    DELETE_SAVE_SUCCESS: 'DELETE_SAVE_SUCCESS',
    DELETE_SAVE_ERROR: 'DELETE_SAVE_ERROR',
    UPDATE_SAVE: 'UPDATE_SAVE',

    // story actions.
    NODE_START: 'NODE_START',
    NODE_SUCCESS: 'NODE_SUCCESS',
    NODE_ERROR: 'NODE_ERROR',

    //choice actions.
    CHOICE_START: 'CHOICE_START',
    CHOICE_SUCCESS: 'CHOICE_SUCCESS',
    CHOICE_ERROR: 'CHOICE_ERROR',

    // navigation actions.
    NAVIGATE_START: 'NAVIGATE_START',
    NAVIGATE_SUCCESS: 'NAVIGATE_SUCCESS',
    NAVIGATE_ERROR: 'NAVIGATE_ERROR',

    // dialogue actions.
    DIALOGUE_START: 'DIALOGUE_START',
    DIALOGUE_SUCCESS: 'DIALOGUE_SUCCESS',
    DIALOGUE_ERROR: 'DIALOGUE_ERROR',

    // UI actions.
    SET_LOADING: 'SET_LOADING',
    SET_ERROR: 'SET_ERROR',
    CLEAR_ERROR: 'CLEAR_ERROR',
    SET_GAME_OVER: 'SET_GAME_OVER',

    // account actions
    UPDATE_USERNAME_START: 'UPDATE_USERNAME_START',
    UPDATE_USERNAME_SUCCESS: 'UPDATE_USERNAME_SUCCESS',
    UPDATE_USERNAME_ERROR: 'UPDATE_USERNAME_ERROR',

    UPDATE_PASSWORD_START: 'UPDATE_PASSWORD_START',
    UPDATE_PASSWORD_SUCCESS: 'UPDATE_PASSWORD_SUCCESS',
    UPDATE_PASSWORD_ERROR: 'UPDATE_PASSWORD_ERROR',

    DELETE_ACCOUNT_START: 'DELETE_ACCOUNT_START',
    DELETE_ACCOUNT_SUCCESS: 'DELETE_ACCOUNT_SUCCESS',
    DELETE_ACCOUNT_ERROR: 'DELETE_ACCOUNT_ERROR',
};

/**
 * game reducer is a function that takes in the current state and actions.
 * and then returns a new state based on the action.
 */

function gameReducer(state, action) {
    //switch on the action type, to return a new state.
    switch (action.type) {
        // Auth
        case ActionTypes.LOGIN_START:
            return { ...state, loading: true, error: null };
        case ActionTypes.LOGIN_SUCCESS:
            return { ...state, loading: false, authenticated: true, user: action.payload, error: null };
        case ActionTypes.LOGIN_ERROR:
            return { ...state, loading: false, authenticated: false, error: action.payload };
        case ActionTypes.REGISTER_START:
            return { ...state, loading: true, error: null };
        case ActionTypes.REGISTER_SUCCESS:
            return { ...state, loading: false, authenticated: true, user: action.payload, error: null };
        case ActionTypes.REGISTER_ERROR:
            return { ...state, loading: false, error: action.payload };
        case ActionTypes.LOGOUT:
            return { ...startState };

        // Game
        case ActionTypes.START_GAME:
            return { ...state, loading: true, error: null };

        // if the game starts successfully, we set the current save state.
        case ActionTypes.GAME_SUCCESS:
            return {
                ...state,
                loading: false,
                error: null,
                currentSave: action.payload,
                // reset playerState to initial or from payload if available
                playerState:
                    action.payload.playerCharacter ?? 
                    action.payload.playerState ??
                    state.playerState,
            };
        case ActionTypes.GAME_ERROR:
            return { ...state, loading: false, error: action.payload };
        case ActionTypes.GET_ALL_SAVES:
            return { ...state, allSaves: action.payload || [] };
        case ActionTypes.DELETE_SAVE_START:
            return { ...state, loading: true, error: null };
        case ActionTypes.DELETE_SAVE_SUCCESS:
            return { ...state, loading: false, allSaves: state.allSaves.filter(s => s.id !== action.payload) };
        case ActionTypes.DELETE_SAVE_ERROR:
            return { ...state, loading: false, error: action.payload };
        case ActionTypes.UPDATE_SAVE:
            return {
                ...state,
                allSaves: state.allSaves.map(s => s.id === action.payload.id ? action.payload : s)
            };

        // Node - NOW INCLUDES PLAYER STATE
        case ActionTypes.NODE_START:
            return { ...state, loading: true, error: null };
        case ActionTypes.NODE_SUCCESS:
            return {
                ...state,
                loading: false,
                error: null,
                currentNode: action.payload.node || action.payload,
                availableChoices: (action.payload.node || action.payload).choices || [],
                // always update playerState if payload contains it
                playerState: action.payload.playerCharacter ?? action.payload.playerState ?? state.playerState
            };

        case ActionTypes.NODE_ERROR:
            return { ...state, loading: false, error: action.payload };

        // Choice
        case ActionTypes.CHOICE_START:
            return { ...state, loading: true, error: null };
        case ActionTypes.CHOICE_SUCCESS:
            return {
                ...state,
                loading: false,
                error: null,
                currentNode: action.payload.CurrentStoryNode || state.currentNode,
                availableChoices: action.payload.AvailableChoices || [],
                playerState: action.payload.PlayerCharacter || state.playerState,
            };
        case ActionTypes.CHOICE_ERROR:
            return { ...state, loading: false, error: action.payload };

        // Dialogue
        case ActionTypes.DIALOGUE_START:
            return { ...state, loading: true, error: null };
        case ActionTypes.DIALOGUE_SUCCESS:
            return { ...state, loading: false, error: null, dialogueIndex: action.payload.index || 0 };
        case ActionTypes.DIALOGUE_ERROR:
            return { ...state, loading: false, error: action.payload };

        // Navigation
        case ActionTypes.NAVIGATE_START:
            return { ...state, loading: true, error: null };
        case ActionTypes.NAVIGATE_SUCCESS:
            return {
                ...state,
                loading: false,
                error: null,
                currentNode: action.payload.node || action.payload,
                availableChoices: (action.payload.node || action.payload).choices || [],
                // Update playerState if included
                playerState: action.payload.playerCharacter || action.payload.playerState || state.playerState
            };
        case ActionTypes.NAVIGATE_ERROR:
            return { ...state, loading: false, error: action.payload };

        // UI
        case ActionTypes.SET_LOADING:
            return { ...state, loading: action.payload };
        case ActionTypes.SET_ERROR:
            return { ...state, error: action.payload };
        case ActionTypes.CLEAR_ERROR:
            return { ...state, error: null };
        case ActionTypes.SET_GAME_OVER:
            return { ...state, gameOver: action.payload };

        // AccountManagement
        case ActionTypes.UPDATE_USERNAME_START:
            return { ...state, loading: true, error: null };
        case ActionTypes.UPDATE_USERNAME_SUCCESS:
            return { ...state, loading: false, user: action.payload, error: null };
        case ActionTypes.UPDATE_USERNAME_ERROR:
            return { ...state, loading: false, error: action.payload };
        case ActionTypes.UPDATE_PASSWORD_START:
            return { ...state, loading: true, error: null };
        case ActionTypes.UPDATE_PASSWORD_SUCCESS:
            return { ...state, loading: false, error: null };
        case ActionTypes.UPDATE_PASSWORD_ERROR:
            return { ...state, loading: false, error: action.payload };
        case ActionTypes.DELETE_ACCOUNT_START:
            return { ...state, loading: true, error: null };
        case ActionTypes.DELETE_ACCOUNT_SUCCESS:
            return { ...startState, loading: false };
        case ActionTypes.DELETE_ACCOUNT_ERROR:
            return { ...state, loading: false, error: action.payload };

        // return a state as default.
        default:
            return state;
    }
}

// create the context.
const GameContext = createContext();

// provider compontent that wraps the application 
// and provides the context to the children components.

export function GameProvider({ children }) {
    // use the reducer to manage the state, initialized with saved state
    const [state, dispatch] = useReducer(gameReducer, initialState);
    const [authRestored, setAuthRestored] = useState(false);

    // restore auth from storage on mount
    useEffect(() => {
        const token = localStorage.getItem('token');
        
        if (token) {
            //set authenticated user state
            dispatch({
                type: ActionTypes.LOGIN_SUCCESS,
                payload: { token}
            });
        }
        setAuthRestored(true);
    }, []);

    // handles the login process.
    const login = async (credentials) => {
        // try to send the credentials to the backend.
        try {
            dispatch({ type: ActionTypes.LOGIN_START });

            // get the response from the API.
            const user = await auth.login(credentials);

            // if the login is successful, we dispatch the success action.
            // to update the state.
            dispatch({ type: ActionTypes.LOGIN_SUCCESS, payload: user });
            
            // save the token to the local storage.
            localStorage.setItem('token', user.token);
            // return the user.
            return user;
        } catch (error) {
            dispatch({ type: ActionTypes.LOGIN_ERROR, payload: error.message || 'Login failed' });
            throw error;
        }
    };

    // register user and return the result.
    const register = async (userData) => {
        // try to register the user,
        try {
            dispatch({ type: ActionTypes.REGISTER_START });
            const user = await auth.register(userData);

            dispatch({ type: ActionTypes.REGISTER_SUCCESS, payload: user });
            // return the user.
            return user;
        } catch (error) {
            dispatch({ type: ActionTypes.REGISTER_ERROR, payload: error.message || 'Registration failed' });
            throw error;
        }
    };

    // logout user and return the result.
    const logout = async () => {
        // try to logout the user,
        try {
            await auth.logout();
        } catch (error) {
            console.error('Logout error:', error);
        } finally {
            dispatch({ type: ActionTypes.LOGOUT });
        }
        return true;
    };

    // update username
    const updateUsername = async (usernameData) => {
        try {
            dispatch({ type: ActionTypes.UPDATE_USERNAME_START });
            const updatedUsername = await account.updateUsername(usernameData);
            dispatch({ type: ActionTypes.UPDATE_USERNAME_SUCCESS, payload: updatedUsername });
            return updatedUsername;
        } catch (error) {
            const errorMessage = error.response?.data || 'Failed to update username';
            dispatch({ type: ActionTypes.UPDATE_USERNAME_ERROR, payload: errorMessage });
            throw error;
        }
    };

    // update password
    const updatePassword = async (passwordData) => {
        try {
            dispatch({ type: ActionTypes.UPDATE_PASSWORD_START });
            await account.updatePassword(passwordData);
            dispatch({ type: ActionTypes.UPDATE_PASSWORD_SUCCESS });
            return true;
        } catch (error) {
            const errorMessage = error.response?.data || 'Failed to update password';
            dispatch({ type: ActionTypes.UPDATE_PASSWORD_ERROR, payload: errorMessage });
            throw error;
        }
    };

    // delete account
    const deleteAccount = async (userId) => {
        try {
            dispatch({ type: ActionTypes.DELETE_ACCOUNT_START });
            await account.deleteAccount(userId);
            dispatch({ type: ActionTypes.DELETE_ACCOUNT_SUCCESS });
            clearGameStateFromStorage();
            return true;
        } catch (error) {
            const errorMessage = error.response?.data || 'Failed to delete account';
            dispatch({ type: ActionTypes.DELETE_ACCOUNT_ERROR, payload: errorMessage });
            throw error;
        }
    };


    // start the game and return the result.
    const startGame = async (gameData) => {
        try {
            dispatch({ type: ActionTypes.START_GAME });

            const save = await game.startGame(gameData);

            dispatch({ type: ActionTypes.GAME_SUCCESS, payload: save });

            // return the save.
            return save;
        } catch (error) {
            const errorMessage = error.response?.data || 'Failed to start game';

            dispatch({ type: ActionTypes.GAME_ERROR, payload: errorMessage });

            //return the error.
            throw error;
        }
    };

    // load the game and return the result.
    const loadGame = async (saveId) => {
        try {
            // send the load game start action to the reducer.
            dispatch({ type: ActionTypes.START_GAME });

            // get the save from the API.
            const save = await game.loadGame(saveId);

            // send the load game success action to the reducer.

            dispatch({ type: ActionTypes.GAME_SUCCESS, payload: save });
            // return the save.
            return save;
        } catch (error) {
            const errorMessage = error.response?.data || 'Failed to load game';
            dispatch({ type: ActionTypes.GAME_ERROR, payload: errorMessage });
            // return the error.
            throw error;
        }
    };

    // get all saves and return the result.
    const getAllSaves = async (userId) => {
        try {
            dispatch({ type: ActionTypes.GET_ALL_SAVES });
            const saves = await game.getAllSaves(userId);
            dispatch({ type: ActionTypes.GAME_SUCCESS, payload: saves });
            // return the saves.
            return saves;
        }
        catch (error) {
            const errorMessage = error.response?.data || 'Failed to get all saves';
            dispatch({ type: ActionTypes.GAME_ERROR, payload: errorMessage });
            // return the error.
            throw error;
        }
    };

    const getSaveById = async (saveId) => {
        try {
            const save = state.allSaves.find(s => s.id === saveId);
            if (!save) throw new Error('Save not found');
            return save;
        } catch (error) {
            dispatch({ type: ActionTypes.SET_ERROR, payload: error.message || 'Failed to get save' });
            return null;
        }
    };

    const deleteSave = async (saveId) => {
        try {
            dispatch({ type: ActionTypes.DELETE_SAVE_START });
            await game.deleteSave(saveId); // API call
            dispatch({ type: ActionTypes.DELETE_SAVE_SUCCESS, payload: saveId });
            // return the result.
            return true;
        } catch (error) {
            dispatch({ type: ActionTypes.DELETE_SAVE_ERROR, payload: error.message || 'Failed to delete save' });
        }
    };


    // update a save and return the result.
    const updateSave = async (saveId, saveData) => {
        try {
            dispatch({ type: ActionTypes.UPDATE_SAVE });

            // get the save from the API.
            const save = await game.updateSave(saveId, saveData);
            dispatch({ type: ActionTypes.GAME_SUCCESS, payload: save });
            // return the save.
            return save;
        } catch (error) {
            dispatch({ type: ActionTypes.SET_ERROR, payload: error.message || 'Failed to update save' });
        }
    };

    // get player state and return the result.
    const getPlayerState = async (playerCharacterId) => {
        try {
            dispatch({ type: ActionTypes.PLAYER_STATE_START });
            const playerState = await story.getPlayerState(playerCharacterId);
            dispatch({ type: ActionTypes.PLAYER_STATE_SUCCESS, payload: playerState });
            return playerState;
        } catch (error) {
            const errorMessage = error.response?.data || 'Failed to get player state';
            dispatch({ type: ActionTypes.PLAYER_STATE_ERROR, payload: errorMessage });
            throw error;
        }
    };

    // get the current node the user is on and return the result.
    const getCurrentNode = async (saveId) => {
        try {
            dispatch({ type: ActionTypes.NODE_START });

            // get the current node from the API.
            const response = await story.getCurrentNode(saveId);

            // The response might be just the node, or it might include playerCharacter
            // Handle both cases
            const nodeData = response.node || response.currentStoryNode || response;
            const playerData = response.playerCharacter || response.playerState;

            // send the get current node success action to the reducer.
            dispatch({
                type: ActionTypes.NODE_SUCCESS,
                payload: {
                    node: nodeData,
                    playerCharacter: playerData
                }
            });
            // return the node.
            return nodeData;
        }
        catch (error) {
            const errorMessage = error.response?.data || 'Failed to get current node';
            dispatch({ type: ActionTypes.NODE_ERROR, payload: errorMessage });
            // return the error.
            throw error;
        }
    };


    // make a choice and return the result.
    const makeChoice = async (saveId, choiceId) => {
        try {
            dispatch({ type: ActionTypes.CHOICE_START });

            // Track the current node as visited BEFORE making the choice
            if (state.currentNode?.id && state.currentSave) {
                // Ensure visitedNodeIds is always an array
                let visitedIds = state.currentSave.visitedNodeIds;

                if (!visitedIds) {
                    visitedIds = [];
                } else if (!Array.isArray(visitedIds)) {
                    visitedIds = [];
                } else {
                    visitedIds = [...visitedIds];
                }

                // Add current node if not already visited
                if (!visitedIds.includes(state.currentNode.id)) {
                    visitedIds.push(state.currentNode.id);

                    // Update currentSave in state immediately
                    dispatch({
                        type: ActionTypes.GAME_SUCCESS,
                        payload: {
                            ...state.currentSave,
                            visitedNodeIds: visitedIds
                        }
                    });

                    // Fire and forget backend update (don't wait for it)
                    updateSave(saveId, { visitedNodeIds: visitedIds }).catch(err => {
                        console.error('Failed to sync visited nodes to backend:', err);
                    });
                }
            }

            const gameState = await story.makeChoice(saveId, choiceId);

            dispatch({
                type: ActionTypes.CHOICE_SUCCESS,
                payload: {
                    CurrentStoryNode: gameState.currentStoryNode,
                    AvailableChoices: gameState.availableChoices,
                    PlayerCharacter: gameState.playerCharacter
                        ? { ...gameState.playerCharacter, health: gameState.playerCharacter.health }
                        : null,
                }
            });
            
            return gameState;
        } catch (error) {
            dispatch({ type: ActionTypes.CHOICE_ERROR, payload: error.message || 'Failed to make choice' });
            throw error;
        }
    };

    // go back to the previous node and return the result.
    const goBack = async (saveId) => {
        try {
            dispatch({ type: ActionTypes.NAVIGATE_START });

            // get the previous node from the API.
            const response = await story.goBack(saveId);

            // Handle different response formats
            const nodeData = response.node || response.currentStoryNode || response;
            const playerData = response.playerCharacter || response.playerState;

            // send the go back success action to the reducer.
            dispatch({
                type: ActionTypes.NAVIGATE_SUCCESS,
                payload: {
                    node: nodeData,
                    playerCharacter: playerData
                }
            });
            // return the node.
            return nodeData;

        } catch (error) {
            const errorMessage = error.response?.data || 'Failed to go back';
            dispatch({ type: ActionTypes.NAVIGATE_ERROR, payload: errorMessage });
            // return the error.
            throw error;
        }
    };

    // go forward to the next node and return the result.
    const nextNode = async (saveId) => {
        try {
            dispatch({ type: ActionTypes.NAVIGATE_START });

            // get the next node from the API.
            const response = await story.nextNode(saveId);

            // Handle different response formats
            const nodeData = response.node || response.currentStoryNode || response;
            const playerData = response.playerCharacter || response.playerState;

            // send the go forward success action to the reducer.
            dispatch({
                type: ActionTypes.NAVIGATE_SUCCESS,
                payload: {
                    node: nodeData,
                    playerCharacter: playerData
                }
            });
            // return the node.
            return nodeData;
        } catch (error) {
            const errorMessage = error.response?.data || 'Failed to go forward';
            dispatch({ type: ActionTypes.NAVIGATE_ERROR, payload: errorMessage });
            // return the error.
            throw error;
        }
    };

    // get the next dialogue and return the result.
    const getNextDialogue = async (saveId) => {
        try {
            dispatch({ type: ActionTypes.DIALOGUE_START });

            // get the next dialogue from the API.
            const dialogue = await story.getNextDialogue(saveId);

            // send the get next dialogue success action to the reducer.
            dispatch({ type: ActionTypes.DIALOGUE_SUCCESS, payload: dialogue });
            // return the dialogue.
            return dialogue;
        }
        catch (error) {
            const errorMessage = error.response?.data || 'Failed to get next dialogue';
            dispatch({ type: ActionTypes.DIALOGUE_ERROR, payload: errorMessage });
            // return the error.
            throw error;
        }
    };

    // skip to the last dialogue and return the result.
    const skipToLastDialogue = async (saveId) => {
        try {
            dispatch({ type: ActionTypes.DIALOGUE_START });

            // get the last dialogue from the API.
            const dialogue = await story.skipToLastDialogue(saveId);

            // send the skip to last dialogue success action to the reducer.
            dispatch({ type: ActionTypes.DIALOGUE_SUCCESS, payload: dialogue });
            // return the dialogue.
            return dialogue;

        } catch (error) {

            const errorMessage = error.response?.data || 'Failed to skip to last dialogue';
            dispatch({ type: ActionTypes.DIALOGUE_ERROR, payload: errorMessage });
            // return the error.
            throw error;
        }
    };

    // UI actions.
    // set the loading state.
    const setLoading = (loading) => {
        dispatch({ type: ActionTypes.SET_LOADING, payload: loading });
    };

    // set the error state.
    const setError = (error) => {
        dispatch({ type: ActionTypes.SET_ERROR, payload: error });
    };

    // clear the error state.
    const clearError = () => {
        dispatch({ type: ActionTypes.CLEAR_ERROR });
    };

    const setGameOver = (value) => {
        dispatch({ type: ActionTypes.SET_GAME_OVER, payload: value });
    };




    // values to return.
    const values = {
        // state.
        ...state,
        authRestored,
        // auth actions.
        login,
        register,
        logout,
        // game actions.
        startGame,
        loadGame,
        getAllSaves,
        getSaveById,
        deleteSave,
        updateSave,

        // story actions.
        getCurrentNode,
        makeChoice,
        goBack,
        nextNode,
        getPlayerState,
        setGameOver,

        // dialogue actions.
        getNextDialogue,
        skipToLastDialogue,

        // UI actions.
        setLoading,
        setError,
        clearError,

        // account management
        updateUsername,
        updatePassword,
        deleteAccount,
    };

    // finally packagge the context and return it.
    return (
        <GameContext.Provider value={values}>
            {children}
        </GameContext.Provider>
    );
}

// hooks for using the context.
export function useGame() {
    const context = useContext(GameContext);
    if (!context) {
        throw new Error('useGame must be used within a GameProvider');
    }
    return context;
}

// export the context.
export default GameContext;