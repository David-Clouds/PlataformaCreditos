# Plataforma de Créditos

Sistema web desarrollado con ASP.NET Core MVC para la gestión de solicitudes de crédito y su evaluación por analistas de riesgo.

---

##  URL del sistema

https://plataformacreditos-n794.onrender.com

---

##  Repositorio

https://github.com/David-Clouds/PlataformaCreditos

---

##  Tecnologías utilizadas

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core
- SQLite
- Identity (autenticación y roles)
- Razor Views
- Session
- Redis (Render KeyValue)
- Docker
- Render (deploy)

---

##  Funcionalidades

###  Usuario

- Registro e inicio de sesión
- Registro de solicitudes de crédito

#### Validaciones de negocio:

- No monto negativo
- No más de una solicitud pendiente
- Monto ≤ 10 veces ingresos

#### Catálogo:

- Listado de solicitudes
- Filtros por:
  - Estado
  - Monto
  - Fecha
- Visualización de detalle

---

###  Sesión y Cache

- Guarda la última solicitud visitada
- Muestra acceso rápido en el layout
- Cache distribuido de solicitudes (60s)

---

###  Analista

- Panel exclusivo para rol **Analista**
- Visualiza solicitudes pendientes
- Aprobar solicitudes
- Rechazar solicitudes con motivo obligatorio

#### Validaciones:

- No aprobar si monto > 5x ingresos
- No procesar solicitudes ya resueltas

---