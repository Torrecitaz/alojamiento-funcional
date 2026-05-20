import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { HiOutlineUser, HiOutlineMail, HiOutlineLockClosed, HiOutlinePhone } from 'react-icons/hi';
import toast from 'react-hot-toast';
import api from '../services/api';
import './LoginPage.css';

export default function RegisterPage() {
  const [form, setForm] = useState({ nombreCompleto: '', email: '', password: '', confirmPassword: '', telefono: '' });
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const update = (field) => (e) => setForm({ ...form, [field]: e.target.value });

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.nombreCompleto || !form.email || !form.password) {
      toast.error('Nombre, email y contraseña son obligatorios.');
      return;
    }
    if (form.password !== form.confirmPassword) {
      toast.error('Las contraseñas no coinciden.');
      return;
    }
    if (form.password.length < 6) {
      toast.error('La contraseña debe tener al menos 6 caracteres.');
      return;
    }
    setLoading(true);
    try {
      await api.post('/clientes', {
        nombreCompleto: form.nombreCompleto,
        email: form.email,
        password: form.password,
        telefono: form.telefono || null,
      });
      toast.success('¡Cuenta creada exitosamente! Ahora inicia sesión.');
      navigate('/login');
    } catch (err) {
      const data = err.response?.data;
      if (data?.errores && data.errores.length > 0) {
        // Mostrar cada error de validación
        data.errores.forEach(error => toast.error(error));
      } else {
        const msg = data?.mensaje || data?.Mensaje || 'Error al crear la cuenta.';
        toast.error(msg);
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-page">
      <div className="login-card animate-fade-in-up">
        <div className="login-header">
          <span className="brand-icon">✦</span>
          <h1>Crear Cuenta</h1>
          <p>Regístrate para reservar propiedades</p>
        </div>

        <form onSubmit={handleSubmit} className="login-form">
          <div className="form-group">
            <label className="input-label">Nombre Completo *</label>
            <div className="input-with-icon">
              <HiOutlineUser className="input-icon" />
              <input type="text" className="input-field" placeholder="Juan Pérez" value={form.nombreCompleto} onChange={update('nombreCompleto')} />
            </div>
          </div>

          <div className="form-group">
            <label className="input-label">Correo Electrónico *</label>
            <div className="input-with-icon">
              <HiOutlineMail className="input-icon" />
              <input type="email" className="input-field" placeholder="correo@ejemplo.com" value={form.email} onChange={update('email')} autoComplete="email" />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label className="input-label">Contraseña *</label>
              <div className="input-with-icon">
                <HiOutlineLockClosed className="input-icon" />
                <input type="password" className="input-field" placeholder="••••••••" value={form.password} onChange={update('password')} />
              </div>
            </div>
            <div className="form-group">
              <label className="input-label">Confirmar Contraseña *</label>
              <div className="input-with-icon">
                <HiOutlineLockClosed className="input-icon" />
                <input type="password" className="input-field" placeholder="••••••••" value={form.confirmPassword} onChange={update('confirmPassword')} />
              </div>
            </div>
          </div>

          <div className="form-group">
            <label className="input-label">Teléfono (opcional)</label>
            <div className="input-with-icon">
              <HiOutlinePhone className="input-icon" />
              <input type="tel" className="input-field" placeholder="+51 999 999 999" value={form.telefono} onChange={update('telefono')} />
            </div>
          </div>

          <button type="submit" className="btn btn-primary login-btn" disabled={loading}>
            {loading ? 'Creando cuenta...' : 'Registrarme'}
          </button>
        </form>

        <div className="login-footer">
          <p>¿Ya tienes cuenta? <Link to="/login">Iniciar Sesión</Link></p>
        </div>
      </div>
    </div>
  );
}
