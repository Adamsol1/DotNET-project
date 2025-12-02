import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { validateLoginForm, validateRegisterForm } from '../utils/validation';
 import { useNavigate } from 'react-router-dom'; 

// component imports . gameContext has api calls and game state management.
//import { useGame } from '../context/GameContext';
// planet, spacehsip, stars components. are for the background animation.
import Planet from '../components/Home/Planet';
import Spaceship from '../components/Home/Spaceship';
import Stars from '../components/Home/Stars';

import {useAuth} from "../context/Authentication";
import * as authservice from "../endpoints/AuthenticationService";
// alert modal for unsaved changes.
import AlertModal from '../components/AlertModal';

export function Home() {
  const { user, logout, login, register } = useAuth();
  //CHAT
  const  authenticated= !!user;

  //check the role of the user, we create a button for the admin to access the admin page.
  const isAdmin = user?.role === "admin";


  const navigate = useNavigate();
  // const { authenticated, user, logout, login, register } = useGame();
  const [activeTab, setActiveTab] = useState('login');
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [validationErrors, setValidationErrors] = useState({});

  const [showLeaveAlert, setShowLeaveAlert] = useState(false);
  const [showLogoutAlert, setShowLogoutAlert] = useState(false);
  const hasUnsavedChanges = username !== '' || password !== '';
  const normalizeUsername = (value) => (value || '').toLowerCase();

  // log inn the user, call the login function. from api.auth.login
  const handleLogin = async () => {
    // validate the form.
    setError("");
    setValidationErrors({});

    const validationResult = validateLoginForm(username, password);

    if (!validationResult.isValid) {
      setValidationErrors(validationResult.errors);
      return;
    }
    
    try {
      // passes inn the username and password captured from the form 
      // to the login function.
      await login({ username, password });

    } catch (error) {
      console.error('Login failed:', error);
      const errorMessage = error.response?.data?.message || error.message || 'Login failed';
      setError(errorMessage);
    }
  }

  // register the user, call the register function. from api.auth.register
  const handleRegister = async () => {
    setError("");
    setValidationErrors({});

    const validationResult = validateRegisterForm(username, password);

    if (!validationResult.isValid) {
      setValidationErrors(validationResult.errors);
      return;
    }

    try {
      // passes inn the username and password captured from the form 
      // to the register function.
      await authservice.register( username, password );
      
      // Automatically log in after successful registration
      await login({ username, password });
    } catch (error) {
      const errorMessage = error.response?.data?.message || 'Failed to register. Please try again.';
      setError(errorMessage);

      console.error('Register failed:', error);
    }
  }

  // handle the logout: -> function call to logout method.
  // which in the feature just removes the session.
  const handleLogout = async () => {
    try {
      await logout();
      setActiveTab('login');
      setUsername('');
      setPassword('');
    } catch (error) {
      console.error('Logout failed:', error);
    }
  };

  // Warn user if they try to close/refresh the tab
  useEffect(() => {
    const handleBeforeUnload = (e) => {
      if (hasUnsavedChanges) {
        e.preventDefault();
        e.returnValue = ''; 
      }
    };

    window.addEventListener('beforeunload', handleBeforeUnload);
    return () => window.removeEventListener('beforeunload', handleBeforeUnload);
  }, [hasUnsavedChanges]);

  // handle tab click with unsaved changes check
  const handleTabClick = (tab) => {
    if (tab === activeTab) return;
    setError("");
    // if there are unsaved changes on register tab, show the alert modal
    if (activeTab === 'register' && tab === 'login' && hasUnsavedChanges) {
      setShowLeaveAlert(true);
    } else {
      setActiveTab(tab);
      setUsername('');
      setPassword('');
    }
  };

  // handle confirm leave when there are unsaved changes
  const confirmLeave = () => {
    setShowLeaveAlert(false);
    setActiveTab('login');
    setUsername('');
    setPassword('');
  };

  // handle cancel leave
  const cancelLeave = () => {
    setShowLeaveAlert(false);
  };

  const handleLogoutConfirm = async () => {
    setShowLogoutAlert(false);
    await handleLogout();
  };

  // handle the admin button click
  const handleAdminClick = () => {
    if (isAdmin) {
      navigate('/admin');
    } else {
      setError("You are not authorized to access the admin page.");
    }
  }

  // since login becomes essentially the HomePage, 
  // we check if the user is logged in, if yes, then we show play game button.
  // instead of the login form.

  // we are also using tailwind for styling the home page.
  return (
    <div 
      className="relative min-h-screen"
    >
      <audio src="/assets/audio/music/menuMusic.mp3" autoPlay loop hidden />
      <Stars />
      <Planet />
      <Spaceship />
      {/* Main Content */}
      <div className="relative z-10 min-h-screen flex flex-col">
        {/* Header */}
        <header className="p-6 pt-20 flex justify-center items-center">
          <motion.h1
            initial={{ opacity: 0, y: -20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.5 }}
            className="text-7xl font-bold text-white pixel-text tracking-wider text-center w-full"
            style={{ textShadow: '3px 3px 0px rgba(0, 0, 0, 0.8)' }}
          >
            AFTER THE JUMP
          </motion.h1>
        </header>

        {/* Main Content - Login/Register eller Welcome */}
        <main className="flex-1 flex items-center justify-center px-6">
          <motion.div
            initial={{ opacity: 0, scale: 0.9 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ duration: 0.5 }}
            className="w-full max-w-md"
          >
            {authenticated ? (
              // Authenticated View
              <div className="text-center">
                <div className="mb-8">
                  <h2 className="text-2xl font-bold text-white mb-4 pixel-text">
                    Welcome, {user?.username}!
                  </h2>
                  <p className="text-gray-300 mb-8">
                    Ready to continue your adventure?
                  </p>
                </div>
                
                <div className="space-y-4">
                  <motion.button
                    whileHover={{ scale: 1.05 }}
                    whileTap={{ scale: 0.95 }}
                    onClick={() => navigate('/game')}
                    className="w-full px-4 py-3 bg-gray-200 text-black font-bold border-2 border-black"
                  >
                    PLAY GAME
                  </motion.button>

                  <motion.button
                    whileHover={{ scale: 1.05 }}
                    whileTap={{ scale: 0.95 }}
                    onClick={() => navigate('/account')}
                    className="w-full px-4 py-3 bg-gray-200 text-black font-bold border-2 border-black"
                  >
                    ACCOUNT MANAGEMENT
                  </motion.button>

                  {isAdmin && (
                    <motion.button
                      whileHover={{ scale: 1.05 }}
                      whileTap={{ scale: 0.95 }}
                      onClick={handleAdminClick}
                      className="w-full px-4 py-3 bg-gray-200 text-black font-bold border-2 border-black"
                    >
                      ADMIN
                    </motion.button>
                  )}
                  
                  <motion.button
                    whileHover={{ scale: 1.05 }}
                    whileTap={{ scale: 0.95 }}
                    onClick={() => setShowLogoutAlert(true)}
                    className="w-full px-4 py-3 bg-transparent text-white font-bold border-2 border-white"
                  >
                    LOGOUT
                  </motion.button>
                </div>
              </div>
            ) : (
              // Login/Register View
              <>
                {/* Tabs */}
                <div className="flex mb-6 border-2 border-black">
                  <motion.button
                    whileHover={{ scale: 1.02 }}
                    whileTap={{ scale: 0.98 }}
                    onClick={() => handleTabClick('login')}
                    className={`flex-1 px-6 py-3 font-bold transition-colors ${
                      activeTab === 'login' 
                        ? 'bg-blue-600 text-white' 
                        : 'bg-white text-blue-600'
                    }`}
                  >
                    LOGIN
                  </motion.button>
                  <motion.button
                    whileHover={{ scale: 1.02 }}
                    whileTap={{ scale: 0.98 }}
                    onClick={() => handleTabClick('register')}
                    className={`flex-1 px-6 py-3 font-bold transition-colors ${
                      activeTab === 'register' 
                        ? 'bg-blue-600 text-white' 
                        : 'bg-white text-blue-600'
                    }`}
                  >
                    REGISTER
                  </motion.button>
                </div>

                {/* Form */}
                <div className="space-y-4">
                  {/* Error dispaly */}
                  {error && (
                    <motion.div
                    initial={{ opacity: 0, y: -10 }}
                    animate={{ opacity: 1, y: 0 }}
                    className="px-4 py-3 bg-red-600 text-white font-bold border-2 border-red-800 text-center"
                  >
                    {error}
                  </motion.div>
                  )}
                  
                  {/* Username Field */}
                  <motion.div
                    initial={{ x: -20, opacity: 0 }}
                    animate={{ x: 0, opacity: 1 }}
                    transition={{ delay: 0.2 }}
                  >
                    <input
                      type="text"
                      placeholder="Username"
                      value={username}
                      onChange={(e) => {
                        const normalizedUsername = normalizeUsername(e.target.value);
                        setUsername(normalizedUsername);
                        if (validationErrors.username) {
                          setValidationErrors(prev => ({ ...prev, username: [] }));
                        }
                      }}
                      onBlur={() => setUsername((current) => normalizeUsername(current))}
                      className={`w-full px-4 py-3 bg-gray-200 text-black text-center font-bold border-2 lowercase ${
                        validationErrors.username?.length > 0 
                          ? 'border-red-600' 
                          : 'border-black'
                      } focus:outline-none focus:ring-2 focus:ring-blue-500`}
                    />
                    {validationErrors.username?.length > 0 && (
                      <div className="mt-1 text-red-500 text-sm text-center">
                        {validationErrors.username[0]}
                      </div>
                    )}
                  </motion.div>
                  
                  {/* Password Field */}
                  <motion.div
                    initial={{ x: -20, opacity: 0 }}
                    animate={{ x: 0, opacity: 1 }}
                    transition={{ delay: 0.3 }}
                  >
                    <input
                      type="password"
                      placeholder="PASSWORD"
                      value={password}
                      onChange={(e) => {
                        setPassword(e.target.value);
                        if (validationErrors.password) {
                          setValidationErrors(prev => ({ ...prev, password: [] }));
                        }
                      }}
                      className={`w-full px-4 py-3 bg-gray-200 text-black text-center font-bold border-2 ${
                        validationErrors.password?.length > 0 
                          ? 'border-red-600' 
                          : 'border-black'
                      } focus:outline-none focus:ring-2 focus:ring-blue-500`}
                    />
                    {validationErrors.password?.length > 0 && (
                      <div className="mt-1 text-red-500 text-sm text-center">
                        {validationErrors.password[0]}
                      </div>
                    )}
                  </motion.div>
                  
                  {/* Single Dynamic Button */}
                  <motion.div
                    initial={{ y: 20, opacity: 0 }}
                    animate={{ y: 0, opacity: 1 }}
                    transition={{ delay: 0.4 }}
                  >
                    <motion.button
                      whileHover={{ scale: 1.05, boxShadow: '0 0 20px rgba(255, 255, 255, 0.5)' }}
                      whileTap={{ scale: 0.95 }}
                      onClick={activeTab === 'login' ? handleLogin : handleRegister}
                      className="w-full px-4 py-3 bg-gray-200 text-black font-bold border-2 border-black"
                    >
                      {activeTab === 'login' ? 'LOG-IN' : 'REGISTER'}
                    </motion.button>
                  </motion.div>
                </div>
              </>
            )}
          </motion.div>
        </main>
      </div>
      {/* Custom AlertModal for unsaved changes */}
      {showLeaveAlert && (
        <AlertModal
          title="Unsaved Changes"
          message="You have unsaved changes. Are you sure you want to leave?"
          confirmLabel="LEAVE"
          cancelLabel="STAY"
          onCancel={cancelLeave}
          onConfirm={confirmLeave}
        />
      )}
      {showLogoutAlert && (
        <AlertModal
          title="Confirm Logout"
          message="Are you sure you want to log out?"
          confirmLabel="Confirm"
          cancelLabel="CANCEL"
          onConfirm={handleLogoutConfirm}
          onCancel={() => setShowLogoutAlert(false)}
        />
      )}
    </div>
  );
}
