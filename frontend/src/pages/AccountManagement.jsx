import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion'; // For animations
import { useNavigate } from 'react-router-dom'; // For page navigation
// Background components
import Stars from '../components/Home/Stars';
import Planet from '../components/Home/Planet';
import Spaceship from '../components/Home/Spaceship';

import { useGame } from '../context/GameContext'; // Context for API calls and game state
import { useAuth } from '../context/Authentication'; // Context for user authentication
import AlertModal from '../components/shared/AlertModal'; // Modal to warn about unsaved changes



export function AccountManagement() {
  const navigate = useNavigate(); // Navigation hook
  const { updateUsername: updateUsernameGame, updatePassword, deleteAccount } = useGame(); // Functions from context
  const { updateUsername: updateUsernameAuth, logout } = useAuth(); // Auth functions

  // Input field state
  const [username, setUsername] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');

  // Error message state
  const [usernameError, setUsernameError] = useState('');
  const [passwordError, setPasswordError] = useState('');
  const [confirmPasswordError, setConfirmPasswordError] = useState('');

  // State for custom alert modal
  const [showLeaveAlert, setShowLeaveAlert] = useState(false);
  const [pendingNavigation, setPendingNavigation] = useState(null);

  // State for delete confirmation modal
  const [showDeleteModal, setShowDeleteModal] = useState(false);

  // Validates username rules
  const validateUsername = (value) => {
    if (!value) return "Username is required";
    if (value.length < 3) return "Username must be at least 3 characters long";
    if (value.length > 20) return "Username must be less than 20 characters long";
    if (!/^[A-Za-z0-9_]+$/.test(value)) return "Username can only contain letters, numbers, and underscores";
    return '';
  };

  // Validates password rules
  const validatePassword = (value) => {
    if (!value) return "Password is required";
    if (value.length < 8) return "Password must be at least 8 characters long";
    if (value.length > 50) return "Password must be less than 50 characters long";
    if (!/^[A-Za-z0-9!@#$%^&*()_+=-]+$/.test(value)) return "Password can only contain letters, numbers, and special characters";
    return '';
  };
  
  // Handles username update
  const handleUpdateUsername = async (e) => {
    e.preventDefault(); // Prevent page reload
    const error = validateUsername(username); // Validate input
    setUsernameError(error);
    if (error) return; // Stop submit if there is an error

    try {
      await updateUsernameGame({ username });
      updateUsernameAuth(username); // Update Auth context with new username
      setUsername(''); // Reset input
      setUsernameError(''); // Reset error
    } catch (error) {
      console.error('Update failed:', error);
    }
  };

  // Handles password update
  const handleUpdatePassword = async (e) => {
    e.preventDefault();
    const passwordErr = validatePassword(newPassword);
    setPasswordError(passwordErr);

    const confirmErr = newPassword !== confirmPassword ? "Passwords do not match" : '';
    setConfirmPasswordError(confirmErr);

    if (passwordErr || confirmErr) return; // Stop submit if there are errors

    try {
      await updatePassword({ newPassword, confirmPassword });
      // Reset fields and errors
      setNewPassword('');
      setConfirmPassword('');
      setPasswordError('');
      setConfirmPasswordError('');
    } catch (error) {
      console.error('Password update failed:', error);
    }
  };

  // Handles account deletion
  const handleDeleteAccount = async () => {
    try {
      await deleteAccount();
      logout(); // Logout to clear all user data
      navigate('/'); // Redirect home after deletion
    } catch (error) {
      console.error('Account deletion failed:', error);
    }
  };

  

  // Checks for unsaved changes
  const hasUnsavedChanges = username !== '' || newPassword !== '' || confirmPassword !== '';

  // Handles navigation with alert for unsaved changes
  const handleNavigateHome = () => {
    if (hasUnsavedChanges) {
      setPendingNavigation('home');
      setShowLeaveAlert(true);
    } else {
      navigate('/');
    }
  };

  // Warns user when closing/refreshing the page if there are unsaved changes
  useEffect(() => {
    const handleBeforeUnload = (e) => {
      if (hasUnsavedChanges) {
        e.preventDefault();
        e.returnValue = ''; // Shows default browser warning
      }
    };
    window.addEventListener('beforeunload', handleBeforeUnload);
    return () => window.removeEventListener('beforeunload', handleBeforeUnload);
  }, [hasUnsavedChanges]);

  return (
    <div className="relative min-h-screen overflow-auto">
      {/* Background decorative components */}
      <Stars />
      <Planet />
      <Spaceship />

      {/* Foreground container */}
      <div className="relative z-10 flex flex-col items-center justify-start min-h-screen px-6 py-24"
      style={{ overflowY: 'auto', maxHeight: '100vh' }}>
        {/* Animated heading */}
        <motion.h1
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5 }}
          className="text-6xl font-bold text-white tracking-wider text-center mb-12 pixel-text"
          style={{ textShadow: '3px 3px 0px rgba(0,0,0,0.8)' }}
        >
          ACCOUNT MANAGEMENT
        </motion.h1>

        {/* Back to home button */}
        <motion.button
          whileHover={{ scale: 1.05, boxShadow: '0 0 20px rgba(255, 255, 255, 0.5)' }}
          whileTap={{ scale: 0.95 }}
          onClick={handleNavigateHome}
          className="mb-8 px-6 py-3 bg-gray-200 text-black font-bold border-2 border-black"
        >
          BACK TO HOME
        </motion.button>

        <div className="w-full max-w-md space-y-8">
          {/* Update Username Section */}
          <div>
            <h2 className="font-bold text-white mb-2">Update Username</h2>
            <form onSubmit={handleUpdateUsername} className="space-y-3">
              <input
                type="text"
                placeholder="New username"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                className="w-full px-4 py-3 bg-gray-200 text-black text-center font-bold border-2 border-black focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
              {/* Display username error */}
              {usernameError && <p className="text-red-500 text-sm text-center mt-1">{usernameError}</p>}
              <motion.button
                whileHover={{ scale: 1.05 }}
                whileTap={{ scale: 0.95 }}
                type="submit"
                className="w-full px-4 py-3 bg-gray-200 text-black font-bold border-2 border-black"
              >
                SAVE USERNAME
              </motion.button>
            </form>
          </div>

          {/* Update Password Section */}
          <div>
            <h2 className="font-bold text-white mb-2">Update Password</h2>
            <form onSubmit={handleUpdatePassword} className="space-y-3">
              <input
                type="password"
                placeholder="New password"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                className="w-full px-4 py-3 bg-gray-200 text-black text-center font-bold border-2 border-black focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
              {/* Display password error */}
              {passwordError && <p className="text-red-500 text-sm text-center mt-1">{passwordError}</p>}
              <input
                type="password"
                placeholder="Confirm new password"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                className="w-full px-4 py-3 bg-gray-200 text-black text-center font-bold border-2 border-black focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
              {/* Display confirm password error */}
              {confirmPasswordError && <p className="text-red-500 text-sm text-center mt-1">{confirmPasswordError}</p>}
              <motion.button
                whileHover={{ scale: 1.05 }}
                whileTap={{ scale: 0.95 }}
                type="submit"
                className="w-full px-4 py-3 bg-gray-200 text-black font-bold border-2 border-black"
              >
                SAVE PASSWORD
              </motion.button>
            </form>
          </div>

          {/* Delete Account Section */}
          <div>
            <h2 className="font-bold text-red-500 mb-2">Delete Account</h2>
            <p className="text-white mb-2">
              This action is permanent and cannot be undone.
            </p>
            <motion.button
              whileHover={{ scale: 1.05 }}
              whileTap={{ scale: 0.95 }}
              onClick={() =>setShowDeleteModal(true)}
              className="w-full px-4 py-3 bg-red-600 text-white font-bold border-2 border-red-800"
            >
              DELETE ACCOUNT
            </motion.button>
          </div>
        </div>
      </div>
      {/* Custom Delete confirmation modal */}
      {showDeleteModal && (
        <AlertModal 
          title='This action deletes the user!'
          message='Are you sure you want to delete this user? This action cannot be undone.'
          onConfirm={() => handleDeleteAccount()}
          onCancel={() => setShowDeleteModal(false)}
          confirmLabel='Delete'
          cancelLabel='Cancel'
          isDangerous={true}
        />
      )}
      {/* Custom alert modal for unsaved changes */}
      {showLeaveAlert && (
        <AlertModal
          title="Unsaved Changes"
          message="You have unsaved changes. Are you sure you want to leave?"
          confirmLabel="LEAVE"
          cancelLabel="CANCEL"
          onCancel={() => {
            setShowLeaveAlert(false);
            setPendingNavigation(null);
          }}
          onConfirm={() => {
            setShowLeaveAlert(false);
            navigate(pendingNavigation);
          }}
        />
      )}
    </div>
  );
}