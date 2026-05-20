import './Footer.css';

export default function Footer() {
  return (
    <footer className="footer">
      <div className="container footer-inner">
        <div className="footer-grid">
          <div className="footer-col">
            <h4 className="footer-brand">
              <span className="brand-icon">✦</span> BookingPro
            </h4>
            <p className="footer-desc">
              Tu destino premium para reservas de hospedaje. Tecnología de punta respaldada por una arquitectura segura.
            </p>
          </div>
          <div className="footer-col">
            <h5 className="footer-title">Explorar</h5>
            <ul className="footer-list">
              <li><a href="/propiedades">Propiedades</a></li>
              <li><a href="/login">Iniciar Sesión</a></li>
            </ul>
          </div>
          <div className="footer-col">
            <h5 className="footer-title">Soporte</h5>
            <ul className="footer-list">
              <li><a href="#">Centro de Ayuda</a></li>
              <li><a href="#">Políticas de Cancelación</a></li>
              <li><a href="#">Términos y Condiciones</a></li>
            </ul>
          </div>
          <div className="footer-col">
            <h5 className="footer-title">Tecnología</h5>
            <ul className="footer-list">
              <li>REST + GraphQL + gRPC</li>
              <li>Secured con JWT & XSS Filter</li>
              <li>Event-Driven Architecture</li>
            </ul>
          </div>
        </div>
        <div className="footer-bottom">
          <p>&copy; {new Date().getFullYear()} BookingPro — Microservicio de Clientes. Todos los derechos reservados.</p>
        </div>
      </div>
    </footer>
  );
}
