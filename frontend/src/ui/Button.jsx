import React from 'react';
import { tokens } from '../design/tokens';

//reusable button with hover and press animations
export function Button ({ children, style, ...rest }) {
    return( 
        <button
            {...rest}
            style={{
                width: '100%',
                padding: '10px 16px',
                background: tokens.color.primary,
                color: '#071318',
                border: '1px solid ' + tokens.color.primary,
                fontWeight: 700,
                cursor: 'pointer',
                transition: 'background 120ms ease, transform 120ms ease',
                ...style

            }}
            onMouseDown={(e)=> e.currentTarget.style.transform='translateY(1px)'}
            onMouseUp={(e)=> e.currentTarget.style.transform='translateY(0)'}
            onMouseLeave={(e)=> e.currentTarget.style.transform='translateY(0)'}
            onMouseOver={(e)=> e.currentTarget.style.background=tokens.color.primaryHover}
            onMouseOut={(e)=> e.currentTarget.style.background=tokens.color.primary}
            >
            {children}
        </button>
            
    );
}