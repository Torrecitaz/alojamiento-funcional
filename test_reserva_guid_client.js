const axios = require('axios');

const BASE_URL = "https://api-gateway-y75a.onrender.com/api/v1";

async function testReservaGuidClient() {
    console.log("Testing API Gateway reservation endpoint with GUID clienteId...");
    const payload = {
        clienteId: "dca0889e-dbc4-4240-a9f4-9c64f4643c81", // GUID instead of INT
        alojamientoId: 1,
        fechaCheckIn: "2026-09-01",
        fechaCheckOut: "2026-09-05",
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
        const res = await axios.post(`${BASE_URL}/reservas`, payload);
        console.log("[+] Success:", res.data);
    } catch (err) {
        console.log("[-] Failed with GUID client:");
        if (err.response) {
            console.log("Status:", err.response.status);
            console.log("Data:", JSON.stringify(err.response.data, null, 2));
        } else {
            console.log("Message:", err.message);
        }
    }
}

testReservaGuidClient();
