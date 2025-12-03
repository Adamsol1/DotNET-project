import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom'; 
import { useGame } from '../context/GameContext';
import { useAuth } from '../context/Authentication';
import { StartGame } from '../components/Game/NewGame';
import { PlayGame } from '../components/Game/PlayGame';
import { motion } from 'framer-motion';
import RockPaperScissors from '../components/GameUI/miniGames/RockPaperScissors';
import TerminalPowerRestore from '../components/GameUI/miniGames/TerminalPower'; 
import AlertModal from '../components/Shared/AlertModal';
import PixelCloseButton from '../shared/assets/icons/pixel-close-button.svg';

export function Game() {

  // navigation
  const navigate = useNavigate();
  // game api function imports.
  const { getAllSaves, deleteSave } = useGame();
  // authentication
  const { user } = useAuth();
  const authenticated = !!user;

  const isAdmin = user?.role === 'admin';

  // states
  const [currentSave, setCurrentSave] = useState(null);
  const [saves, setSaves] = useState([]);
  const [showGameStart, setShowGameStart] = useState(false);
  const [showMiniGame, setShowMiniGame] = useState(false);
  const [showInfoBox, setShowInfoBox] = useState(false);
  const [miniGameType, setMiniGameType] = useState(null);

  // delete states
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [saveToDelete, setSaveToDelete] = useState(null);

  // get saves that belongs to the logged in user and set the saves state.
  const loadSaves = useCallback(async () => {
    if (!authenticated || !user) return;
    try {
      //console.log('[Game] loadSaves called, user object:', user);
      //console.log('[Game] user.id (userId):', user?.id);
      //console.log('[Game] localStorage user_id:', localStorage.getItem('user_id'));

      const userSaves = await getAllSaves(user.id);
      setSaves(userSaves);
    } catch (error) {
      console.error('Failed to load saves:', error);
    }
  }, [authenticated, user, getAllSaves]);

  // Load saves when component mounts
  useEffect(() => {
    loadSaves();
  }, [loadSaves]);

  const handleNewGame = () => {
    setShowGameStart(true);
  };

  // handle the game start event and set the current save state.
  const handleGameStart = (gameSave) => {
    setCurrentSave(gameSave);
    setShowGameStart(false);
  };

  // handle the back to menu event and set the current save state to null.
  const handleBackToMenu = () => {
    setCurrentSave(null);
    setShowGameStart(false);
    loadSaves(); // Reload saves list to show updated saves from gameplay
  };

  // handle the load save event and set the current save state to the selected save.
  const handleLoadSave = (save) => {
    setCurrentSave(save);
  };

  // we want the user so get an alert modal to confirm before deleting the save.
  const handleDeleteSave = ( save ) => {
    setSaveToDelete(save);

    setShowDeleteModal(true);
  }
  
  // function to cancel a deletion.
  const cancelDeleteSave = () => {
    setShowDeleteModal(false);
    setSaveToDelete(null);
  }

  // confirm delete save action.
  const confirmDeleteSave = async ( saveId ) => {

    try {
      await deleteSave(saveId); 
      setShowDeleteModal(false);
      setSaveToDelete(null);
      await loadSaves(); // Refresh the saves list
    
    } catch (error) {
      console.error("Failed to delete save:", error);
      setShowDeleteModal(false);
    }

  }




  // exit game event and navigate to the home page.
  const exitGame = () => {
    navigate('/');
  };

  const handleMiniGameWin = () => {
    console.log('Player won the mini-game!');
  };

  const handleMiniGameLose = () => {
    console.log('Player lost the mini-game!');
  };

  // Show StartGame component if new game is requested
  if (showGameStart) {
    return <StartGame onGameStart={handleGameStart} onBack={handleBackToMenu} />;
  }

  //return the  game play component from the current save 
  // lets the user play the game.
  if (currentSave && currentSave.id) {
    return (
      <PlayGame 
        saveId={currentSave.id} 
        onBackToMenu={handleBackToMenu}
      />
    );
  }


  // delete modal uses the AlertModal component.
  

  // Main menu view.
  return (

    <div 
      className="min-h-screen text-white font-mono relative overflow-y-auto bg-cover bg-center bg-no-repeat"
      style={{
        backgroundImage: `url('/assets/backgrounds/afterLogin.png')`,
        height: '100vh',
        overflowY: 'auto',
      }}
    >
      {/* Mini-game overlay */}
      {showMiniGame && (
        <div
          style={{
            position: 'fixed',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            backgroundColor: 'rgba(0, 0, 0, 0.8)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            zIndex: 2000,
            padding: '20px'
          }}
          onClick={(e) => {
            if (e.target === e.currentTarget) {
              setShowMiniGame(false);
              setMiniGameType(null);
            }
          }}
        >
          {miniGameType === 'rockPaperScissors' && (
            <RockPaperScissors
              onComplete={() => {
                setShowMiniGame(false);
                setMiniGameType(null);
              }}
              onWin={handleMiniGameWin}
              onLose={handleMiniGameLose}
            />
          )}
          {miniGameType === 'terminal' && (
            <TerminalPowerRestore
              onComplete={() => {
                setShowMiniGame(false);
                setMiniGameType(null);
              }}
              onWin={handleMiniGameWin}
              onLose={handleMiniGameLose}
            />
          )}
        </div>
      )}

      {/* INFO POPUP */}
      {showInfoBox && (
        <div
          className="fixed inset-0 bg-black bg-opacity-70 flex items-center justify-center z-[9999] p-6"
          onClick={(e) => {
            if (e.target === e.currentTarget) setShowInfoBox(false);
          }}
        >
          <motion.div
            initial={{ opacity: 0, scale: 0.8 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ duration: 0.3 }}
            className="bg-white text-black border-4 border-black p-8 max-w-lg w-full relative"
          >

            {/* CLOSE BUTTON */}
            <button
              onClick={() => setShowInfoBox(false)}
              className="absolute top-3 right-3 text-black text-xl font-bold hover:text-gray-700"
            >
              <img src={PixelCloseButton} alt="Close" className="w-6 h-6" />
            </button>

            <h2 className="text-3xl font-bold mb-6">How to Play</h2>

            <div className="space-y-4 text-lg text-black">
              <div>
                <h3 className="font-bold text-xl mb-2">Starting Your Adventure</h3>
                <p className="ml-4">
                  Click <strong>NEW GAME</strong> to create a new character and begin your journey. 
                  Or select a previous save from <strong>CONTINUE ADVENTURE</strong> to pick up where you left.
                </p>
              </div>
              <div>
                <h3 className="font-bold text-xl mb-2">Story & Dialogue</h3>
                <p className="ml-4">
                  Read through dialogue by clicking <strong>NEXT</strong>. Your choices shape the story, 
                  each decision can lead to different outcomes and paths.
                </p>
              </div>
              <div>
                <h3 className="font-bold text-xl mb-2">Mini-Games</h3>
                <p className="ml-4">
                  Complete mini-games when they appear to overcome challenges. 
                  The outcome affects your story progression.
                </p>
              </div>
              <div>
                <h3 className="font-bold text-xl mb-2">Saving Progress</h3>
                <p className="ml-4">
                  Your progress is automatically saved. Use the <strong>Back</strong> button to return 
                  to the main menu. You can manage multiple save files from the game menu.
                </p>
              </div>
            </div>
          </motion.div>
        </div>
      )}

      {/* DELETE MODAL */}
      {showDeleteModal && (
        <AlertModal 
          title='This action deletes the game save!'
          message='Are you sure you want to delete this save? This action cannot be undone.'
          onConfirm={() => confirmDeleteSave(saveToDelete.id)}
          onCancel={cancelDeleteSave}
          confirmLabel='Delete'
          cancelLabel='Cancel'
          isDangerous={true}
        />
      )}

      <div className="relative z-10 min-h-screen flex flex-col">
        {/* Header */}
        <header className="p-6 flex justify-end items-center gap-4">
          <motion.button
            whileHover={{ scale: 1.05 }}
            whileTap={{ scale: 0.95 }}
            onClick={() => setShowInfoBox(true)}
            className="px-4 py-2 bg-transparent text-white font-bold border-2 border-white hover:bg-white hover:text-black transition-colors"
          >
            INFO
          </motion.button>
          
          <motion.button
            whileHover={{ scale: 1.05 }}
            whileTap={{ scale: 0.95 }}
            onClick={exitGame}
            className="px-4 py-2 bg-transparent text-white font-bold border-2 border-white hover:bg-white hover:text-black transition-colors"
          >
            HOME
          </motion.button>
        </header>

        {/* Main Content */}
        <main className="flex-1 flex items-center justify-center px-6">
          <motion.div
            initial={{ opacity: 0, scale: 0.9 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ duration: 0.5 }}
            className="w-full max-w-2xl"
          >
            {/* Welcome Message */}
            <div className="text-center mb-12">
              <motion.h2
                initial={{ opacity: 0, y: -10 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.2 }}
                className="text-4xl font-bold text-white mb-4 tracking-wider"
                style={{ textShadow: '3px 3px 0px rgba(0, 0, 0, 0.8)' }}
              >
                WELCOME {user?.username}!
              </motion.h2>
              <p className="text-xl text-gray-300 mb-8">
                Ready to continue your adventure?
              </p>
            </div>

            {/* New Game Button - Top */}
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.3 }}
              className="mb-8"
            >
              <motion.button
                whileHover={{ scale: 1.05 }}
                whileTap={{ scale: 0.95 }}
                onClick={handleNewGame}
                className="w-full px-8 py-6 bg-white text-black font-bold text-2xl border-4 border-black hover:bg-gray-200 transition-colors"
                style={{ boxShadow: '8px 8px 0px rgba(0, 0, 0, 0.8)' }}
              >
                NEW GAME
              </motion.button>
            </motion.div>

            {!showMiniGame && isAdmin && (
              <motion.div
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.35 }}
                className="mb-8 space-y-4"
              >
                <motion.button
                  whileHover={{ scale: 1.05 }}
                  whileTap={{ scale: 0.95 }}
                  onClick={() => {
                    setMiniGameType('rockPaperScissors');
                    setShowMiniGame(true);
                  }}
                  className="w-full px-8 py-6 bg-blue-600 text-white font-bold text-2xl border-4 border-blue-400 hover:bg-blue-500 transition-colors"
                  style={{ boxShadow: '8px 8px 0px rgba(0, 0, 0, 0.8)' }}
                >
                  TEST ROCK PAPER SCISSORS
                </motion.button>
              </motion.div>
            )}

            {/* Load Previous Saves - Below */}
            {saves.length > 0 && (
              <motion.div
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.4 }}
                className="mb-8"
              >
                <h3 className="text-2xl font-bold text-white mb-4 tracking-wide text-center" style={{ textShadow: '3px 3px 0px rgba(0, 0, 0, 0.8)' }}>
                  CONTINUE ADVENTURE
                </h3>
                
                <div className="space-y-3">
                  {saves.map((save, index) => (
                    <motion.div
                      key={save.id}
                      initial={{ opacity: 0, x: -20 }}
                      animate={{ opacity: 1, x: 0 }}
                      transition={{ delay: 0.5 + index * 0.1 }}
                    >
                      <motion.button
                        whileHover={{ scale: 1.02 }}
                        whileTap={{ scale: 0.98 }}
                        onClick={() => handleLoadSave(save)}
                        className="w-full px-6 py-4 bg-gray-200 text-black font-bold text-lg border-4 border-black hover:bg-white transition-colors flex justify-between items-center"
                        style={{ boxShadow: '6px 6px 0px rgba(0, 0, 0, 0.8)' }}
                      >
                        <div className="text-left">
                            <div className="text-xl font-bold">{save.saveName}</div>
                            <div className="text-sm text-gray-600">
                                {save.characterName ? `Character: ${save.characterName}` : 'Saved Game'}
                            </div>
                        </div>
                          
                          
                        <div className="flex items-center gap-3">
                            <div className="text-right text-sm text-gray-500">
                                {new Date(save.lastUpdate).toLocaleDateString()}
                            </div>
                            
                            <div
                            onClick={(e) => {
                              e.stopPropagation(); // Prevent triggering the load save function
                              handleDeleteSave(save);
                            }}
                            className="p-4 rounded-full hover:bg-red-100 text-red-600 cursor-pointer"
                            role="button"
                            tabIndex={0}
                            onKeyDown={(e) => {
                              if (e.key === 'Enter' || e.key === ' ') {
                                e.preventDefault();
                                e.stopPropagation();
                                handleDeleteSave(save);
                              }
                            }}
                            >
                                <img src="/assets/icons/trash-alt-svgrepo-com.svg" alt="Delete Save" className="w-5 h-5" />
                            </div>
                            
                        </div>  
                      </motion.button>
                    </motion.div>
                  ))}
                </div>
              </motion.div>
            )}
          </motion.div>
        </main>
      </div>
    </div>
  );
}