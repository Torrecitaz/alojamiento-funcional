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
