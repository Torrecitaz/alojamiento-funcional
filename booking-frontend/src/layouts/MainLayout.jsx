import { Outlet } from 'react-router-dom';
import Navbar from '../components/ui/Navbar';
import Footer from '../components/ui/Footer';

export default function MainLayout() {
  return (
    <>
      <Navbar />
      <main style={{ paddingTop: 72, minHeight: 'calc(100vh - 72px)' }}>
        <Outlet />
      </main>
      <Footer />
    </>
  );
}
