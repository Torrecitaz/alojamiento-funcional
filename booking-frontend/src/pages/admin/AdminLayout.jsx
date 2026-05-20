import { Outlet, NavLink, useNavigate, Link } from 'react-router-dom';
import { HiOutlineHome, HiOutlineOfficeBuilding, HiOutlineUsers, HiOutlineCalendar, HiOutlineLogout, HiOutlineKey, HiOutlineExternalLink } from 'react-icons/hi';
import useAuthStore from '../../store/useAuthStore';
import './AdminLayout.css';

export default function AdminLayout() {
  const { user, logout } = useAuthStore();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  return (
    <div className="admin-shell">
      <aside className="admin-sidebar">
        <div className="admin-brand">
          <span className="brand-icon">✦</span>
          <div>
            <span className="admin-brand-text">BookingPro</span>
            <span className="admin-role">{user?.roles?.includes('Administrador') ? 'Panel Admin' : 'Panel Socio'}</span>
          </div>
        </div>

        <nav className="admin-nav">
          <NavLink to="/admin" end className="admin-nav-link">
            <HiOutlineHome size={18} /> Dashboard
          </NavLink>
          <NavLink to="/admin/propiedades" className="admin-nav-link">
            <HiOutlineOfficeBuilding size={18} /> Propiedades
          </NavLink>
          <NavLink to="/admin/habitaciones" className="admin-nav-link">
            <HiOutlineKey size={18} /> Habitaciones
          </NavLink>
          
          {user?.roles?.includes('Administrador') && (
            <>
              <NavLink to="/admin/usuarios" className="admin-nav-link">
                <HiOutlineUsers size={18} /> Usuarios
              </NavLink>
              <NavLink to="/admin/colaboradores" className="admin-nav-link">
                <HiOutlineOfficeBuilding size={18} /> Colaboradores
              </NavLink>
            </>
          )}

          <NavLink to="/admin/reservas" className="admin-nav-link">
            <HiOutlineCalendar size={18} /> Reservas
          </NavLink>

          <div style={{ marginTop: 'auto', paddingTop: 16, borderTop: '1px solid rgba(255,255,255,.08)' }}>
            <Link to="/" className="admin-nav-link" style={{ color: 'var(--color-accent-light)' }}>
              <HiOutlineExternalLink size={18} /> Ir al Sitio Principal
            </Link>
          </div>
        </nav>

        <div className="admin-sidebar-footer">
          <div className="admin-user-info">
            <span className="admin-user-name">{user?.nombreCompleto || 'Admin'}</span>
            <span className="admin-user-email">{user?.email || ''}</span>
          </div>
          <button className="admin-logout-btn" onClick={handleLogout}>
            <HiOutlineLogout size={18} /> Salir
          </button>
        </div>
      </aside>

      <main className="admin-main">
        <Outlet />
      </main>
    </div>
  );
}
