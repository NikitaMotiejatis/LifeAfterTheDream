import { type ReactNode, useState, useEffect } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { LayoutDashboard, BarChart3, Settings, Ship } from 'lucide-react';

interface LayoutProps {
  children: ReactNode;
}

export default function Layout({ children }: LayoutProps) {
  const location = useLocation();
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [time, setTime] = useState(new Date());

  useEffect(() => {
    const timer = setInterval(() => setTime(new Date()), 1000);
    return () => clearInterval(timer);
  }, []);

  const isActive = (path: string) =>
    path === '/'
      ? location.pathname === '/' || location.pathname === '/dashboard'
      : location.pathname === path;

  const navItems = [
    { path: '/', label: 'Dashboard', icon: LayoutDashboard },
    { path: '/analytics', label: 'Analytics', icon: BarChart3 },
    { path: '/settings', label: 'Settings', icon: Settings },
  ];

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const formattedTime = time.toLocaleTimeString('en-GB', {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  });
  const formattedDate = time.toLocaleDateString('en-GB', {
    weekday: 'short',
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  });

  return (
    <div className="min-h-screen bg-gray-100">
      {/* Top Navbar */}
      <header
        className="px-6 pt-4 shadow-lg"
        style={{
          background: 'linear-gradient(90deg, #155DFC 0%, #193CB8 100%)',
        }}
      >
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center gap-4">
            <Ship className="w-8 h-8 text-white" />
            <div>
              <h1 className="text-xl font-bold text-white leading-7">
                Klaipėda Port Operations
              </h1>
              <p className="text-sm text-blue-100">Risk Management System</p>
            </div>
          </div>
          <div className="flex items-center gap-6">
            <div className="text-right">
              <div className="text-2xl font-bold text-white tracking-wide">
                {formattedTime}
              </div>
              <div className="text-xs text-blue-100">{formattedDate}</div>
            </div>
            <div className="text-right border-l border-white/20 pl-6">
              <div className="text-sm font-semibold text-white">
                {user?.name}
              </div>
              <button
                onClick={handleLogout}
                className="text-xs text-blue-200 hover:text-white font-medium"
              >
                Logout
              </button>
            </div>
          </div>
        </div>
        <nav className="flex gap-2">
          {navItems.map((item) => {
            const Icon = item.icon;
            return (
              <Link
                key={item.path}
                to={item.path}
                className={`flex items-center gap-2 px-4 py-2 rounded-t-lg text-base font-medium transition ${
                  isActive(item.path)
                    ? 'bg-white text-blue-600'
                    : 'text-white hover:bg-white/10'
                }`}
              >
                <Icon className="w-4 h-4" />
                {item.label}
              </Link>
            );
          })}
        </nav>
      </header>

      {/* Page Content */}
      <main className="max-w-7xl mx-auto px-6 py-6">{children}</main>
    </div>
  );
}
