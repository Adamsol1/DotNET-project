import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { GameProvider } from './context/GameContext';
import { AudioProvider } from './context/AudioContext';
import { AccountManagement } from './pages/AccountManagement';
import { Home } from './pages/Home';
import { Game } from './pages/Game';
import './App.css';
import { AuthProvider } from './context/Authentication';

function App() {
  
  return (
    <AuthProvider>
    <GameProvider>
      <AudioProvider>

        <BrowserRouter>
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/game" element={<Game />} />
            <Route path="/account" element={<AccountManagement />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </BrowserRouter>

      </AudioProvider>
    </GameProvider>
    </AuthProvider>
  );
}

export default App;