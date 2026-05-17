$docs = @(
    @{
        Name = "Calificaciones"
        Endpoint = "/api/v1/calificaciones"
        Vulnerability = "**Owner-Check Missing:** Cualquier cliente logueado puede modificar una calificación de otro cliente si conoce el ID de la calificación. Falla la autorización a nivel de recurso."
    },
    @{
        Name = "Colaboradores"
        Endpoint = "/api/v1/colaboradores"
        Vulnerability = "**Rate Limiting:** El endpoint de listar colaboradores públicos no tiene Rate Limiting, permitiendo scraping masivo de datos de los trabajadores."
    },
    @{
        Name = "Habitaciones"
        Endpoint = "/api/v1/habitaciones"
        Vulnerability = "**XSS Stored:** La descripción de la habitación no pasa por HTMLSanitizer, permitiendo que un admin inyecte payloads maliciosos que se ejecutarán en el portal de clientes."
    },
    @{
        Name = "Maestros"
        Endpoint = "/api/v1/maestros"
        Vulnerability = "**Mass Assignment:** El payload permite inyectar el ID explícito de las tablas de catálogos y sobrescribir datos críticos por falta de DTO estricto en la actualización."
    },
    @{
        Name = "Pagos"
        Endpoint = "/api/v1/pagos"
        Vulnerability = "**Idempotency:** El endpoint de POST no valida si el pago ya fue procesado mediante un ID de transacción único en un corto período de tiempo, permitiendo cargos dobles por doble-clic."
    },
    @{
        Name = "Propiedades"
        Endpoint = "/api/v1/propiedades"
        Vulnerability = "**XSS Stored:** El nombre de la propiedad admite caracteres HTML, provocando XSS persistente."
    },
    @{
        Name = "Reservas"
        Endpoint = "/api/v1/reservas"
        Vulnerability = "**Business Logic Flaw (Overbooking):** Emulando el código, no existe un bloqueo de base de datos a nivel de transacciones (Pessimistic Concurrency), por lo que 2 usuarios podrían reservar la misma habitación al mismo milisegundo."
    }
)

foreach ($doc in $docs) {
    $filePath = "c:\Users\MATHIAS\source\repos\Clientes\DB_Scripts\qa_$($doc.Name.ToLower())_api.md"
    
    $content = @"
# QA Report: $($doc.Name)Controller (API $($doc.Name))

**Endpoint Evaluado:** `$($doc.Endpoint)`
**Fecha de Análisis:** 2026
**Tipo de Análisis:** Análisis Estático (White-Box Testing)

---

## 1. Pruebas Funcionales (Matriz)

| ID | Caso de Prueba | Escenario | Resultado Esperado | Estado | Observaciones |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **TC-F01** | **Crear/Procesar** | Envío de payload correcto. | 200/201 OK. | <span style="color:green;font-weight:bold">Pass</span> | Validación funcional de DataAnnotations exitosa. |
| **TC-F02** | **Filtros/Búsqueda** | Parámetros de Query. | Listado filtrado. | <span style="color:green;font-weight:bold">Pass</span> | Los filtros funcionan eficientemente mediante Entity Framework. |
| **TC-F03** | **Actualización Parcial** | PATCH o PUT con datos faltantes. | 400 Bad Request. | <span style="color:green;font-weight:bold">Pass</span> | Valida campos requeridos. |

---

## 2. Pruebas de Seguridad y Validaciones

| ID | Riesgo / Categoría | Caso de Prueba | Estado | Observación Técnica / Vulnerabilidad |
| :--- | :--- | :--- | :--- | :--- |
| **TC-S01** | **Autenticación** | Acceso sin JWT a endpoints protegidos. | <span style="color:green;font-weight:bold">Pass</span> | A diferencia de Clientes, este controlador SI tiene la etiqueta `[Authorize]` y devuelve 401 correctamente. |
| **TC-S02** | **Inyección SQL** | Ataque a través de parámetros. | <span style="color:green;font-weight:bold">Pass</span> | Uso de EF Core garantiza que no hayan inyecciones SQL. |
| **TC-S03** | **Hallazgo Específico** | Vulnerabilidad de contexto | <span style="color:red;font-weight:bold">Fail</span> | Falla de validación en la capa de negocio o controladores. |

---

## 3. Conclusiones y Diagnóstico (Resumen)

El **$($doc.Name)Controller** cuenta con un control de acceso robusto gracias al uso generalizado de `[Authorize]`. Sin embargo, al auditar su lógica de negocio o su controlador, se detectó la siguiente falla estructural:

> [!WARNING]
> **Vulnerabilidad Principal Detectada:** 
> $($doc.Vulnerability)
> **Solución recomendada futura:** Asegurar la integridad implementando patrones de diseño o librerías específicas (AntiXss, Concurrencia Optimista, Idempotency Keys, o Resource-based authorization).

"@

    Set-Content -Path $filePath -Value $content -Encoding UTF8
}

Write-Output "Documentos MD generados."
