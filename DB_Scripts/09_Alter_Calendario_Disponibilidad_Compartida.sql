-- =====================================================
-- MIGRACIÓN 09: Disponibilidad Compartida (ReservaId & Origen)
-- BASE DE DATOS: db_alojamientos (Supabase / Local)
-- PROPÓSITO: Agregar campos ReservaId y Origen a la tabla
--            CalendarioDisponibilidad para identificar reservas
--            y bloquear disponibilidad de forma bidireccional.
-- ORDEN: Ejecutar después de 02_DB_Alojamientos.sql
-- =====================================================

-- 1. Agregar la columna ReservaId (puede almacenar INT local o UUID de Booking)
ALTER TABLE CalendarioDisponibilidad
    ADD COLUMN IF NOT EXISTS ReservaId VARCHAR(50) NULL;

-- 2. Agregar la columna Origen (ALOJAEXPRESS o BOOKING)
ALTER TABLE CalendarioDisponibilidad
    ADD COLUMN IF NOT EXISTS Origen VARCHAR(20) NOT NULL DEFAULT 'ALOJAEXPRESS';

-- =====================================================
-- VERIFICACIÓN
-- =====================================================
SELECT column_name, data_type 
FROM information_schema.columns 
WHERE table_name = 'calendariodisponibilidad' AND column_name IN ('reservaid', 'origen');
