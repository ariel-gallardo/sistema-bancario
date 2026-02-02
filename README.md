# Sistema Bancario Aurora

> Monorepo educativo que simula el home banking de Aurora Bank combinando React 19 + Vite en el frontend y microservicios minimalistas en .NET 10 con SQL Server y JWT.

## Vision general
- Frontend single page app con React Router, Material UI y Redux Toolkit que guia al usuario desde el login hasta un tablero financiero en tiempo real.
- Cuatro microservicios independientes (Clientes, Cuentas, Tarjetas y Pagos) con datos sembrados y endpoints REST protegidos con JWT.
- Orquestacion opcional con .NET Aspire (`SistemaBancario.AppHost`) que levanta SQL Server en contenedores y arranca cada servicio con health checks.
- Libreria compartida `SistemaBancario.ServiceDefaults` lista para anadir telemetria, descubrimiento de servicios y politicas de resiliencia.

```text
+----------------------+        +---------------------------+
| React + Vite (5173)  | -----> | Clientes API (65534)      |--+
| Redux + MUI          |        | JWT + Perfil / Login      |  |
+----------+-----------+        +---------------------------+  |
           |                                            HTTP  |
           |   Cuentas API (65532)                         |  |
           |   Tarjetas API (65515)                        |  |
           v                                               v  |
     Dashboard Redux <--> Microservicios SQL Server      Pagos |
```

| Componente | Proyecto | Puerto por defecto | Stack principal |
|------------|----------|--------------------|-----------------|
| Frontend SPA | `frontend/` | 5173 (Vite dev server) | React 19, Vite 7, Material UI 6, Redux Toolkit 2 |
| Clientes API | `Servicios/Clientes` | 65534 | .NET 10, Minimal APIs, EF Core 9, JWT, LocalDB/SQL Server |
| Cuentas API | `Servicios/Cuentas` | 65532 | .NET 10, Minimal APIs, EF Core 9 |
| Tarjetas API | `Servicios/Tarjetas` | 65515 | .NET 10, Minimal APIs, EF Core 9 |
| Pagos API | `Servicios/Pagos` | 65530 | .NET 10, Minimal APIs, EF Core 9 |
| App Host | `SistemaBancario.AppHost` | n/a | .NET Aspire 9.5, SQL Server container orchestration |

## Frontend (`frontend/`)
- **Estado global**: `src/store/index.ts` combina `authSlice` y `dashboardSlice`. Los thunks usan Axios con cabeceras `Authorization` extraidas del store.
- **Configuracion HTTP**: `src/config/api.ts` expone `VITE_CLIENTES_URL`, `VITE_CUENTAS_URL` y `VITE_TARJETAS_URL` (fallbacks alineados con los puertos del AppHost).
- **Autenticacion**: `authSlice` envia `POST /api/auth/login` y almacena `token` + perfil (`clienteId`, `cuentaPrincipalId`, etc.). `logout` resetea todo.
- **Dashboard**: `dashboardSlice` orquesta tres flujos clave.
  1. `fetchDashboardData` llama en paralelo a `/api/cuentas/principal` y `/api/tarjetas/principal/movimientos?take=5`.
  2. `fetchAccountById` refresca cualquier cuenta seleccionada desde `AccountSelector`.
  3. `updateFavoriteAccount` persiste la bandera favorita via `PUT /api/cuentas/{id}/favorita` o `DELETE /api/cuentas/favorita`.
- **UI**: componentes en `src/components/` encapsulan tarjetas Material UI (`AccountCard`, `MovementsCard`, `SessionCard`, `LoginView`, etc.). Los helpers de `src/utils/formatters.ts` usan `Intl.NumberFormat` y `dayjs` con locale `es`.
- **Demo login**: `demoCredentials` publica `demo@aurorabank.com / B4nco$123`, alineado con los seeds del servicio de Clientes.

## Servicios .NET
Todos comparten Minimal APIs, autenticacion JWT, CORS `frontend`, health check en `/health` y seeding idempotente via `EnsureCreated`.

### Clientes (`Servicios/Clientes`)
- Maneja autenticacion y perfiles.
- `POST /api/auth/login` valida el hash SHA256 (`PasswordHasher`) y emite JWT firmado (`JwtTokenService`) con claims `clienteId`, `cuentaPrincipalId`, `tarjetaPrincipalId`, `nombre`.
- `GET /api/clientes/me` devuelve `ClienteProfileResponse` usando el claim `clienteId`.
- Seeds en `ClientesDbInitializer` crean el usuario demo consistente con los demas servicios.

### Cuentas (`Servicios/Cuentas`)
- Expone resumenes de cuentas bancarias y preferencias.
- Endpoints: `GET /api/cuentas/principal`, `GET /api/cuentas/{cuentaId}`, `PUT /api/cuentas/{cuentaId}/favorita`, `DELETE /api/cuentas/favorita`.
- `EndpointHelpers.BuildAccountSummaryAsync` calcula saldo disponible, limite de descubierto y `otrasCuentas` ordenadas.
- Seeds (`CuentasDbInitializer`) generan una cuenta sueldo ARS (favorita) y una cuenta ahorro USD.

### Tarjetas (`Servicios/Tarjetas`)
- Provee datos de la tarjeta principal y sus movimientos.
- Endpoint: `GET /api/tarjetas/principal/movimientos?take=5` calcula disponible = `Limite - SaldoUtilizado` y proyecta cada compra como `MovimientoTarjetaResponse`.
- `TarjetasDbInitializer` replica los GUID compartidos y genera consumos de prueba.

### Pagos (`Servicios/Pagos`)
- Ofrece agenda de pagos programados y un historial filtrable (`take` maximo 20).
- Endpoints: `GET /api/pagos/programados`, `GET /api/pagos/historial?take=5`.
- Aunque el frontend aun no consume estos datos, el contrato (`PagosContracts`) cubre empresa, categoria, medio de pago y flags de debito automatico.

### AppHost y Service Defaults
- `SistemaBancario.AppHost` usa .NET Aspire para levantar SQL Server, los cuatro servicios y la app de Node, definiendo puertos y dependencias `WaitFor` para asegurar que las bases esten listas.
- `SistemaBancario.ServiceDefaults` centraliza OpenTelemetry, health/liveness y politicas de resiliencia para cuando quieras compartir dichas configuraciones.

## Flujo principal (login -> dashboard)
1. El usuario visita `/login`, completa el formulario (`LoginView`) y despacha `login(credentials)`.
2. `Clientes` valida, emite JWT y retorna `LoginResponse`. `authSlice` guarda token + perfil.
3. `App.tsx` detecta `auth.token` y dispara `fetchDashboardData`.
4. El thunk arma cabeceras `Authorization` y hace `Promise.all` contra `Cuentas` y `Tarjetas`.
5. `AccountCard` y `AccountSelector` muestran la respuesta de `Cuentas`, permitiendo cambiar de cuenta y ajustar la favorita.
6. `MovementsCard` imprime los ultimos cinco consumos de la tarjeta principal y muestra alertas si falla alguna llamada.

## Datos sembrados y credenciales demo
| Concepto | GUID | Servicio |
|----------|------|----------|
| Cliente demo (Ariel Gallardo) | `8b45f7ed-1e24-4b21-9fcf-163b2d7a2a2f` | Clientes, Pagos, Tarjetas, Cuentas |
| Cuenta principal | `ca2f2eb2-e1a7-4fcf-9b19-63c19e78146a` | Clientes, Cuentas |
| Tarjeta principal | `2c23c2a8-276a-4782-9a05-754e665d4e05` | Clientes, Tarjetas |

Credenciales demo (tambien visibles en la UI):
- Email: `demo@aurorabank.com`
- Password: `B4nco$123`

## Puesta en marcha
### Requisitos previos
- .NET SDK 10.0 (preview) o superior.
- Node.js 20+ y npm 10+.
- SQL Server LocalDB (Windows) o Docker Desktop para permitir que Aspire levante SQL Server en contenedores.

### Opcion A: .NET Aspire
1. Ejecuta `dotnet restore` en la raiz del repo.
2. Corre `dotnet run --project SistemaBancario.AppHost`.
   - Aspire crea las bases `clientesdb`, `cuentasdb`, `tarjetasdb`, `pagosdb`.
   - Inicia cada microservicio y les inyecta la cadena de conexion del contenedor.
   - Levanta el frontend con `npm run dev` una vez que todas las dependencias estan listas.
3. Navega a http://localhost:5173 y logueate con las credenciales demo.

### Opcion B: procesos manuales
1. Bases locales: cada servicio ya apunta a `(localdb)\MSSQLLocalDB`; `EnsureCreated` + `DbInitializer` generan esquema y datos.
2. Levanta los servicios en terminales separadas:
   ```powershell
   dotnet run --project .\Servicios\Clientes\Clientes.csproj
   dotnet run --project .\Servicios\Cuentas\Cuentas.csproj
   dotnet run --project .\Servicios\Tarjetas\Tarjetas.csproj
   dotnet run --project .\Servicios\Pagos\Pagos.csproj
   ```
3. Configura y ejecuta el frontend:
   ```powershell
   cd frontend
   npm install
   echo "VITE_CLIENTES_URL=http://localhost:65534" > .env.local
   echo "VITE_CUENTAS_URL=http://localhost:65532" >> .env.local
   echo "VITE_TARJETAS_URL=http://localhost:65515" >> .env.local
   npm run dev
   ```
4. Abre http://localhost:5173.

## Variables de entorno y configuracion
- **Frontend** (`.env` o `.env.local`):
  ```ini
  VITE_CLIENTES_URL=http://localhost:65534
  VITE_CUENTAS_URL=http://localhost:65532
  VITE_TARJETAS_URL=http://localhost:65515
  ```
  Si planeas consumir Pagos desde la UI agrega `VITE_PAGOS_URL`.
- **Servicios .NET**: puedes sobreescribir `ConnectionStrings:SqlServer`, `Jwt:Key` y `Cors:AllowedOrigins` via variables de entorno o `appsettings.{Environment}.json`.
- Todos los servicios reutilizan la misma clave JWT con fines educativos; en produccion deberias delegar la emision a un Identity Provider central.

## Salud, observabilidad y seguridad
- Health checks disponibles en `/health`; Aspire los usa para saber cuando cada servicio esta listo.
- JWT Bearer valida issuer, audience y usa clock skew de 1 minuto para simular expiraciones reales.
- `PasswordHasher` aplica SHA256 (demostrativo; en produccion preferi Argon2 o BCrypt con salt).
- CORS restringido a `http://localhost:5173` por defecto.
- `SistemaBancario.ServiceDefaults` incluye OpenTelemetry listo para habilitar trazas y metricas compartidas.

## Pruebas automatizadas

### Backend (.NET)
- Cada microservicio cuenta con pruebas unitarias e integracion en `tests/Clientes.Tests`, `tests/Cuentas.Tests`, `tests/Pagos.Tests` y `tests/Tarjetas.Tests`.
- Ejecuta toda la bateria con:
   ```powershell
   dotnet test sistema-bancario.slnx
   ```
- Los proyectos usan EF Core InMemory y `WebApplicationFactory` para montar los endpoints con autenticacion simulada, por lo que no se requiere una base real durante las pruebas.

### Frontend (Playwright)
- Las pruebas end-to-end viven en `frontend/tests/e2e` y utilizan Playwright para automatizar login, dashboard, panel admin y registro de movimientos.
- Instalacion inicial:
   ```powershell
   cd frontend
   npm install
   npx playwright install
   ```
- Ejecucion:
   ```powershell
   npm run test:e2e
   ```
- El `playwright.config.ts` arranca Vite en puerto 4173 y maqueta las llamadas HTTP a los microservicios, por lo que la suite UI no necesita que los servicios .NET esten levantados.

## Proximos pasos sugeridos
1. Consumir los endpoints de Pagos desde el frontend (widgets de pagos programados e historial).
2. Referenciar `SistemaBancario.ServiceDefaults` en cada microservicio para habilitar trazas distribuidas.
3. Automatizar despliegues combinando pipelines que ejecuten `dotnet test` y `npm run test:e2e` antes de promover a QA/Prod.
4. Contenerizar el frontend para despliegues homogeneiados junto al resto de los servicios.
