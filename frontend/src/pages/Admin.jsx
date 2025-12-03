import React, { useState, useEffect, useCallback } from 'react';
import { motion } from 'framer-motion';
import { useAuth } from '../context/Authentication';
import { useNavigate } from 'react-router-dom';
import { admin } from '../shared/services/api';
import AlertModal from '../components/Shared/AlertModal';
import { tokens } from '../shared/constants/design/tokens';
import { validatePassword, validateUsername } from '../shared/utils/validation';
import Planet from '../components/Home/Planet';
import Spaceship from '../components/Home/Spaceship';
import Stars from '../components/Home/Stars';


/**
 *  Used chatGpt 5.1 to generate the HTML and CSS for the admin, page based on the logic. 
 * as well as debugging the code logic for errors.
 */

export function Admin() {
    // get the user from the auth context
    const { user } = useAuth();

    // navigation method from the react router dom
    const navigate = useNavigate();

    // check if the user is an admin
    const isAdmin = user?.role === 'admin';

    // use effect to check if the user is an admin
    useEffect(() => {
        if (!isAdmin) {
            navigate('/');
        }
    }, [isAdmin, navigate]);

    // state to store the users
    const [users, setUsers] = useState([]);
    // state to store the selected user
    const [selectedUser, setSelectedUser] = useState(null);
    // state management to set the loading state
    const [loading, setLoading] = useState(true);
    // state management to show errors.
    const [error, setError] = useState(null);
    // state management to store the username
    const [editUsername, setEditUsername] = useState('');
    // state management to store the password
    const [editPassword, setEditPassword] = useState('');
    // state management to show the delete modal
    const [showDeleteModal, setShowDeleteModal] = useState(false);

    // function / method to load all users.
    const loadUsers = useCallback(async () => {
        setLoading(true);
        setError(null);

        try {
            const response = await admin.getAllUsers();

            // set the users state to the response.users or response or an empty array.
            setUsers(response.users || response || []);

            setLoading(false);
        } catch (err) {
            setError('Failed to load users.');
            console.error(err);
            setLoading(false);
        }
    }, []);

    // use effect to load users only if the user is an admin.
    useEffect(() => {
        if (isAdmin) {
            loadUsers();
        }
    }, [isAdmin, loadUsers]);

    // Select a user from the list
    // function / method to select a user from the list.
    const selectUser = async (userId) => {
        // set the loading state to true
        setLoading(true);
        // set the error state to null
        setError(null);

        try {
            // get the user by id from the admin service.
            const response = await admin.getUserbyId(userId);

            // return the data as reponse or response user or null.
            const userData = response.user || response || null;

            // if the user data is not found, set the error state to 'User not found.'
            if (!userData) {
                // set the error state to 'User not found.'
                setError('User not found.');
                // set the loading state to false.
                setLoading(false);

                // return from the function.
                return;
            }

            setSelectedUser(userData);
            // set the username state to the user's username.
            setEditUsername(userData.username);

            // set the password state to an empty string.
            // as we dont want to fetch the users password from the database.
            // and display it in the form.
            setEditPassword('');
            // set the loading state to false.
            setLoading(false);

        } catch (err) {
            // set the error state to 'Failed to load user.'
            setError('Failed to load user.');
            console.error(err);
            setLoading(false);
        }
    };

    // Update username
    const updateUsername = async () => {
        // check if the selected user is not null.
        if (!selectedUser) return;

        // check if the username is valid
        const validation = validateUsername(editUsername);
        // check if the username is valid by checking the validation object.
        if (!validation.isValid) {
            setError(validation.errors.username?.[0] || 'Invalid username');
            return;
        }
        
        // set the loading state to true
        setLoading(true);
        // set the error state to null if the username is valid.
        setError(null);

        try {
            // update the username by id through the API
            await admin.updateUsername(selectedUser.id, { username: editUsername });
            // reload the users
            await loadUsers();
            // clear the selected user and and form fields.
            clearSelection();
        } catch (err) {
            setError('Failed to update username.');
            console.error(err);
            setLoading(false);
        }
    };

    // Update password
    const updatePassword = async () => {

        // check if the selected user is not null and the password is not empty.
        if (!selectedUser || !editPassword) {
            setError("Cannot update to empty password.");
            return;
        }

        // check if the password is valid 
        const validation = validatePassword(editPassword);
        // if not valid, set error state
        if (!validation.isValid) {
            setError(validation.errors.password?.[0] || 'Invalid password');
            return;
        }

        // set loading state to true
        setLoading(true);
        // set error state to null if the password is valid.
        setError(null);


        try {

            // call the update password /API
            await admin.updatePassword(selectedUser.id, { password: editPassword });
            // reload the users for constistancy, if something is changed.
            await loadUsers();
            // clear the selected user and and form fields.
            clearSelection();
        } catch (err) {
            setError('Failed to update password.');
            console.error(err);
            setLoading(false);
        }
    };

    // Helper to clear selection and reset form
    const clearSelection = () => {
        setSelectedUser(null);
        setEditUsername('');
        setEditPassword('');
        setLoading(false);
    };

    // Show delete confirmation modal
    const handleDeleteUser = () => {
        // set the show delete modal state to true
        setShowDeleteModal(true);
    };

    // Confirm and execute delete
    const confirmDelete = async () => {
        // check if the selected user is not null.
        if (!selectedUser) return;

        // set loading state to true
        setLoading(true);
        // reset the error state to null
        setError(null);

        try {
            // call the delete user /API
            await admin.deleteUser(selectedUser.id);
            // reload the users for constistancy, if something is changed.
            await loadUsers();
            clearSelection();
            setShowDeleteModal(false);
        } catch (err) {
            setError('Failed to delete user.');
            setLoading(false);
        }
    };

    // Go back to homescreen
    const goBack = () => {
        navigate('/');
    };

    if (!isAdmin) {
        return null;
    }

    return (
        <div 
            className="relative min-h-screen text-white font-mono"
        >
            <Stars />
            <Planet />
            <Spaceship />
            <div className="relative z-10 min-h-screen flex flex-col px-6 py-8">
                <div className="flex items-center justify-between mb-8">
                    <motion.h1 
                        initial={{ opacity: 0, y: -20 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ duration: 0.4 }}
                        className="flex-1 text-center text-4xl md:text-5xl font-bold tracking-wider pixel-text"
                        style={{ textShadow: '3px 3px 0px rgba(0, 0, 0, 0.8)' }}
                    >
                        ADMIN TERMINAL
                    </motion.h1>

                    <motion.button 
                        whileHover={{ scale: 1.05 }}
                        whileTap={{ scale: 0.95 }}
                        onClick={goBack} 
                        className="px-4 py-2 bg-gray-200 text-black font-bold border-2 border-black hover:bg-white transition-colors"
                    >
                        HOME
                    </motion.button>
                </div>

                {error && (
                    <motion.div
                        initial={{ opacity: 0, y: -10 }}
                        animate={{ opacity: 1, y: 0 }}
                        className="mb-4 px-4 py-3 bg-red-600 text-white font-bold border-2 border-red-800 text-center"
                    >
                        {error}
                    </motion.div>
                )}

                {loading && (
                    <div className="flex-1 flex items-center justify-center text-xl"> 
                        <p>Loading...</p>
                    </div>
                )}

                {!loading && (
                    <main className="flex-1 flex items-center justify-center">
                        <motion.div
                            initial={{ opacity: 0, scale: 0.95 }}
                            animate={{ opacity: 1, scale: 1 }}
                            transition={{ duration: 0.4 }}
                            className={`w-full max-w-6xl flex flex-col lg:flex-row gap-6 ${selectedUser ? 'lg:justify-between' : 'lg:justify-center'}`}
                        >
                            {/* User list table - centered by default, semi-collapsed when a user is selected */}
                            <motion.div
                                animate={{
                                    flexBasis: selectedUser ? '40%' : '70%',
                                    x: selectedUser ? 0 : 0,
                                }}
                                transition={{ duration: 0.4, ease: 'easeInOut' }}
                                className={selectedUser ? 'mx-auto lg:mx-0 bg-transparent' : 'mx-auto bg-transparent'}
                                style={{
                                    border: `2px solid ${tokens.color.surfaceBorder}`,
                                    borderRadius: tokens.radius.lg,
                                    boxShadow: '0 0 30px rgba(0,0,0,0.6)',
                                }}
                            >
                                <div className="p-4 border-b border-gray-700/60 flex items-center justify-between">
                                    <h2 className="text-xl md:text-2xl font-bold tracking-wide">
                                        USERS
                                    </h2>
                                    <span className="text-xs md:text-sm text-gray-300">
                                        {users.length} total
                                    </span>
                                </div>

                                <div
                                    className="overflow-y-auto"
                                    style={{
                                        maxHeight: '500px',
                                    }}
                                >
                                    {users.length === 0 ? (
                                        <div className="p-4 text-gray-400 text-center">
                                            No users found.
                                        </div>
                                    ) : (
                                        <table className="w-full text-left border-collapse text-sm md:text-base bg-transparent">
                                            <thead className="sticky top-0 z-10 bg-black/20 backdrop-blur-sm">
                                                <tr>
                                                    <th className="px-4 py-2 border-b border-gray-700/60">ID</th>
                                                    <th className="px-4 py-2 border-b border-gray-700/60">USERNAME</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                {users.map((u) => {
                                                    const isSelected = selectedUser?.id === u.id;
                                                    return (
                                                        <motion.tr
                                                            key={u.id}
                                                            onClick={() => selectUser(u.id)}
                                                            whileHover={{ scale: 1.01, backgroundColor: 'rgba(255,255,255,0.06)' }}
                                                            className="cursor-pointer"
                                                            style={{
                                                                backgroundColor: isSelected ? tokens.color.primary : 'transparent',
                                                                color: isSelected ? 'black' : tokens.color.text,
                                                                borderBottom: `1px solid ${tokens.color.surfaceBorder}`,
                                                            }}
                                                        >
                                                            <td className="px-4 py-2 align-middle font-mono text-xs md:text-sm">
                                                                {u.id}
                                                            </td>
                                                            <td className="px-4 py-2 align-middle font-bold">
                                                                {u.username}
                                                            </td>
                                                        </motion.tr>
                                                    );
                                                })}
                                            </tbody>
                                        </table>
                                    )}
                                </div>
                            </motion.div>

                            {/* Detail panel - slides in on the right when a user is selected */}
                            {selectedUser && (
                                <motion.div 
                                    initial={{ opacity: 0, x: 40 }}
                                    animate={{ opacity: 1, x: 0 }}
                                    exit={{ opacity: 0, x: 40 }}
                                    transition={{ duration: 0.4, ease: 'easeOut' }}
                                    className="lg:flex-1"
                                    style={{
                                        background: tokens.color.surface,
                                        border: `2px solid ${tokens.color.primary}`,
                                        borderRadius: tokens.radius.lg,
                                        boxShadow: '0 0 35px rgba(0,0,0,0.7)',
                                    }}
                                >
                                    <div className="p-6 space-y-5">
                                        <h2 className="text-xl md:text-2xl font-bold mb-2 tracking-wide text-center">
                                            USER DETAIL
                                        </h2>

                                        <div className="pt-2">
                                            <label className="block text-sm font-bold mb-2">Edit username</label>
                                            <input 
                                                type="text" 
                                                value={editUsername}
                                                onChange={(e) => setEditUsername(e.target.value)}
                                                className="w-full p-2 bg-gray-200 text-black font-bold border-2 border-black focus:outline-none focus:ring-2 focus:ring-blue-500"
                                            />
                                        </div>

                                        <motion.button 
                                            whileHover={{ scale: 1.02 }}
                                            whileTap={{ scale: 0.98 }}
                                            onClick={updateUsername}
                                            className="w-full px-4 py-2 bg-blue-600 rounded hover:bg-blue-700 text-white font-bold border-2 border-blue-800 transition-colors"
                                        >
                                            UPDATE USERNAME
                                        </motion.button>

                                        <div className="pt-2">
                                            <label className="block text-sm font-bold mb-2">Set new password</label>
                                            <input 
                                                type="password" 
                                                value={editPassword}
                                                onChange={(e) => setEditPassword(e.target.value)}
                                                className="w-full p-2 bg-gray-200 text-black font-bold border-2 border-black focus:outline-none focus:ring-2 focus:ring-blue-500"
                                            />
                                        </div>
                                        
                                        <motion.button 
                                            whileHover={{ scale: 1.02 }}
                                            whileTap={{ scale: 0.98 }}
                                            onClick={updatePassword}
                                            className="w-full px-4 py-2 bg-blue-600 rounded hover:bg-blue-700 text-white font-bold border-2 border-blue-800 transition-colors"
                                        >
                                            UPDATE PASSWORD
                                        </motion.button>

                                        <motion.button 
                                            whileHover={{ scale: 1.02 }}
                                            whileTap={{ scale: 0.98 }}
                                            onClick={handleDeleteUser}
                                            className="w-full px-4 py-2 bg-red-600 rounded hover:bg-red-700 text-white font-bold border-2 border-red-800 transition-colors"
                                        >
                                            DELETE USER
                                        </motion.button>
                                    </div>
                                </motion.div>
                            )}
                        </motion.div>
                    </main>
                )} 
            </div>
            {showDeleteModal && (
                <AlertModal 
                    title='This action deletes the user!'
                    message='Are you sure you want to delete this user? This action cannot be undone.'
                    onConfirm={confirmDelete}
                    onCancel={() => setShowDeleteModal(false)}
                    confirmLabel='Delete'
                    cancelLabel='Cancel'
                    isDangerous={true}
                />
            )}
        </div>
    );
}

