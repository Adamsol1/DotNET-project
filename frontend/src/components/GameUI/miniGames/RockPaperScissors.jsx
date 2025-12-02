import React, { useState, useEffect} from "react";
import {HoloButton} from "./UI/HoloButton";


/*
* The rock paper sissors game logic was created by Ahmed.
* 
* 
* The design was created with help and insperation from Gemini 2.5 free
* based on earlier UI design that I had implemented, so that the game could look more visually appealing.
* and tie into the space theme, aswell as fixing animations that I had struggled with. 
* 
* */

// choice array the player can make.
const choices = ["rock", "paper", "scissors"];

// the score needed to win the game.
const winScore = 2;

//labels, renamed to tie into the game.
const labels = {
    rock: "Astroid",
    paper: "Shield",
    scissors: "Laser",
}

// SVGS that displays the choices that player and machine has picked.
// svgs are used from https://www.svgrepo.com/ 20.11.2025
const choiceSVGS = {
    rock: "/assets/icons/asteroid-2-svgrepo-com.svg",
    paper: "/assets/icons/paper-svgrepo-com.svg",
    scissors: "/assets/icons/scissors-svgrepo-com.svg",
}


// design function to display the choices that the user makes.
const DisplayChoice = ({ choice, side, winner, rolling }) => {

    const isPlayer = side === 'left';
    const isWinner = winner === (isPlayer ? 'player' : 'computer');
    const isLoser = winner === (isPlayer ? 'computer' : 'player');
    const isTie = winner === 'tied';

    let glowColor = isPlayer ? 'border-cyan-500/30' : 'border-rose-500/30';
    let textColor = isPlayer ? 'text-cyan-400' : 'text-rose-400';

    if (!rolling && choice) {
        if (isWinner) {
            glowColor = 'border-emerald-400 shadow-[0_0_30px_rgba(52,211,153,0.4)]';
            textColor = 'text-emerald-400';
        } else if (isLoser) {
            glowColor = 'border-rose-600 opacity-60';
            textColor = 'text-rose-600';
        } else if (isTie) {
            glowColor = 'border-amber-400 shadow-[0_0_20px_rgba(251,191,36,0.4)]';
            textColor = 'text-amber-400';
        }
    }

    return (
        <div className={`flex flex-col items-center justify-center w-1/3 transition-all duration-500 ${isPlayer ? 'order-1' : 'order-3'}`}>
            {/* HUD Label */}
            <div className={`text-xs font-mono tracking-[0.2em] mb-4 border-b pb-1 ${textColor} border-current opacity-80`}>
                {isPlayer ? 'PILOT SYSTEM' : 'HOSTILE CPU'}
            </div>

            {/* Icon Container - Reactor Core Look */}
            <div className={`
                relative flex items-center justify-center 
                w-28 h-28 md:w-36 md:h-36 rounded-full 
                bg-slate-900/90 backdrop-blur-sm
                border-[3px] ${glowColor}
                transition-all duration-300
                ${rolling ? 'animate-pulse shadow-[0_0_15px_rgba(255,255,255,0.1)]' : ''}
            `}>
                {/* Rotating HUD Ring */}
                {rolling && (
                    <div className="absolute inset-0 rounded-full border border-dashed border-white/20 animate-[spin_3s_linear_infinite]" />
                )}

                {choice ? (
                    <img
                        src={choiceSVGS[choice]}
                        alt={choice}
                        className={`w-16 h-16 md:w-20 md:h-20 drop-shadow-lg transition-transform duration-300 ${isWinner ? 'scale-110' : 'scale-100'}`}
                        style={{ filter: isWinner ? 'brightness(1.2)' : 'none' }}
                    />
                ) : (
                    <div className="text-4xl font-mono text-slate-700 animate-pulse">?</div>
                )}
            </div>

            {/* Text Description */}
            <div className="mt-6 h-8 flex items-center justify-center">
                <span className={`text-lg font-mono font-bold ${textColor}`}>
                    {choice ? labels[choice] : (rolling ? 'SCANNING...' : 'AWAITING INPUT')}
                </span>
            </div>
        </div>
    );
}

// Game function
// onComplete: function to call when the game is complete.
// onWin: function to call when the user wins.
// onLose: function to call when the user loses.
const RockPaperScissors = ({ onComplete, onWin, onLose }) => {
    // keep track of the scores how many times a user or the machine have won.
    const [playerScore, setPlayerScore] = useState(0);
    const [computerScore, setComputerScore] = useState(0);

    // tracks how many rounds the user has played.
    const [rounds, setRounds] = useState(0);

    // tracks the last result.
    const [lastResult, setLastResult] = useState(null);

    // and if the game is over. starting as false.
    const [gameOver, setGameOver] = useState(false);

    // I also want to display the choices, and track them
    // so the choice can get displayed in the UI.
    const [playerChoice, setPlayerChoice] = useState(null);
    const [computerChoice, setComputerChoice] = useState(null);

    //creating also small animation using framer to show the different choices being made.
    const [rolling, setRolling] = useState(false);
    const [roundWinner, setRoundWinner] = useState(null);


    // function to calculate the machines choice.
    const getComputerChoice = () => {
        // return a random choice from the choices array.
        return choices[Math.floor(Math.random() * choices.length)];
    };

    // function to calculate who wins a a hand or round.
    const calculateRound = (player, computer) => {
        // if the player and machine has the same choice, tied.
        if (player === computer) return 'tied';

        if (
            // there is 3 possible wins for the player.
            // and the same for the machine.
            (player === 'rock' && computer === 'scissors') ||
            (player === 'scissors' && computer === 'paper') ||
            (player === 'paper' && computer === 'rock')
        ) {
            return 'player';
        }
        // if the player does not win, the machine wins.
        return 'computer';
    }

    // function to determine the winner and update scores
    const determineWinner = (playerChoiceValue, computerChoiceValue) => {
        const winner = calculateRound(playerChoiceValue, computerChoiceValue);

        if (winner === 'player') {
            // increment the player's score.
            const newPlayerScore = playerScore + 1;
            setPlayerScore(newPlayerScore);

            // check if the player has won the game.
            if (newPlayerScore >= winScore) {
                // if the score is equal or bigger than winscore
                // player has won
                setGameOver(true);
                setLastResult({
                    playerChoice: playerChoiceValue,
                    computerChoice: computerChoiceValue,
                    winner: 'player',
                    roundWinner: 'player',
                });
                // if onWin is defined, call it. else nothing happens.
                onWin?.();
                return;
            }
        } else if (winner === 'computer') {
            const newComputerScore = computerScore + 1;
            setComputerScore(newComputerScore);

            if (newComputerScore >= winScore) {
                setGameOver(true);
                setLastResult({
                    playerChoice: playerChoiceValue,
                    computerChoice: computerChoiceValue,
                    winner: 'computer',
                    roundWinner: 'computer',
                });
                onLose?.();
                return;
            }
        }

        // set the last result to the choice and computer choice.
        setLastResult({
            playerChoice: playerChoiceValue,
            computerChoice: computerChoiceValue,
            winner,
            roundWinner: winner,
        });

        // increment the rounds.
        setRounds(prev => prev + 1);
        setRoundWinner(winner);
    };

    // function to handle the choice. and get the winner by result
    const handleChoice = (choice) => {
        // if the game is over or rolling, do nothing.
        if (gameOver || rolling) return;

        // get the computer's choice.
        const computerChoiceValue = getComputerChoice();

        // start the animation
        animateChoice(choice, computerChoiceValue);
    };

    // function to handle the rolling animation.
    const animateChoice = (playerChoiceValue, computerChoiceValue) => {
        // shows that the choice is being made.
        setRolling(true);
        //set the players choice.
        setPlayerChoice(null);
        //set the computers choice.
        setComputerChoice(null);
        // start round winner as null, as no one has won during the animation.
        setRoundWinner(null);

        // we need to calculate the amout of times
        // the different choices will be displayed. and set a max of 10 times.
        const maxRolls = 10;
        // start the roll count at 0.
        let rollCount = 0;

        const rollingInterval = setInterval(() => {
            // set the rolling choices to random choices from the choices array.
            setPlayerChoice(choices[Math.floor(Math.random() * choices.length)]);
            setComputerChoice(choices[Math.floor(Math.random() * choices.length)]);

            // increment the roll count.
            rollCount++;

            // if the roll count is bigger or equal to maxRolls,
            // stop the interval, and determine the winner.
            if (rollCount >= maxRolls) {
                clearInterval(rollingInterval);
                setRolling(false);
                // set the final choices
                setPlayerChoice(playerChoiceValue);
                setComputerChoice(computerChoiceValue);
                // determine the winner of the round.
                determineWinner(playerChoiceValue, computerChoiceValue);
            }
        }, 80);
    }
    
    return (
        <div className="min-h-[600px] w-full max-w-5xl mx-auto p-4 flex items-center justify-center bg-transparent font-sans">

            {/* 
                Card Container 
                Replaced Card component with div styled for Glassmorphism/Space Panel look 
            */}
            <div className="relative w-full bg-slate-900/90 border border-slate-700/50 rounded-2xl shadow-[0_0_40px_rgba(0,0,0,0.5)] backdrop-blur-xl overflow-hidden">

                {/* Decorative Top Bar */}
                <div className="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-cyan-500 via-purple-500 to-rose-500 opacity-70" />

                <div className="p-6 md:p-8">

                    {/* Header Section */}
                    <div className="text-center mb-8">
                        <h1 className="text-3xl md:text-4xl font-black text-transparent bg-clip-text bg-gradient-to-r from-cyan-300 to-cyan-100 uppercase tracking-widest mb-2 drop-shadow-[0_0_10px_rgba(6,182,212,0.5)]">
                            Combat Simulation
                        </h1>
                        <p className="text-slate-400 font-mono text-sm">
                            OBJECTIVE: REACH <span className="text-white font-bold">{winScore}</span> VICTORIES
                        </p>
                    </div>

                    {/* Scoreboard HUD */}
                    <div className="flex justify-between items-center bg-slate-950/50 border-y border-slate-800 py-4 px-8 mb-10 relative">
                        {/* Player Score */}
                        <div className="text-center">
                            <div className="text-4xl font-mono font-bold text-cyan-400 drop-shadow-[0_0_8px_rgba(34,211,238,0.8)]">
                                {playerScore.toString().padStart(2, '0')}
                            </div>
                            <div className="text-[10px] text-slate-500 uppercase tracking-widest mt-1">Player</div>
                        </div>

                        {/* VS Badge Center */}
                        <div className="absolute left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 bg-slate-900 px-4 border border-slate-800 rounded-full">
                            <span className="text-xs font-mono text-slate-500">VS</span>
                        </div>

                        {/* CPU Score */}
                        <div className="text-center">
                            <div className="text-4xl font-mono font-bold text-rose-500 drop-shadow-[0_0_8px_rgba(244,63,94,0.8)]">
                                {computerScore.toString().padStart(2, '0')}
                            </div>
                            <div className="text-[10px] text-slate-500 uppercase tracking-widest mt-1">Hostile</div>
                        </div>
                    </div>

                    {/* Battle Arena */}
                    <div className="flex justify-between items-center mb-12 px-2 md:px-8 relative min-h-[220px]">

                        {/* display the choice of the user and machine. */}
                        <DisplayChoice
                            choice={playerChoice}
                            side="left"
                            winner={roundWinner}
                            rolling={rolling}
                        />

                        {/* VS Graphic / Round Result */}
                        <div className="absolute left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 z-10 order-2 w-1/3 flex justify-center">
                            {roundWinner && !gameOver && !rolling ? (
                                <div className={`
                                    px-4 py-2 rounded border backdrop-blur-md text-center animate-bounce-slight whitespace-nowrap
                                    ${roundWinner === 'player' ? 'bg-cyan-900/40 border-cyan-500/50 text-cyan-300' : ''}
                                    ${roundWinner === 'computer' ? 'bg-rose-900/40 border-rose-500/50 text-rose-300' : ''}
                                    ${roundWinner === 'tied' ? 'bg-amber-900/40 border-amber-500/50 text-amber-300' : ''}
                                `}>
                                    <div className="text-xs font-mono uppercase tracking-wider mb-1">Result</div>
                                    <div className="font-bold text-sm md:text-base">
                                        {roundWinner === 'tied' ? 'SYSTEM TIED' : roundWinner === 'player' ? 'TARGET HIT' : 'HULL DAMAGE'}
                                    </div>
                                </div>
                            ) : (
                                <div className="h-px w-full bg-slate-800/50" /> /* Invisible spacer when no result */
                            )}
                        </div>

                        <DisplayChoice
                            choice={computerChoice}
                            side="right"
                            winner={roundWinner}
                            rolling={rolling}
                        />
                    </div>

                    {/* Controls / Game Over Area */}
                    <div className="mt-auto">
                        {gameOver ? (
                            <div className="animate-fade-in">
                                <div className={`
                                    mb-6 p-8 text-center rounded-xl border backdrop-blur-sm
                                    ${playerScore > computerScore
                                    ? 'bg-emerald-900/20 border-emerald-500/50 shadow-[inset_0_0_30px_rgba(16,185,129,0.1)]'
                                    : 'bg-rose-900/20 border-rose-500/50 shadow-[inset_0_0_30px_rgba(244,63,94,0.1)]'}
                                `}>
                                    <h2 className={`text-3xl font-black uppercase tracking-widest mb-2 ${playerScore > computerScore ? 'text-emerald-400' : 'text-rose-500'}`}>
                                        {playerScore > computerScore ? 'MISSION ACCOMPLISHED' : 'CRITICAL FAILURE'}
                                    </h2>
                                    <p className="text-slate-400 font-mono">
                                        PLAYER {playerScore} - CPU {computerScore}
                                    </p>
                                </div>

                                {/* Button to continue after the game is over.*/}
                                <HoloButton onClick={onComplete} color={playerScore > computerScore ? 'emerald' : 'rose'}>
                                    Continue
                                </HoloButton>
                            </div>
                        ) : (
                            <div className="grid grid-cols-3 gap-4">
                                {choices.map(choice => (
                                    <HoloButton
                                        key={choice}
                                        onClick={() => handleChoice(choice)}
                                        disabled={gameOver || rolling}
                                    >
                                        <img
                                            src={choiceSVGS[choice]}
                                            alt={choice}
                                            className="w-6 h-6 invert opacity-80 group-hover:opacity-100 group-hover:scale-110 transition-all"
                                        />
                                        <span className="hidden md:inline text-sm">{labels[choice]}</span>
                                    </HoloButton>
                                ))}
                            </div>
                        )}
                    </div>

                </div>
            </div>
        </div>
    );

}
export default RockPaperScissors;
