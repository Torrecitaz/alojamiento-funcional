-- =====================================================
-- MIGRACIÓN 05: Agregar ExternalId (UUID) a MetodosPagoCliente
-- BASE DE DATOS: DB_Facturacion (Supabase West-2)
-- PROPÓSITO: Permitir que Booking referencie métodos de pago
--            por UUID externo sin necesidad de conocer el ID interno.
-- ORDEN: Ejecutar DESPUÉS de 04_DB_Facturacion.sql
-- =====================================================

-- 1. Agregar la columna ExternalId (UUID, opcional)
ALTER TABLE metodospagocliente
    ADD COLUMN IF NOT EXISTS ExternalId UUID NULL;

-- 2. Crear índice único parcial (solo en registros con ExternalId no nulo)
CREATE UNIQUE INDEX IF NOT EXISTS UX_MetodosPagoCliente_ExternalId
    ON metodospagocliente (ExternalId)
    WHERE ExternalId IS NOT NULL;

-- =====================================================
-- 3. ACTUALIZAR REGISTROS EXISTENTES CON UUIDS DE EJEMPLO
--    (Copiar estos UUIDs exactos al panel de Booking para pruebas)
-- =====================================================

-- Asegurar existencia de los registros base
INSERT INTO metodospagocliente (MetodoPagoId, Tipo) VALUES
(1, 'DEBITO'),
(2, 'CREDITO'),
(3, 'EnSitio')
ON CONFLICT (MetodoPagoId) DO NOTHING;

-- DEBITO  (MetodoPagoId = 1)
UPDATE metodospagocliente
SET ExternalId = '11111111-1111-1111-1111-111111111111'
WHERE MetodoPagoId = 1 AND ExternalId IS NULL;

-- CREDITO  (MetodoPagoId = 2)
UPDATE metodospagocliente
SET ExternalId = '22222222-2222-2222-2222-222222222222'
WHERE MetodoPagoId = 2 AND ExternalId IS NULL;

-- EnSitio  (MetodoPagoId = 3)
UPDATE metodospagocliente
SET ExternalId = '33333333-3333-3333-3333-333333333333'
WHERE MetodoPagoId = 3 AND ExternalId IS NULL;

-- =====================================================
-- VERIFICACIÓN
-- =====================================================
SELECT MetodoPagoId, Tipo, ExternalId FROM metodospagocliente ORDER BY MetodoPagoId;
