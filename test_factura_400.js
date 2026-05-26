const axios = require('axios');

const BASE_URL = "https://api-gateway-y75a.onrender.com/api/v1";

async function testFactura() {
    console.log("Testing Facturacion POST endpoint with GUID metodoPagoId...");
    const payload = {
        reservaId: 1,
        metodoPagoId: "6737b3ac-3416-4f1e-abdb-49311f6c5bde", // GUID instead of INT
        monto: 480.00,
        fechaPago: new Date().toISOString(),
        detalles: [
            {
                concepto: "Hospedaje 4 noches",
                subTotalDetalle: 480.00
            }
        ]
    };

    try {
        const res = await axios.post(`${BASE_URL}/facturas`, payload);
        console.log("[+] Success:", res.data);
    } catch (err) {
        console.log("[-] Failed with GUID payment method:");
        if (err.response) {
            console.log("Status:", err.response.status);
            console.log("Data:", JSON.stringify(err.response.data, null, 2));
        } else {
            console.log("Message:", err.message);
        }
    }
}

testFactura();
