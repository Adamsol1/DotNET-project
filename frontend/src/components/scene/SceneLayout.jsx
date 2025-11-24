import React from 'react';
import { tokens } from '../../design/tokens';
import { Background } from './Background';

//container for an entire scene, handles the background and scene content
export function SceneLayout ({ backgroundUrl, children }) {
    return (
        <div style={{
            position: 'relative',
            width: '100%', 
            height:'78vh', //scene height
            maxWidth:1200, 
            margin: '0 auto',
            overflow: 'hidden',
            border: '1px solid ' + tokens.color.surfaceBorder,
            background: tokens.color.bg,
        }}
        >
            <Background imageUrl={backgroundUrl} /> {/*sets background image*/}
            {children}
            </div>
        );
}