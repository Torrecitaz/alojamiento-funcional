-- =====================================================
-- SCRIPT 06: ACTUALIZACIONES DE BASE DE DATOS (AlojaExpress)
-- ORDEN DE EJECUCIÓN: 6
-- =====================================================

-- Conectarse/Ejecutar en la base de datos db_alojamientos
-- Agregar columnas adicionales requeridas para la gestión de alojamientos
ALTER TABLE Alojamientos ADD COLUMN IF NOT EXISTS Provincia VARCHAR(100) NULL;
ALTER TABLE Alojamientos ADD COLUMN IF NOT EXISTS Pais VARCHAR(100) NULL;
ALTER TABLE Alojamientos ADD COLUMN IF NOT EXISTS Politicas TEXT NULL;
ALTER TABLE Alojamientos ADD COLUMN IF NOT EXISTS CheckInTime VARCHAR(50) NULL;
ALTER TABLE Alojamientos ADD COLUMN IF NOT EXISTS CheckOutTime VARCHAR(50) NULL;
ALTER TABLE Alojamientos ADD COLUMN IF NOT EXISTS Servicios TEXT NULL;

-- Agregar columnas adicionales requeridas para la gestión de habitaciones
ALTER TABLE Habitaciones ADD COLUMN IF NOT EXISTS Estado VARCHAR(20) DEFAULT 'Activo';
ALTER TABLE Habitaciones ADD COLUMN IF NOT EXISTS Fotos TEXT NULL;

-- Asegurar que los registros existentes tengan datos válidos por defecto
UPDATE Alojamientos SET Provincia = 'Pichincha', Pais = 'Ecuador', CheckInTime = '14:00', CheckOutTime = '11:00' WHERE Provincia IS NULL;
UPDATE Habitaciones SET Estado = 'Activo' WHERE Estado IS NULL;
