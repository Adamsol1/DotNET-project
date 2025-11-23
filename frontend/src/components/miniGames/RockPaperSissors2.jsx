import React, { useState } from 'react';
import { Button } from '../../ui/Button';
import { Card } from '../../ui/Card';
import { Text } from '../../ui/Text';


// constant choice array.
const choices = ['rock', 'paper', 'scissors'];

// the the score needed to win a game is 2.
const winScore = 2;

// maybe a level system they choose to play at.
//const levels = [1, 2, 3];

// labels for the choices.
const labels = {
    rock: 'Rock',
    paper: 'Paper',
    scissors: 'Scissors',
};

// SVGS that displays the choices that player and machine has picked.
// svgs are used from https://www.svgrepo.com/ 20.11.2025
const choiceSVGS = {
    rock: "/assets/icons/asteroid-2-svgrepo-com.svg",
    paper: "/assets/icons/paper-svgrepo-com.svg",
    scissors: "/assets/icons/scissors-svgrepo-com.svg",
}

// Game function
// onComplete: function to call when the game is complete.
// onWin: function to call when the user wins.
// onLose: function to call when the user loses.
const RockPaperScissors = ({ onComplete, onWin, onLose}) => {
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
        // takes the choice array, sets value between 0 and 1. 
        // and multiplies it by the length of the array.
        // then it rounds it down to the nearest integer.
        // and returns the choice at that index.
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
        // will be updated after the animation.
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
    // function to display the choice,
    // ads the machine and the players choice into the UI
    const ChoiceDisplay = ({ choice, side }) => {

        return (
            <div className={`flex flex-col items-center justify-center w-1/2 transition-all duration-300 ${side === 'left' ? 'order-1' : 'order-3'}`}>
                {/* Label */}
                <div className={`text-sm font-bold tracking-widest mb-3 ${side === 'left' ? 'text-cyan-400' : 'text-rose-400'}`}>
                    {side === 'left' ? 'PLAYER' : 'CPU'}
                </div>

                {/* Icon Circle */}
                <div className={`
                relative flex items-center justify-center 
                w-32 h-32 rounded-full bg-gray-800 
                border-4 border-gray-700 
                transition-all duration-300
                ${rolling ? 'animate-pulse' : ''}
            `}>
                    {choice ? (
                        <img
                            src={choiceSVGS[choice]}
                            alt={choice}
                            className={`w-16 h-16 drop-shadow-lg transition-transform duration-300 `}
                        />
                    ) : (
                        <span className="text-4xl opacity-20">?</span>
                    )}
                </div>

                {/* Text Description */}
                <div className="mt-4 h-8 flex items-center justify-center">
                <span className="text-lg font-medium text-gray-300">
                    {choice ? labels[choice].toUpperCase() : (rolling ? 'ROLLING...' : 'WAITING')}
                </span>
                </div>
            </div>
        );
    };

    return (
        <Card style={{ padding: '40px', maxWidth: '900px', width: '90%',
            margin: '20px auto', backgroundColor: 'rgba(20, 20, 30, 0.95)',
            border: '2px solid rgba(255, 255, 255, 0.1)',
            borderRadius: '16px',
        }}>

            <Text size={32} style={{ marginBottom: '12px', textAlign: 'center' }}>
                Rock Paper Scissors
            </Text>

            <Text style={{ marginBottom: '8px', textAlign: 'center' }}>
                First to {winScore} wins the game!
            </Text>

            <div style={{
                marginBottom: '32px',
                textAlign: 'center',
                padding: '16px',
                backgroundColor: 'rgba(0, 0, 0, 0.3)',
                borderRadius: '8px'
            }}>
                <Text style={{ fontSize: '24px', fontWeight: 'bold' }}>
                    Score: <span style={{ color: '#3db7d8' }}>You {playerScore}</span> - <span style={{ color: '#e65a5a' }}>{computerScore} PC</span>
                </Text>

            </div>

            <div style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                marginBottom: '40px',
                padding: '40px 20px',
                backgroundColor: 'rgba(0, 0, 0, 0.4)',
                borderRadius: '12px',
                minHeight: '250px',
                border: '2px solid rgba(255, 255, 255, 0.1)'
            }}>
                {/* display the choice of the user and machine. */}

                <ChoiceDisplay
                    choice={playerChoice}
                    side="left"
                />

                <div className="text-4xl font-bold text-yellow-400" style={{ minWidth: '60px', textAlign: 'center' }}>
                    VS
                </div>

                <ChoiceDisplay
                    choice={computerChoice}
                    side="right"
                />

            </div>

            {/* show the round winner */}
            {roundWinner && !gameOver && (
                <div style={{
                    marginBottom: '24px',
                    padding: '20px',
                    textAlign: 'center',
                    backgroundColor: roundWinner === 'player'
                        ? 'rgba(61, 183, 216, 0.2)'
                        : roundWinner === 'computer'
                            ? 'rgba(230, 90, 90, 0.2)'
                            : 'rgba(255, 255, 0, 0.2)',
                    borderRadius: '8px',
                    border: `2px solid ${roundWinner === 'player'
                        ? 'rgba(61, 183, 216, 0.5)'
                        : roundWinner === 'computer'
                            ? 'rgba(230, 90, 90, 0.5)'
                            : 'rgba(255, 255, 0, 0.5)'}`
                }}>
                    <Text style={{
                        fontSize: '22px',
                        fontWeight: 'bold',
                        color: roundWinner === 'player'
                            ? '#3db7d8'
                            : roundWinner === 'computer'
                                ? '#e65a5a'
                                : '#ffd700'
                    }}>
                        {roundWinner === 'tied'
                            ? 'IT WAS A TIE!'
                            : roundWinner === 'player'
                                ? 'YOU WON THIS ROUND!'
                                : 'MACHINE WON THIS ROUND!'}
                    </Text>
                </div>
            )}


            {gameOver ? (
                <div>

                    <div style={{ marginBottom: '24px', padding: '30px', textAlign: 'center',
                        backgroundColor: playerScore > computerScore
                            ? 'rgba(61, 183, 216, 0.2)'
                            : 'rgba(230, 90, 90, 0.2)',
                        borderRadius: '12px',
                        border: `2px solid ${playerScore > computerScore ? 'rgba(19, 243, 154, 0.5)' : 'rgba(245, 71, 71, 0.5)'}`
                    }}>

                        <Text size={28}
                              style={{
                                  marginBottom: '16px',
                                  fontWeight: 'bold',
                                  color: playerScore > computerScore ? '#3db7d8' : '#e65a5a'
                              }}>
                            {playerScore > computerScore ? 'YOU WON THE GAME!' : 'YOU LOST THE GAME!'}
                        </Text>
                        <Text size={24} style={{ marginBottom: '16px' }}>
                            Final score: Player {playerScore} - PC {computerScore}
                        </Text>
                    </div>
                    {/* Button to continue after the game is over.*/}
                    <Button onClick={onComplete} style={{ width: '100%' }}>
                        Continue
                    </Button>
                </div>

            ) : (
                <div style={{ display: 'flex', gap: '12px', justifyContent: 'center', flexWrap: 'wrap' }}>
                    {choices.map(choice => (
                        <Button
                            key={choice}
                            onClick={() => handleChoice(choice)}
                            disabled={gameOver || rolling}
                            style={{
                                padding: '20px 40px',
                                fontSize: '20px',
                                fontWeight: 'bold',
                                minWidth: '150px',
                                opacity: rolling ? 0.5 : 1,
                                cursor: rolling ? 'not-allowed' : 'pointer'
                            }}
                        >
                            <img src={choiceSVGS[choice]} alt={choice} style={{ width: '24px', height: '24px', display: 'inline-block', marginRight: '8px', verticalAlign: 'middle' }} />
                            {labels[choice]}
                        </Button>
                    ))}
                </div>
            )}
        </Card>
    );
}

export default RockPaperScissors;