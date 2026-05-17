-- EXTENSIONES NECESARIAS
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-----------------------------------------------------
-- 1. PROVEEDORES (Servicios externos)
-----------------------------------------------------
CREATE TABLE proveedores (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  nombre text NOT NULL,
  url_api_base text NOT NULL, -- URL del servicio externo
  tipo_servicio text NOT NULL, -- Ej: 'atracciones', 'vuelos', 'hoteles'
  activo boolean DEFAULT true,
  created_at timestamptz DEFAULT now()
);

-----------------------------------------------------
-- 2. CATEGORÍAS
-----------------------------------------------------
CREATE TABLE categorias (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  nombre text NOT NULL UNIQUE, -- Ej: 'Atracciones', 'Vuelos'
  descripcion text,
  created_at timestamptz DEFAULT now()
);

-----------------------------------------------------
-- 3. PRODUCTOS (ESPEJO DE SERVICIOS EXTERNOS)
-----------------------------------------------------
CREATE TABLE productos (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),

  id_proveedor uuid REFERENCES proveedores(id) ON DELETE CASCADE,
  id_categoria uuid REFERENCES categorias(id),

  id_externo text NOT NULL, -- ID del sistema externo

  nombre text NOT NULL,
  descripcion text,

  precio decimal(10,2) NOT NULL, -- precio desde
  moneda text DEFAULT 'USD',

  ubicacion text,
  imagen_url text,

  disponible boolean DEFAULT true,

  metadata jsonb, -- 🔥 FLEXIBLE (para datos extra según servicio)

  created_at timestamptz DEFAULT now(),

  UNIQUE (id_proveedor, id_externo) -- 🔥 evita duplicados
);

-----------------------------------------------------
-- 4. DISPONIBILIDAD (NUEVO - CLAVE)
-----------------------------------------------------
CREATE TABLE disponibilidad_productos (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),

  id_producto uuid REFERENCES productos(id) ON DELETE CASCADE,

  fecha date NOT NULL,
  cupos_disponibles integer NOT NULL,

  created_at timestamptz DEFAULT now(),

  UNIQUE (id_producto, fecha)
);

-----------------------------------------------------
-- 5. USUARIOS (ADMIN)
-----------------------------------------------------
CREATE TABLE usuarios (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  email text UNIQUE NOT NULL,
  password_hash text NOT NULL,
  rol text CHECK (rol IN ('admin', 'staff')) DEFAULT 'admin',
  created_at timestamptz DEFAULT now()
);

-----------------------------------------------------
-- 6. CLIENTES
-----------------------------------------------------
CREATE TABLE clientes (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  nombre text NOT NULL,
  apellido text NOT NULL,
  email text UNIQUE NOT NULL,
  telefono text,
  created_at timestamptz DEFAULT now()
);

-----------------------------------------------------
-- 7. MÉTODOS DE PAGO
-----------------------------------------------------
CREATE TABLE metodos_pago (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  nombre text NOT NULL UNIQUE,
  created_at timestamptz DEFAULT now()
);

-----------------------------------------------------
-- 8. CARRITOS
-----------------------------------------------------
CREATE TABLE carritos (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  id_cliente uuid REFERENCES clientes(id) ON DELETE CASCADE,
  created_at timestamptz DEFAULT now()
);

-----------------------------------------------------
-- 9. ITEMS DEL CARRITO
-----------------------------------------------------
CREATE TABLE items_carrito (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),

  id_carrito uuid REFERENCES carritos(id) ON DELETE CASCADE,
  id_producto uuid REFERENCES productos(id),

  cantidad integer DEFAULT 1,

  precio_unitario decimal(10,2) NOT NULL,

  created_at timestamptz DEFAULT now()
);

-----------------------------------------------------
-- 10. RESERVAS
-----------------------------------------------------
CREATE TABLE reservas (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),

  id_cliente uuid REFERENCES clientes(id),

  estado text DEFAULT 'pendiente'
    CHECK (estado IN ('pendiente', 'confirmada', 'cancelada')),

  total decimal(10,2) NOT NULL,

  fecha_reserva timestamptz DEFAULT now(),

  created_at timestamptz DEFAULT now()
);

-----------------------------------------------------
-- 11. DETALLE DE RESERVA
-----------------------------------------------------
CREATE TABLE detalles_reserva (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),

  id_reserva uuid REFERENCES reservas(id) ON DELETE CASCADE,

  id_producto uuid REFERENCES productos(id),
  id_proveedor uuid REFERENCES proveedores(id),

  id_externo text NOT NULL, -- ID del servicio externo

  nombre text, -- snapshot
  descripcion text,

  cantidad integer NOT NULL,
  precio_unitario decimal(10,2) NOT NULL,

  metadata jsonb, -- 🔥 snapshot flexible

  created_at timestamptz DEFAULT now()
);

-----------------------------------------------------
-- 12. PAGOS
-----------------------------------------------------
CREATE TABLE pagos (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),

  id_reserva uuid REFERENCES reservas(id),

  id_metodo_pago uuid REFERENCES metodos_pago(id),

  monto decimal(10,2) NOT NULL,

  estado text DEFAULT 'pendiente'
    CHECK (estado IN ('pendiente', 'pagado', 'fallido')),

  fecha_pago timestamptz DEFAULT now()
);

-----------------------------------------------------
-- 13. FACTURAS
-----------------------------------------------------
CREATE TABLE facturas (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),

  id_reserva uuid REFERENCES reservas(id),

  numero_factura text UNIQUE NOT NULL,

  total decimal(10,2) NOT NULL,

  fecha_emision timestamptz DEFAULT now()
);