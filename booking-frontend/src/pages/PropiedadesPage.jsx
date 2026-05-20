import { useState, useEffect } from 'react';
import { useLocation, Link } from 'react-router-dom';
import { HiOutlineStar, HiOutlineLocationMarker, HiOutlineSearch } from 'react-icons/hi';
import { PawPrint } from 'lucide-react';
import toast from 'react-hot-toast';
import api from '../services/api';
import './PropiedadesPage.css';

export default function PropiedadesPage() {
  const [propiedades, setPropiedades] = useState([]);
  const [ciudades, setCiudades] = useState([]);
  const [loading, setLoading] = useState(true);
  const location = useLocation();
  
  // Filtros Avanzados y QueryParams
  const searchParams = new URLSearchParams(location.search);
  const ciudadTextParam = searchParams.get('ciudadText') || '';
  
  const [nombreBusqueda, setNombreBusqueda] = useState('');
  const [ciudadFiltro, setCiudadFiltro] = useState('');
  const [estrellasMinimas, setEstrellasMinimas] = useState('');
  const [admiteMascotas, setAdmiteMascotas] = useState(false);
  const [fechaCheckIn, setFechaCheckIn] = useState(searchParams.get('checkIn') || '');
  const [fechaCheckOut, setFechaCheckOut] = useState(searchParams.get('checkOut') || '');
  const [numAdultos, setNumAdultos] = useState(searchParams.get('adultos') || '1');
  const [numNinos, setNumNinos] = useState(searchParams.get('ninos') || '0');

  useEffect(() => {
    fetchCiudades();
    fetchPropiedades();
    // eslint-disable-next-line
  }, []);

  const fetchCiudades = async () => {
    try {
      const { data } = await api.get('/maestros/ciudades');
      setCiudades(data.datos || []);
    } catch (err) {
      console.error('Error fetching ciudades', err);
    }
  };

  const fetchPropiedades = async () => {
    setLoading(true);
    try {
      const params = new URLSearchParams();
      if (ciudadFiltro) params.append('CiudadId', ciudadFiltro);
      if (estrellasMinimas) params.append('EstrellasMinimas', estrellasMinimas);
      if (admiteMascotas) params.append('AdmiteMascotas', 'true');
      if (fechaCheckIn) params.append('FechaCheckIn', fechaCheckIn);
      if (fechaCheckOut) params.append('FechaCheckOut', fechaCheckOut);
      if (numAdultos) params.append('NumAdultos', numAdultos);
      if (numNinos) params.append('NumNinos', numNinos);

      const { data } = await api.get(`/propiedades/buscar?${params.toString()}`);
      setPropiedades(data.datos?.items || []);
    } catch {
      toast.error('Error al cargar propiedades.');
    } finally {
      setLoading(false);
    }
  };

  const filtered = propiedades.filter((p) => {
    const matchName = !nombreBusqueda || (p.nombre || '').toLowerCase().includes(nombreBusqueda.toLowerCase());
    const matchCiudadText = !ciudadTextParam || (p.ciudad || '').toLowerCase().includes(ciudadTextParam.toLowerCase());
    return matchName && matchCiudadText;
  });

  const handleFilterSubmit = (e) => {
    e.preventDefault();
    fetchPropiedades();
  };

  const renderStars = (n) =>
    Array.from({ length: 5 }, (_, i) => (
      <HiOutlineStar key={i} style={{ fill: i < n ? 'var(--color-accent)' : 'none', color: i < n ? 'var(--color-accent)' : 'var(--color-text-muted)' }} />
    ));

  return (
    <div className="propiedades-page container">
      <div className="page-header">
        <div>
          <h1 className="page-title">Propiedades Disponibles</h1>
          <p className="page-subtitle">Explora nuestro catálogo de hospedaje premium.</p>
        </div>
      </div>

      <form className="advanced-filters" onSubmit={handleFilterSubmit}>
        <div className="filter-group">
          <HiOutlineSearch className="filter-icon" />
          <input
            type="text"
            placeholder="Buscar por nombre..."
            className="filter-input"
            value={nombreBusqueda}
            onChange={(e) => setNombreBusqueda(e.target.value)}
          />
        </div>
        <div className="filter-group">
          <select 
            className="filter-input" 
            value={ciudadFiltro} 
            onChange={(e) => setCiudadFiltro(e.target.value)}
          >
            <option value="">Cualquier Ciudad</option>
            {ciudades.map(c => (
              <option key={c.ciudadId} value={c.ciudadId}>{c.nombre}</option>
            ))}
          </select>
        </div>
        <div className="filter-group">
          <select 
            className="filter-input" 
            value={estrellasMinimas} 
            onChange={(e) => setEstrellasMinimas(e.target.value)}
          >
            <option value="">Cualquier Categoría</option>
            <option value="3">3+ Estrellas</option>
            <option value="4">4+ Estrellas</option>
            <option value="5">5 Estrellas</option>
          </select>
        </div>
        <div className="filter-group checkbox-group">
          <label className="checkbox-label">
            <input 
              type="checkbox" 
              checked={admiteMascotas}
              onChange={(e) => setAdmiteMascotas(e.target.checked)}
            />
            <span>Pet Friendly</span>
          </label>
        </div>
        <button type="submit" className="btn btn-primary btn-search">
          Buscar
        </button>
      </form>

      {loading ? (
        <div className="grid-propiedades">
          {[1,2,3,4,5,6].map(i => (
            <div className="card prop-skeleton" key={i}>
              <div className="skeleton" style={{ height: 200 }} />
              <div style={{ padding: 20 }}>
                <div className="skeleton" style={{ height: 20, width: '70%', marginBottom: 10 }} />
                <div className="skeleton" style={{ height: 14, width: '50%', marginBottom: 10 }} />
                <div className="skeleton" style={{ height: 14, width: '40%' }} />
              </div>
            </div>
          ))}
        </div>
      ) : filtered.length === 0 ? (
        <div className="empty-state">
          <HiOutlineSearch size={48} />
          <h3>No se encontraron propiedades</h3>
          <p>Intenta cambiar los filtros de búsqueda.</p>
        </div>
      ) : (
        <div className="grid-propiedades">
          {filtered.map((prop, idx) => (
            <Link to={`/propiedades/${prop.propiedadId}`} className="card prop-card animate-fade-in-up" key={prop.propiedadId} style={{ animationDelay: `${idx * .05}s` }}>
              <div className="prop-img-container">
                <div className="prop-img-placeholder">
                  <HiOutlineLocationMarker size={32} />
                  <span>{prop.ciudad || 'Hotel'}</span>
                </div>
                {prop.admiteMascotas && (
                  <span className="prop-badge-pet">
                    <PawPrint size={14} /> Pet-Friendly
                  </span>
                )}
              </div>
              <div className="prop-info">
                <h3 className="prop-name">{prop.nombre}</h3>
                <div className="prop-meta">
                  <span className="prop-location">
                    <HiOutlineLocationMarker size={14} /> {prop.ciudad || 'N/D'}
                  </span>
                  <div className="stars">{renderStars(prop.estrellas)}</div>
                </div>
                <div className="prop-footer">
                  <span className="prop-rating">
                    ★ {prop.calificacionPromedio?.toFixed(1) || '0.0'}
                  </span>
                  <span className="btn btn-primary btn-sm">Ver Detalle</span>
                </div>
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
