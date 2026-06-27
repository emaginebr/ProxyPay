# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ProxyPay is a monorepo with two top-level projects:

- **`backend/`** — a .NET 8 multi-tenant payment orchestration API (PIX billings, invoices, QR codes and AbacatePay webhooks) following Clean Architecture, exposing REST + GraphQL. See `backend/` for the solution (`ProxyPay.sln`) and all .NET projects.
- **`frontend/`** — `proxypay-react`, a React + TypeScript component library (npm package) for consuming the payment API (PIX, invoices, recurring billing), plus an `example-app/` admin dashboard that demonstrates it.

Shared, repo-level concerns live at the root: `docker-compose*.yml` (build context `./backend`), `.github/workflows/`, `GitVersion.yml`, `LICENSE`, and the unified `README.md`.

## Commands

### Frontend — library (`frontend/`)
```bash
cd frontend
npm run build      # vite build → dist/ (ESM + CJS + .d.ts)
npm run dev        # vite build --watch
npm run typecheck  # tsc --noEmit
npm run lint       # eslint src/
```

### Frontend — example app (`frontend/example-app/`)
```bash
cd frontend/example-app
npm install        # installs deps + links the library via "proxypay-react": "file:.."
npm run dev        # vite dev server
npm run build      # tsc -b && vite build
```

### Backend (.NET 8 - from the `backend/` directory)
All .NET code lives under `backend/`. Run `cd backend` first, or pass the
`backend/` prefix to the paths below.
```bash
cd backend
dotnet build ProxyPay.sln                          # Build entire solution
dotnet run --project ProxyPay.API                  # Run API (https://localhost:44374)
dotnet run --project ProxyPay.BackgroundService    # Run background jobs
```

### Docker
```bash
docker-compose up        # Starts nginx-proxy (8081) + API services
```

## Architecture

### Backend - Clean Architecture (.NET 8)

All backend projects and the `ProxyPay.sln` solution live under `backend/`.

```
ProxyPay.API              → Controllers, Startup/DI config, auth middleware
ProxyPay.GraphQL          → HotChocolate GraphQL schemas, queries, type extensions
ProxyPay.Application      → DI bootstrap (Startup.cs), wires up services via ConfigureProxyPay()
ProxyPay.Domain           → Business logic: Models/, Services/, Core/, Interfaces/
ProxyPay.DTO              → Data transfer objects shared across layers
ProxyPay.BackgroundService → Scheduled background jobs
ProxyPay.ACL              → Anti-corruption layer (external API adapters)
ProxyPay.Infra.Interfaces → Infrastructure abstractions (IUnitOfWork, Repository interfaces)
ProxyPay.Infra            → EF Core 9 DbContext (ProxyPayContext), repositories, Unit of Work
Lib/                    → External DLLs: NAuth.ACL, NAuth.DTO, NTools.ACL, NTools.DTO
```

**Dependency flow:** API → GraphQL / Application → Domain → ProxyPay.Infra → PostgreSQL (Npgsql)

**Key patterns:**
- Repository + Unit of Work (ProxyPay.Infra)
- EF Core with lazy loading proxies
- Custom `RemoteAuthHandler` for Bearer token auth (delegates to NAuth)
- DI registration centralized in `ProxyPay.Application/Startup.cs` via `ConfigureProxyPay()` extension method

### GraphQL - HotChocolate (ProxyPay.GraphQL)

```
GraphQLServiceExtensions.cs → DI registration (AddProxyPayGraphQL), configures both schemas
GraphQLErrorLogger.cs       → Diagnostic event listener for logging GraphQL errors
Public/PublicQuery.cs       → Public queries (stores, products, categories, featuredProducts)
Public/PublicStoreType.cs   → ObjectType<Store> hiding internal fields (OwnerId, StoreUsers, Orders)
Admin/AdminQuery.cs         → Authenticated queries (myStores, myProducts, myCategories, myOrders)
Types/                      → ObjectType extensions adding computed fields via field resolvers
```

**Endpoints:**
- `POST /graphql` — public schema (anonymous)
- `POST /graphql/admin` — admin schema (requires Bearer token)
- Both endpoints expose interactive Banana Cake Pop playground

**Type extensions (computed fields):**
- `StoreTypeExtension` → `logoUrl` (resolves via IFileClient)
- `ProductTypeExtension` → `imageUrl` (resolves via IFileClient)
- `ProductImageTypeExtension` → `imageUrl` (resolves via IFileClient)
- `CategoryTypeExtension` → `productCount` (counts active products via navigation property)

**Key patterns:**
- Queries return `IQueryable<Entity>` directly from EF Core DbContext (no DTOs)
- `[UseProjection]`, `[UseFiltering]`, `[UseSorting]` for HotChocolate optimizations
- `[ExtendObjectType]` for adding computed fields without modifying entities

### Frontend — `proxypay-react` library (`frontend/`)

A lightweight React component library (npm package `proxypay-react`) for payment
processing — PIX QR codes, invoices, and recurring billings. Ships as ESM + CJS
with TypeScript declarations, zero runtime dependencies beyond React (peer dep).

```
frontend/
├── src/                       → Library source (the published package)
│   ├── types/payment.ts       → All TypeScript interfaces, enums, component props
│   ├── services/proxyPayService.ts → Class-based REST API client (configure(): baseUrl, clientId, tenantId)
│   ├── contexts/ProxyPayContext.tsx → Provider instantiating a service per app
│   ├── hooks/useProxyPay.ts   → Typed context consumer (throws outside provider)
│   ├── components/            → PixPayment (modal + polling), InvoicePayment, BillingPayment
│   └── index.ts               → Public API surface (all exports go through here)
├── example-app/               → Full admin dashboard demo (consumes the lib via "proxypay-react": "file:..")
│   └── src/                   → pages/, contexts/, hooks/, services/, types/ (one layer per entity)
├── docs/                      → Frontend docs (API_REFERENCE.md, system-design.mmd/png)
├── vite.config.ts             → Library build (lib mode, ESM + CJS, dts rollup)
└── package.json               → Package metadata + scripts
```

**Library architecture:** `types → services → contexts → hooks → components`.
The library uses the `X-Tenant-Id` header for multi-tenancy; it carries **no**
bearer token — authentication is the consumer's responsibility.

**Example app architecture:** same layered pattern per entity (Store, Customer,
Invoice, Billing, Balance) — `service (REST + GraphQL) → context → hook → page`.
Auth via `nauth-react` (NAuthProvider); GraphQL at `{API_BASE_URL}/graphql` for
reads, REST for mutations. Provider chain in `main.tsx`: NAuth → Store → Balance
→ Customer → Invoice → Billing.

**Key frontend patterns:**
- Library public API must stay stable — changes to `src/index.ts` exports are breaking changes
- No runtime dependencies — only React as peer dependency; full TypeScript strict mode
- Service classes export both a singleton (`camelCase`) and the class (`default`)
- Contexts include `loading`, `error` and data state; methods wrapped in `useCallback`
- Versioned with **GitVersion** (commit-prefix driven); published to npm via GitHub Actions

### API Endpoints (Backend)

**GraphQL (read operations):**
- `POST /graphql` — public schema (anonymous): stores, products, categories, storeBySlug, featuredProducts
- `POST /graphql/admin` — admin schema (authenticated): myStores, myProducts, myCategories, myOrders

**REST — Store** (`/store`):
- `POST /store/insert` — [Authorize] create store
- `POST /store/update` — [Authorize] update store
- `POST /store/uploadLogo/{storeId}` — [Authorize] upload logo (100MB limit)
- `DELETE /store/delete/{storeId}` — [Authorize] delete store

**REST — Product** (`/product`):
- `POST /product/{storeSlug}/insert` — [Authorize] create product
- `POST /product/{storeSlug}/update` — [Authorize] update product
- `POST /product/search` — public product search with pagination

**REST — Category** (`/category`):
- `POST /category/{storeSlug}/insert` — [Authorize] create category
- `POST /category/{storeSlug}/update` — [Authorize] update category
- `DELETE /category/{storeSlug}/delete/{categoryId}` — [Authorize] delete category

**REST — Order** (`/order`):
- `POST /order/update` — [Authorize] update order
- `POST /order/search` — [Authorize] search orders with pagination
- `POST /order/list` — [Authorize] list orders by store/user/status
- `GET /order/getById/{orderId}` — [Authorize] get order by ID

**REST — Image** (`/image`):
- `POST /image/upload/{productId}` — [Authorize] upload product image (100MB limit)
- `GET /image/list/{productId}` — [Authorize] list images for product
- `DELETE /image/delete/{imageId}` — [Authorize] delete image

**REST — StoreUser** (`/storeuser`):
- `GET /storeuser/{storeSlug}/list` — [Authorize] list store members
- `POST /storeuser/{storeSlug}/insert` — [Authorize] add user to store
- `DELETE /storeuser/{storeSlug}/delete/{storeUserId}` — [Authorize] remove user from store

**Other:**
- `GET /` — health check
- `/swagger/ui` — Swagger UI (dev/docker only)

### Example app routing (`frontend/example-app`)
- Public: `/` (Home), `/docs`, `/demo/pix`, `/demo/invoice`, `/demo/billing`
- Admin: `/admin/dashboard`, `/admin/store`, `/admin/customers`, `/admin/invoices`, `/admin/billings`

## Environment

### Backend (`backend/`)
- **Dev API URL:** `https://localhost:44374`
- **Prod API URL:** configured in `.env.production`
- **Database:** PostgreSQL (one physical database per tenant)

### Frontend example app (`frontend/example-app/.env`)
```
VITE_API_BASE_URL     # Backend API URL
VITE_NAUTH_API_URL    # Auth service URL
VITE_CLIENT_ID        # ProxyPay client identifier
VITE_TENANT_ID        # Multi-tenant identifier
```

## Conventions

### Backend
- See `dotnet-architecture` skill — Clean Architecture layering across `backend/` projects
- PostgreSQL `snake_case` tables/columns; EF Core is the only ORM

### Frontend (`proxypay-react`)
- Library follows `types → services → contexts → hooks → components`; all exports go through `src/index.ts`
- Changes to `src/index.ts` exports are breaking changes (the public API must stay stable)
- No runtime dependencies — only React as peer dependency; full TypeScript strict mode
- The `example-app` consumes the library via `"proxypay-react": "file:.."` — run `npm run build` in `frontend/` before testing the example against local changes
