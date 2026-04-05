import { createBrowserRouter } from "react-router-dom";
import Root from "../domains/Root";
import NotFound from "../domains/not-found/NotFound";
import ProtectedRoute from "./auth/ProtectedRoute";
import PublicRoute from "./auth/PublicRoute";
import Login from "../domains/login/Login";
import Register from "../domains/register/Register";

export const Routes = createBrowserRouter([
    {
        path: '/',
        element: <Root />,
        children: [
            // Public routes
            {
                element: <PublicRoute />,
                children: [
                    { path: 'login', element: <Login /> },
                    { path: 'register', element: <Register /> },
                ],
            },

            // Protected routes
            {
                element: <ProtectedRoute />,
                children: [
                    {
                        index: true,
                        element: <div>Dashboard</div>
                    }
                ],
            },

            {
                path: '*',
                element: <NotFound />
            }
        ]
    }
], {
    basename: '/'
});