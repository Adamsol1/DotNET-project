import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/Authentication';
import { useNavigate } from 'react-router-dom';
import { admin } from '../shared/services/api';
import AlertModal from '../components/shared/AlertModal';
import { tokens } from '../shared/constants/design/tokens';


export function Admin() {
    const { user } = useAuth();
    const navigate = useNavigate();

    // State management
    const [users, setUsers] = useState([]); // Ensure this is initialized as an empty array
    const [selectedUser, setSelectedUser] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [editData, setEditData] = useState({
        username: '',
        email: '',
    });

    const [newPassword, setNewPassword] = useState('');
    const [showPasswordModal, setShowPasswordModal] = useState(false);
    const [showResetAlert, setShowResetAlert] = useState(false);

    // Load all users
    useEffect(() => {
        loadUsers();
    }, []);

    const loadUsers = async () => {
        setLoading(true);
        setError(null);

        try {
            const response = await admin.getAllUsers();
            setUsers(response.users || []); // Ensure response.users is defined

            setLoading(false);
        } catch (err) {
            setError('Failed to load users.');
            console.error(err);
            setLoading(false); // Ensure loading is set to false on error
        }
    };

    // Select a user from the list
    const selectUser = async (userId) => {
        setLoading(true);
        setError(null);

        try {
            const response = await admin.getUserbyId(userId);

            // Set the selected user in state
            setSelectedUser(response.user);

            // Set edit data
            setEditData({
                username: response.user.username,
                email: response.user.email,
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

    // Go back to homescreen
    const goBack = () => {
        navigate('/');
    };

    return (
        <div className="min-h-screen text-white font-mono"
            style={{
                background: tokens.color.bg,
                padding: tokens.space.lg
            }}>

            {/* Header Section */}
            <div className="flex justify-between items-center mb-8">
                <h1 className="text-4xl font-bold">Admin Overview</h1>

                {/* Home button */}
                <button onClick={goBack} className="px-4 py-2 bg-blue-600 rounded hover:bg-white text-white hover:text-blue-600 transition">
                    HOME
                </button>
            </div>

            {/* Error message display */}
            {error && (
                <div className="text-red-500 text-center mb-4">
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
                    {/* Users List */}
                    <div className="p-6">
                        <h2 className="text-2xl font-bold mb-4 tracking-wide">ALL USERS</h2>

                        <div className='space-y-2'
                            style={{
                                maxHeight: '500px',
                                overflowY: 'auto'
                            }}>
                            {users.length === 0 ? (
                                <p>No users found.</p>
                            ) : (
                                users.map((u) => (
                                    <button key={u.id}
                                        onClick={() => selectUser(u.id)}
                                        className="w-full p-3 text-left font-bold border-2 transition-colors"
                                        style={{
                                            backgroundColor: selectedUser?.id === u.id ? tokens.color.primary : 'transparent',
                                            borderColor: selectedUser?.id === u.id ? tokens.color.primary : tokens.color.border,
                                            color: selectedUser?.id === u.id ? 'black' : 'white'
                                        }}>
                                        <div>{u.username}</div>
                                    </button>
                                ))
                            )}
                        </div>
                    </div>

                    {/* User Details and Edit Section */}
                    {selectedUser && (
                        <div className="p-6 space-y-4"
                            style={{
                                background: tokens.color.surface,
                                border: `2px solid ${tokens.color.primary}`,
                                borderRadius: tokens.radius.lg
                            }}>
                            <h2 className="text-2xl font-bold mb-4 tracking-wide">EDIT USER</h2>

                            {/* Username Edit */}
                            <div>
                                <label className="block text-sm font-bold mb-2">Username</label>
                                <input type="text" value={editData.username}
                                    onChange={(e) => setEditData({ ...editData, username: e.target.value })}
                                    className="w-full p-2 bg-gray-200 text-black font-bold border-2 border-black focus:outline-none focus:ring-2 focus:ring-blue-500"
                                />
                            </div>

                            {/* Update Username Button */}
                            <button onClick={updateUsername}
                                className="px-4 py-2 bg-green-600 rounded hover:bg-green-700 text-white font-bold transition">
                                Update Username
                            </button>

                            {/* Password Reset Section */}
                            <div>
                                <label className="block text-sm font-bold mb-2">New Password</label>
                                <input type="password" value={newPassword}
                                    onChange={(e) => setNewPassword(e.target.value)}
                                    className="w-full p-2 bg-gray-200 text-black font-bold border-2 border-black focus:outline-none focus:ring-2 focus:ring-blue-500"
                                />
                            </div>
                            
                            {/* Update Password Button */} 
                            <button onClick={updatePassword}
                                className="px-4 py-2 bg-yellow-600 rounded hover:bg-yellow-700 text-white font-bold transition">
                                Update Password
                            </button>
                        </div>
                    )}
                </div>
            )} 
        </div>
    );
}

