const axios = require('axios');

const SERVICES = {
    "api-gateway": "https://api-gateway-y75a.onrender.com/swagger/index.html",
    "usuarios-api": "https://usuarios-api-y75a.onrender.com/swagger/index.html",
    "alojamientos-api": "https://alojamientos-api-y75a.onrender.com/swagger/index.html",
    "reservas-api": "https://reservas-api-y75a.onrender.com/swagger/index.html",
    "facturacion-api": "https://facturacion-api-y75a.onrender.com/swagger/index.html"
};

async function testAll() {
    console.log("Pinging microservices directly...");
    for (const [name, url] of Object.entries(SERVICES)) {
        try {
            console.log(`Pinging ${name} (${url})...`);
            const start = Date.now();
            const res = await axios.get(url, { timeout: 15000 });
            console.log(`[+] ${name} responded with status ${res.status} in ${Date.now() - start}ms`);
        } catch (err) {
            console.log(`[-] ${name} failed: Status ${err.response?.status || 'No Response'}, Message: ${err.message}`);
            if (err.response) {
                console.log(`    Headers:`, err.response.headers);
            }
        }
    }
}

testAll();
