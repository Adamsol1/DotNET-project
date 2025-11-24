import React from 'react';
import { Button } from '../ui/Button';
import { Text } from '../ui/Text';
import { tokens } from '../design/tokens';

function PixelHPBar({ hp = 0, maxHp = 100 }) {
    const pct = Math.max(0, Math.min(100, Math.round((hp / (maxHp || 1)) * 100)));
    // Width/height are small, then we scale up to get a crisp pixel look.
    const baseW = 64;   // logical "pixels"
    const baseH = 8;    // logical "pixels"
    const scale = 2.5;  // visual size multiplier

    // Choose color by percentage (retro style)
    const fillColor =
        pct > 60 ? '#36FF7C' : pct > 30 ? '#FFD23F' : '#FF4D4D';

    // Make blocky steps every 4px for a pixelated segment feel
    const step = 4;

    return (
        <div
            aria-label="HP bar"
            title={`HP: ${hp}/${maxHp}`}
            style={{
                display: 'inline-flex',
                alignItems: 'center',
                gap: 8,
            }}
        >
            {/* Frame (scaled) */}
            <div
                style={{
                    width: baseW * scale,
                    height: baseH * scale,
                    imageRendering: 'pixelated',
                    boxShadow: '0 0 0 2px #00A2FF inset, 0 0 10px rgba(0,162,255,0.3)',
                    background:
                    // dark checkerboard background for the empty area
                        `repeating-linear-gradient(
               90deg,
               rgba(0,0,0,0.75) 0px,
               rgba(0,0,0,0.75) ${step * scale}px,
               rgba(0,0,0,0.85) ${step * scale}px,
               rgba(0,0,0,0.85) ${step * 2 * scale}px
             )`,
                    border: '1px solid #003B5C',
                    position: 'relative',
                    borderRadius: 2,
                    overflow: 'hidden',
                }}
            >
                {/* Fill */}
                <div
                    style={{
                        width: `${pct}%`,
                        height: '100%',
                        // stepped fill for pixel vibe
                        background: `repeating-linear-gradient(
              90deg,
              ${fillColor} 0px,
              ${fillColor} ${step * scale}px,
              ${fillColor} ${step * scale}px,
              ${fillColor} ${step * 2 * scale}px
            )`,
                        // subtle inner highlight
                        boxShadow: 'inset 0 0 6px rgba(255,255,255,0.12)',
                        transition: 'width 120ms steps(6, end)',
                    }}
                />
                {/* Tiny scanline gloss */}
                <div
                    style={{
                        position: 'absolute',
                        left: 0,
                        right: 0,
                        top: 0,
                        height: Math.ceil(baseH * scale * 0.35),
                        background:
                            'linear-gradient(to bottom, rgba(255,255,255,0.15), rgba(255,255,255,0))',
                        pointerEvents: 'none',
                    }}
                />
            </div>

            {/* Numeric HP (small, retro) */}
            <Text size={12} weight={700} style={{ color: '#9EE7FF', letterSpacing: 0.5}}>
                {hp}/{maxHp}
            </Text>
        </div>
    );
}

export function HUD({ playerState, currentNode, onBackToMenu }) {
    // Handle both 'health' and 'hp' property names
    const hp = playerState?.health ?? playerState?.hp ?? 0;
    const maxHp = playerState?.maxHealth ?? playerState?.maxHp ?? 100;

    return (
        <div
            style={{
                position: 'fixed',
                top: 0,
                left: 0,
                right: 0,
                zIndex: tokens.z.hud,
                display: 'flex',
                alignItems: 'center',
                background: 'rgba(0, 0, 0, 0.9)',
                borderBottom: `2px solid #00A2FF`,
                padding: '8px 16px',
                height: '48px',
                boxShadow: '0 2px 10px rgba(0, 0, 0, 0.5)',
            }}
        >
            {/* Left: Pixel HP bar */}
            <div style={{ flex: '0 0 auto' }}>
                <PixelHPBar hp={hp} maxHp={maxHp} />
            </div>

            {/* Center: Title (truly centered regardless of left/right widths) */}
            <div
                style={{
                    position: 'absolute',
                    left: '50%',
                    transform: 'translateX(-50%)',
                    maxWidth: '60%',
                    whiteSpace: 'nowrap',
                    overflow: 'hidden',
                    textOverflow: 'ellipsis',
                    textAlign: 'center',
                    pointerEvents: 'none', // lets clicks pass through if needed
                }}
            >
                {currentNode && (
                    <Text size={14} weight={700} style={{ color: '#00A2FF', textShadow: '0 0 8px rgba(0,162,255,0.35)' }}>
                        {currentNode.title}
                    </Text>
                )}
            </div>

            {/* Right: Menu */}
            <div style={{ marginLeft: 'auto', display: 'flex', alignItems: 'center', gap: 8 }}>
                <Button
                    onClick={onBackToMenu}
                    style={{
                        padding: '0px 16px',
                        fontSize: 20,
                        background: 'rgba(0, 162, 255, 0.2)',
                        border: `1px solid #00A2FF`,
                        color: '#FFFFFF',
                    }}
                >
                    Menu
                </Button>
            </div>
        </div>
    );
}