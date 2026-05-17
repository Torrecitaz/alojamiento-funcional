-- =====================================================
-- SCRIPT 01: MICROSERVICIO USUARIOS (DB_Usuarios)
-- ORDEN DE EJECUCIÓN: 1
-- =====================================================

-- Limpieza previa para evitar errores si se ejecuta múltiples veces
DROP TABLE IF EXISTS Clientes CASCADE;
DROP TABLE IF EXISTS Usuarios CASCADE;
DROP TABLE IF EXISTS Localizaciones CASCADE;
DROP FUNCTION IF EXISTS update_fecha_modificacion() CASCADE;
DROP PROCEDURE IF EXISTS sp_registrar_cliente(VARCHAR, VARCHAR, VARCHAR, VARCHAR, VARCHAR, VARCHAR) CASCADE;

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
CREATE TABLE Localizaciones (
    LocalizacionId  SERIAL PRIMARY KEY,
    Area            POLYGON,
    Descripcion     VARCHAR(500) NULL
);

CREATE TABLE Usuarios (  
    UsuarioId       SERIAL PRIMARY KEY,
    Rol             VARCHAR(10) NOT NULL DEFAULT 'Cliente',
    Email           VARCHAR(200) NOT NULL UNIQUE,
    PasswordHash    VARCHAR(500) NOT NULL,
    NombreCompleto  VARCHAR(200) NOT NULL,
    Estado          BOOLEAN DEFAULT TRUE,
    FechaCreacion   TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FechaModificacion TIMESTAMP NULL
);

CREATE TABLE Clientes (
    ClienteId       SERIAL PRIMARY KEY,
    UsuarioId       INT UNIQUE NULL,
    Cedula          VARCHAR(20) NOT NULL UNIQUE,
    FotoUrl         VARCHAR(500) NULL,
    Telefono        VARCHAR(20) NULL,
    Domicilio       VARCHAR(300) NULL,
    Email           VARCHAR(200) NOT NULL UNIQUE,
    TotalReservas   INT DEFAULT 0,
    FechaCreacion   TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FechaModificacion TIMESTAMP NULL,
    CONSTRAINT FK_Clientes_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId)
);

-- -----------------------------------------------------
-- 3. TRIGGERS
-- -----------------------------------------------------
CREATE TRIGGER TRG_Update_Usuarios
BEFORE UPDATE ON Usuarios
FOR EACH ROW EXECUTE PROCEDURE update_fecha_modificacion();

CREATE TRIGGER TRG_Update_Clientes
BEFORE UPDATE ON Clientes
FOR EACH ROW EXECUTE PROCEDURE update_fecha_modificacion();

-- -----------------------------------------------------
-- 4. PROCEDIMIENTOS ALMACENADOS Y FUNCIONES
-- -----------------------------------------------------
-- SP: Registrar un Cliente y su Usuario de forma transaccional
CREATE OR REPLACE PROCEDURE sp_registrar_cliente(
    p_email VARCHAR, p_password VARCHAR, p_nombre VARCHAR, p_cedula VARCHAR, p_telefono VARCHAR, p_domicilio VARCHAR
)
LANGUAGE plpgsql AS $$
DECLARE
    v_usuario_id INT;
BEGIN
    INSERT INTO Usuarios (Rol, Email, PasswordHash, NombreCompleto)
    VALUES ('Cliente', p_email, p_password, p_nombre)
    RETURNING UsuarioId INTO v_usuario_id;

    INSERT INTO Clientes (UsuarioId, Cedula, Telefono, Domicilio, Email)
    VALUES (v_usuario_id, p_cedula, p_telefono, p_domicilio, p_email);
END;
$$;
