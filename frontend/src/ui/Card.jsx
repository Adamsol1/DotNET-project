import React from 'react';
import { tokens } from '../design/tokens';

//simple container with color, rounded corners and shadow
export function Card ({ style, children }) {
    return( 
        <div style={{
            background: tokens.color.surface,
            border: '1px solid ' + tokens.color.surfaceBorder,
            backdropFilter: 'blur(6px)', //blur effect for glass look 
            boxShadow: tokens.shadow.md,
            ...style
        }}>
            {children}
        </div>
    );
}