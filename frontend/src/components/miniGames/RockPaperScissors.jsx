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

    // function to handle the choice. and get the winner by result
    const handleChoice = (choice) => {
        // if the game is over, do nothing.
        if (gameOver) return;

        // get the computer's choice.
        const computerChoice = getComputerChoice();

        // calculate the winner 
        const winner = calculateRound(choice, computerChoice);

        // calculate the winner of the round.
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
                    playerChoice: choice,
                    computerChoice,
                    winner: 'player',
                    roundWinner: 'player',
                });
                // if onWin is defined, call it. else nothing happens.
                onWin?.();
                return;
            }
        } else if (winner === 'computer') {
            // increment the computer's score.
            const newComputerScore = computerScore + 1;
            setComputerScore(newComputerScore);

            if (newComputerScore >= winScore) {
                setGameOver(true);
                setLastResult({
                    playerChoice: choice,
                    computerChoice,
                    winner: 'computer',
                    roundWinner: 'computer',
                });
                onLose?.();
                return;
            }
        }

        // set the last result to the choice and computer choice.
        setLastResult({
            playerChoice: choice,
            computerChoice,
            winner,
            roundWinner: winner,
        });

        // increment the rounds.
        setRounds(prev => prev + 1);
    };

   return (
        <Card style={{ padding: '24px', maxWidth: '500px', margin: '20px auto' }}>
            <Text size={24} style={{ marginBottom: '16px', textAlign: 'center' }}>
                Rock Paper Scissors
            </Text>
            
            <Text style={{ marginBottom: '8px', textAlign: 'center' }}>
                Best of 2 - First to {winScore} wins!
            </Text>

            <Text style={{ marginBottom: '16px', textAlign: 'center' }}>
                Score: You {playerScore} - {computerScore} Machine
            </Text>

            {lastResult && (
                <Card style={{ 
                    padding: '12px', 
                    marginBottom: '16px', 
                    backgroundColor: lastResult.roundWinner === 'player' 
                        ? 'rgba(61, 183, 216, 0.2)' 
                        : lastResult.roundWinner === 'computer' 
                        ? 'rgba(230, 90, 90, 0.2)' 
                        : 'rgba(169, 184, 196, 0.2)'
                }}>
                    <Text style={{ textAlign: 'center', marginBottom: '4px' }}>
                        You chose: <strong>{labels[lastResult.playerChoice]}</strong>
                    </Text>
                    <Text style={{ textAlign: 'center', marginBottom: '4px' }}>
                        Maskin valgte: <strong>{labels[lastResult.computerChoice]}</strong>
                    </Text>
                    <Text style={{ textAlign: 'center', fontWeight: 'bold' }}>
                        {lastResult.roundWinner === 'tied' 
                            ? 'It was a tie!' 
                            : lastResult.roundWinner === 'player' 
                            ? 'You won this round!' 
                            : 'The machine won this round!'}
                    </Text>
                </Card>
            )}

            {gameOver ? (
                <div>
                    <Text 
                        size={20} 
                        style={{ 
                            marginBottom: '16px', 
                            textAlign: 'center',
                            fontWeight: 'bold',
                            color: playerScore > computerScore ? '#3db7d8' : '#e65a5a'
                        }}
                    >
                        {playerScore > computerScore 
                            ? 'You won the game!' 
                            : 'You lost the game!'}
                    </Text>
                    <Text style={{ marginBottom: '16px', textAlign: 'center' }}>
                        Final score: {playerScore} - {computerScore}
                    </Text>
                    <Button 
                        onClick={onComplete} 
                        style={{ width: '100%' }}
                    >
                        Continue
                    </Button>
                </div>
            ) : (
                <div style={{ display: 'flex', gap: '12px', justifyContent: 'center', flexWrap: 'wrap' }}>
                    {choices.map(choice => (
                        <Button
                            key={choice}
                            onClick={() => handleChoice(choice)}
                            disabled={gameOver}
                        >
                            {labels[choice]}
                        </Button>
                    ))}
                </div>
            )}
        </Card>
    );
}

export default RockPaperScissors;