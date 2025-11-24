import React, { useEffect, useState } from 'react';
import { useGame } from '../../context/GameContext';
import { SceneLayout } from '../scene/SceneLayout';
import { DialoguePanel } from '../DialoguePanel';
import { HUD } from '../Hudv2';
import { Button } from '../../ui/Button';
import { Card } from '../../ui/Card';
import { Text } from '../../ui/Text';
import { tokens } from '../../design/tokens';
import {useAudio} from "../../context/AudioContext";

export function PlayGame({ saveId, onBackToMenu }) {
    const {
        currentNode,
        availableChoices,
        playerState,
        loading,
        error,
        loadGame,
        getCurrentNode,
        getNextDialogue,
        makeChoice,
        goBack,
        nextNode,
        getPlayerState,
        clearError,
        gameOver,
        currentSave,
        setGameOver,
    } = useGame();

    const {
        playBackgroundMusic,
        playAmbientSound,
        playChoiceAudio
    } = useAudio();

    // dialogue index for multi-line dialogues loaded in currentNode.Dialogues
    const [dialogueIndex, setDialogueIndex] = useState(0);
    const [showChoices, setShowChoices] = useState(false);

    let isRevisit = false;
    if (currentSave && currentNode?.id) {
        let visitedNodeIds = currentSave.visitedNodeIds;

        // handle string type coming from backend
        if (typeof visitedNodeIds === 'string') {
            try {
                visitedNodeIds = JSON.parse(visitedNodeIds);
            } catch {
                visitedNodeIds = [];
            }
        }

        if (Array.isArray(visitedNodeIds)) {
            // normalize to numbers before comparing
            isRevisit = visitedNodeIds.some(
                (id) => Number(id) === Number(currentNode.id)
            );
        }
    }

    // Load node when saveId changes
    useEffect(() => {
        if (saveId) {
            loadGameData();
        }
    }, [saveId]);
    
    //TODO: there might be a case were we use the backgroundsMusicUrl for ambient sounds for a node, so will see if there is
    // a need to change the nesting of the if statements under
    
    // Play audio when node changes
    useEffect(() => {
        if (!currentNode) return;

        // Normalize visited node ids (string → array)
        let visitedIds = currentSave?.visitedNodeIds;
        if (typeof visitedIds === 'string') {
            try {
                visitedIds = JSON.parse(visitedIds);
            } catch {
                visitedIds = [];
            }
        }

        // Check if this node is a revisit
        const isNodeRevisit =
            Array.isArray(visitedIds) &&
            visitedIds.some((id) => Number(id) === Number(currentNode.id));

        if (isNodeRevisit) {
            // For revisits:
            setDialogueIndex(0);
            setShowChoices(true);
            playAmbientSound(null);

            return; // Exit before any new ambient or dialogues start
        }
        
        if (currentNode.backgroundMusicUrl) {
            playBackgroundMusic(currentNode.backgroundMusicUrl);
        }
        
        if (currentNode.ambientSoundUrl) {
            playAmbientSound(currentNode.ambientSoundUrl, false);
        } else {
            playAmbientSound(null);
        }

        // Dialogue setup
        setDialogueIndex(0);
        const hasDialogues =
            currentNode.dialogues && currentNode.dialogues.length > 0;
        setShowChoices(!hasDialogues);
    }, [currentNode?.id, currentSave?.visitedNodeIds]);

    useEffect(() => {
        const hp = playerState?.health ?? playerState?.hp ?? 100;
        const shouldBeGameOver = hp <= 0;

        if (shouldBeGameOver && !gameOver) {
            setGameOver(true);
        }
    }, [playerState, gameOver, setGameOver]);

    // load current node from backend
    const loadGameData = async () => {
        try {
            clearError();
            await getCurrentNode(saveId);
            setShowChoices(false);
            setDialogueIndex(0);
        } catch (err) {
            console.error('Failed to load game data:', err);
        }
    };

    // Resolve the current dialogue using currentNode.dialogues and dialogueIndex
    const dialogues = currentNode?.dialogues || [];
    const currentDialogue = dialogues.length > 0 ? dialogues[dialogueIndex] : null;

    // Try to resolve character image:
    const resolveCharacterImage = () => {
        // 1) Check if dialogue has character image directly
        if (currentDialogue?.characterImageUrl) {
            return currentDialogue.characterImageUrl;
        }

        // 2) Fallback to checking characters array
        const chars = currentNode?.charactersInScene
            || currentNode?.characters;

        if (chars && currentDialogue?.characterId) {
            const found = chars.find(c => c.id === currentDialogue.characterId);
            if (found?.imageUrl) return found.imageUrl;
        }

        // 3) Default avatar
        return '/assets/characters/hero.png';
    };

    const characterImageUrl = resolveCharacterImage();

    // Handle making a choice
    const handleChoice = async (choice) => {
        try {
            // Play choice audio if present on the choice
            if (choice.audioUrl) playChoiceAudio(choice.audioUrl);

            // Call makeChoice
            const res = await makeChoice(saveId, choice.id);

            // After making a choice, refresh the current node and playerState
            await loadGameData();

            // Reset dialogue index
            setDialogueIndex(0);
            setShowChoices(false);

        } catch (err) {
            console.error('Failed to make choice:', err);
        }
    };

    // Next dialogue (advance through currentNode.dialogues)
    const handleNextDialogue = async () => {
        try {
            // If there is a next dialogue, advance index
            if (dialogues && dialogueIndex + 1 < dialogues.length) {
                setDialogueIndex((prev) => prev + 1);
            } else {
                // end of dialogues -> show choices
                setShowChoices(true);
            }
        } catch (err) {
            console.error('Failed to advance dialogue:', err);
        }
    };

    // Loading / error / empty safeguards
    if (loading && !currentNode) {
        return (
            <div
                style={{
                    minHeight: '100vh',
                    background: tokens.color.bg,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center'
                }}
            >
                <Text size={18} style={{ color: tokens.color.textMuted }}>
                    Loading game...
                </Text>
            </div>
        );
    }

    if (error) {
        return (
            <div
                style={{
                    minHeight: '100vh',
                    background: tokens.color.bg,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    padding: tokens.space.lg
                }}
            >
                <Card
                    style={{
                        padding: tokens.space.xl,
                        background: tokens.color.surface,
                        border: `1px solid ${tokens.color.danger}`,
                        borderRadius: tokens.radius.lg
                    }}
                >
                    <Text
                        size={16}
                        style={{
                            color: tokens.color.danger,
                            marginBottom: tokens.space.lg
                        }}
                    >
                        Error loading game: {error}
                    </Text>
                    <Button onClick={loadGameData}>Retry</Button>
                </Card>
            </div>
        );
    }

    if (!currentNode) {
        return (
            <div
                style={{
                    minHeight: '100vh',
                    background: tokens.color.bg,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center'
                }}
            >
                <Text size={18} style={{ color: tokens.color.textMuted }}>
                    No game data available
                </Text>
            </div>
        );
    }

    // compute whether the player is dead / game over
    const hp = playerState?.health ?? playerState?.hp ?? 100;
    const isGameOver = (typeof gameOver === 'boolean') ? gameOver : hp <= 0;

    console.log('Debug:', {
        isRevisit,
        showChoices,
        availableChoices: availableChoices?.length,
        currentNodeId: currentNode?.id,
        visitedNodeIds: currentSave?.visitedNodeIds
    });
    
    return (
        <div
            style={{
                minHeight: '100vh',
                background: tokens.color.bg,
                paddingTop: '1px'
            }}
        >
            {/* HUD now receives playerState and currentNode directly */}
            <HUD
                playerState={playerState}
                currentNode={currentNode}
                onBackToMenu={onBackToMenu}
            />

            {/* Main Game Content */}
            <div
                style={{
                    width: '100%',
                    height: 'calc(100vh - 48px)',
                    position: 'relative',
                    border: '2px solid #00A2FF',
                    borderRadius: '12px',
                    overflow: 'hidden',
                    margin: '40px',
                    boxSizing: 'border-box'
                }}
            >
                {/* Scene/Background */}
                <div
                    style={{
                        width: '100%',
                        height: '100%',
                        backgroundImage: `url(${currentNode.backgroundUrl || '/assets/bg/space-tunnel.png'})`,
                        backgroundSize: 'cover',
                        backgroundPosition: 'center'
                    }}
                />

                {/* Dialogue Panel - Fixed at Bottom */}
                {!isGameOver && (
                    <div
                        style={{
                            position: 'absolute',
                            bottom: '16px',
                            left: '16px',
                            right: '16px',
                            backgroundColor: '#0a0f1a',
                            border: '2px solid #3ae6ff',
                            padding: '12px 16px',
                            display: 'flex',
                            gap: '12px',
                            imageRendering: 'pixelated',
                            color: '#d8faff',
                            boxShadow: '0 0 8px #003644, 0 0 2px #3ae6ff inset',
                            alignItems: 'flex-start'
                        }}
                    >
                        {/* Avatar - only show if NOT a revisit */}
                        {!isRevisit && (
                            <div
                                style={{
                                    flex: '0 0 96px',
                                    height: '96px',
                                    border: '2px solid #3ae6ff',
                                    backgroundColor: '#000',
                                    overflow: 'hidden',
                                    imageRendering: 'pixelated'
                                }}
                            >
                                <img
                                    src={characterImageUrl}
                                    alt="Character"
                                    style={{
                                        width: '100%',
                                        height: '100%',
                                        objectFit: 'cover',
                                        imageRendering: 'pixelated'
                                    }}
                                />
                            </div>
                        )}

                        {/* Text + Choices + Button wrapper as row */}
                        <div
                            style={{
                                flex: 1,
                                display: 'flex',
                                flexDirection: 'row',
                                alignItems: 'flex-start'
                            }}
                        >
                            <div style={{ flex: 1, paddingRight: '12px' }}>
                                {/* Only show dialogue text if NOT a revisit */}
                                {!isRevisit && (
                                    <p
                                        style={{
                                            color: '#FFFFFF',
                                            lineHeight: '1.4',
                                            margin: 0,
                                            marginBottom: showChoices ? '12px' : '0',
                                            fontFamily: '"visitor1", monospace',
                                            textTransform: 'uppercase',
                                            letterSpacing: '0.05em'
                                        }}
                                    >
                                        {currentDialogue?.text || currentNode.description || 'Welcome to the adventure!'}
                                    </p>
                                )}

                                {/* Show choices when appropriate - EITHER showChoices is true OR it's a revisit */}
                                {(showChoices || isRevisit) && (availableChoices?.length > 0) && (
                                    <div
                                        style={{
                                            marginTop: isRevisit ? '0' : '12px',
                                            display: 'flex',
                                            flexDirection: 'column',
                                            gap: '8px'
                                        }}
                                    >
                                        {availableChoices.map((choice) => (
                                            <button
                                                key={choice.id}
                                                onClick={() => handleChoice(choice)}
                                                disabled={loading}
                                                style={{
                                                    padding: '8px 12px',
                                                    background: '#0a0f1a',
                                                    border: '2px solid #3ae6ff',
                                                    borderRadius: '0px',
                                                    color: '#d8faff',
                                                    fontSize: '20px',
                                                    lineHeight: '1.4',
                                                    fontFamily: '"visitor1", monospace',
                                                    cursor: loading ? 'not-allowed' : 'pointer',
                                                    textAlign: 'left',
                                                    boxShadow: '0 0 6px #003644, 0 0 2px #3ae6ff inset',
                                                    imageRendering: 'pixelated'
                                                }}
                                                onMouseEnter={(e) => {
                                                    if (!loading) e.target.style.background = '#112032';
                                                }}
                                                onMouseLeave={(e) => {
                                                    e.target.style.background = '#0a0f1a';
                                                }}
                                            >
                                                {choice.text}
                                            </button>
                                        ))}
                                    </div>
                                )}
                            </div>

                            {/* Next button column - only show if NOT revisit AND NOT showing choices */}
                            {!showChoices && !isRevisit && (
                                <div
                                    style={{
                                        flex: '0 0 auto',
                                        display: 'flex'
                                    }}
                                >
                                    <button
                                        onClick={handleNextDialogue}
                                        disabled={loading}
                                        style={{
                                            padding: '8px 12px',
                                            background: '#0a0f1a',
                                            border: '2px solid #3ae6ff',
                                            borderRadius: '0px',
                                            color: '#d8faff',
                                            fontSize: '20px',
                                            lineHeight: '1.4',
                                            fontWeight: 'bold',
                                            fontFamily: '"visitor1", monospace',
                                            cursor: loading ? 'not-allowed' : 'pointer',
                                            boxShadow:
                                                '0 0 6px #003644, 0 0 2px #3ae6ff inset',
                                            imageRendering: 'pixelated',
                                            whiteSpace: 'nowrap'
                                        }}
                                        onMouseEnter={(e) => {
                                            if (!loading) e.target.style.background = '#112032';
                                        }}
                                        onMouseLeave={(e) => {
                                            e.target.style.background = '#0a0f1a';
                                        }}
                                    >
                                        {loading ? '...' : 'NEXT'}
                                    </button>
                                </div>
                            )}
                        </div>
                    </div>
                )}

                {/* Game Over overlay */}
                {isGameOver && (
                    <div style={{
                        position: 'fixed',
                        top: 0,
                        left: 0,
                        right: 0,
                        bottom: 0,
                        background: 'rgba(0,0,0,0.95)',
                        display: 'flex',
                        flexDirection: 'column',
                        justifyContent: 'center',
                        alignItems: 'center',
                        zIndex: 9999,
                        color: '#FF4D4D',
                        fontFamily: '"visitor1", monospace',
                        textAlign: 'center',
                        padding: 24
                    }}>
                        <div style={{ fontSize: 36, letterSpacing: 2 }}>████ GAME OVER ████</div>
                        <div style={{ marginTop: 16, color: '#fff', fontSize: 18 }}>
                            Your journey ends here.
                        </div>
                        <div style={{ marginTop: 24 }}>
                            <Button onClick={onBackToMenu}>Return to Menu</Button>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
}