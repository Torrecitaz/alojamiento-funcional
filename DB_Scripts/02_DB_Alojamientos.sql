-- =====================================================
-- SCRIPT 02: MICROSERVICIO ALOJAMIENTOS (DB_Alojamientos)
-- ORDEN DE EJECUCIÓN: 2
-- =====================================================

-- Limpieza previa para evitar errores si se ejecuta múltiples veces
DROP TABLE IF EXISTS CalendarioDisponibilidad CASCADE;
DROP TABLE IF EXISTS Habitaciones CASCADE;
DROP TABLE IF EXISTS AlojamientoFotos CASCADE;
DROP TABLE IF EXISTS Alojamientos CASCADE;
DROP TABLE IF EXISTS TiposAlojamiento CASCADE;
DROP FUNCTION IF EXISTS update_fecha_modificacion() CASCADE;

-- -----------------------------------------------------
-- 1. FUNCIONES COMUNES
-- -----------------------------------------------------
CREATE OR REPLACE FUNCTION update_fecha_modificacion()
RETURNS TRIGGER AS $$
BEGIN
    NEW.FechaModificacion = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- -----------------------------------------------------
-- 2. TABLAS BASE
-- -----------------------------------------------------
CREATE TABLE TiposAlojamiento (
    TipoAlojamientoId   SERIAL PRIMARY KEY,
    Nombre              VARCHAR(50) NOT NULL UNIQUE,
    Descripcion         VARCHAR(200) NULL
);

CREATE TABLE Alojamientos (
    AlojamientoId         SERIAL PRIMARY KEY,
    SocioId               INT NOT NULL, -- Ref Lógica a DB_Usuarios.Usuarios
    TipoAlojamientoId     INT NOT NULL,
    Ciudad                VARCHAR(100), 
    Nombre                VARCHAR(200) NOT NULL,
    Descripcion           TEXT NULL,
    Direccion             VARCHAR(300) NOT NULL,
    Coordenadas           POINT,
    Estrellas             INT NULL CHECK (Estrellas BETWEEN 1 AND 5),
    CalificacionPromedio  DECIMAL(3,2) DEFAULT 0,
    TotalResenas          INT DEFAULT 0,
    AdmiteMascotas        BOOLEAN DEFAULT FALSE, 
    TienePiscina          BOOLEAN DEFAULT FALSE, -- Mantenido por simplicidad
    TieneParqueadero      BOOLEAN DEFAULT FALSE, -- Mantenido por simplicidad
    Estado                VARCHAR(20) DEFAULT 'Pendiente',
    FechaCreacion         TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FechaModificacion     TIMESTAMP NULL,
    CONSTRAINT FK_Alojamiento_TipoAlojamiento FOREIGN KEY (TipoAlojamientoId) REFERENCES TiposAlojamiento(TipoAlojamientoId)
);

CREATE TABLE AlojamientoFotos (
    FotoId          SERIAL PRIMARY KEY,
    AlojamientoId   INT NOT NULL,
    Url             VARCHAR(500) NOT NULL,
    Orden           INT DEFAULT 0,
    Descripcion     VARCHAR(200) NULL,
    CONSTRAINT FK_AlojamientoFotos_Alojamiento FOREIGN KEY (AlojamientoId) REFERENCES Alojamientos(AlojamientoId)
);

CREATE TABLE Habitaciones (
    HabitacionId        SERIAL PRIMARY KEY,
    AlojamientoId       INT NOT NULL,
    Nombre              VARCHAR(100) NOT NULL,
    Descripcion         VARCHAR(500) NULL,
    CapacidadAdultos    INT NOT NULL DEFAULT 2,
    CapacidadNinos      INT NOT NULL DEFAULT 0,
    NumBanos            INT NOT NULL DEFAULT 1,
    NumDormitorios      INT NOT NULL DEFAULT 1,
    TieneCocina         BOOLEAN DEFAULT FALSE,
    TieneAireAcondicionado BOOLEAN DEFAULT FALSE,
    SuperficieM2        DECIMAL(6,2) NULL,
    PrecioNoche         DECIMAL(10,2) NOT NULL DEFAULT 0, -- Precio estático por simplicidad
    FechaModificacion   TIMESTAMP NULL,
    CONSTRAINT FK_Habitaciones_Alojamiento FOREIGN KEY (AlojamientoId) REFERENCES Alojamientos(AlojamientoId)
);

-- -----------------------------------------------------
-- 3. CALENDARIO DE DISPONIBILIDAD
-- -----------------------------------------------------
CREATE TABLE CalendarioDisponibilidad (
    CalendarioId    SERIAL PRIMARY KEY,
    HabitacionId    INT NOT NULL,
    Fecha           DATE NOT NULL,
    Estado          VARCHAR(20) NOT NULL DEFAULT 'Disponible', -- Disponible, Ocupado, Bloqueado
    FechaModificacion TIMESTAMP NULL,
    CONSTRAINT FK_Calendario_Habitacion FOREIGN KEY (HabitacionId) REFERENCES Habitaciones(HabitacionId),
    CONSTRAINT UQ_Habitacion_Fecha UNIQUE (HabitacionId, Fecha)
);

-- -----------------------------------------------------
-- 4. TRIGGERS
-- -----------------------------------------------------
CREATE TRIGGER TRG_Update_Alojamientos
BEFORE UPDATE ON Alojamientos
FOR EACH ROW EXECUTE PROCEDURE update_fecha_modificacion();

CREATE TRIGGER TRG_Update_Habitaciones
BEFORE UPDATE ON Habitaciones
FOR EACH ROW EXECUTE PROCEDURE update_fecha_modificacion();

CREATE TRIGGER TRG_Update_Calendario
BEFORE UPDATE ON CalendarioDisponibilidad
FOR EACH ROW EXECUTE PROCEDURE update_fecha_modificacion();

-- -----------------------------------------------------
-- 5. SEMILLA DE DATOS DE PRUEBA (DATA SEEDING)
-- -----------------------------------------------------

-- Insertar Tipos de Alojamiento
INSERT INTO TiposAlojamiento (Nombre, Descripcion) VALUES
('Hotel', 'Establecimiento que ofrece alojamiento y servicios de comedor y otros servicios complementarios.'),
('Resort', 'Complejo de vacaciones con instalaciones recreativas, de ocio y servicios de lujo.'),
('Eco-Lodge', 'Hospedaje ecologico disenado para tener el menor impacto posible en el entorno natural.'),
('Suite', 'Habitacion de gran tamano con sala de estar independiente y servicios premium.')
ON CONFLICT (Nombre) DO NOTHING;

-- Insertar Alojamientos (SocioId = 1 para pruebas)
INSERT INTO Alojamientos (SocioId, TipoAlojamientoId, Ciudad, Nombre, Descripcion, Direccion, Estrellas, CalificacionPromedio, TotalResenas, AdmiteMascotas, TienePiscina, TieneParqueadero, Estado) VALUES
(1, 2, 'Santa Cruz', 'Royal Galapagos Beach Resort', 'Disfrute de una experiencia de lujo inigualable frente al mar en las Islas Galapagos. Habitaciones con vista al oceano, spa completo, restaurante gourmet y acceso directo a la playa de arena blanca.', 'Av. Charles Darwin km 4.5, Puerto Ayora', 5, 4.90, 48, TRUE, TRUE, TRUE, 'Activo'),
(1, 1, 'Quito', 'Hotel Vista Hermosa Historic Center', 'Un hotel boutique ubicado en el corazon del Centro Historico de Quito. Ofrece terrazas con vistas panoramicas a las iglesias coloniales, arquitectura del siglo XVII restaurada y un ambiente intimo y acogedor.', 'Calle Chile 456 y Guayaquil, Centro Historico', 4, 4.70, 85, FALSE, FALSE, TRUE, 'Activo'),
(1, 3, 'Tena', 'Selva Mistica Eco-Lodge', 'Sumerjase en la majestuosidad de la Amazonia ecuatoriana. Hospedaje ecologico construido con materiales locales sustentables, excursiones guiadas de observacion de flora y fauna, y cocina amazonica organica.', 'Km 12 Via al Puyo, Margen del Rio Napo', 4, 4.85, 32, TRUE, TRUE, TRUE, 'Activo'),
(1, 1, 'Guayaquil', 'Reto2_Mateo_Torres', 'Alojamiento moderno y funcional, disenado especialmente para cumplir con el Reto 2. Cuenta con todas las comodidades tecnologicas y servicios premium en una ubicacion privilegiada.', 'Av. Francisco de Orellana 123', 5, 5.00, 10, TRUE, TRUE, TRUE, 'Activo')
ON CONFLICT DO NOTHING;

-- Insertar Fotos para Alojamientos (IDs correspondientes 1, 2, 3 y 4)
INSERT INTO AlojamientoFotos (AlojamientoId, Url, Orden, Descripcion) VALUES
(1, 'https://images.unsplash.com/photo-1571003123894-1f0594d2b5d9?auto=format&fit=crop&w=800&q=80', 1, 'Vista aerea de la piscina infinity frente al mar'),
(1, 'https://images.unsplash.com/photo-1566073771259-6a8506099945?auto=format&fit=crop&w=800&q=80', 2, 'Fachada principal y jardines tropicales'),
(2, 'https://images.unsplash.com/photo-1543968332-f99478b1ebdc?auto=format&fit=crop&w=800&q=80', 1, 'Fachada colonial iluminada por la noche'),
(2, 'https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?auto=format&fit=crop&w=800&q=80', 2, 'Terraza panoramica con vistas a la Basilica'),
(3, 'https://images.unsplash.com/photo-1470770841072-f978cf4d019e?auto=format&fit=crop&w=800&q=80', 1, 'Bungalows construidos sobre el dosel de la selva'),
(3, 'https://images.unsplash.com/photo-1507525428034-b723cf961d3e?auto=format&fit=crop&w=800&q=80', 2, 'Rio Napo desde la terraza principal'),
(4, 'https://images.unsplash.com/photo-1551882547-ff40c63fe5fa?auto=format&fit=crop&w=800&q=80', 1, 'Vista exterior de Reto2_Mateo_Torres'),
(4, 'https://images.unsplash.com/photo-1566665797739-1674de7a421a?auto=format&fit=crop&w=800&q=80', 2, 'Habitacion de lujo en Reto2_Mateo_Torres')
ON CONFLICT DO NOTHING;

-- Insertar Habitaciones
INSERT INTO Habitaciones (AlojamientoId, Nombre, Descripcion, CapacidadAdultos, CapacidadNinos, NumBanos, NumDormitorios, TieneCocina, TieneAireAcondicionado, SuperficieM2, PrecioNoche) VALUES
(1, 'Ocean View Villa', 'Exclusiva villa privada frente a la playa con terraza, piscina de inmersion, bano spa de marmol y sistema de entretenimiento premium.', 2, 2, 2, 2, TRUE, TRUE, 120.00, 250.00),
(1, 'Garden Suite', 'Elegante suite rodeada de jardines exoticos y palmeras. Cuenta con cama king size, bano privado con ducha tipo lluvia y balcon privado.', 2, 0, 1, 1, FALSE, TRUE, 65.00, 180.00),
(2, 'Habitacion Colonial Superior', 'Espaciosa habitacion colonial con techos altos de madera tallada original, mobiliario de epoca restaurado, balcon a la calle peatonal y bano moderno.', 2, 1, 1, 1, FALSE, FALSE, 45.00, 85.00),
(2, 'Suite Panoramica', 'Lujosa suite en el piso superior con ventanales de piso a techo y vistas de 360 grados al centro historico de la ciudad.', 2, 0, 1, 1, TRUE, FALSE, 75.00, 120.00),
(3, 'Bungalow Familiar de la Selva', 'Bungalow rustico de dos pisos suspendido entre los arboles. Cuenta con mallas mosquiteras de alta calidad, balcon con hamacas y bano al aire libre estilo eco.', 4, 2, 2, 3, FALSE, TRUE, 95.00, 110.00),
(4, 'Suite Master Mateo Torres', 'Espaciosa suite equipada con cama King, bano con jacuzzi, Smart TV, cocina completa y aire acondicionado de ultima tecnologia.', 2, 2, 1, 1, TRUE, TRUE, 70.00, 160.00),
(4, 'Estudio Ejecutivo Torres', 'Estudio funcional ideal para viajeros de negocios, con area de trabajo comoda, bano privado y conexion a internet de alta velocidad.', 1, 0, 1, 1, FALSE, TRUE, 35.00, 90.00)
ON CONFLICT DO NOTHING;

-- Insertar Disponibilidad inicial en CalendarioDisponibilidad para los proximos 30 dias para todas las habitaciones
DO $$
DECLARE
    r RECORD;
    d DATE;
BEGIN
    FOR r IN SELECT HabitacionId FROM Habitaciones LOOP
        FOR i IN 0..29 LOOP
            d := CURRENT_DATE + i;
            INSERT INTO CalendarioDisponibilidad (HabitacionId, Fecha, Estado)
            VALUES (r.HabitacionId, d, 'Disponible')
            ON CONFLICT (HabitacionId, Fecha) DO NOTHING;
        END LOOP;
    END LOOP;
END $$;
