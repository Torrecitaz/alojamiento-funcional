const axios = require('axios');

const BASE_URL = "https://api-gateway-y75a.onrender.com/api/v1";

const randomEmail = `reserva_test_${Math.floor(Math.random() * 100000)}@test.com`;

async function testReserva() {
    console.log("Iniciando prueba de creación de reserva a través del API Gateway...");
    console.log(`URL base del API Gateway: ${BASE_URL}\n`);

    let clienteId = null;

    // 1. Registrar Cliente
    try {
        console.log("1. Registrando cliente nuevo...");
        const resRegistro = await axios.post(`${BASE_URL}/clientes`, {
            nombreCompleto: "Juan Perez Reservas",
            email: randomEmail,
            password: "PasswordSeguro123",
            cedula: `17${Math.floor(10000000 + Math.random() * 90000000)}`,
            telefono: "0991234567",
            domicilio: "Av. de los Granados, Quito"
        });
        
        console.log("[+] Cliente registrado con éxito:", resRegistro.data);
        clienteId = resRegistro.data.datos?.clienteId || resRegistro.data.clienteId;
        if (!clienteId) {
            // Intenta leer de la estructura directa si cambió
            clienteId = resRegistro.data.id || resRegistro.data.clienteId;
        }
        console.log(`ID del cliente obtenido: ${clienteId}\n`);
    } catch (err) {
        console.error("[-] Error en registro de cliente:", err.response?.data || err.message);
        return;
    }

    // 2. Obtener un Alojamiento y una Habitación del catálogo
    let alojamientoId = 1;
    let habitacionId = 1;
    let precioPorNoche = 120.00;

    try {
        console.log("2. Consultando catálogo de alojamientos para verificar IDs disponibles...");
        const resAlojamientos = await axios.get(`${BASE_URL}/alojamientos`);
        const alojamientos = resAlojamientos.data;
        if (alojamientos && alojamientos.length > 0) {
            const primerAlojamiento = alojamientos[0];
            alojamientoId = primerAlojamiento.alojamientoId || primerAlojamiento.id;
            console.log(`[+] Alojamiento seleccionado: ID ${alojamientoId} (${primerAlojamiento.nombre})`);
            
            // Verificamos si tiene habitaciones
            if (primerAlojamiento.habitaciones && primerAlojamiento.habitaciones.length > 0) {
                const primerHabitacion = primerAlojamiento.habitaciones[0];
                habitacionId = primerHabitacion.habitacionId || primerHabitacion.id;
                precioPorNoche = primerHabitacion.precioPorNoche || primerHabitacion.precio;
                console.log(`[+] Habitación seleccionada: ID ${habitacionId}, Precio: $${precioPorNoche}`);
            } else {
                console.log("[-] El alojamiento seleccionado no tiene habitaciones en el catálogo expuesto. Usando fallback ID 1.");
            }
        } else {
            console.log("[-] Catálogo de alojamientos vacío. Usando fallbacks (AlojamientoId=1, HabitacionId=1).");
        }
        console.log("");
    } catch (err) {
        console.warn("[-] No se pudo consultar el catálogo. Usando fallbacks.", err.message);
    }

    // 3. Crear Reserva
    const reservaPayload = {
        clienteId: clienteId || 1, // Si no se obtuvo, usar fallback 1
        alojamientoId: alojamientoId,
        fechaCheckIn: "2026-08-01",
        fechaCheckOut: "2026-08-05",
        numAdultos: 2,
        numNinos: 0,
        llevaMascotas: false,
        habitaciones: [
            {
                habitacionId: habitacionId,
                precioPorNoche: precioPorNoche,
                numNoches: 4
            }
        ]
    };

    console.log("3. Intentando crear la reserva...");
    console.log("Payload enviado:", JSON.stringify(reservaPayload, null, 2));

    try {
        const resReserva = await axios.post(`${BASE_URL}/reservas`, reservaPayload);
        console.log("\n[+] RESERVA CREADA EXITOSAMENTE!");
        console.log("Respuesta:", JSON.stringify(resReserva.data, null, 2));
    } catch (err) {
        console.error("\n[-] Error al crear la reserva:");
        if (err.response) {
            console.error("Status Code:", err.response.status);
            console.error("Datos del error:", JSON.stringify(err.response.data, null, 2));
        } else {
            console.error("Mensaje:", err.message);
        }
    }
}

testReserva();
