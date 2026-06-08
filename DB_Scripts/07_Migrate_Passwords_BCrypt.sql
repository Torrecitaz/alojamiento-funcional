-- =====================================================
-- SCRIPT 07: MIGRACIÓN DE CONTRASEÑAS A BCRYPT
-- ORDEN DE EJECUCIÓN: 7 (Ejecutar en db_usuarios)
-- =====================================================

-- 1. Habilitar la extensión pgcrypto (requerido para crypt y gen_salt)
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- 2. Encriptar las contraseñas en texto plano existentes usando BCrypt (Blowfish 'bf')
-- Solo encripta si el passwordhash actual no tiene el prefijo de hash de BCrypt ($2a$ o $2b$)
UPDATE usuarios 
SET passwordhash = crypt(passwordhash, gen_salt('bf')) 
WHERE passwordhash NOT LIKE '$2a$%' 
  AND passwordhash NOT LIKE '$2b$%';
