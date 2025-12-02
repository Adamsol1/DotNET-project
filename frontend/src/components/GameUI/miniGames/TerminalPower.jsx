import React, { useState, useEffect, useRef, useCallback } from 'react';
import { AnimatePresence, motion } from 'framer-motion';

// intro lines that will be displayed and animated when the game starts.
//
const INTRO_LINES = [
  '> sudo systemctl status power-grid',
  '[ FAIL ] Power grid offline',
  '> sudo diagnostics --run',
  'Running diagnostics...',
  '████████████████████ 100%',
  'ERROR: Power nodes disconnected',
  'ERROR: Element clusters misaligned',
  '> sudo power-restore --init',
  'Initializing restoration protocol...',
  'Phase 1: Element Sorting - READY',
  'Awaiting user input...'
];

// elements that will be displayed in game grid
// so that the player can select the correct elements
const ELEMENT_LIBRARY = {
  wires: {
    id: 1,
    name: 'Wires',
    color: '#ff5b5b',
    image: '/assets/icons/wires-wire-svgrepo-com.svg'
  },
  reactor: {
    id: 2,
    name: 'Reactor',
    color: '#5bc0ff',
    image: '/assets/icons/reactor-svgrepo-com.svg'
  },
  fuel: {
    id: 3,
    name: 'Fuel',
    color: '#ffe066',
    image: '/assets/icons/fuel-pump-svgrepo-com.svg'
  },
  switch: {
    id: 4,
    name: 'Switch',
    color: '#6bffb0',
    image: '/assets/icons/switch-svgrepo-com.svg'
  }
};

// main component for the terminal power restore game
// this component will display the terminal lines, the game grid, and the score
// it will also handle the game logic and the game completion
//
const TerminalPowerRestore = ({ onComplete, onWin, onLose }) => {
  // phase decides what is diplayed.
  //if 1 intro display animation, 2 is game. 
  const [phase, setPhase] = useState(1);
  // terminal lines to be displayed
  const [terminalLines, setTerminalLines] = useState([]);
  // player/user score.
  const [score, setScore] = useState(0);
  // intro complete is used to track if the intro animation is complete
  const [introComplete, setIntroComplete] = useState(false);
  // grid elements is the elements displayed in the game grid.
  const [gridElements, setGridElements] = useState([]);
  // elements player has selected
  const [selectedElements, setSelectedElements] = useState([]);
  // target key is the key of the element that the player needs to select
  const [targetKey, setTargetKey] = useState('');
  // round is the current round of the game
  const [round, setRound] = useState(1);
  // terminal ref is the reference to the terminal element
  const terminalRef = useRef(null);
  const maxRounds = 3;

  // function to scroll to the bottom of the terminal
  // used after intro lines are displayed and new lines are added.
  const scrollToBottom = useCallback(() => {
    if (terminalRef.current) {
      terminalRef.current.scrollTop = terminalRef.current.scrollHeight;
    }
  }, []);



  // function to start a new round
  // function sets the grid elements, and targets that the player has to
  //select, chooses randomly from the elements library.
  const startRound = useCallback(() => {
    // get the keys of the elements library
    const keys = Object.keys(ELEMENT_LIBRARY);

    // create a pool of 12 elements 4x3 grid.

    const pool = Array.from({ length: 12 }, (_, idx) => {
      // randomly select a key from the elements library
      const key = keys[Math.floor(Math.random() * keys.length)];
      // get the id and the rest of the element library
      const { id: typeId, ...lib } = ELEMENT_LIBRARY[key];
      // return the id, typeId, key, and the rest of the element library
      return { id: idx, typeId, key, ...lib };
    });
    // get the available keys from the pool
    const available = [...new Set(pool.map(item => item.key))];
    // randomly select a target from the available keys
    const nextTarget = available[Math.floor(Math.random() * available.length)];
    // set the grid elements
    setGridElements(pool);
    // set the target key
    setTargetKey(nextTarget);
    // reset the selected elements
    setSelectedElements([]);

    // add the new lines to the existing terminal lines
    // ...prev is the existing terminal lines
    setTerminalLines(prev => [
      ...prev,
      '',
      `> ROUND ${round}/${maxRounds}: LOCATE EVERY [${ELEMENT_LIBRARY[nextTarget].name.toUpperCase()}]`,
      ''
    ]);
    // scroll to the bottom of the terminal
    setTimeout(scrollToBottom, 50);
  }, [round, scrollToBottom]);

  // useEffect to display the intro lines and start the game
  useEffect(() => {
    if (phase !== 1 || introComplete) return;
    let index = 0;
    // set the interval to display the intro lines
    // and iterate through the intro Lines array.
    const interval = setInterval(() => {
      setTerminalLines(prev => [...prev, INTRO_LINES[index]]);
      index += 1;
      scrollToBottom();

      // if the index is greater than the length of the intro lines array, 
      // clear the interval and start the game.
      if (index >= INTRO_LINES.length) {
        clearInterval(interval);
        setTimeout(() => {
          // set the intro complete to true
          setIntroComplete(true);
          setPhase(2);
          // start the first round
          startRound();
        }, 1000);
      }
    }, 200);
    return () => clearInterval(interval);
  }, [phase, introComplete, scrollToBottom, startRound]);

  

  // function to select an element
  const selectElement = elementId => {
    // if the element is already selected, remove it from the selected elements
    // otherwise add it to the selected elements
    setSelectedElements(prev =>
      prev.includes(elementId) ? prev.filter(id => id !== elementId) : [...prev, elementId]
    );
  };

  // function to submit the selection
  // it will check if the selection is correct
  // if it is correct, it will add 50 points to the score
  // if it is incorrect, it will reset the selection
  // if the round is greater or equal to the max rounds, game is completed.
  const submitSelection = () => {
    const correctIds = gridElements.filter(item => item.key === targetKey).map(item => item.id);
    const isCorrect =
      selectedElements.length === correctIds.length &&
      selectedElements.every(id => correctIds.includes(id));
    if (isCorrect) {
      setScore(prev => prev + 50);
      setTerminalLines(prev => [...prev, 'SUCCESS: Correct cluster isolated.', '']);
      scrollToBottom();
      // if the round is greater than the max rounds, complete the game
      if (round >= maxRounds) {
        completeGame(true);
      } else {
        setRound(prev => prev + 1);

        // start the next round
        setTimeout(startRound, 900);
      }
    } else {
      setTerminalLines(prev => [...prev, 'ERROR: Incorrect selection. Resetting...', '']);
      scrollToBottom();
      setSelectedElements([]);
    }
  };

  // function to complete the game
  const completeGame = won => {
    setTerminalLines(prev => [
      ...prev,
      '',
      won ? 'POWER SYSTEMS RESTORED.' : 'CRITICAL FAILURE: POWER OVERLOAD.',
      `Final Score: ${score}`
    ]);
    scrollToBottom();
    setTimeout(() => {
      if (won) {
        onWin?.();
      } else {
        onLose?.();
      }
      onComplete?.();
    }, 1500);
  };

  const targetMeta = targetKey ? ELEMENT_LIBRARY[targetKey] : null;

  return (
    <div className="w-full max-w-4xl h-[600px] bg-black border-4 border-green-500 p-6 font-mono text-green-300 overflow-hidden flex flex-col shadow-[0_0_20px_rgba(0,255,0,0.25)]">
      <div className="flex justify-between border-b border-green-700 pb-2 mb-4 z-10">
        <span className="font-semibold tracking-widest">TERMINAL ACCESS</span>
        <span className="text-yellow-400">SCORE {score.toString().padStart(4, '0')}</span>
      </div>
      <div
        ref={terminalRef}
        className="flex-1 overflow-y-auto mb-6 space-y-1 pr-2 scrollbar-thin scrollbar-thumb-green-700 scrollbar-track-transparent z-10"
      >
        {terminalLines.map((line, idx) => (
          <div key={idx} className="whitespace-pre-wrap text-sm md:text-base tracking-wide">
            {line}
          </div>
        ))}
      </div>
      {phase === 2 && targetMeta && (
        <motion.div
          initial={{ opacity: 0, y: 15 }}
          animate={{ opacity: 1, y: 0 }}
          className="border-t border-green-700 pt-4 z-10"
        >
          <div className="flex flex-col items-center gap-3 mb-4 text-yellow-300">
            <span className="text-xs uppercase tracking-[0.4em]">Select every</span>
            <div className="flex items-center gap-2 bg-black/40 px-4 py-2 rounded border border-yellow-600/40">
              <img src={targetMeta.image} alt={targetMeta.name} className="w-5 h-5" />
              <span className="font-semibold text-lg" style={{ color: targetMeta.color }}>
                {targetMeta.name}
              </span>
            </div>
          </div>
          <div className="grid grid-cols-4 md:grid-cols-6 gap-3 mb-6">
            <AnimatePresence>
              {gridElements.map(item => {
                const active = selectedElements.includes(item.id);
                return (
                  <motion.button
                    key={item.id}
                    initial={{ scale: 0 }}
                    animate={{ scale: 1 }}
                    exit={{ scale: 0 }}
                    whileHover={{ scale: 1.06 }}
                    whileTap={{ scale: 0.94 }}
                    onClick={() => selectElement(item.id)}
                    className="relative w-16 h-16 md:w-20 md:h-20 flex items-center justify-center rounded-xl border-2 transition-all duration-200"
                    style={{
                      borderColor: active ? item.color : '#2c3e50',
                      backgroundColor: active ? `${item.color}22` : '#07111d',
                      boxShadow: active ? `0 0 8px ${item.color}55` : 'none'
                    }}
                  >
                    {active ? (
                      <img
                        src={item.image}
                        alt={item.name}
                        className="w-8 h-8 md:w-10 md:h-10"
                        style={{ filter: `drop-shadow(0 0 6px ${item.color})` }}
                      />
                    ) : (
                      <span className="text-xl text-gray-600">?</span>
                    )}
                    {active && <div className="absolute top-1 right-1 w-3 h-3 bg-green-500 rounded-full" />}
                  </motion.button>
                );
              })}
            </AnimatePresence>
          </div>
          <button
            onClick={submitSelection}
            disabled={selectedElements.length === 0}
            className="w-full py-3 text-center font-semibold text-lg border-2 border-green-500 bg-green-600 hover:bg-green-500 disabled:opacity-40 disabled:cursor-not-allowed"
          >
            {selectedElements.length === 0 ? 'Pick Targets' : 'Submit Selection'}
          </button>
        </motion.div>
      )}
    </div>
  );
};

export default TerminalPowerRestore;
