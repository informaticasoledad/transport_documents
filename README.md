# DTD — Documento Digital de Transporte

Backend en **.NET 10 / C# 14** que genera **Documentos Digitales de Transporte (DDT)** agrupando
expediciones recuperadas por HTTP de un microservicio ERP, y los transmite por API al gestor
documental **Docuten** (eCMR). **1 DDT = 1 lote** con N shipments (las expediciones); la creación
es asíncrona (`POST /api/v1/lots` → `pending` + `callback_url`, sondeo con `GET /api/v1/lots/{id}`).
Docuten comunica a los **conductores** (uno o más por DDT, con un canal `email`/`sms`/`whatsapp`),
cuyo catálogo vive en la base de datos por agencia (el ERP no los aporta).

Construido con **Clean Architecture + DDD**. El frontend (basado en `legacy/`) queda fuera del
alcance de esta fase y se abordará más adelante.

## Arquitectura

Dirección de dependencias: `Domain ← Application ← Infrastructure ← Api`.

```
src/
  Dtd.Domain/          Agregados, value objects, eventos de dominio, puertos de repositorio
  Dtd.Application/    CQRS (MediatR), validación (FluentValidation), DTOs, puertos de gateway
  Dtd.Infrastructure/  EF Core + Npgsql, repositorios, gateways HTTP (real + mock), Options, Serilog
  Dtd.Api/            Minimal API, módulos, DI, ProblemDetails, health checks, OpenAPI
tests/
  Dtd.Domain.Tests/        Unit tests de agregados y value objects
  Dtd.Application.Tests/   Handlers con doubles (NSubstitute)
  Dtd.Architecture.Tests/  Reglas de dependencias entre capas (NetArchTest)
```

`Directory.Build.props` common: `net10.0`, `LangVersion=14`, `Nullable=enable`,
`TreatWarningsAsErrors=true`.

### Modelo de dominio

- **`DocumentoDigitalTransporte`** (aggregate root): `Empresa`, `AlmacenCodigo`, `AgenciaCodigo`,
  `Origen` (`OrigenDocumento`, común a todas las expediciones del almacén/agencia), `Conductores`
  (**N**, mínimo 1 para enviar; snapshots inmutables `ConductorAsignado` del catálogo de la agencia),
  `RangoFechas`, `Estado`, `DocutenId`, `DocutenEstado`, colección de `Expedicion` y **log genérico
  de `DocumentoEvento`** (un único sitio donde buscar toda la vida del documento: generación,
  altas/bajas de conductor, envío a Docuten, fallos, cambios de estado sondeados, anulación).
  Invariantes: no se envía a Docuten sin **al menos un conductor** y con **canal de comunicación
  adecuado** en todos ellos (`email`/`sms`/`whatsapp` con contacto coherente) ni sin expediciones;
  ambas reglas (más la de estado `Nuevo`) se consolidan en `DocumentoDigitalTransporte.ValidarListoParaEnviar()`
  (única fuente de verdad: `ErrorOr<Success>` que el handler de envío propaga como
  `Documento.YaConfirmado` / `Documento.ExpedicionRequerida` / `Documento.ConductorRequerido` /
  `Documento.ConductorSinCanal`, **antes** de transmitir); los conductores sólo se añaden/quitan en estado `Nuevo` (idempotente por `ConductorCodigo`); no
  se permiten expediciones duplicadas por `ErpId`; los envíos fallidos no avanzan el estado (el
  documento sigue `Nuevo`) y dejan registrado un evento `EnvioFallido` en el log (el último error
  se lee del evento más reciente de ese tipo).
- **Pipeline de estados** (`EstadoDocumento`, en español):
  `Nuevo → Enviando → EnProgreso → Finalizado` (+ terminales `Anulado`, `Cancelado`, `Error`).
  Generar deja el documento en `Nuevo` **sin conductores**: el back no auto-asigna los defaults de
  la tupla (almacén, agencia) —el front los obtiene vía `.../conductores-default` y los añade—;
  mientras esté `Nuevo` se añaden (bulk, varios códigos)/quitan conductores del catálogo de la
  agencia; al `confirmar` se valida ≥1 conductor con canal adecuado, se transmite el lote a
  Docuten (N drivers) y pasa a `Enviando`; al sondear el estado, Docuten
  (`shipment_status`) puede moverlo a `EnProgreso` (`pending_delivery`) y `Finalizado`
  (`delivered`/`completed`), o a `Cancelado`/`Error`. `Anulado` es **local** (forzado desde el front,
  terminal): se permite desde `Nuevo`, `Enviando` y `EnProgreso`; en los dos últimos el back cancela
  antes los shipments del lote en Docuten (`POST /api/v1/shipments/{id}/cancel`) y, sólo si tiene
  éxito, pasa el documento a `Anulado`. Docuten **no tiene** "rechazado".
- **`Expedicion`** (child): `ErpId` (referencia externa `"…|26"`, única para tracking),
  `DocumentNumber`, `ExpeditionCode`, `ExpeditionType` (1=entrega a cliente, 2=transfer entre
  almacenes), `Empresa`, `AlmacenCodigo`, `AgenciaCodigo`, `Fecha`, `Cliente`, `Destino`, `Bultos`
  (= nº de líneas `expeditionDetails` del ERP; las líneas no se persisten).
- **Value objects:** `OrigenDocumento`, `RangoFechas`, `DestinoExpedicion` (`AlmacenDestino`
  string, `AddressName`/`AddressStreet` para el `address` del destino en Docuten; `Pais` guarda
  el ISO), `Movil` (normaliza a dígitos, 6–15), `Email`, `Canal` (`email`|`sms`|`whatsapp`, con
  `RequiereMovil`/`RequiereEmail`).
- **`Conductor`** (aggregate root, tabla `conductores`): catálogo **per-empresa** vinculado
  **M:N a agencias** (tabla `conductor_agencias`; un conductor puede servir a varias agencias de
  su empresa, p.ej. DPDFR/DPDEU) con el perfil completo del party Docuten: `Empresa`, `Codigo`,
  `Nombre`, `TaxId?`, `LicensePlate?`, `Movil?`, `Email?`, `Canal` (explícito), `Language` (default
  `"es"`), `Activo`. Clave única `(Empresa, Codigo)`. Invariante de coherencia canal-contacto
  (`email`→`Email`, `sms`/`whatsapp`→`Movil`). Población manual SQL (como `empresas`/`almacenes`);
  el CRUD de escritura (maestro agencias/conductores) queda para otra fase.
- **`Agencia`** (aggregate root, tabla `agencias`): **per-empresa** (columna `empresa`, unique
  `(empresa, codigo)`), con `Nombre`/`Activa`. Relación **M:N con `Almacen`** vía `almacen_agencias`
  (qué carriers sirven a cada almacén). Los **conductores por defecto** de cada tupla
  (almacén, agencia) —pueden ser **varios**— viven en `almacen_agencia_conductores_defecto`.
  El back **no** los auto-asigna al `generar`: se exponen vía
  `GET /api/empresas/{empresa}/almacenes/{almacen}/agencias/{agencia}/conductores-default` y los
  añade el front con `POST /api/documentos/{id}/conductores` (bulk, varios códigos).
- **`Almacen`** (aggregate root, tabla local `almacenes`): la delegación de origen, **no** se
  recupera del ERP (población manual SQL). Clave natural `(empresa, codigo)` única; datos de
  dirección + contacto (`calle`/`codigo_postal`/`municipio`/`pais`/`email`/`telefono`). Relación
  **M:N con `agencias`** vía `almacen_agencias` → en cada almacén sólo aparecen sus agencias
  disponibles. El consignor del lote Docuten combina `empresas` (name/tax_id/signer_*) + `almacenes`
  (dirección/contacto); el back valida al `generar` que el almacén exista para la empresa y la
  agencia esté entre sus disponibles (`Almacen.NoConfigurado` / `Almacen.AgenciaNoDisponible`).
- **Tipos de documento:** `consignment_note` (carta de portes, nacional) cuando
  `destino.Pais == origen.CountryIsoCode`; `ecmr` (internacional) en caso contrario. Inferido
  en el mapeo; el PDF (`documents[]`) queda **diferido** pendiente de consulta con Docuten.
- **Eventos:** `DocumentoGenerado`, `DocumentoEnviadoADocuten`, `DocumentoEstadoCambiado`
  (dispatched por el `UnitOfWork` vía MediatR). Además, todo lo que le pasa al documento queda en el
  **log genérico persistido** `DocumentoEvento` (tabla `documento_eventos`): generación, altas/bajas
  de conductor, envío a Docuten, fallos, cambios de estado sondeados y anulación —un único sitio
  donde buscar el historial, vía `GET /api/documentos/{id}/eventos`.

### Multiempresa

Una sola BBDD PostgreSQL compartida. `Empresa` es una columna más en `documentos` (y la heredan
las expediciones). El identificador de empresa es **numérico de 3 dígitos** (`"001"`); el front
normaliza el parámetro de la URL (`empresa=1` → `"001"`) y el back **también normaliza** antes de
usarlo (defense-in-depth) y de compararlo con el token. Los permisos por empresa se verifican en el
back a partir del token OIDC de Keycloak (ver *Autenticación*).

### Tracking de "expediciones no incluidas todavía"

`Unique index` sobre `(empresa, almacen_codigo, agencia_codigo, erp_id)` en `expediciones`. Al
generar, se consultan los `ErpId` ya incluidos en cualquier documento para el mismo
empresa/almacén/carrier y se excluyen. Repetir `generar` para el mismo rango/almacén/agencia no
reinserta expediciones ya agrupadas. El ERP filtra por `warehouseId` (almacén) y `carrierId`
(agencia), así que un mismo `ErpId` vuelve siempre bajo el mismo par (almacén, carrier) en la
práctica — el índice ampliado documenta ese scope.

## Ejecutar

### Requisitos

- .NET SDK 10
- PostgreSQL 14+ accesible (local o en la red)

### Configuración

`src/Dtd.Api/appsettings.json` (o mediante variables de entorno con `__` como separador en k8s):

```jsonc
{
  "Database": {
    // Connection string SIN la contraseña: va cifrada abajo (mismo patrón que el client_secret).
    "ConnectionString": "Host=localhost;Port=5432;Database=dtd;Username=postgres",
    "Password_Enc": { "Ciphertext": "", "Nonce": "", "Tag": "" },
    "AutoApplyMigrations": false
  },
  "Erp":     { "UseMock": false, "TimeoutSeconds": 30, "EndpointCacheMinutes": 2,
              "TokenSkewSeconds": 60, "TokenDefaultTtlSeconds": 300,
              "ClientSecret_Enc": { "Ciphertext": "", "Nonce": "", "Tag": "" } },
  "Docuten": { "UseMock": true, "BaseAddress": "...", "TokenId": "", "TimeoutSeconds": 30,
              "CallbackUrl": "", "DefaultLanguage": "es" }
}
```

`UseMock` selecciona el gateway mock (datos deterministas) frente al adaptador HTTP real. Las
`Options` se validan con `ValidateDataAnnotations().ValidateOnStart()`. El **adaptador real de
Docuten** (`/api/v1/lots`, cabecera `X-API-KEY`) ya está cableado: su API key (`Docuten:TokenId`)
es un **secret env-only** que **nunca** se commitea (user-secrets en dev / env var
`Docuten__TokenId` desde un Secret de k8s en prod); obligatorio cuando `Docuten:UseMock=false`.

> **Secrets cifrados (dos planos, una master key):** el `client_secret` del ERP (`Erp:ClientSecret_Enc`)
> y la contraseña de PostgreSQL (`Database:Password_Enc`) van **cifrados** (AES-256-GCM) en
> `appsettings.json` — son **committables**: es ciphertext. La **master key** (32 bytes base64)
> **nunca** se commitea: se inyecta por `ERPAUTH_MASTER_KEY` (config → user-secrets en dev / env var /
> Secret de k8s) o `ERPAUTH_MASTER_KEY_FILE` (fichero, montaje de Secret). Ambos secrets usan la
> **misma master key**. Para reconstruir un secret hacen falta **los dos planos** (repo + env). Si
> existe el bloque pero falta/invalida la master key, el API **falla al arrancar**. Genera el bloque
> con `tools/Dtd.Tools.SecretCipher`:
> ```bash
> $env:ERPAUTH_MASTER_KEY='<base64-32B>'
> 'mi-secret' | dotnet run --project tools/Dtd.Tools.SecretCipher   # → pega el JSON en el *_Enc que toque
> ```
> La contraseña de PostgreSQL **se rota en el servidor** y luego se cifra el valor **nuevo**; el valor
> anterior (`Docuten26!`, ya en el historial de git) queda muerto tras la rotación.

> **Endpoint del ERP por empresa:** cada empresa tiene su propio microservicio ERP (URL distinta)
> y **se autentica con un JWT bearer token obtenido por OAuth2 client-credentials**. El cliente
> OAuth2 del ERP es **único y común a todas las empresas** (un mismo cliente: `token_endpoint` +
> `client_id` + `scope` + `client_secret`), así que **toda esa configuración es compartida y vive en
> `appsettings.json`** (sección `Erp`: `TokenEndpoint`, `ClientId`, `Scope`, `TimeoutSeconds` y el
> `ClientSecret_Enc` cifrado). De la tabla **`empresas`** (PK `empresa`, código de 3 dígitos)
> **solo se lee lo que varía por empresa: `base_address`** (la URL de su instancia de ERP); se cachea
> (`Erp:EndpointCacheMinutes`) y el token se cachea aparte hasta `expires_in − TokenSkewSeconds`. El
> **`client_secret` es común a todas las empresas** y **no va en la BD**: viaja **cifrado**
> (AES-256-GCM) en `appsettings.json` (`Erp:ClientSecret_Enc: { Ciphertext, Nonce, Tag }`, base64 — es
> **committable**, es ciphertext) y se descifra al arrancar con la **master key** inyectada por
> `ERPAUTH_MASTER_KEY` (env / user-secrets / Secret de k8s) o `ERPAUTH_MASTER_KEY_FILE`. Para
> reconstruir el secret hacen falta **los dos planos** (repo + env). El path y el DTO son los mismos
> para todas las empresas; una empresa sin fila (o sin secret descifrado) devuelve
> `Error.Failure("Empresa.ErpNoConfigurado")`. Sembrar a mano (ver `docs/seed-empresa-001.sql`):
> ```sql
> INSERT INTO empresas (empresa, base_address)
> VALUES ('001', 'https://soluciona-iseries.gruposoledad.com');   -- raíz del host, SIN /api (el /api va en la ruta del gateway)
> ```
> Y el secret, cifrado en `appsettings.json` (la master key, por env — **nunca** en el repo):
> ```bash
> # 1) Genera la master key (32 bytes) y guárdala en el Secret de k8s / user-secrets:
> [Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
> # 2) Cifra el client_secret con esa master key → pega el JSON en "Erp:ClientSecret_Enc":
> $env:ERPAUTH_MASTER_KEY='<base64-32B>'
> 'mi-secret' | dotnet run --project tools/Dtd.Tools.SecretCipher
> # 3) Inyecta la master key en runtime (dev: user-secrets / env; prod: Secret de k8s):
> dotnet user-secrets set "ERPAUTH_MASTER_KEY" "<base64-32B>" --project src/Dtd.Api
> ```
> Para tirar del mock en local (sin ERP real ni secret): `Erp:UseMock=true`.

### Autenticación y autorización (OIDC / Keycloak)

El back se autentica por OIDC SSO contra el Keycloak corporativo (resource server JWT); la app
que lo embebre tiene su propia auth, que el back no fía. Toda `/api/*` exige un token válido (401 si
falta/inválido). El usuario (auditoría) **sale del token** (`sub`/`preferred_username`), nunca del
body; el parámetro `user=` de la URL se ignora en el back.

La **autorización por empresa** se verifica contra el **claim `empresas`** del token (las empresas
a las que el usuario tiene acceso, ya normalizadas a 3 dígitos): toda operación empresa-alcance
comprueba que la empresa pedida ∈ claim → `403 Empresa.NoAutorizada` si no. Esto incluye los
endpoints por `{id}` (`/{id}`, `/confirmar`, `/sincronizar-estado`, `/eventos`, `/anular`), que cargan el
documento y comprueban su empresa (cierra el IDOR). Los permisos se gestionan en Keycloak (atributo
de usuario / rol / grupo + protocol mapper al claim), **sin** tabla `usuarios_empresas` en el DDT.

Toggle **`Auth:Enabled`** (bool, por config) permite montar el back sin bloquear el flujo dev/mock
mientras llega el claim real:

```jsonc
"Auth":     { "Enabled": false },               // dev: API anónima, sin chequeo de empresa
"Keycloak": { "Authority": "<realm-url>", "Audience": "dtd-api",
              "EmpresasClaimType": "empresas", "NameClaimType": "preferred_username",
              "RequireHttpsMetadata": true }
```

Con `Auth:Enabled=false` todo es anónimo y `IUsuarioContexto.Current` es null (los handlers omiten
el chequeo de empresa, así el smoke test `UseMock=true` sin token sigue funcionando). En producción
`Auth:Enabled=true` activa `AddJwtBearer` (Authority = realm, ValidAudience) + una `FallbackPolicy`
que exige usuario autenticado en todos los endpoints salvo health/openapi (`AllowAnonymous`).

### Base de datos

```bash
# Aplicar la migración inicial
dotnet ef database update --project src/Dtd.Infrastructure --startup-project src/Dtd.Api

# O dejar que el Api la aplique al arrancar (solo dev): Database:AutoApplyMigrations=true
```

### Arrancar

```bash
dotnet run --project src/Dtd.Api
# OpenAPI:      http://localhost:8080/openapi/v1.json  (solo en Development)
# Health:       http://localhost:8080/health  y  /health/ready
```

### Docker / Kubernetes

```bash
docker compose up            # postgres + api (ver docker-compose.yml)
```

Manifiestos esqueleto en `deploy/k8s/` (namespace, configmap, secret, deployment con
readiness/liveness, service). Aplicar con `kubectl apply -f deploy/k8s/`.

## API

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/documentos/generar` | Genera un DDT con expediciones del ERP del rango/almacén/carrier no incluidas todavía (estado `Nuevo`, sin conductores) |
| POST | `/api/documentos/{id}/conductores` | Asigna uno o varios conductores del catálogo (vinculados a la agencia del documento) a un documento `Nuevo` (body `{conductoresId:["<guid>",...]}`); idempotente por Id de catálogo; all-or-nothing |
| DELETE | `/api/documentos/{id}/conductores/{conductorId}` | Quita un conductor de un documento `Nuevo` |
| POST | `/api/documentos/{id}/confirmar` | Valida (estado `Nuevo`, ≥1 expedición, ≥1 conductor con canal adecuado — `ValidarListoParaEnviar`) y transmite un lote a Docuten (N drivers) → `Enviando` con `lot_id`. Si falla, registra el intento y mantiene `Nuevo` para reintentar |
| POST | `/api/documentos/{id}/sincronizar-estado` | Sondea el estado del lote en Docuten (`GET /api/v1/lots/{lot_id}`) y actualiza el documento (`EnProgreso`/`Finalizado`/`Cancelado`/`Error`) |
| GET  | `/api/documentos/{id}` | Obtiene un documento (incluye `conductores`) |
| GET  | `/api/documentos/{id}/eventos` | Log genérico de eventos del documento (generación, conductores, envío a Docuten, fallos, cambios de estado, anulación) |
| POST | `/api/documentos/{id}/anular` | Anula el documento (forzado desde el front, terminal). Desde `Nuevo` sólo vuelca el estado; desde `Enviando`/`EnProgreso` cancela antes el lote en Docuten. Body opcional `{ "motivo": "..." }` |
| GET  | `/api/documentos` | Lista con filtros (`empresa`, `almacenCodigo`, `agenciaCodigo`, `fechaDesde`, `fechaHasta`, `estado`) |
| GET  | `/api/expediciones/disponibles` | Expediciones del ERP del rango/almacén/carrier no incluidas todavía |
| GET  | `/api/empresas/{empresa}/almacenes` | Almacenes activos de la empresa (dropdown del front) |
| GET  | `/api/empresas/{empresa}/almacenes/{almacenCodigo}/agencias` | Agencias disponibles para ese almacén (M:N `almacen_agencias`) |
| GET  | `/api/empresas/{empresa}/almacenes/{almacenCodigo}/agencias/{agenciaCodigo}/conductores-default` | Conductores por defecto (varios) de la tupla (almacén, agencia) que el front auto-adjunta al generar |
| GET  | `/api/empresas/{empresa}/agencias` | Agencias activas de la empresa (dropdown del front) |
| GET  | `/api/empresas/{empresa}/agencias/{agenciaCodigo}/conductores` | Conductores activos del catálogo de esa agencia (dropdown del front) |

Ejemplo:

```bash
curl -X POST http://localhost:8080/api/documentos/generar \
  -H 'Content-Type: application/json' \
  -d '{
    "empresa": "001",
    "almacenCodigo": "21",
    "agenciaCodigo": "AG01",
    "fechaDesde": "2026-07-12",
    "fechaHasta": "2026-07-12"
  }'
```

Los errores se devuelven como **ProblemDetails (RFC 9457)** con `traceId`.

## Tests

```bash
dotnet test dtd.slnx
```

Cubre dominio (agregados/VOs), handlers de Application con doubles y reglas de arquitectura.

## Contratos externos

Los contratos HTTP reales de ERP y Docuten (incluidos los mocks) se documentan en
[`docs/erp-docuten-contracts.md`](docs/erp-docuten-contracts.md). El adaptador real del ERP
(OAuth2 client-credentials por empresa, tabla `empresas`) y el de Docuten (`/api/v1/lots`,
asíncrono, `X-API-KEY`) ya están conectados. Ambos se activan con `UseMock=false` (+ los secrets
que cada uno requiera).

## Fuera de alcance (fase actual)

- Frontend (se hará después partiendo de `legacy/`).
- Configuración real del claim `empresas` en Keycloak (el back queda listo; el mapper/atributo se
  configura en Keycloak cuando haya realm).
- **PDF de Docuten** (`documents[]` + `IDocutenDocumentoProvider`): diferido pendiente de consulta
  con Docuten (¿hace falta subir PDF o basta con el lote JSON?).
- **Webhooks reales de Docuten** (`callback_url`): se sondea el estado hasta tenerlos.
- **Fuentes completas de party data (Fase 2):** `tax_id`/`email`/`mobile` del consignee (fuente
  por definir); override de conductores por body en `confirmar`. El **consignor ya está completo**
  (empresa + almacén local) y los **drivers ya están completos** (catálogo `conductores` con
  `tax_id`/`license_plate`/`channel` explícito + snapshot por documento).
- **CRUD de `almacenes`/`agencias`/`conductores`/`almacen_agencias`:** fuera de alcance en esta fase
  (población manual SQL como `empresas`); el front lo gestionará más adelante con endpoints de
  escritura.