const axios = require('axios');

const BASE_URL = "https://api-gateway-y75a.onrender.com/api/v1";

async function debug() {
    try {
        console.log("Sending POST to registration endpoint...");
        const response = await axios.post(`${BASE_URL}/clientes`, {
            nombreCompleto: "Mateo Torres Debug",
            email: `debug_${Date.now()}@example.com`,
            password: "Supabase_2?",
            telefono: "51999999999"
        });
        console.log("Success:", response.data);
    } catch (err) {
        console.log("Failed with status:", err.response?.status);
        console.log("Headers:", err.response?.headers);
        console.log("Data:", err.response?.data);
    }
}

debug();
