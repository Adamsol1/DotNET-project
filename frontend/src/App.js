import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { GameProvider } from './context/GameContext';
import { AudioProvider } from './context/AudioContext';
import { AccountManagement } from './pages/AccountManagement';
import { Home } from './pages/Home';
import { Game } from './pages/Game';
import { Admin } from './pages/Admin';
import './App.css';
import { AuthProvider } from './context/Authentication';

// Main application component that sets up routing and context providers.
// It includes routes for Home, Game, Account Management, and Admin pages.
// Any undefined route redirects to the Home page.
// Context providers for game state, audio settings, and authentication wrap the entire application.

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
            <Route path="/admin" element={<Admin />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </BrowserRouter>

      </AudioProvider>
    </GameProvider>
    </AuthProvider>
  );
}

export default App;