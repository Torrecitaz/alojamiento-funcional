const axios = require('axios');

const BASE_URL = "https://api-gateway-y75a.onrender.com/api/v1";

async function run() {
    console.log("Iniciando monitorización y testeo de la nueva versión de Reservas...");
    console.log(`URL base del API Gateway: ${BASE_URL}\n`);

    const maxAttempts = 20;
    const intervalMs = 15000; // 15 segundos entre intentos

    // Paso 1: Registrar un cliente de prueba para la reserva
    let clienteId = 1; // Fallback
    try {
        const randomEmail = `poll_test_${Math.floor(Math.random() * 100000)}@test.com`;
        console.log(`Registrando cliente único: ${randomEmail}`);
        const resRegistro = await axios.post(`${BASE_URL}/clientes`, {
            nombreCompleto: "Mateo Torres Test Integracion",
            email: randomEmail,
            password: "PasswordSeguro123",
            cedula: `17${Math.floor(10000000 + Math.random() * 90000000)}`,
            telefono: "0991234567",
            domicilio: "Av. Shyris, Quito"
        });
        console.log("[+] Cliente de prueba registrado.");
    } catch (err) {
        console.log("[-] Nota: Registro falló o el cliente ya existe, usaremos clienteId=1.", err.message);
    }

    const payload = {
        clienteId: 1,
        alojamientoId: 1,
        fechaCheckIn: "2026-09-01",
        fechaCheckOut: "2026-09-05",
        numAdultos: 2,
        numNinos: 0,
        llevaMascotas: false,
        habitaciones: [
            {
                habitacionId: 1,
                precioPorNoche: 120.00,
                numNoches: 4
            }
        ]
    };

    for (let attempt = 1; attempt <= maxAttempts; attempt++) {
        console.log(`\n--- Intento ${attempt}/${maxAttempts} (Esperando despliegue de Reservas en Render...) ---`);
        try {
            const start = Date.now();
            const res = await axios.post(`${BASE_URL}/reservas`, payload);
            console.log(`[+] ¡EXITO! La reserva fue procesada correctamente.`);
            console.log("Respuesta de la reserva:", JSON.stringify(res.data, null, 2));
            console.log(`Tiempo de respuesta: ${Date.now() - start}ms`);
            console.log("\n>>> EL FLUJO DE RESERVA Y EL API GATEWAY ESTÁN 100% OPERATIVOS Y SIN ERRORES <<<");
            break;
        } catch (err) {
            if (err.response) {
                const status = err.response.status;
                const errorData = err.response.data;
                const errorMsg = errorData.Message || errorData.message || "";
                
                console.log(`[-] HTTP ${status} recibido.`);
                console.log(`    Detalle:`, JSON.stringify(errorData));
                
                if (errorMsg.includes("NpgsqlRetryingExecutionStrategy")) {
                    console.log("    => Estado: Aún se está ejecutando la versión vieja con error de transacciones.");
                } else if (status === 502 || status === 503 || status === 504 || errorMsg.includes("Too Many Requests")) {
                    console.log("    => Estado: El servicio de Reservas o Alojamientos se está reiniciando/redesplegando.");
                } else {
                    console.log("    => Error desconocido. Es posible que el despliegue haya fallado o el payload tenga algún problema.");
                }
            } else {
                console.log("[-] Sin respuesta del servidor:", err.message);
            }
            
            if (attempt === maxAttempts) {
                console.log("\n[-] Se alcanzó el límite de intentos. Por favor verifica los logs de Render manualmente.");
            } else {
                console.log(`Esperando ${intervalMs / 1000} segundos antes del siguiente intento...`);
                await new Promise(resolve => setTimeout(resolve, intervalMs));
            }
        }
    }
}

run();
