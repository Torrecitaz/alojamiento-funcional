import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import MainLayout from './layouts/MainLayout';
import AdminGuard from './components/AdminGuard';
import AuthGuard from './components/AuthGuard';
import HomePage from './pages/HomePage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import PropiedadesPage from './pages/PropiedadesPage';
import PropiedadDetallePage from './pages/PropiedadDetallePage';
import MisReservasPage from './pages/MisReservasPage';
import CheckoutPage from './pages/CheckoutPage';
import FacturaPage from './pages/FacturaPage';
import AdminLayout from './pages/admin/AdminLayout';
import AdminDashboard from './pages/admin/AdminDashboard';
import AdminPropiedades from './pages/admin/AdminPropiedades';
import AdminHabitaciones from './pages/admin/AdminHabitaciones';
import CalendarioAdmin from './pages/admin/CalendarioAdmin';
import AdminColaboradores from './pages/admin/AdminColaboradores';
import AdminUsuarios from './pages/admin/AdminUsuarios';
import AdminReservas from './pages/admin/AdminReservas';
import useSignalR from './hooks/useSignalR';

export default function App() {
  useSignalR();
  return (
    <BrowserRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}>
      <Toaster
        position="top-right"
        toastOptions={{
          duration: 4000,
          style: {
            borderRadius: '10px',
            background: '#1a202c',
            color: '#fff',
            fontSize: '.9rem',
          },
        }}
      />
      <Routes>
        {/* Rutas públicas con Navbar + Footer */}
        <Route element={<MainLayout />}>
          <Route path="/" element={<HomePage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/propiedades" element={<PropiedadesPage />} />
          <Route path="/propiedades/:id" element={<PropiedadDetallePage />} />
          <Route path="/mis-reservas" element={<AuthGuard><MisReservasPage /></AuthGuard>} />
          <Route path="/checkout/:codigo" element={<AuthGuard><CheckoutPage /></AuthGuard>} />
          <Route path="/factura/:codigo" element={<AuthGuard><FacturaPage /></AuthGuard>} />
        </Route>

        {/* Rutas Admin con layout propio (sin Navbar/Footer público) */}
        <Route path="/admin" element={<AdminGuard><AdminLayout /></AdminGuard>}>
          <Route index element={<AdminDashboard />} />
          <Route path="propiedades" element={<AdminPropiedades />} />
          <Route path="habitaciones" element={<AdminHabitaciones />} />
          <Route path="calendario" element={<CalendarioAdmin />} />
          <Route path="usuarios" element={<AdminUsuarios />} />
          <Route path="colaboradores" element={<AdminColaboradores />} />
          <Route path="reservas" element={<AdminReservas />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
