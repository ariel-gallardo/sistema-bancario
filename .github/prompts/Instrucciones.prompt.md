---
agent: agent
---
Define the task to achieve, including specific requirements, constraints, and success criteria.

### Desafío técnico
- **Perfil:** Desarrollador Fullstack Ssr (.NET + React.js + SQL Server)

### Modelo de negocio
Sistema financiero de **tarjetas de crédito**, con las siguientes entidades:
- Clientes
- Cuentas
- Tarjetas
- Pagos
- Movimientos

### Contexto de negocio
Estás trabajando en el **backend y frontend** de un sistema de home banking.

- Un **cliente** puede tener una o varias **cuentas**.
- Cada **cuenta** puede tener una o varias **tarjetas de crédito** asociadas.
- Cada cliente posee:
  - Una **cuenta principal**
  - Una **tarjeta de crédito principal**

### Objetivo funcional
Implementar una vista de **resumen** para el home banking del cliente que muestre:
- **Saldo actual** de la cuenta principal.
- **Últimos 5 movimientos** de la tarjeta de crédito principal.

---

## Requerimientos técnicos

### Backend (.NET)
- Crear pruebas unitarias y de integración.
- Implementar Event Store.
- Utilizar CQRS (Command Query Responsibility Segregation) con **Mediatr**.
- Utilizar **.Net 10** como framework principal.
- Utilizar **Event Sourcing**.
- Diseño del modelo de datos (entidades, eventos y relaciones).
- Implementar **Pagos y Movimientos**.
- Para **Clientes, Cuentas y Tarjetas**:
  - Implementar únicamente operaciones **ABM** (Alta, Baja lógica y Modificación).
  - Las bajas deben ser **lógicas** (soft delete), no físicas.
- Persistencia en **SQL Server**.
- Documentar README con instrucciones para:
  - Configuración del entorno.
  - Ejecución de la aplicación.
  - Pruebas unitarias y de integración.

### Frontend (React.js)
- Utilizar **React.js** como framework principal.
- Manejo de estado global con **Redux.js**.
- Componentes visuales con **Material UI**.
- Consumir los endpoints del backend para:
  - Mostrar la información del resumen
  - Consulta del saldo de la cuenta principal.
  - Consulta de los últimos 5 movimientos de la tarjeta principal.
  - Vista simple en React que muestre saldo y últimos movimientos.
---

## Criterios de éxito
- El saldo corresponde a la **cuenta principal** del cliente.
- Se muestran exactamente los **últimos 5 movimientos**, ordenados por fecha descendente.
- Separación clara entre **Command y Query**.
- Uso correcto de **Event Sourcing** para entidades maestras.
- Código claro, legible y mantenible.
