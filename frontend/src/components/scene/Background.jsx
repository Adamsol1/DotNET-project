import React from 'react';

//renders the scene background 
export function Background ({ imageUrl }) {
    const style = imageUrl
    ? {
        backgroundImage: 'url(' + imageUrl + ')',
        backgroundSize: 'cover',
        backgroundPosition: 'center',
    }
  : {
    background:
    'radial-gradient(1200px 600px at 50% 20%, #142430 0%, #0b0f14 60%)', 
    };

    return (
        <div
            style={{ position: 'absolute', inset: 0, ...style }}
            aria-label="bg" //for screen readers
            />
        );
 
}