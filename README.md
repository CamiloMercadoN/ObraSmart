# ObraSmart

**Plataforma digital para la elaboración de presupuestos y cotizaciones en servicios de gasfitería mediante un modelo simplificado de Análisis de Precios Unitarios (APU).**

## Descripción

ObraSmart es una aplicación web progresiva (PWA) orientada a trabajadores independientes del rubro de la gasfitería.

La solución busca apoyar el proceso de presupuestación mediante una estructura organizada y reutilizable de costos, permitiendo administrar insumos, precios, clientes y estructuras APU, elaborar presupuestos y generar cotizaciones de manera estructurada y trazable.

El proyecto fue desarrollado como Producto Mínimo Viable (MVP) en el contexto de un proyecto académico de Ingeniería en Computación e Informática.

## Funcionalidades principales

ObraSmart incorpora las siguientes capacidades:

- Registro y autenticación de usuarios.
- Dashboard principal.
- Gestión de clientes.
- Gestión de insumos.
- Actualización de precios de referencia.
- Catálogo controlado de unidades de medida.
- Gestión de etiquetas.
- Creación y actualización de estructuras APU.
- Cálculo automático de costos mediante APU.
- Consulta y reutilización de estructuras APU.
- Creación y gestión de presupuestos.
- Duplicación independiente de estructuras APU y presupuestos.
- Conservación de antecedentes históricos asociados a los presupuestos.
- Configuración de información comercial.
- Gestión de cotizaciones.
- Generación de cotizaciones en formato PDF.
- Interfaz responsive.
- Soporte como aplicación web progresiva (PWA).

## Arquitectura

ObraSmart utiliza una arquitectura cliente-servidor desacoplada.

El frontend se implementa mediante Vue 3 y se comunica con una API REST desarrollada en ASP.NET Core. El backend se organiza siguiendo principios de Clean Architecture, separando las responsabilidades de dominio, aplicación, infraestructura y presentación.

La solución se distribuye en los siguientes proyectos:

```text
ObraSmart/
├── ObraSmart.Domain/
├── ObraSmart.Application/
├── ObraSmart.Infrastructure/
├── ObraSmart.Server/
├── obrasmart.client/
├── ObraSmart.Application.Tests/
├── ObraSmart.IntegrationTests/
├── ObraSmart.slnx
├── v1.0.0-mvp.md
└── README.md
```

### Responsabilidades principales

- **ObraSmart.Domain:** entidades y conceptos centrales del dominio.
- **ObraSmart.Application:** servicios de aplicación, contratos y coordinación de casos de uso.
- **ObraSmart.Infrastructure:** persistencia, repositorios y mecanismos de infraestructura.
- **ObraSmart.Server:** API REST, controladores, configuración de servicios y punto de entrada de la aplicación.
- **obrasmart.client:** aplicación web progresiva desarrollada con Vue 3.
- **ObraSmart.Application.Tests:** pruebas unitarias de servicios del backend.
- **ObraSmart.IntegrationTests:** pruebas de integración de la API.

La infraestructura implementa los contratos definidos por las capas internas, manteniendo las reglas principales del sistema separadas de los mecanismos específicos de persistencia y despliegue.

## Tecnologías

### Backend

- ASP.NET Core 10
- C#
- Entity Framework Core 10
- API REST
- JWT
- BCrypt
- FluentValidation
- SQL Server
- Azure SQL

### Frontend

- Vue 3
- TypeScript
- Vite
- PrimeVue
- Pinia
- Vue Router
- PWA
- Vitest
- Vue Test Utils

### Infraestructura

- Docker
- Microsoft Azure
- Azure Container Registry
- Azure Container Apps
- Azure SQL

### Herramientas de desarrollo y pruebas

- Visual Studio
- Git
- GitHub
- xUnit
- FluentAssertions
- WebApplicationFactory
- Vitest
- Vue Test Utils

## Requisitos para desarrollo

Para ejecutar el proyecto localmente se requiere:

- .NET 10 SDK.
- Node.js compatible con las dependencias del proyecto.
- npm.
- SQL Server.
- Visual Studio con soporte para ASP.NET Core.
- Herramientas de desarrollo para contenedores Docker.

Las cadenas de conexión, claves y credenciales no deben almacenarse directamente en el repositorio.

## Configuración

La configuración sensible debe proporcionarse mediante los mecanismos correspondientes al ambiente de ejecución.

La conexión principal a la base de datos utiliza la clave:

```text
ConnectionStrings__DefaultConnection
```

En ambientes productivos, su valor debe almacenarse mediante mecanismos seguros de configuración y secretos.

## Ejecución en desarrollo

### Backend

Desde la raíz de la solución:

```bash
dotnet restore
dotnet run --project ObraSmart.Server
```

### Frontend

Desde la carpeta `obrasmart.client`:

```bash
npm install
npm run dev
```

Para generar una compilación del frontend:

```bash
npm run build
```

## Base de datos

ObraSmart utiliza Entity Framework Core bajo un enfoque Code-First.

El esquema de persistencia se administra mediante migraciones de Entity Framework Core.

En el ambiente productivo, la aplicación se conecta a Azure SQL utilizando la configuración proporcionada externamente al contenedor.

La solución dispone además de un mecanismo de carga inicial de catálogos mediante el argumento:

```text
--seed
```

Este proceso permite inicializar los datos requeridos por la aplicación de manera controlada.

## Pruebas

ObraSmart dispone de una suite de pruebas automatizadas para backend, frontend e integración.

La línea base `v1.0.0-mvp` fue validada mediante:

| Tipo de prueba | Cantidad |
|---|---:|
| Pruebas unitarias del backend | 10 |
| Pruebas de componentes del frontend | 4 |
| Pruebas de integración | 5 |
| **Total** | **19** |

### Resultado

```text
Pruebas ejecutadas : 19
Pruebas superadas  : 19
Errores             : 0
Pruebas omitidas    : 0
```

Las pruebas incluyen escenarios relacionados con:

- registro y autenticación;
- validación de usuarios duplicados;
- gestión de clientes;
- restricciones sobre registros asociados;
- creación y actualización de estructuras APU;
- cálculo de costos;
- actualización de precios;
- creación y validación de presupuestos;
- duplicación independiente de estructuras APU y presupuestos;
- aislamiento de información entre usuarios;
- persistencia de totales y recursos históricos.

### Ejecutar pruebas del backend

```bash
dotnet test
```

### Ejecutar pruebas del frontend

Desde `obrasmart.client`:

```bash
npm test
```

## Despliegue

El MVP de ObraSmart se encuentra desplegado en Microsoft Azure.

La aplicación utiliza una única unidad de despliegue basada en Docker. Durante la construcción de la imagen, el frontend Vue 3 se compila y sus archivos estáticos son incorporados a la aplicación ASP.NET Core.

Kestrel sirve tanto los archivos estáticos del frontend como los servicios de la API REST.

La estrategia de despliegue puede representarse de la siguiente manera:

```text
Usuario
   │
   ▼
Navegador / PWA
   │
   │ HTTPS
   ▼
Azure Container Apps
   │
   ▼
Docker
obrasmartserver:v1.0.0-mvp
   │
   ├── ASP.NET Core 10 / Kestrel
   │      ├── API REST
   │      └── Vue 3
   │
   └── Entity Framework Core 10
              │
              ▼
          Azure SQL
```

La imagen Docker se almacena de manera privada en Azure Container Registry y posteriormente es utilizada por Azure Container Apps para ejecutar la aplicación.

### Infraestructura productiva

| Componente | Servicio |
|---|---|
| Imagen Docker | `obrasmartserver:v1.0.0-mvp` |
| Registro | Azure Container Registry |
| Ejecución | Azure Container Apps |
| Persistencia | Azure SQL |
| Región | West US 3 |

La utilización de una única unidad de despliegue fue adoptada para reducir la complejidad operacional del MVP. Esta decisión no elimina el desacoplamiento lógico entre frontend y backend, los cuales permanecen separados a nivel de proyectos, responsabilidades y comunicación mediante API REST.

## Gestión de configuración

La versión productiva del MVP se identifica mediante una línea base común para el código fuente y el artefacto desplegable.

```text
Código fuente
     │
     ▼
Git tag
v1.0.0-mvp
     │
     ▼
Imagen Docker
obrasmartserver:v1.0.0-mvp
     │
     ▼
Azure Container Registry
     │
     ▼
Azure Container Apps
     │
     ▼
Producción
```

### Línea base actual

| Elemento | Identificación |
|---|---|
| Versión | `v1.0.0-mvp` |
| Git tag | `v1.0.0-mvp` |
| Commit | `abb5ef9` |
| Imagen Docker | `obrasmartserver:v1.0.0-mvp` |
| Ambiente | Producción |
| Plataforma | Microsoft Azure |
| Estado | Liberada |

Esta identificación permite relacionar el código fuente versionado en Git con la imagen Docker almacenada en Azure Container Registry y con la versión ejecutada en el ambiente productivo.

## Release

La primera liberación productiva del MVP corresponde a:

**ObraSmart v1.0.0-mvp**

La nota de versión asociada se encuentra disponible en:

[`v1.0.0-mvp.md`](./v1.0.0-mvp.md)

La versión también se encuentra identificada mediante el tag Git:

```text
v1.0.0-mvp
```

## Seguridad

La solución considera las siguientes medidas básicas de seguridad:

- autenticación mediante JWT;
- almacenamiento seguro de contraseñas mediante hash;
- aislamiento de información entre usuarios;
- validación de operaciones en el backend;
- uso de HTTPS en el ambiente productivo;
- almacenamiento externo de cadenas de conexión y secretos;
- Azure Container Registry privado;
- utilización de identidad administrada para el acceso al registro desde Azure Container Apps.

Las credenciales productivas no deben incorporarse al código fuente ni almacenarse en archivos versionados.

## Estado del proyecto

```text
Versión     : v1.0.0-mvp
Estado      : MVP liberado
Ambiente    : Producción
Plataforma  : Microsoft Azure
Pruebas     : 19/19 superadas
```

ObraSmart se encuentra desplegado como producto mínimo viable y constituye la línea base utilizada para la evaluación técnica y funcional del proyecto.
