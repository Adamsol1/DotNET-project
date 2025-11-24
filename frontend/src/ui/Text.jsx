import React from 'react';
import { tokens } from '../design/tokens';

//consistent text element with optional color size and weight 
export function Text({as: Tag = 'p', muted=false, size=16, weight= 500, style, children }) {
    return (
        <Tag style={{
            margin: 0,
            color: muted ? tokens.color.textMuted : tokens.color.text,
            fontSize: size,
            fontWeight: weight,
            lineHeight: 1.5,
            ...style
        }}>
            {children}
        </Tag>
    );
}