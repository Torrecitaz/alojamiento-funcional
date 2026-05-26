const axios = require('axios');

const BASE_URL = "https://api-gateway-y75a.onrender.com/api/v1";

async function debug() {
    try {
        const res = await axios.post(`${BASE_URL}/facturas`, {
            reservaId: 1,
            metodoPagoId: "6737b3ac-3416-4f1e-abdb-49311f6c5bde",
            monto: 480.00
        });
        console.log("Success:", res.data);
    } catch (err) {
        console.log("Status:", err.response?.status);
        console.log("Headers:", err.response?.headers);
        console.log("Data:", err.response?.data);
    }
}

debug();
