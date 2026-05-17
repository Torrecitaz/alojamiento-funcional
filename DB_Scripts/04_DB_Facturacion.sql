-- =====================================================
-- SCRIPT 04: MICROSERVICIO FACTURACIÓN (DB_Facturacion)
-- ORDEN DE EJECUCIÓN: 4
-- =====================================================

-- Limpieza previa para evitar errores si se ejecuta múltiples veces
DROP TABLE IF EXISTS AuditoriaGeneral CASCADE;
DROP TABLE IF EXISTS DetalleFacturas CASCADE;
DROP TABLE IF EXISTS Facturas CASCADE;
DROP TABLE IF EXISTS MetodosPagoCliente CASCADE;
DROP FUNCTION IF EXISTS update_fecha_modificacion() CASCADE;
DROP PROCEDURE IF EXISTS sp_registrar_factura_completa(INT, INT, DECIMAL, VARCHAR) CASCADE;

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
-- 2. TABLAS FACTURACIÓN
-- -----------------------------------------------------
CREATE TABLE MetodosPagoCliente (
    MetodoPagoId    SERIAL PRIMARY KEY,
    Tipo            VARCHAR(30) NOT NULL -- DEBITO, CREDITO, EnSitio
);

CREATE TABLE Facturas (
    FacturaId       SERIAL PRIMARY KEY,
    ReservaId       INT NOT NULL, -- Ref Lógica a DB_Reservas.Reservas
    MetodoPagoId    INT NULL,
    Monto           DECIMAL(12,2) NOT NULL,
    Estado          VARCHAR(30) DEFAULT 'Aprobado',
    FechaPago       TIMESTAMP NULL,
    FechaCreacion   TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FechaModificacion TIMESTAMP NULL,
    CONSTRAINT FK_Facturas_MetodosPago FOREIGN KEY (MetodoPagoId) REFERENCES MetodosPagoCliente(MetodoPagoId)
);

CREATE TABLE DetalleFacturas(
    DetalleFacturaId SERIAL PRIMARY KEY,
    FacturaId        INT NOT NULL,
    Descripcion      VARCHAR(200) NOT NULL, 
    Cantidad         INT NOT NULL DEFAULT 1, 
    PrecioUnitario   DECIMAL(12,2) NOT NULL, 
    CONSTRAINT FK_DetalleFacturas_Facturas FOREIGN KEY (FacturaId) REFERENCES Facturas(FacturaId)
);

-- -----------------------------------------------------
-- 3. TABLA AUDITORÍA
-- -----------------------------------------------------
CREATE TABLE AuditoriaGeneral (
    AuditoriaId     BIGSERIAL PRIMARY KEY,
    NombreTabla     VARCHAR(100) NOT NULL,
    Operacion       VARCHAR(10) NOT NULL,
    RegistroId      VARCHAR(50) NOT NULL,
    DatosAnteriores TEXT NULL,
    DatosNuevos     TEXT NULL,
    UsuarioAccion   VARCHAR(100) NULL,
    FechaAccion     TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Origen          VARCHAR(45) NULL
);

-- -----------------------------------------------------
-- 4. TRIGGERS
-- -----------------------------------------------------
CREATE TRIGGER TRG_Update_Facturas
BEFORE UPDATE ON Facturas
FOR EACH ROW EXECUTE PROCEDURE update_fecha_modificacion();

-- -----------------------------------------------------
-- 5. PROCEDIMIENTOS ALMACENADOS Y FUNCIONES
-- -----------------------------------------------------
CREATE OR REPLACE PROCEDURE sp_registrar_factura_completa(
    p_reserva_id INT, p_metodo_pago INT, p_monto DECIMAL, p_descripcion VARCHAR
)
LANGUAGE plpgsql AS $$
DECLARE
    v_factura_id INT;
BEGIN
    INSERT INTO Facturas (ReservaId, MetodoPagoId, Monto, FechaPago)
    VALUES (p_reserva_id, p_metodo_pago, p_monto, CURRENT_TIMESTAMP)
    RETURNING FacturaId INTO v_factura_id;

    INSERT INTO DetalleFacturas (FacturaId, Descripcion, Cantidad, PrecioUnitario)
    VALUES (v_factura_id, p_descripcion, 1, p_monto);
END;
$$;
