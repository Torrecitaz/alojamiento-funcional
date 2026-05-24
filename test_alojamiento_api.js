const axios = require('axios');

// URL publica de su API Gateway en Render
const BASE_URL = "https://api-gateway-y75a.onrender.com/api/v1";

// Generar un correo aleatorio para evitar colisiones de clave unica al registrar multiples veces
const randomEmail = `usuario_${Math.floor(Math.random() * 100000)}@ejemplo.com`;

async function testApi() {
    console.log("Iniciando pruebas de API de Alojamiento Funcional...");
    console.log(`URL base del API Gateway: ${BASE_URL}`);

    // ── 1. Prueba de Registro de Cliente ─────────────────
    console.log("\n----------------------------------------");
    console.log("1. Probando REGISTRO de un nuevo cliente...");
    const registroPayload = {
        nombreCompleto: "Mateo Torres",
        email: randomEmail,
        password: "Supabase_2?",
        telefono: "51999999999" // Telefono corregido (solo numeros para validacion de C#)
    };
    console.log("Datos de registro enviados:", JSON.stringify(registroPayload, null, 2));

    try {
        const resRegistro = await axios.post(`${BASE_URL}/clientes`, registroPayload);
        console.log("[+] Respuesta de registro exitosa.");
        console.log("Respuesta completa:", JSON.stringify(resRegistro.data, null, 2));
    } catch (err) {
        console.error("[-] Error en Registro:", err.response?.data || err.message);
    }

    // ── 2. Prueba de Inicio de Sesion (Login) ─────────────
    console.log("\n----------------------------------------");
    console.log("2. Probando INICIO DE SESIÓN (Login)...");
    const loginPayload = {
        email: randomEmail,
        password: "Supabase_2?"
    };
    console.log("Datos de login enviados:", JSON.stringify(loginPayload, null, 2));

    try {
        const resLogin = await axios.post(`${BASE_URL}/auth/login`, loginPayload);
        console.log("[+] Respuesta de login exitosa.");
        console.log("Token y datos recibidos:", JSON.stringify(resLogin.data, null, 2));
        
        const token = resLogin.data.datos?.token;
        if (token) {
            console.log("\n[+] JWT Token recibido con éxito.");
        }
    } catch (err) {
        console.error("[-] Error en Login:", err.response?.data || err.message);
    }

    // ── 3. Prueba de Listar Alojamientos ──────────────────
    console.log("\n----------------------------------------");
    console.log("3. Probando OBTENER ALOJAMIENTOS (Catálogo)...");
    try {
        const resAlojamientos = await axios.get(`${BASE_URL}/alojamientos`);
        console.log("[+] Respuesta de obtener alojamientos exitosa.");
        console.log("Total de alojamientos en catalogo:", resAlojamientos.data?.length || 0);
        console.log("Datos de alojamientos:", JSON.stringify(resAlojamientos.data, null, 2));
    } catch (err) {
        console.error("[-] Error en Catalogo:", err.response?.data || err.message);
    }
}

testApi();
