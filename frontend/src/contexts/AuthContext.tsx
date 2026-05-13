import { createContext, useContext, useState, type ReactNode } from 'react';

interface User {
  name: string;
  email: string;
  role: string;
}

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => boolean;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

// Mock credentials for testing
const MOCK_CREDENTIALS = {
  email: 'operator@klaipeda.lt',
  password: 'operator123',
};

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(() => {
    const stored = sessionStorage.getItem('auth_user');
    return stored ? JSON.parse(stored) : null;
  });

  const login = (email: string, password: string): boolean => {
    if (
      email === MOCK_CREDENTIALS.email &&
      password === MOCK_CREDENTIALS.password
    ) {
      const localPart = email.split('@')[0];
      const displayName = localPart.includes('.')
        ? localPart
            .split('.')
            .map((p) => p.charAt(0).toUpperCase() + p.slice(1))
            .join(' ')
        : localPart.charAt(0).toUpperCase() + localPart.slice(1);
      const loggedUser: User = { name: displayName, email, role: 'operator' };
      setUser(loggedUser);
      sessionStorage.setItem('auth_user', JSON.stringify(loggedUser));
      return true;
    }
    return false;
  };

  const logout = () => {
    setUser(null);
    sessionStorage.removeItem('auth_user');
  };

  return (
    <AuthContext.Provider
      value={{ user, isAuthenticated: !!user, login, logout }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
