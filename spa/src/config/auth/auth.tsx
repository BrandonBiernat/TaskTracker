import { createContext, useCallback, useContext, useEffect, useState, type ReactNode } from "react";

interface AuthContextType {
    isAuthenticated: boolean;
    isLoading: boolean;
    accessToken: string | null;
    login: (email: string, password: string) => Promise<string[] | null>;
    register: (email: string, password: string, firstName: string, lastName: string) => Promise<string[] | null>;
    logout: () => Promise<void>;
}

interface AuthResponse {                                                                                                                    
    access_token: string;                                                                                                                   
    refresh_token: string;
}                                                                                                                                           

interface ErrorResponse {                                                                                                                   
    errors: string[];
}

const AuthContext = createContext<AuthContextType | null>(null);

const API_URL = 'http://localhost:5056/api';

export const AuthProvider = ({ children }: { children: ReactNode }) => {
    const [accessToken, setAccessToken] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState<boolean>(true);

    const isAuthenticated = accessToken !== null;

    const clearAuth = useCallback(() => {
        setAccessToken(null);
        localStorage.removeItem('refresh_token');
    }, []);

    const refresh = useCallback(() => {
        const stored = localStorage.getItem('refresh_token');
        if (!stored) {
            setIsLoading(false);
            return;
        }

        fetch(`${API_URL}/auth/refresh`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ refresh_token: stored })
        })
        .then(async (res) => {
            if(!res.ok) {
                clearAuth();
                return;
            }
            const data: AuthResponse = await res.json();
            setAccessToken(data.access_token);
    
            localStorage.setItem('refresh_token', data.refresh_token);
        })
        .catch(() => clearAuth())
        .finally(() => setIsLoading(false));
    }, [clearAuth]);

    const login = async (email: string, password: string): Promise<string[] | null> => {
        const res = await fetch(`${API_URL}/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password })
        });

        if(!res.ok) {
            const data: ErrorResponse = await res.json();
            return data.errors;
        }

        const data: AuthResponse = await res.json();
        setAccessToken(data.access_token);

        localStorage.setItem('refresh_token', data.refresh_token);
        return null;
    };

    const register = async (
        email: string, password: string, firstName: string, lastName: string
    ): Promise<string[] | null> => {
        const res = await fetch(`${API_URL}/auth/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                email,
                password,
                first_name: firstName,
                last_name: lastName
            })
        });

        if (!res.ok) {
            const data: ErrorResponse = await res.json();
            return data.errors;
        }
        return null;
    };

    const logout = async () => {
        if (accessToken) {
            await fetch(`${API_URL}/auth/logout`, {
                method: 'POST',
                headers: { Authorization: `Bearer ${accessToken}` },
            }).catch(() => {});
            clearAuth();
        }
    };

    useEffect(() => {
        refresh();
    }, [refresh]);

    return (
        <AuthContext.Provider
            value={{ isAuthenticated, isLoading, accessToken, login, register, logout }}
        >
            {children}
        </AuthContext.Provider>
    );
}

export const useAuth = () => {
    const context = useContext(AuthContext);
    if(!context) throw new Error('useAuth must be used within AuthProvider');
    return context;
}