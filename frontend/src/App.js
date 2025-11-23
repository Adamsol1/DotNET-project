import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
import { GameProvider } from './context/GameContext';
import { AudioProvider } from './context/AudioContext';
import { Home } from './pages/Home';
import { Game } from './pages/Game';
import { AccountManagement } from './pages/AccountManagement';
import { useGame } from './context/GameContext';

/*Did a complete rebuild of this file for browser and navigation management.
* The browser should be able to remeber which route the user is on. 
* */

//protected routes that the user has to be authenticated for. 
const ProtectedRoute = ({ children }) => {
  const { authenticated, authRestored } = useGame();
  
  if (!authRestored) {
    return <div className="loading-screen">
      <div className="loading-screen-content">
        <div className="loading-screen-content-text">
          <h1>Loading...</h1>
        </div>
      </div>
    </div>;
  }
  
  return authenticated ? children : <Navigate to="/" replace />;
};

function AppContent() {
  return(
    <Routes>
      <Route path="/" element={<Home />} />

      <Route path="/game"
      element={
        <ProtectedRoute>
          <Game />
        </ProtectedRoute>
      }
      />
      
      <Route path="/account"
      element={
        <ProtectedRoute>
          <AccountManagement />
        </ProtectedRoute>
      }
      />

      <Route path="*" element={<Navigate to="/" replace />} />

    </Routes>
  );

}

export default function App() {
  return (
    <Router>
      <GameProvider>

        <AudioProvider>

          <AppContent />

        </AudioProvider>

      </GameProvider>
    </Router>

  );
}
