import { Navigate } from 'react-router-dom';
import useAuthStore from '../store/useAuthStore';

export default function AdminGuard({ children }) {
  const { isAuthenticated, user } = useAuthStore();
  
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  
  const isAdmin = user?.roles?.some(r => 
    r.toLowerCase() === 'administrador' || r.toLowerCase() === 'admin'
  );
  
  if (!isAdmin) return <Navigate to="/" replace />;
  
  return children;
}
