const axios = require('axios');

const BASE_URL = "https://api-gateway-y75a.onrender.com/api/v1";

async function test400() {
    console.log("Testing API Gateway with ISO datetime strings...");
    const payloadISO = {
        clienteId: 1,
        alojamientoId: 1,
        fechaCheckIn: "2026-09-01T00:00:00.000Z", // Formato ISO completo con hora
        fechaCheckOut: "2026-09-05T00:00:00.000Z",
        numAdultos: 2,
        numNinos: 0,
        llevaMascotas: false,
        habitaciones: [
            {
                habitacionId: 1,
                precioPorNoche: 120.00
            }
        ]
    };

    try {
        const res = await axios.post(`${BASE_URL}/reservas`, payloadISO);
        console.log("[+] Success:", res.data);
    } catch (err) {
        console.log("[-] Failed with ISO dates:");
        if (err.response) {
            console.log("Status:", err.response.status);
            console.log("Data:", JSON.stringify(err.response.data, null, 2));
        } else {
            console.log("Message:", err.message);
        }
    }
}

test400();
