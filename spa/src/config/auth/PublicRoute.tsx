import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "./auth"

const PublicRoute = () => {
    const { isAuthenticated, isLoading } = useAuth();

    if (isLoading) return null;

    return isAuthenticated ? <Navigate to='/' replace /> : <Outlet />
}

export default PublicRoute;
