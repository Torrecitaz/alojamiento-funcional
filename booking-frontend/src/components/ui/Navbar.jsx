import { Link, useNavigate } from 'react-router-dom';
import { HiOutlineUserCircle, HiOutlineMenu, HiOutlineX } from 'react-icons/hi';
import useAuthStore from '../../store/useAuthStore';
import { useState } from 'react';
import './Navbar.css';

export default function Navbar() {
  const { isAuthenticated, user, logout } = useAuthStore();
  const navigate = useNavigate();
  const [menuOpen, setMenuOpen] = useState(false);

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  return (
    <nav className="navbar">
      <div className="navbar-inner container">
        <Link to="/" className="navbar-brand">
          <span className="brand-icon">✦</span>
          <span className="brand-text">BookingPro</span>
        </Link>

        <div className={`navbar-links ${menuOpen ? 'open' : ''}`}>
          <Link to="/" className="nav-link" onClick={() => setMenuOpen(false)}>Inicio</Link>
          <Link to="/propiedades" className="nav-link" onClick={() => setMenuOpen(false)}>Propiedades</Link>
          {isAuthenticated && (
            <Link to="/mis-reservas" className="nav-link" onClick={() => setMenuOpen(false)}>Mis Reservas</Link>
          )}
          {isAuthenticated && user?.roles?.some(r => r.toLowerCase() === 'administrador' || r.toLowerCase() === 'admin') && (
            <Link to="/admin" className="nav-link nav-link-admin" onClick={() => setMenuOpen(false)}>⚙ Panel Admin</Link>
          )}
        </div>

        <div className="navbar-actions">
          {isAuthenticated ? (
            <div className="user-menu">
              <button className="user-btn" onClick={() => setMenuOpen(!menuOpen)}>
                <HiOutlineUserCircle size={22} />
                <span>{user?.nombreCompleto?.split(' ')[0]}</span>
              </button>
              <button className="btn btn-ghost" onClick={handleLogout}>Salir</button>
            </div>
          ) : (
            <Link to="/login" className="btn btn-accent">
              Iniciar Sesión
            </Link>
          )}
          <button className="mobile-toggle" onClick={() => setMenuOpen(!menuOpen)}>
            {menuOpen ? <HiOutlineX size={24} /> : <HiOutlineMenu size={24} />}
          </button>
        </div>
      </div>
    </nav>
  );
}
