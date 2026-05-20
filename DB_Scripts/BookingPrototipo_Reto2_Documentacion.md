# BookingPrototipo — Reto 2: Arquitectura de Microservicios
> Documento de referencia para continuar el desarrollo en una nueva sesión.

---

## 1. Visión general

El proyecto migra de una arquitectura monolítica (Reto 1) a **4 microservicios independientes**, cada uno con su propia base de datos PostgreSQL, comunicados a través de un **API Gateway** (Ocelot) y un **Bus de Eventos** (RabbitMQ). El frontend/Booking accede únicamente al API Gateway; nunca directamente a los microservicios.

```
[Frontend / Booking App]
         │
         ▼
    [API Gateway]  ←── JWT validation, routing, rate limiting
    /   |   |   \
   ↓    ↓   ↓    ↓
  MS1  MS2  MS3  MS4   ←── cada uno en su propio puerto
   │    │    │    │
  DB1  DB2  DB3  DB4   ←── bases de datos aisladas
   └────┴────┴────┘
         │
    [RabbitMQ Event Bus]  ←── comunicación asíncrona entre MS
         │
    [gRPC]  ←── comunicación síncrona interna (entre MS cuando se necesita respuesta)
```

---

## 2. Los 4 microservicios y sus bases de datos

| # | Microservicio | Base de datos | Puerto sugerido |
|---|---|---|---|
| 1 | **Usuarios** | `DB_Usuarios` | 5001 |
| 2 | **Alojamientos** | `DB_Alojamientos` | 5002 |
| 3 | **Reservas** | `DB_Reservas` | 5003 |
| 4 | **Facturacion** | `DB_Facturacion` | 5004 |
| — | API Gateway | — | 5000 |

---

## 3. Entidades por microservicio (tablas reales de la BD)

### MS Usuarios — `DB_Usuarios`
- `Localizaciones` (LocalizacionId, Area POLYGON, Descripcion)
- `Usuarios` (UsuarioId, Rol, Email, PasswordHash, NombreCompleto, Estado, FechaCreacion, FechaModificacion)
- `Clientes` (ClienteId, UsuarioId→FK real, Cedula, FotoUrl, Telefono, Domicilio, Email, TotalReservas)
- SP: `sp_registrar_cliente(email, password, nombre, cedula, telefono, domicilio)`

### MS Alojamientos — `DB_Alojamientos`
- `TiposAlojamiento` (TipoAlojamientoId, Nombre, Descripcion)
- `Alojamientos` (AlojamientoId, SocioId→ref.lógica a Usuarios, TipoAlojamientoId, Ciudad, Nombre, Descripcion, Direccion, Coordenadas POINT, Estrellas, CalificacionPromedio, TotalResenas, AdmiteMascotas, TienePiscina, TieneParqueadero, Estado)
- `AlojamientoFotos` (FotoId, AlojamientoId→FK real, Url, Orden, Descripcion)
- `Habitaciones` (HabitacionId, AlojamientoId→FK real, Nombre, Descripcion, CapacidadAdultos, CapacidadNinos, NumBanos, NumDormitorios, TieneCocina, TieneAireAcondicionado, SuperficieM2, PrecioNoche)
- `CalendarioDisponibilidad` (CalendarioId, HabitacionId→FK real, Fecha, Estado: Disponible/Ocupado/Bloqueado)

### MS Reservas — `DB_Reservas`
- `Descuentos` (DescuentoId, Codigo, Porcentaje, Activo)
- `Reservas` (ReservaId, DescuentoId→FK real, ClienteId→ref.lógica Usuarios, AlojamientoId→ref.lógica Alojamientos, FechaCheckIn, FechaCheckOut, NumAdultos, NumNinos, LlevaMascotas, NumHabitaciones, SubTotal, Total, Estado, CodigoReserva)
- `ReservaDetalleHabitacion` (DetalleId, ReservaId→FK real, HabitacionId→ref.lógica Alojamientos, PrecioPorNoche, NumNoches, SubTotalHabitacion)
- FN: `fn_calcular_noches(checkin, checkout)`
- SP: `sp_asignar_codigo_reserva(reserva_id)`

### MS Facturacion — `DB_Facturacion`
- `MetodosPagoCliente` (MetodoPagoId, Tipo: DEBITO/CREDITO/EnSitio)
- `Facturas` (FacturaId, ReservaId→ref.lógica Reservas, MetodoPagoId→FK real, Monto, Estado, FechaPago)
- `DetalleFacturas` (DetalleFacturaId, FacturaId→FK real, Descripcion, Cantidad, PrecioUnitario)
- `AuditoriaGeneral` (AuditoriaId, NombreTabla, Operacion, RegistroId, DatosAnteriores, DatosNuevos, UsuarioAccion, FechaAccion, Origen)
- SP: `sp_registrar_factura_completa(reserva_id, metodo_pago, monto, descripcion)`

---

## 4. Flujo de negocio y referencias lógicas entre bases

```
DB_Usuarios ──────────────────────────────────────────────────────┐
  Usuarios.UsuarioId ──► Alojamientos.SocioId (ref. lógica)       │
  Clientes.ClienteId ──► Reservas.ClienteId   (ref. lógica)       │
                                                                    ▼
DB_Alojamientos ──────────────────────────────────────────────────┐
  Alojamientos.AlojamientoId ──► Reservas.AlojamientoId (ref. lógica)│
  Habitaciones.HabitacionId  ──► ReservaDetalleHabitacion (ref. lógica)│
                                                                    ▼
DB_Reservas ───────────────────────────────────────────────────────┐
  Reservas.ReservaId ──► Facturas.ReservaId (ref. lógica)          │
                                                                    ▼
DB_Facturacion
```

**Importante:** Las referencias lógicas NO son FOREIGN KEYs físicas. Se validan en la capa Business de cada microservicio llamando via gRPC al servicio correspondiente para verificar que el registro exista antes de persistir.

---

## 5. Bus de eventos — quién publica y quién consume

| Microservicio | Publica | Consume |
|---|---|---|
| **Usuarios** | `UsuarioCreatedEvent`, `UsuarioUpdatedEvent` | — |
| **Alojamientos** | `HabitacionDisponibilidadChangedEvent`, `AlojamientoEstadoChangedEvent` | — |
| **Reservas** | `ReservaCreatedEvent`, `ReservaConfirmedEvent`, `ReservaCancelledEvent` | `UsuarioCreatedEvent`, `AlojamientoEstadoChangedEvent` |
| **Facturacion** | — | `ReservaCreatedEvent` (para generar factura automáticamente) |

### Flujo del evento más importante:
```
[Usuario crea una reserva]
    → Reservas.Business crea la reserva
    → publica ReservaCreatedEvent en RabbitMQ
    → Facturacion.Business.EventHandlers.ReservaEventHandler lo recibe
    → llama sp_registrar_factura_completa(...)
    → genera la factura automáticamente
```

---

## 6. Estructura de carpetas completa

```
BookingProto_Reto2/
│
├── BookingPrototipo.slnx
├── docker-compose.yml
├── .gitignore
│
├── ApiGateway/
│   └── ApiGateway/
│       ├── ApiGateway.csproj
│       ├── Program.cs
│       ├── appsettings.json
│       ├── Configuration/
│       │   ├── ocelot.json
│       │   └── ocelot.Development.json
│       ├── Middleware/
│       │   ├── AuthMiddleware.cs
│       │   ├── RateLimitingMiddleware.cs
│       │   └── CorrelationIdMiddleware.cs
│       └── Extensions/
│           ├── ServiceCollectionExtensions.cs
│           └── AuthenticationExtensions.cs
│
├── EventBus/
│   ├── EventBus.Contracts/
│   │   ├── EventBus.Contracts.csproj
│   │   ├── Interfaces/
│   │   │   ├── IEventBus.cs
│   │   │   ├── IEventHandler.cs
│   │   │   └── IIntegrationEvent.cs
│   │   └── Events/
│   │       ├── Usuarios/
│   │       │   ├── UsuarioCreatedEvent.cs
│   │       │   └── UsuarioUpdatedEvent.cs
│   │       ├── Alojamientos/
│   │       │   ├── HabitacionDisponibilidadChangedEvent.cs
│   │       │   └── AlojamientoEstadoChangedEvent.cs
│   │       └── Reservas/
│   │           ├── ReservaCreatedEvent.cs
│   │           ├── ReservaConfirmedEvent.cs
│   │           └── ReservaCancelledEvent.cs
│   └── EventBus.RabbitMQ/
│       ├── EventBus.RabbitMQ.csproj
│       ├── RabbitMQEventBus.cs
│       └── RabbitMQConnection.cs
│
├── Protos/
│   ├── usuarios.proto
│   ├── alojamientos.proto
│   ├── reservas.proto
│   └── facturacion.proto
│
├── Shared/
│   └── Shared.Kernel/
│       ├── Shared.Kernel.csproj
│       ├── Common/
│       │   ├── PagedResult.cs
│       │   ├── ApiResponse.cs
│       │   └── ApiErrorResponse.cs
│       └── Exceptions/
│           ├── BusinessException.cs
│           ├── NotFoundException.cs
│           ├── ValidationException.cs
│           └── UnauthorizedBusinessException.cs
│
└── Microservices/
    │
    ├── Usuarios/
    │   ├── Usuarios.DataAccess/
    │   │   ├── Usuarios.DataAccess.csproj
    │   │   ├── Common/
    │   │   │   ├── PagedResult.cs
    │   │   │   └── RepositoryBase.cs
    │   │   ├── Context/
    │   │   │   └── UsuariosDbContext.cs
    │   │   ├── Entities/
    │   │   │   ├── Usuarios.cs
    │   │   │   ├── Clientes.cs
    │   │   │   └── Localizaciones.cs
    │   │   ├── Configurations/
    │   │   │   ├── UsuariosConfiguration.cs
    │   │   │   ├── ClientesConfiguration.cs
    │   │   │   └── LocalizacionesConfiguration.cs
    │   │   ├── Repositories/
    │   │   │   ├── Interfaces/
    │   │   │   │   ├── IUsuariosRepository.cs
    │   │   │   │   ├── IClientesRepository.cs
    │   │   │   │   └── ILocalizacionesRepository.cs
    │   │   │   ├── UsuariosRepository.cs
    │   │   │   ├── ClientesRepository.cs
    │   │   │   └── LocalizacionesRepository.cs
    │   │   └── Queries/
    │   │       ├── UsuariosQueryRepository.cs
    │   │       ├── ClientesQueryRepository.cs
    │   │       └── LocalizacionesQueryRepository.cs  
    │   ├── Usuarios.DataManagement/
    │   │   ├── Usuarios.DataManagement.csproj
    │   │   ├── Common/
    │   │   │   └── DataPagedResult.cs
    │   │   ├── Interfaces/
    │   │   │   ├── IUnitOfWork.cs
    │   │   │   ├── IusuariosDataService.cs
    │   │   │   ├── IclientesDataService.cs
    │   │   │   └── IlocalizacionesDataService.cs
    │   │   ├── Mappers/
    │   │   │   ├── UsuariosMapper.cs
    │   │   │   ├── ClientesMapper.cs
    │   │   │   └── LocalizacionesMapper.cs  
    │   │   ├── Models/
    │   │   │   ├── ClientesDataModels.cs
    │   │   │   ├── UsuariosDataModels.cs
    │   │   │   └── LocalizacionesDataModels.cs   
    │   │   └── Services/
    │   │       ├── UnitOfWork.cs
    │   │       ├── UsuariosDataService.cs
    │   │       ├── ClientesDataService.cs
    │   │       └── LocalizacionesDataService.cs
    │   ├── Usuarios.Business/
    │   │   ├── Usuarios.Business.csproj
    │   │   ├── DTOs/
    │   │   │   ├── Localizaciones/
    │   │   │   │   ├── CrearLocalizacionRequest.cs
    │   │   │   │   ├── ActualizarLocalizacionRequest.cs
    │   │   │   │   ├── LocalizacionFiltroRequest.cs
    │   │   │   │   └── LocalizacionesResponse.cs
    │   │   │   ├── Usuarios/
    │   │   │   │   ├── CrearUsuariosRequest.cs
    │   │   │   │   ├── ActualizarUsuariosRequest.cs
    │   │   │   │   ├── UsuariosFiltroRequest.cs
    │   │   │   │   └── UsuariosResponse.cs
    │   │   │   ├── Clientes/
    │   │   │   │   ├── CrearClienteRequest.cs
    │   │   │   │   ├── ActualizarClienteRequest.cs
    │   │   │   │   ├── ClienteFiltroRequest.cs
    │   │   │   │   └── ClienteResponse.cs
    │   │   │   └── Auth/
    │   │   │       ├── LoginRequest.cs
    │   │   │       └── LoginResponse.cs
    │   │   ├── Interfaces/
    │   │   │   ├── ILocalizacionesService.cs
    │   │   │   ├── IUsuariosService.cs
    │   │   │   ├── IClientesService.cs
    │   │   │   └── IAuthService.cs
    │   │   ├── Services/
    │   │   │   ├── LocalizacionesService.cs
    │   │   │   ├── UsuariosService.cs
    │   │   │   ├── ClientesService.cs
    │   │   │   └── AuthService.cs
    │   │   ├── Mappers/
    │   │   │   ├── LocalizacionesMapper.cs
    │   │   │   ├── UsuariosMapper.cs
    │   │   │   └── ClientesMapper.cs
    │   │   ├── Validators/
    │   │   │   ├── LocalizacionesValidator.cs
    │   │   │   ├── UsuariosValidator.cs
    │   │   │   ├── ClientesValidator.cs
    │   │   │   └── AuthValidator.cs
    │   │   ├── Exceptions/
    │   │   │   ├── UsuarioNotFoundException.cs
    │   │   │   ├── ClienteNotFoundException.cs
    │   │   │   ├── EmailAlreadyExistsException.cs
    │   │   │   └── AuthException.cs
    │   │   └── EventPublishers/
    │   │       ├── UsuarioEventPublisher.cs
    │   │       ├── LocalizacionEventPublisher.cs
    │   │       ├── ClienteEventPublisher.cs
    │   │       └── AuthEventPublisher.cs
    │   └── Usuarios.API/
    │       ├── Usuarios.API.csproj
    │       ├── Program.cs
    │       ├── appsettings.json
    │       ├── appsettings.Development.json
    │       ├── Controllers/V1/
    │       │   ├── LocalizacionesController.cs
    │       │   ├── UsuariosController.cs
    │       │   ├── ClientesController.cs
    │       │   └── AuthController.cs
    │       ├── GrpcServices/
    │       │   └── UsuariosGrpcService.cs
    │       ├── Extensions/
    │       │   ├── ServiceCollectionExtensions.cs
    │       │   ├── AuthenticationExtensions.cs
    │       │   ├── ApiVersioningExtensions.cs
    │       │   ├── CorsExtensions.cs
    │       │   ├── SwaggerExtensions.cs
    │       │   └── EventBusExtensions.cs
    │       ├── Middleware/
    │       │   └── ExceptionHandlingMiddleware.cs
    │       ├── Models/
    │       │   ├── Common/
    │       │   │   ├── ApiResponse.cs
    │       │   │   └── ApiErrorResponse.cs
    │       │   └── Settings/
    │       │       └── JwtSettings.cs
    │       └── Properties/
    │           └── launchSettings.json
    │
    ├── Alojamientos/
    │   ├── Alojamientos.DataAccess/
    │   │   ├── Alojamientos.DataAccess.csproj
    │   │   ├── Common/
    │   │   │   ├── PagedResult.cs
    │   │   │   └── RepositoryBase.cs
    │   │   ├── Context/
    │   │   │   └── AlojamientosDbContext.cs
    │   │   ├── Entities/
    │   │   │   ├── TiposAlojamiento.cs
    │   │   │   ├── Alojamientos.cs
    │   │   │   ├── AlojamientoFotos.cs
    │   │   │   ├── Habitaciones.cs
    │   │   │   └── CalendarioDisponibilidad.cs
    │   │   ├── Configurations/
    │   │   │   ├── TiposAlojamientoConfiguration.cs
    │   │   │   ├── AlojamientosConfiguration.cs
    │   │   │   ├── AlojamientoFotosConfiguration.cs
    │   │   │   ├── HabitacionesConfiguration.cs
    │   │   │   └── CalendarioDisponibilidadConfiguration.cs
    │   │   ├── Repositories/
    │   │   │   ├── Interfaces/
    │   │   │   │   ├── IAlojamientosRepository.cs
    │   │   │   │   ├── IHabitacionesRepository.cs
    │   │   │   │   ├── IAlojamientoFotosRepository.cs
    │   │   │   │   ├── ITiposAlojamientoRepository.cs
    │   │   │   │   └── ICalendarioDisponibilidadRepository.cs
    │   │   │   ├── AlojamientosRepository.cs
    │   │   │   ├── HabitacionesRepository.cs
    │   │   │   ├── AlojamientoFotosRepository.cs
    │   │   │   ├── TiposAlojamientoRepository.cs
    │   │   │   └── CalendarioDisponibilidadRepository.cs
    │   │   └── Queries/
    │   │       ├── AlojamientosQueryRepository.cs
    │   │       ├── AlojamientoFotosQueryRepository.cs
    │   │       ├── TiposAlojamientoQueryRepository.cs
    │   │       ├── HabitacionesQueryRepository.cs
    │   │       └── CalendarioDisponibilidadQueryRepository.cs
    │   ├── Alojamientos.DataManagement/
    │   │   ├── Alojamientos.DataManagement.csproj
    │   │   ├── Common/
    │   │   │   └── DataPagedResult.cs
    │   │   ├── Interfaces/
    │   │   │   ├── IUnitOfWork.cs
    │   │   │   ├── IalojamientosDataService.cs
    │   │   │   ├── ItiposAlojamientoDataService.cs
    │   │   │   ├── IhabitacionesDataService.cs
    │   │   │   ├── IalojamientoFotosDataService.cs
    │   │   │   └── IcalendarioDisponibilidadDataService.cs
    │   │   ├── Mappers/
    │   │   │   ├── AlojamientosMapper.cs
    │   │   │   ├── HabitacionesMapper.cs 
    │   │   │   ├── AlojamientoFotosMapper.cs
    │   │   │   ├── TiposAlojamientoMapper.cs
    │   │   │   └── CalendarioDisponibilidadMapper.cs
    
    │   │   └── Services/
    │   │       ├── UnitOfWork.cs
    │   │       ├── AlojamientosDataService.cs
    │   │       ├── TiposAlojamientoDataService.cs
    │   │       ├── AlojamientoFotosDataService.cs
    │   │       ├── HabitacionesDataService.cs
    │   │       └── CalendarioDisponibilidadDataService.cs
    │   ├── Alojamientos.Business/
    │   │   ├── Alojamientos.Business.csproj
    │   │   ├── DTOs/
    │   │   │   ├── Alojamientos/
    │   │   │   │   ├── CrearAlojamientoRequest.cs
    │   │   │   │   ├── ActualizarAlojamientoRequest.cs
    │   │   │   │   ├── AlojamientoFiltroRequest.cs
    │   │   │   │   └── AlojamientoResponse.cs
    │   │   │   ├── TiposAlojamiento/
    │   │   │   │   ├── CrearTipoAlojamientoRequest.cs
    │   │   │   │   └── TipoAlojamientoResponse.cs
    │   │   │   ├── AlojamientoFotos/
    │   │   │   │   ├── CrearAlojamientoFotoRequest.cs
    │   │   │   │   └── AlojamientoFotoResponse.cs
    │   │   │   ├── Habitaciones/
    │   │   │   │   ├── CrearHabitacionRequest.cs
    │   │   │   │   ├── ActualizarHabitacionRequest.cs
    │   │   │   │   ├── HabitacionFiltroRequest.cs
    │   │   │   │   └── HabitacionResponse.cs
    │   │   │   └── CalendarioDisponibilidad/
    │   │   │       ├── CrearCalendarioRequest.cs
    │   │   │       ├── ActualizarCalendarioRequest.cs
    │   │   │       └── CalendarioResponse.cs
    │   │   ├── Interfaces/
    │   │   │   ├── IAlojamientosService.cs
    │   │   │   ├── IHabitacionesService.cs
    │   │   │   ├── IAlojamientoFotosService.cs
    │   │   │   ├── ITiposAlojamientoService.cs
    │   │   │   └── ICalendarioDisponibilidadService.cs
    │   │   ├── Services/
    │   │   │   ├── AlojamientosService.cs
    │   │   │   ├── HabitacionesService.cs
    │   │   │   ├── AlojamientoFotosService.cs
    │   │   │   ├── TiposAlojamientoService.cs
    │   │   │   └── CalendarioDisponibilidadService.cs
    │   │   ├── Mappers/
    │   │   │   ├── AlojamientosMapper.cs
    │   │   │   ├── HabitacionesMapper.cs
    │   │   │   ├── AlojamientoFotosMapper.cs
    │   │   │   ├── TiposAlojamientoMapper.cs
    │   │   │   └── CalendarioDisponibilidadMapper.cs
    │   │   ├── Validators/
    │   │   │   ├── AlojamientosValidator.cs
    │   │   │   ├── HabitacionesValidator.cs
    │   │   │   ├── AlojamientoFotosValidator.cs
    │   │   │   ├── TiposAlojamientoValidator.cs
    │   │   │   └── CalendarioDisponibilidadValidator.cs
    │   │   ├── Exceptions/
    │   │   │   ├── AlojamientosException.cs
    │   │   │   ├── HabitacionesException.cs
    │   │   │   ├── AlojamientoFotosException.cs
    │   │   │   ├── TiposAlojamientoException.cs
    │   │   │   └── CalendarioDisponibilidadException.cs
    │   │   └── EventPublishers/
    │   │       ├── AlojamientoEventPublisher.cs
    │   │       ├── HabitacionesEventPublisher.cs
    │   │       ├── TiposAlojamientoEventPublisher.cs
    │   │       ├── AlojamientoFotosEventPublisher.cs
    │   │       └── CalendarioDisponibilidadEventPublisher.cs
    │   └── Alojamientos.API/
    │       ├── Alojamientos.API.csproj
    │       ├── Program.cs
    │       ├── appsettings.json
    │       ├── Controllers/V1/
    │       │   ├── AlojamientosController.cs
    │       │   ├── HabitacionesController.cs
    │       │   ├── TiposAlojamientoController.cs
    │       │   ├── AlojamientoFotosController.cs
    │       │   └── CalendarioDisponibilidadController.cs
    │       ├── GrpcServices/
    │       │   └── AlojamientosGrpcService.cs
    │       ├── Extensions/
    │       │   └── EventBusExtensions.cs
    │       │   └── ApiVersionExtensions.cs
    │       │   └── AuthenticationExtensions.cs
    │       │   └── ServiceCollectionExtensions.cs
    │       │   └── SwaggerExtensions.cs
    │       ├── Middleware/
    │       │   └── ErrorHandlerMiddleware.cs
    │       └── Models/
    │           ├── Common/
    │           │   ├── ApiResponse.cs
    │           │   └── ApiErrorResponse.cs
    │           └── Settings/
    │               └── JwtSettings.cs
    │               
    ├── Reservas/
    │   ├── Reservas.DataAccess/
    │   │   ├── Reservas.DataAccess.csproj
    │   │   ├── Common/
    │   │   │   ├── PagedResult.cs
    │   │   │   └── RepositoryBase.cs
    │   │   ├── Context/
    │   │   │   └── ReservasDbContext.cs
    │   │   ├── Entities/
    │   │   │   ├── Reservas.cs
    │   │   │   ├── ReservaDetalleHabitacion.cs
    │   │   │   └── Descuentos.cs
    │   │   ├── Configurations/
    │   │   │   ├── ReservasConfiguration.cs
    │   │   │   ├── ReservaDetalleHabitacionConfiguration.cs
    │   │   │   └── DescuentosConfiguration.cs
    │   │   ├── Repositories/
    │   │   │   ├── Interfaces/
    │   │   │   │   ├── IReservasRepository.cs
    │   │   │   │   ├── IReservaDetalleHabitacionRepository.cs
    │   │   │   │   └── IDescuentosRepository.cs
    │   │   │   ├── ReservasRepository.cs
    │   │   │   ├── ReservaDetalleHabitacionRepository.cs
    │   │   │   └── DescuentosRepository.cs
    │   │   └── Queries/
    │   │       ├── ReservasQueryRepository.cs
    │   │       ├── ReservaDetalleHabitacionQueryRepository.cs
    │   │       └── DescuentosQueryRepository.cs
    │   ├── Reservas.DataManagement/
    │   │   ├── Reservas.DataManagement.csproj
    │   │   ├── Interfaces/
    │   │   │   ├── IUnitOfWork.cs
    │   │   │   ├── IReservasDataManagementService.cs
    │   │   │   ├── IReservaDetalleHabitacionDataManagementService.cs
    │   │   │   └── IDescuentosDataManagementService.cs
    │   │   ├── Common/
    │   │   │   └── DataPagedResult.cs
    │   │   ├── Mappers/
    │   │   │   ├── ReservasDataMapper.cs
    │   │   │   ├── ReservaDetalleHabitacionDataMapper.cs
    │   │   │   └── DescuentosDataMapper.cs
    │   │   ├── Models/
    │   │   │   ├── ReservasDataModel.cs
    │   │   │   ├── ReservaDetalleHabitacionDataModel.cs
    │   │   │   └── DescuentosDataModel.cs
    │   │   └── Services/
    │   │       ├── UnitOfWork.cs
    │   │       ├── ReservasDataManagementService.cs
    │   │       ├── ReservaDetalleHabitacionDataManagementService.cs
    │   │       └── DescuentosDataManagementService.cs
    │   ├── Reservas.Business/
    │   │   ├── Reservas.Business.csproj
    │   │   ├── DTOs/
    │   │   │   ├── Reservas/
    │   │   │   │   ├── CrearReservaRequest.cs
    │   │   │   │   ├── ActualizarReservaRequest.cs
    │   │   │   │   ├── ReservaFiltroRequest.cs
    │   │   │   │   └── ReservaResponse.cs
    │   │   │   ├── ReservaDetalleHabitacion/
    │   │   │   │   ├── CrearReservaDetalleRequest.cs
    │   │   │   │   └── ReservaDetalleResponse.cs
    │   │   │   └── Descuentos/
    │   │   │       ├── CrearDescuentoRequest.cs
    │   │   │       └── DescuentoResponse.cs
    │   │   ├── Interfaces/
    │   │   │   ├── IReservasService.cs
    │   │   │   ├── IReservaDetalleService.cs
    │   │   │   └── IDescuentosService.cs
    │   │   ├── Services/
    │   │   │   ├── ReservasService.cs
    │   │   │   ├── ReservaDetalleService.cs
    │   │   │   └── DescuentosService.cs
    │   │   ├── Mappers/
    │   │   │   ├── ReservasMapper.cs
    │   │   │   ├── ReservaDetalleHabitacionMapper.cs
    │   │   │   └── DescuentosMapper.cs
    │   │   ├── Validators/
    │   │   │   ├── ReservasValidator.cs
    │   │   │   ├── ReservaDetalleHabitacionValidator.cs
    │   │   │   └── DescuentosValidator.cs
    │   │   ├── Exceptions/
    │   │   │   ├── ReservasException.cs
    │   │   │   ├── ReservaDetalleException.cs
    │   │   │   └── DescuentosException.cs
    │   │   ├── EventHandlers/
    │   │   │   ├── UsuarioEventHandler.cs
    │   │   │   └── AlojamientoEventHandler.cs
    │   │   └── EventPublishers/
    │   │       └── ReservaEventPublisher.cs
    │   └── Reservas.API/
    │       ├── Reservas.API.csproj
    │       ├── Program.cs
    │       ├── appsettings.json
    │       ├── Common/
    │       │   └── ApiResponse.cs
    │       ├── Controllers/V1/
    │       │   ├── ReservasController.cs
    │       │   ├── ReservaDetalleController.cs
    │       │   └── DescuentosController.cs
    │       ├── GrpcServices/
    │       │   └── ReservasGrpcService.cs
    │       ├── Extensions/
    │       │   └── CorsExtensions.cs
    │       │   └── EventBusExtensions.cs
    │       │   └── ApiVersionExtensions.cs
    │       │   └── AuthenticationExtensions.cs
    │       │   └── ServiceCollectionExtensions.cs
    │       │   └── SwaggerExtensions.cs
    │       ├── Middleware/
    │       │   └── ErrorHandlerMiddleware.cs
    │       └── Models/
    │           ├── Common/
    │           │   ├── ApiResponse.cs
    │           │   └── ApiErrorResponse.cs
    │           └── Settings/
    │               └── JwtSettings.cs
    │
    └── Facturacion/
        ├── Facturacion.DataAccess/
        │   ├── Facturacion.DataAccess.csproj
        │   ├── Common/
        │   │   └── DataPagedResult.cs
        │   ├── Context/
        │   │   └── FacturacionDbContext.cs
        │   ├── Entities/
        │   │   ├── Facturas.cs
        │   │   ├── DetalleFacturas.cs
        │   │   ├── MetodosPagoCliente.cs
        │   │   └── AuditoriaGeneral.cs
        │   ├── Configurations/
        │   │   ├── FacturasConfiguration.cs
        │   │   ├── DetalleFacturasConfiguration.cs
        │   │   ├── MetodosPagoClienteConfiguration.cs
        │   │   └── AuditoriaGeneralConfiguration.cs
        │   ├── Repositories/
        │   │   ├── Interfaces/
        │   │   │   ├── IFacturasRepository.cs
        │   │   │   ├── IDetalleFacturasRepository.cs
        │   │   │   ├── IMetodosPagoClienteRepository.cs
        │   │   │   └── IAuditoriaGeneralRepository.cs
        │   │   ├── FacturasRepository.cs
        │   │   ├── DetalleFacturasRepository.cs
        │   │   ├── MetodosPagoClienteRepository.cs
        │   │   └── AuditoriaGeneralRepository.cs
        │   └── Queries/
        │       └── FacturasQueryRepository.cs
        │       └── DetalleFacturasQueryRepository.cs
        │       └── MetodosPagoClienteQueryRepository.cs
        │       └── AuditoriaGeneralQueryRepository.cs
        ├── Facturacion.DataManagement/
        │   ├── Facturacion.DataManagement.csproj
        │   ├── Interfaces/
        │   │   ├── IFacturasDataManagementService.cs
        │   │   ├── IDetalleFacturasDataManagementService.cs
        │   │   ├── IUnitOfWork.cs
        │   │   ├── IMetodosPagoClienteDataManagementService.cs
        │   │   └── IAuditoriaGeneralDataManagementService.cs        
        │   ├── Common/
        │   │   └── DataPagedResult.cs
        │   ├── Mappers/
        │   │   ├── FacturasDataMapper.cs
        │   │   ├── DetalleFacturasDataMapper.cs
        │   │   ├── MetodosPagoClienteDataMapper.cs
        │   │   └── AuditoriaGeneralDataMapper.cs
        │   ├── Models/
        │   │   ├── FacturasDataModel.cs
        │   │   ├── DetalleFacturasDataModel.cs
        │   │   ├── MetodosPagoClienteDataModel.cs
        │   │   └── AuditoriaGeneralDataModel.cs
        │   └── Services/
        │       ├── FacturasDataManagementService.cs
        │       ├── DetalleFacturasDataManagementService.cs
        │       ├── MetodosPagoClienteDataManagementService.cs
        │       └── AuditoriaGeneralDataManagementService.cs
        ├── Facturacion.Business/
        │   ├── Facturacion.Business.csproj
        │   ├── DTOs/
        │   │   ├── Facturas/
        │   │   │   ├── CrearFacturaRequest.cs
        │   │   │   ├── ActualizarFacturaRequest.cs
        │   │   │   ├── FacturaFiltroRequest.cs
        │   │   │   └── FacturaResponse.cs
        │   │   ├── DetalleFacturas/
        │   │   │   ├── CrearDetalleFacturaRequest.cs
        │   │   │   └── DetalleFacturaResponse.cs
        │   │   ├── MetodosPago/
        │   │   │   └── MetodoPagoResponse.cs
        │   │   └── Auditoria/
        │   │       ├── CrearAuditoriaRequest.cs
        │   │       └── AuditoriaResponse.cs
        │   ├── Interfaces/
        │   │   ├── IFacturasService.cs
        │   │   ├── IDetalleFacturasService.cs
        │   │   ├── IMetodosPagoService.cs
        │   │   └── IAuditoriaService.cs
        │   ├── Services/
        │   │   ├── FacturasService.cs
        │   │   ├── DetalleFacturasService.cs
        │   │   ├── MetodosPagoService.cs
        │   │   └── AuditoriaService.cs
        │   ├── Mappers/
        │   │   ├── FacturasBusinessMapper.cs
        │   │   ├── DetalleFacturasBusinessMapper.cs
        │   │   ├── MetodosPagoBusinessMapper.cs
        │   │   └── AuditoriaBusinessMapper.cs
        │   ├── Validators/
        │   │   ├── FacturasBusinessValidator.cs
        │   │   ├── DetalleFacturasBusinessValidator.cs
        │   │   ├── MetodosPagoBusinessValidator.cs
        │   │   └── AuditoriaBusinessValidator.cs
        │   ├── Exceptions/
        │   │   ├── FacturasBusinessException.cs
        │   │   ├── DetalleFacturasBusinessException.cs
        │   │   ├── MetodosPagoBusinessException.cs
        │   │   └── AuditoriaBusinessException.cs
        │   └── EventHandlers/
        │       └── ReservaEventHandler.cs     ← dispara creación de factura
        └── Facturacion.API/
            ├── Facturacion.API.csproj
            ├── Program.cs
            ├── appsettings.json
            ├── Controllers/V1/
            │   ├── FacturasController.cs
            │   ├── DetalleFacturasController.cs
            │   ├── MetodosPagoController.cs
            │   └── AuditoriaController.cs
            ├── GrpcServices/
            │   └── FacturacionGrpcService.cs
            ├── Extensions/
            │   └── CorsExtensions.cs
            │   └── EventBusExtensions.cs
            │   └── ApiVersionExtensions.cs
            │   └── AuthenticationExtensions.cs
            │   └── ServiceCollectionExtensions.cs
            │   └── SwaggerExtensions.cs
            ├── Middleware/
            │   ├── ErrorHandlingMiddleware.cs
            │   └── RequestLoggingMiddleware.cs
            │   └── ApiVersionMiddleware.cs
            └── Models/
                ├── FacturasModels.cs
                ├── DetalleFacturasModels.cs
                ├── MetodosPagoModels.cs
                └── AuditoriaModels.cs
```

---

## 7. Capas internas de cada microservicio (patrón del Reto 1 conservado)

```
[MS].DataAccess     → Entities, EF Configurations, DbContext, Repositories, Queries
[MS].DataManagement → Abstracción sobre DataAccess, IUnitOfWork, transacciones
[MS].Business       → DTOs, Interfaces de servicio, Services, Mappers, Validators,
                      EventHandlers (si consume), EventPublishers (si publica)
[MS].API            → Controllers/V1, GrpcServices, Extensions, Middleware, Models
```

**Regla de dependencias:**
```
API → Business → DataManagement → DataAccess
```
Ninguna capa puede referenciar una capa superior.

---

## 8. API Gateway (Ocelot)

- Único punto de entrada para Frontend y Booking App
- Responsabilidades:
  - Enrutamiento a cada microservicio según el path
  - Validación de JWT (el token lo emite Usuarios.API/AuthController)
  - Rate limiting
  - Correlación de requests (CorrelationId header)
- Configuración en `ocelot.json`:
  - `/api/usuarios/**` → `http://localhost:5001`
  - `/api/alojamientos/**` → `http://localhost:5002`
  - `/api/reservas/**` → `http://localhost:5003`
  - `/api/facturacion/**` → `http://localhost:5004`

---

## 9. gRPC — cuándo usarlo

gRPC se usa para comunicación **síncrona** entre microservicios cuando uno necesita validar datos del otro antes de continuar. Ejemplos:

| Llamante | Llamado | Para qué |
|---|---|---|
| Alojamientos | Usuarios | Verificar que el SocioId existe antes de crear un alojamiento |
| Reservas | Usuarios | Verificar que el ClienteId existe |
| Reservas | Alojamientos | Verificar disponibilidad de habitación |
| Facturacion | Reservas | Obtener datos de la reserva al generar factura |

Cada microservicio tiene un `GrpcServices/[MS]GrpcService.cs` que implementa el servidor gRPC, y los clientes que lo consumen lo hacen desde su capa Business.

---

## 10. Tecnologías del proyecto

| Componente | Tecnología |
|---|---|
| Backend | .NET 10, C# |
| ORM | Entity Framework Core + Npgsql |
| Base de datos | PostgreSQL |
| API Gateway | Ocelot |
| Bus de eventos | RabbitMQ |
| Comunicación síncrona interna | gRPC (Grpc.AspNetCore) |
| Autenticación | JWT Bearer |
| Documentación API | Swagger / Scalar |
| Validación | FluentValidation |
| Versionamiento API | Asp.Versioning |
| Contenerización | Docker + docker-compose |

---

## 11. Orden de implementación recomendado

1. **Shared.Kernel** — excepciones y clases comunes
2. **EventBus.Contracts** — definir todos los eventos de integración
3. **EventBus.RabbitMQ** — implementación del bus
4. **Protos** — definir todos los contratos gRPC
5. **MS Usuarios** — primero porque los demás dependen de él lógicamente
6. **MS Alojamientos** — segundo en el flujo
7. **MS Reservas** — nodo central, conecta todo
8. **MS Facturacion** — último en el flujo
9. **API Gateway** — configurar rutas una vez los MS están levantados
10. **docker-compose** — orquestar todo junto

---

## 12. Variables de entorno / appsettings por microservicio

Cada `appsettings.json` debe contener:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=DB_[MS];Username=...;Password=..."
  },
  "JwtSettings": {
    "SecretKey": "...",
    "Issuer": "BookingPrototipo",
    "Audience": "BookingPrototipo",
    "ExpirationMinutes": 60
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  },
  "GrpcEndpoints": {
    "Usuarios": "http://localhost:5101",
    "Alojamientos": "http://localhost:5102",
    "Reservas": "http://localhost:5103"
  }
}
```

> Nota: los puertos gRPC (51xx) son distintos a los puertos REST (50xx) de cada microservicio.

---

## 13. Puntos clave para el Reto 2 según el silabo

| Requisito del Reto | Dónde se implementa |
|---|---|
| Arquitectura de microservicios | Los 4 MS con sus 4 BDs |
| API Gateway | `ApiGateway/` con Ocelot |
| Contratos versionados | `Controllers/V1/` en cada MS, `EventBus.Contracts/` |
| Pruebas de integración | Tests entre MS via Gateway |
| Seguridad (JWT, roles, scopes) | `AuthController` en Usuarios, `AuthenticationExtensions` en cada MS |
| Observabilidad | Logs estructurados en cada MS, CorrelationId en Gateway |
| Trazabilidad distribuida | CorrelationId propagado en headers entre MS |
| Integración síncrona | gRPC entre MS |
| Integración asíncrona | RabbitMQ EventBus |
