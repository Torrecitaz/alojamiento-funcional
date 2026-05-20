import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { HiOutlineMail, HiOutlineLockClosed } from 'react-icons/hi';
import toast from 'react-hot-toast';
import api from '../services/api';
import useAuthStore from '../store/useAuthStore';
import './LoginPage.css';

export default function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const login = useAuthStore((s) => s.login);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!email || !password) {
      toast.error('Por favor completa todos los campos.');
      return;
    }
    setLoading(true);
    try {
      const { data } = await api.post('/auth/login', { email, password });
      login(data.datos);
      toast.success(`¡Bienvenido, ${data.datos.nombreCompleto}!`);
      
      // Redirección basada en rol
      if (data.datos.roles && data.datos.roles.includes('Administrador')) {
        navigate('/admin');
      } else {
        navigate('/');
      }
    } catch (err) {
      const msg = err.response?.data?.mensaje || 'Credenciales inválidas.';
      toast.error(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-page">
      <div className="login-card animate-fade-in-up">
        <div className="login-header">
          <span className="brand-icon">✦</span>
          <h1>Iniciar Sesión</h1>
          <p>Accede a tu cuenta para gestionar reservas</p>
        </div>

        <form onSubmit={handleSubmit} className="login-form">
          <div className="form-group">
            <label className="input-label">Correo Electrónico</label>
            <div className="input-with-icon">
              <HiOutlineMail className="input-icon" />
              <input
                type="email"
                className="input-field"
                placeholder="correo@ejemplo.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                autoComplete="email"
              />
            </div>
          </div>

          <div className="form-group">
            <label className="input-label">Contraseña</label>
            <div className="input-with-icon">
              <HiOutlineLockClosed className="input-icon" />
              <input
                type="password"
                className="input-field"
                placeholder="••••••••"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                autoComplete="current-password"
              />
            </div>
          </div>

          <button type="submit" className="btn btn-primary login-btn" disabled={loading}>
            {loading ? 'Autenticando...' : 'Ingresar'}
          </button>
        </form>

        <div className="login-footer">
          <p>¿No tienes cuenta? <Link to="/register">Regístrate aquí</Link></p>
        </div>
      </div>
    </div>
  );
}
