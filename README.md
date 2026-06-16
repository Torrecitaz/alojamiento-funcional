# ✦ AlojaExpress — Sistema de Gestión de Alojamientos & Reservas

AlojaExpress es una plataforma web modular y distribuida diseñada para la gestión de residencias, habitaciones y reservaciones de alojamiento. Está construida bajo una arquitectura de microservicios con un API Gateway que sirve como único punto de entrada y expone capacidades avanzadas de agregación de datos (BFF mediante GraphQL) y middleware transaccional (Idempotencia).

El sistema también cuenta con integración directa y bidireccional con sistemas externos de reservaciones de atracciones (**Booking/TerraQuest Experiences**).

---

## 🏗️ Arquitectura General del Sistema

El backend está diseñado como un ecosistema de microservicios distribuidos que se comunican de forma síncrona (a través de llamadas REST internas y gRPC) y asíncrona (mediante eventos):

1. **API Gateway (YARP & GraphQL BFF):**
   - Actúa como proxy reverso para redirigir peticiones a los servicios correspondientes de forma transparente.
   - Integra **HotChocolate (GraphQL)** que funciona como un Backend-for-Frontend (BFF), resolviendo en paralelo consultas complejas de alojamientos, habitaciones y fotos en una sola petición.
   - Gestiona la seguridad, CORS y el control de transacciones repetidas.
2. **Microservicio de Usuarios (Usuarios.API):**
   - Gestiona el registro y login de Clientes, Colaboradores y Administradores.
   - Utiliza hashing con **BCrypt** para contraseñas.
   - Base de datos dedicada en Supabase (`db_usuarios`).
3. **Microservicio de Alojamientos (Alojamientos.API):**
   - Administra el catálogo de residencias/propiedades, habitaciones, disponibilidad y fotos.
   - Base de datos dedicada en Supabase (`db_alojamientos`).
4. **Microservicio de Reservas (Reservas.API):**
   - Controla el ciclo de vida de las reservas (creación, confirmación, expiración, cancelación).
   - Base de datos dedicada en Supabase (`db_reservas`).
5. **Microservicio de Facturación (Facturacion.API):**
   - Emite facturas y gestiona los métodos de pago asociados a una reserva.
   - Base de datos dedicada en Supabase (`db_facturacion`).
6. **Event Bus (MassTransit + RabbitMQ):**
   - Facilita la comunicación reactiva. Por ejemplo, cuando se paga una factura en el microservicio de facturación, se emite un evento `FacturaPagadaEvent` que el microservicio de reservas consume de forma asíncrona para confirmar la reserva de forma automática.

---

## 🌟 Pilares Arquitectónicos Implementados

### 1. Desacoplamiento y Agregación con GraphQL
Para solucionar problemas de agregación ineficiente en el cliente (como realizar múltiples llamadas HTTP síncronas en bucle), el API Gateway expone un endpoint GraphQL `/graphql`. Esto permite al frontend consultar una propiedad y, de forma anidada, sus habitaciones y fotos correspondientes en una sola llamada optimizada.

### 2. Estrategia de Idempotencia Transaccional
Para evitar cobros dobles o dobles reservas debido a problemas de red o clicks rápidos del usuario:
- El API Gateway incluye un `IdempotencyMiddleware` en las rutas de creación (versión `V2`).
- Valida la presencia obligatoria de la cabecera `X-Idempotency-Key`.
- Si se detecta un reintento concurrentemente procesándose, responde de inmediato con `409 Conflict`.
- Si la transacción original ya finalizó, devuelve directamente la respuesta guardada en la caché en memoria (`IMemoryCache`), previniendo que la lógica de negocio del microservicio de reservas se ejecute más de una vez.
- En la capa de mensajería (RabbitMQ), el consumidor `FacturaPagadaConsumer` valida si la reserva ya está confirmada antes de procesar la confirmación, garantizando idempotencia en el consumo.

### 3. API V2 y Simplificación del Contrato
El endpoint `POST /api/v2/reservas/booking` expone un DTO simplificado donde el cliente final solo necesita suministrar el ID de la habitación (`HabitacionId`), rango de fechas y datos básicos del huésped. El API Gateway se encarga de consultar internamente al microservicio de Alojamientos para deducir de forma segura el ID de alojamiento y el precio por noche aplicable, aislando al frontend de lógica de negocio redundante.

### 4. Tiempo Real con SignalR (WebSockets)
El API Gateway aloja un hub SignalR (`BookingHub`). Ante cualquier modificación en el estado de una reserva (creación, pago, confirmación), se notifica de inmediato al frontend para actualizar la UI en tiempo real de forma reactiva.

---

## 🔗 Integración con el Sistema de Booking (Contrato V2)

AlojaExpress está preparado para conectarse como microservicio o integrador externo al sistema de booking (**TerraQuest Experiences**). El flujo de integración REST estructurado bajo el contrato OpenAPI V2 funciona de la siguiente manera:

1. **Obtención del catálogo:** El integrador consulta las atracciones/propiedades disponibles en AlojaExpress.
2. **Consulta detallada:** Se obtiene la información extendida y disponibilidad de una propiedad específica utilizando su identificador o slug.
3. **Reserva (Idempotente Obligatorio):** Se envía una solicitud de reserva `POST /api/v2/yanick-maila/booking` pasando los datos del pasajero. Este endpoint requiere de manera obligatoria una clave de idempotencia única para garantizar que no se dupliquen las transacciones ante fallos de conexión.
4. **Cancelación:** Si la reserva necesita ser anulada, se consume el endpoint `POST /api/v1/yanick-maila/booking/{id}/cancel` utilizando el identificador único de la reserva.

---

## 🛠️ Estabilización & Solución de Errores Críticos

Recientemente, se realizaron modificaciones de estabilización indispensables para el despliegue del sistema:

1. **Ampliación del campo de Rol (Login & Registro):**
   - El rol en la tabla `usuarios` estaba limitado a `VARCHAR(10)`, lo que causaba un error de longitud al intentar asignar roles como `'Administrador'` (13 caracteres) o `'Colaborador'` (11 caracteres).
   - Se actualizó la base de datos de producción a `VARCHAR(50)` y se sincronizó el modelo Entity Framework Core `UsuarioEntity` (`MaxLength(50)`).
   - Se sembró con éxito el usuario administrador de producción:
     - **Usuario:** `admin@alojaexpress.com`
     - **Contraseña:** `Admin123!`
     - **Rol:** `Administrador`
2. **Prevención de Conexiones Excesivas (429 Too Many Requests):**
   - Supabase (capa gratuita) limita a **15** las conexiones simultáneas. Para evitar caídas constantes, se configuró en cada microservicio un límite de pool seguro (`Maximum Pool Size=3` por conexión). Esto asegura que el ecosistema de microservicios nunca consuma más de 12 conexiones, dejando espacio para accesos directos o scripts de análisis.
3. **Mejora de Respuestas de Excepción y Errores en Frontend:**
   - Se configuraron los middlewares de error de los 4 microservicios para devolver JSON en formato `camelCase`.
   - Se robusteció el tratamiento de excepciones en `RegisterPage.jsx` y `LoginPage.jsx` para extraer de forma adaptativa y comprensible cualquier mensaje de validación devuelto por ASP.NET Core, evitando alertas genéricas.

---

## 🚀 Despliegue en Render.com

Los servicios están configurados para compilarse y desplegarse mediante Docker a través del blueprint `render.yaml`:

- **alojaexpress-gateway (Servicio Público):** Expone los endpoints HTTP `/api/v1`, `/api/v2` y la interfaz `/graphql`.
- **alojaexpress-usuarios (Servicio Privado):** API interna de gestión de usuarios.
- **alojaexpress-alojamientos (Servicio Privado):** API interna de residencias y habitaciones.
- **alojaexpress-reservas (Servicio Privado):** API interna de reservaciones.
- **alojaexpress-facturacion (Servicio Privado):** API interna de emisión de cobros y facturas.

### Configuración de Variables de Entorno en Render
Cada uno de los servicios requiere las siguientes variables inyectadas en su configuración de Render para interactuar con Supabase:
- `ConnectionStrings__ConexionUsuarios` (en el servicio de usuarios)
- `ConnectionStrings__ConexionAlojamientos` (en el servicio de alojamientos)
- `ConnectionStrings__ConexionReservas` (en el servicio de reservas)
- `ConnectionStrings__ConexionFacturacion` (en el servicio de facturación)
- `ConnectionStrings__RabbitMQ` (en la pasarela y servicios consumidores)
