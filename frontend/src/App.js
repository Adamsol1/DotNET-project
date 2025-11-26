import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom'
import React, {useEffect, useState} from 'react';
import './App.css';
import { GameProvider } from './context/GameContext';
import { AudioProvider } from './context/AudioContext';
import { AccountManagement } from './pages/AccountManagement';
import { Home } from './pages/Home';
import { Game } from './pages/Game';
import { tokens } from './design/tokens';
import {AuthProvider} from "./context/Authentication";

//set the appContent routes
function AppContent() {
  // set the start page as the home page.
  const [currentPage, setCurrentPage] = useState(() => {

    // save the page the user is on to the local storage.
    const savedPage = localStorage.getItem('currentPage');
    return savedPage || 'home';
  });

  // if the page changes, save the new page to the local storage.
  useEffect(() => {
    localStorage.setItem('currentPage', currentPage);
  }, [currentPage]);

  // navigate to a different page
  const navigate = (page) => {
    setCurrentPage(page);
  };

  // render the current page based on the currentPage state.
  const renderPage = () => {
    switch (currentPage) {
      case 'home':
        return <Home onNavigate={navigate} />;
      case 'game':
        return <Game onNavigate={navigate} />;
      case 'account':
        return <AccountManagement onNavigate={navigate} />;
      default:
        return <Home onNavigate={navigate} />;
    }
  };

  return (
    <div style={{
      minHeight: '100vh',
      background: tokens.color.bg,
      color: tokens.color.text
    }}>
      {renderPage()}
    </div>
  );
}

// Setup of the app with the GameProvider, AudioProvider, and AppContent
export default function App() {
  return (
    <AuthProvider>
      <GameProvider>
        <AudioProvider>
          <AppContent />
        </AudioProvider>
      </GameProvider>
    </AuthProvider>
  );
}

/* Dev approach (commented out - uses React Router instead of custom routing):
import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { GameProvider } from './context/GameContext';
import { AudioProvider } from './context/AudioContext';
import { AccountManagement } from './pages/AccountManagement';
import { Home } from './pages/Home';
import { Game } from './pages/Game';
import './App.css';

function App() {
  return (
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
  );
}

export default App;
*/
