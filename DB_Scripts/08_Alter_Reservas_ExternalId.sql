-- =====================================================
-- MIGRACIÓN 08: Agregar ExternalId (UUID) a Reservas
-- BASE DE DATOS: db_reservas (Supabase / Local)
-- PROPÓSITO: Permitir asociar y buscar reservas creadas
--            desde Booking.com usando su UUID único.
-- ORDEN: Ejecutar después de 03_DB_Reservas.sql
-- =====================================================

-- 1. Agregar la columna ExternalId (UUID, opcional)
ALTER TABLE Reservas
    ADD COLUMN IF NOT EXISTS ExternalId UUID NULL;

-- 2. Crear índice único parcial (evita duplicados de Booking)
CREATE UNIQUE INDEX IF NOT EXISTS UX_Reservas_ExternalId
    ON Reservas (ExternalId)
    WHERE ExternalId IS NOT NULL;

-- =====================================================
-- VERIFICACIÓN
-- =====================================================
SELECT column_name, data_type 
FROM information_schema.columns 
WHERE table_name = 'reservas' AND column_name = 'externalid';
