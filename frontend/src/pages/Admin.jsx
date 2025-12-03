import React, { useState, useEffect, useCallback } from 'react';
import { useAuth } from '../context/Authentication';
import { useNavigate } from 'react-router-dom';
import { admin } from '../shared/services/api';
import AlertModal from '../components/Shared/AlertModal';
import { tokens } from '../shared/constants/design/tokens';
import { validatePassword } from '../shared/utils/validation';


export function Admin() {
    const { user } = useAuth();
    const navigate = useNavigate();

    const isAdmin = user?.role === 'admin';

    useEffect(() => {
        if (!isAdmin) {
            navigate('/');
        }
    }, [isAdmin, navigate]);

    const [users, setUsers] = useState([]);
    const [selectedUser, setSelectedUser] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [editData, setEditData] = useState({
        username: '',
        email: '',
    });

    const [newPassword, setNewPassword] = useState('');
    const [showDeleteModal, setShowDeleteModal] = useState(false);
    const [userDelete, setUserDelete] = useState(null);

    const loadUsers = useCallback(async () => {
        setLoading(true);
        setError(null);

        try {
            const response = await admin.getAllUsers();
            setUsers(response.users || response || []);

            setLoading(false);
        } catch (err) {
            setError('Failed to load users.');
            console.error(err);
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        if (isAdmin) {
            loadUsers();
        }
    }, [isAdmin, loadUsers]);

    // Select a user from the list
    const selectUser = async (userId) => {
        setLoading(true);
        setError(null);

        try {
            const response = await admin.getUserbyId(userId);

            // Set the selected user in state
            const user = response.user || response || null;

            if (!user) {
                setError('User not found.');
                setLoading(false);
                return;
            }

            setSelectedUser(user);

            setEditData({
                username: user.username,
            });

            setLoading(false);

        } catch (err) {
            setError('Failed to load user.');
            console.error(err);
            setLoading(false); // Ensure loading is set to false on error
        }
    };

    // Update the username method
    const updateUsername = async () => {
        if (!selectedUser) return;

        /*
        const usernameValidation = validateUsername(editData.username);
        if (!usernameValidation.isValid) {
            setError(usernameValidation.errors.username?.[0] || 'Invalid username');
            return;
        }
        */

        setLoading(true);
        setError(null);

        try {
            await admin.updateUsername(selectedUser.id, { username: editData.username });

            // Refresh the user data
            await loadUsers();
            setSelectedUser(null);
        } catch (err) {
            setError('Failed to update username.');
            console.error(err);
        } finally {
            setLoading(false); // Ensure loading is set to false after operation
        }
    };

    // Update password method
    const updatePassword = async () => {
        if (!selectedUser || !newPassword) {
            setError("Cannot update to empty password.");
            return;
        }

        // validate the password
        const passwordValidation = validatePassword(newPassword);
        // check if password is valid
        if (!passwordValidation.isValid) {
            // if not set error to state so that it shows in UI
            setError(passwordValidation.errors.password?.[0] || 'Invalid password');
            return;
        }

        setLoading(true);
        setError(null);

        try {
            await admin.updatePassword(selectedUser.id, { password: newPassword });

            // Refresh the user data
            await loadUsers();
            setSelectedUser(null);
        } catch (err) {
            setError('Failed to update password.');
            console.error(err);
        } finally {
            setLoading(false); // Ensure loading is set to false after operation
        }
    };

    const handleDeleteUser = async () => {

        setUserDelete(selectedUser);
        setShowDeleteModal(true);

    };

    const confirmDelete = async () => {

        if (!userDelete) return;

        setLoading(true);
        setError(null);

        try {

            await admin.deleteUser(userDelete.id);

            await loadUsers();

            setSelectedUser(null);
            setUserDelete(null);
            setShowDeleteModal(false);


        } catch (err) {
            setError('Failed to delete user.');
        }

    }

    // Go back to homescreen
    const goBack = () => {
        navigate('/');
    };

    if (!isAdmin) {
        return null;
    }

    return (
        <div 
            className="min-h-screen text-white font-mono relative"
            style={{
                background: tokens.color.bg,
            }}
        >
            <div className="relative z-10 min-h-screen flex flex-col px-6 py-8">
                <div className="flex justify-between items-center mb-8">
                    <h1 
                        className="text-4xl font-bold tracking-wider"
                        style={{ textShadow: '3px 3px 0px rgba(0, 0, 0, 0.8)' }}
                    >
                        ADMIN OVERVIEW
                    </h1>

                    <button 
                        onClick={goBack} 
                        className="px-4 py-2 bg-gray-200 text-black font-bold border-2 border-black hover:bg-white transition-colors"
                    >
                        HOME
                    </button>
                </div>

                {error && (
                    <div className="mb-4 px-4 py-3 bg-red-600 text-white font-bold border-2 border-red-800 text-center">
                        {error}
                    </div>
                )}

                {loading && (
                    <div className="text-center text-xl"> 
                        <p>Loading...</p>
                    </div>
                )}

                {!loading && (
                    <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                        <div 
                            className="p-6"
                            style={{
                                background: tokens.color.surface,
                                border: `2px solid ${tokens.color.surfaceBorder}`,
                                borderRadius: tokens.radius.lg
                            }}
                        >
                            <h2 className="text-2xl font-bold mb-4 tracking-wide">ALL USERS</h2>

                            <div 
                                className="space-y-2"
                                style={{
                                    maxHeight: '500px',
                                    overflowY: 'auto'
                                }}
                            >
                                {users.length === 0 ? (
                                    <p className="text-gray-400">No users found.</p>
                                ) : (
                                    users.map((u) => (
                                        <button 
                                            key={u.id}
                                            onClick={() => selectUser(u.id)}
                                            className="w-full p-3 text-left font-bold border-2 transition-colors"
                                            style={{
                                                backgroundColor: selectedUser?.id === u.id ? tokens.color.primary : 'transparent',
                                                borderColor: selectedUser?.id === u.id ? tokens.color.primary : tokens.color.surfaceBorder,
                                                color: selectedUser?.id === u.id ? 'black' : tokens.color.text
                                            }}
                                        >
                                            <div>{u.username}</div>
                                        </button>
                                    ))
                                )}
                            </div>
                        </div>

                        {selectedUser && (
                            <div 
                                className="p-6 space-y-4"
                                style={{
                                    background: tokens.color.surface,
                                    border: `2px solid ${tokens.color.primary}`,
                                    borderRadius: tokens.radius.lg
                                }}
                            >
                                <h2 className="text-2xl font-bold mb-4 tracking-wide">EDIT USER</h2>

                                <div>
                                    <label className="block text-sm font-bold mb-2">Username</label>
                                    <input 
                                        type="text" 
                                        value={editData.username}
                                        onChange={(e) => setEditData({ ...editData, username: e.target.value })}
                                        className="w-full p-2 bg-gray-200 text-black font-bold border-2 border-black focus:outline-none focus:ring-2 focus:ring-blue-500"
                                    />
                                </div>

                                <button 
                                    onClick={updateUsername}
                                    className="w-full px-4 py-2 bg-green-600 rounded hover:bg-green-700 text-white font-bold border-2 border-green-800 transition-colors"
                                >
                                    UPDATE USERNAME
                                </button>

                                <div>
                                    <label className="block text-sm font-bold mb-2">New Password</label>
                                    <input 
                                        type="password" 
                                        value={newPassword}
                                        onChange={(e) => setNewPassword(e.target.value)}
                                        className="w-full p-2 bg-gray-200 text-black font-bold border-2 border-black focus:outline-none focus:ring-2 focus:ring-blue-500"
                                    />
                                </div>
                                
                                <button 
                                    onClick={updatePassword}
                                    className="w-full px-4 py-2 bg-yellow-600 rounded hover:bg-yellow-700 text-white font-bold border-2 border-yellow-800 transition-colors"
                                >
                                    UPDATE PASSWORD
                                </button>

                                <button 
                                    onClick={handleDeleteUser}
                                    className="w-full px-4 py-2 bg-red-600 rounded hover:bg-red-700 text-white font-bold border-2 border-red-800 transition-colors"
                                >
                                    DELETE USER
                                </button>
                            </div>
                        )}
                    </div>
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

