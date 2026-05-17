-- =====================================================
-- SCRIPT 03: MICROSERVICIO RESERVAS (DB_Reservas)
-- ORDEN DE EJECUCIÓN: 3
-- =====================================================

-- Limpieza previa para evitar errores si se ejecuta múltiples veces
DROP TABLE IF EXISTS ReservaDetalleHabitacion CASCADE;
DROP TABLE IF EXISTS Reservas CASCADE;
DROP TABLE IF EXISTS Descuentos CASCADE;
DROP FUNCTION IF EXISTS update_fecha_modificacion() CASCADE;
DROP FUNCTION IF EXISTS fn_calcular_noches(DATE, DATE) CASCADE;
DROP PROCEDURE IF EXISTS sp_asignar_codigo_reserva(INT) CASCADE;

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
-- 2. TABLAS
-- -----------------------------------------------------
CREATE TABLE Descuentos(
  DescuentoId     SERIAL PRIMARY KEY,
  Codigo          VARCHAR(20) NOT NULL UNIQUE,
  Porcentaje      DECIMAL(5,2) NOT NULL,
  Activo          BOOLEAN DEFAULT TRUE  
);

CREATE TABLE Reservas (
    ReservaId       SERIAL PRIMARY KEY,
    DescuentoId     INT,
    ClienteId       INT NOT NULL, -- Ref Lógica a DB_Usuarios.Clientes
    AlojamientoId   INT NOT NULL, -- Ref Lógica a DB_Alojamientos.Alojamientos
    FechaCheckIn    DATE NOT NULL,
    FechaCheckOut   DATE NOT NULL,
    NumAdultos      INT NOT NULL DEFAULT 1,
    NumNinos        INT NOT NULL DEFAULT 0,
    LlevaMascotas   BOOLEAN DEFAULT FALSE,
    NumHabitaciones INT NOT NULL DEFAULT 1,
    SubTotal        DECIMAL(12,2) NOT NULL,
    Total           DECIMAL(12,2) NOT NULL, 
    Estado          VARCHAR(30) DEFAULT 'Pendiente',
    CodigoReserva   VARCHAR(20) NOT NULL UNIQUE,
    FechaCreacion   TIMESTAMP DEFAULT CURRENT_TIMESTAMP,   
    FechaModificacion TIMESTAMP NULL,
    CONSTRAINT CK_Reservas_Fechas CHECK (FechaCheckOut > FechaCheckIn),
    CONSTRAINT FK_Reservas_Descuentos FOREIGN KEY (DescuentoId) REFERENCES Descuentos(DescuentoId)
);

CREATE TABLE ReservaDetalleHabitacion (
    DetalleId       SERIAL PRIMARY KEY,
    ReservaId       INT NOT NULL,
    HabitacionId    INT NOT NULL, -- Ref Lógica a DB_Alojamientos.Habitaciones
    PrecioPorNoche  DECIMAL(10,2) NOT NULL,
    NumNoches       INT NOT NULL,
    SubTotalHabitacion DECIMAL(12,2) NOT NULL,
    CONSTRAINT FK_RDH_Reservas FOREIGN KEY (ReservaId) REFERENCES Reservas(ReservaId)
);

-- -----------------------------------------------------
-- 3. TRIGGERS
-- -----------------------------------------------------
CREATE TRIGGER TRG_Update_Reservas
BEFORE UPDATE ON Reservas
FOR EACH ROW EXECUTE PROCEDURE update_fecha_modificacion();

-- -----------------------------------------------------
-- 4. PROCEDIMIENTOS ALMACENADOS Y FUNCIONES
-- -----------------------------------------------------
CREATE OR REPLACE FUNCTION fn_calcular_noches(p_checkin DATE, p_checkout DATE)
RETURNS INT AS $$
BEGIN
    RETURN p_checkout - p_checkin;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE sp_asignar_codigo_reserva(p_reserva_id INT)
LANGUAGE plpgsql AS $$
DECLARE
    v_codigo VARCHAR;
BEGIN
    v_codigo := 'RES-' || p_reserva_id || '-' || SUBSTRING(MD5(RANDOM()::TEXT), 1, 4);
    
    UPDATE Reservas 
    SET CodigoReserva = v_codigo 
    WHERE ReservaId = p_reserva_id;
END;
$$;
