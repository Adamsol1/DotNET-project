import React from 'react';
import { tokens } from '../design/tokens';
import { Card } from '../ui/Card';
import { Text } from '../ui/Text';
import { Button } from '../ui/Button';

//Shows dialogue area with avatar, text and buttons
export function DialoguePanel({ avatarUrl, text, choices  = [], onSelect }){
    return(
        <div style={{ position: 'absolute', left: 24, right: 24, bottom: 24, zIndex: tokens.z.panel }}>
        <Card style={{ padding: 20, display:'grid', gridTemplateColumns: 'auto 1fr 260px', gap: 20}}>
            {/*avatar portrait*/}
            <div style={{
                width:96,
                height:96, 
                overflow:'hidden', 
                border: '1px solid ' + tokens.color.surfaceBorder,
                background: '#1a2a33',
            }}
            >
                {avatarUrl ? (
                <img src={avatarUrl} alt="avatar" style={{ width:'100%', height:'100%', objectFit:'cover'}}/>
                ) : null}
            </div>

            {/* dialogue bubble */}
        <div style={{ display:'flex', alignItems:'center' }}>
          <div style={{
            background:'#070b0d',
            border:'1px solid ' + tokens.color.surfaceBorder,
            padding: '18px 20px',
            maxWidth: '100%',
            boxShadow: tokens.shadow.md
          }}>
            <Text size={14} weight={600}>{text}</Text>
          </div>
        </div>

        {/* Choice buttons */}
        <div style={{ display:'grid', gap:12, alignContent:'center' }}>
          {choices.map(c => (
            <Button key={c.id} onClick={()=>onSelect(c)}>{c.text}</Button>
          ))}
        </div>
      </Card>
    </div>
  );
}
