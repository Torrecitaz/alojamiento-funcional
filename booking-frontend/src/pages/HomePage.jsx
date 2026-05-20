import { useState } from 'react';
import { Link } from 'react-router-dom';
import { HiOutlineSearch, HiOutlineStar, HiOutlineShieldCheck, HiOutlineLightningBolt, HiOutlineGlobe } from 'react-icons/hi';
import heroBg from '../assets/images/hero-bg.png';
import luxuryRoom from '../assets/images/luxury-room.png';
import lobby from '../assets/images/lobby.png';
import dining from '../assets/images/dining.png';
import spa from '../assets/images/spa.png';
import './HomePage.css';

export default function HomePage() {
  const [destino, setDestino] = useState('');
  const [checkIn, setCheckIn] = useState('');
  const [checkOut, setCheckOut] = useState('');
  const [huespedes, setHuespedes] = useState('1,0'); // Adultos,Niños

  const buildSearchUrl = () => {
    const params = new URLSearchParams();
    if (destino) params.append('ciudadText', destino);
    if (checkIn) params.append('checkIn', checkIn);
    if (checkOut) params.append('checkOut', checkOut);
    
    const [a, n] = huespedes.split(',');
    params.append('adultos', a);
    params.append('ninos', n);

    return `/propiedades?${params.toString()}`;
  };

  return (
    <div className="home">
      {/* ── Hero Section ──────────────────────────────── */}
      <section className="hero" style={{ backgroundImage: `url(${heroBg})` }}>
        <div className="hero-overlay" />
        <div className="hero-content container">
          <span className="hero-badge animate-fade-in">✦ Reservas Premium</span>
          <h1 className="hero-title animate-fade-in-up">
            Descubre tu próximo<br />
            <span className="hero-highlight">destino soñado</span>
          </h1>
          <p className="hero-subtitle animate-fade-in-up" style={{ animationDelay: '.15s' }}>
            Explora propiedades exclusivas con la mejor tecnología y seguridad respaldando cada reserva.
          </p>
          <div className="hero-search animate-fade-in-up" style={{ animationDelay: '.3s' }}>
            <div className="search-field">
              <label>Destino</label>
              <input 
                type="text" 
                placeholder="¿A dónde vas?" 
                className="input-field" 
                value={destino}
                onChange={e => setDestino(e.target.value)}
              />
            </div>
            <div className="search-field">
              <label>Check-in</label>
              <input type="date" className="input-field" value={checkIn} onChange={e => setCheckIn(e.target.value)} />
            </div>
            <div className="search-field">
              <label>Check-out</label>
              <input type="date" className="input-field" value={checkOut} onChange={e => setCheckOut(e.target.value)} />
            </div>
            <div className="search-field">
              <label>Huéspedes</label>
              <select className="input-field" value={huespedes} onChange={e => setHuespedes(e.target.value)}>
                <option value="1,0">1 adulto</option>
                <option value="2,0">2 adultos</option>
                <option value="2,1">2 adultos, 1 niño</option>
                <option value="4,2">Familia (4+)</option>
              </select>
            </div>
            <Link to={buildSearchUrl()} className="btn btn-accent search-btn">
              <HiOutlineSearch size={20} />
              Buscar
            </Link>
          </div>
        </div>
      </section>

      {/* ── Propuesta de Valor ────────────────────────── */}
      <section className="features-section container">
        <div className="features-grid">
          {[
            { icon: <HiOutlineShieldCheck size={28} />, title: 'Seguridad Total', desc: 'Anti-XSS, Rate Limiting, JWT y validaciones de negocio integradas.' },
            { icon: <HiOutlineLightningBolt size={28} />, title: 'Ultra Rápido', desc: 'gRPC binario para comunicaciones internas a velocidad de milisegundos.' },
            { icon: <HiOutlineGlobe size={28} />, title: 'GraphQL Flexible', desc: 'Consultas personalizadas: pide exactamente los datos que necesitas.' },
            { icon: <HiOutlineStar size={28} />, title: 'Experiencia Premium', desc: 'Interfaz intuitiva diseñada para una experiencia de reserva impecable.' },
          ].map((f, i) => (
            <div className="feature-card animate-fade-in-up" key={i} style={{ animationDelay: `${i * .1}s` }}>
              <div className="feature-icon">{f.icon}</div>
              <h3 className="feature-title">{f.title}</h3>
              <p className="feature-desc">{f.desc}</p>
            </div>
          ))}
        </div>
      </section>

      {/* ── Bento Grid de Amenidades ──────────────────── */}
      <section className="amenities-section container">
        <div className="section-header">
          <span className="section-tag">Experiencias</span>
          <h2 className="section-title">Todo lo que necesitas en un solo lugar</h2>
          <p className="section-subtitle">Desde suites de ensueño hasta gastronomía de autor, cada detalle está pensado para ti.</p>
        </div>

        <div className="bento-grid">
          <div className="bento-item bento-large">
            <img src={luxuryRoom} alt="Suite de Lujo" loading="lazy" />
            <div className="bento-caption">
              <span className="bento-tag">Hospedaje</span>
              <h3>Suites de Lujo</h3>
              <p>Habitaciones con vista panorámica y amenidades de primer nivel.</p>
            </div>
          </div>
          <div className="bento-item">
            <img src={lobby} alt="Lobby del Hotel" loading="lazy" />
            <div className="bento-caption">
              <span className="bento-tag">Experiencia</span>
              <h3>Lobbies Icónicos</h3>
            </div>
          </div>
          <div className="bento-item">
            <img src={dining} alt="Gastronomía" loading="lazy" />
            <div className="bento-caption">
              <span className="bento-tag">Gastronomía</span>
              <h3>Fine Dining</h3>
            </div>
          </div>
          <div className="bento-item bento-wide">
            <img src={spa} alt="Spa y Bienestar" loading="lazy" />
            <div className="bento-caption">
              <span className="bento-tag">Bienestar</span>
              <h3>Spa & Wellness</h3>
              <p>Centros de relajación con piscinas interiores y tratamientos premium.</p>
            </div>
          </div>
        </div>
      </section>

      {/* ── CTA Final ────────────────────────────────── */}
      <section className="cta-section">
        <div className="container cta-inner">
          <h2 className="cta-title">¿Listo para vivir la experiencia?</h2>
          <p className="cta-subtitle">Únete a miles de viajeros que confían en nuestra plataforma segura.</p>
          <Link to="/propiedades" className="btn btn-accent">
            Explorar Propiedades
          </Link>
        </div>
      </section>
    </div>
  );
}
