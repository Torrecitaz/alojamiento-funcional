const axios = require('axios');

async function ping() {
    try {
        const start = Date.now();
        console.log("Pinging facturacion-api public URL...");
        const res = await axios.get("https://facturacion-api-y75a.onrender.com/swagger/index.html");
        console.log(`[+] Responded with status ${res.status} in ${Date.now() - start}ms`);
    } catch (err) {
        console.log("[-] Failed:");
        if (err.response) {
            console.log("Status:", err.response.status);
            console.log("Headers:", err.response.headers);
        } else {
            console.log("Message:", err.message);
        }
    }
}

ping();
