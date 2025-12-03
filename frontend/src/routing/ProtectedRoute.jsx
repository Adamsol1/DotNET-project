import React from "react";
import { Navigate } from 'react-router-dom';



//Component that redirects users from routes that they are not authorized to access.
const ProtectedRoute = ({children}) => {
    const token = localStorage.getItem('token');
    
    if (!token) {
        return <Navigate to="/login" replace />;
    }
    return children;
}

export default ProtectedRoute;