# Ganesha - Financial Module Backend

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![EF Core](https://img.shields.io/badge/EF%20Core-9.0-512BD4)
![HotChocolate](https://img.shields.io/badge/HotChocolate-14.3-E535AB)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17-336791)
![License](https://img.shields.io/badge/License-MIT-green)

## Overview

**Ganesha** is a multi-tenant financial module backend built with **.NET 8** following **Clean Architecture** principles. It provides a **GraphQL API** (via HotChocolate) for read operations and **REST API** for write operations, managing stores and invoices. Supports payment processing via Stripe, file storage with AWS S3, and delegated authentication through NAuth.

The solution is organized into 8 layered projects with clear dependency boundaries, including a dedicated **Ganesha.GraphQL** project for all GraphQL schemas, queries, and type extensions.

---

## 🚀 Features

- 🏪 **Multi-Tenant Networks** - Support for multiple networks with isolated data via TenantResolver
- 🧾 **Invoice Management** - Full CRUD with automatic number generation, item management, and status tracking
- 🏷️ **Store Management** - Store CRUD with slug generation and logo upload
- 🔍 **GraphQL API** - HotChocolate-powered queries with projection, filtering, and sorting
- 📡 **REST API** - Write operations with Swagger documentation
- 💳 **Stripe Integration** - Payment processing
- 🔐 **NAuth Authentication** - Delegated Bearer token authentication via custom handler
- ☁️ **AWS S3 Storage** - File upload and signed URL resolution
- 🔧 **zTools Integration** - AI, email sending, file management utilities
- 📄 **Swagger/OpenAPI** - Auto-generated API documentation
- 🐳 **Docker Support** - Docker Compose for development and production
- 🔄 **CI/CD** - GitHub Actions for versioning, releases, NuGet publishing, and deployment

---

## 🛠️ Technologies Used

### Core Framework
- **.NET 8** - Backend runtime and SDK
- **ASP.NET Core** - Web API framework

### GraphQL
- **HotChocolate 14.3** - GraphQL server with projection, filtering, and sorting
- **HotChocolate.Data.EntityFramework** - EF Core integration for IQueryable resolvers

### Database
- **PostgreSQL 17** - Primary relational database
- **Entity Framework Core 9** - ORM with lazy loading proxies
- **Npgsql** - PostgreSQL provider for EF Core

### Security
- **NAuth** - External authentication service integration
- **Custom RemoteAuthHandler** - Bearer token validation middleware

### Payments & Cloud
- **Stripe.net** - Payment gateway integration
- **AWSSDK.S3** - Amazon S3 file storage
- **SixLabors.ImageSharp** - Image processing

### Additional Libraries
- **Newtonsoft.Json** - JSON serialization
- **Swashbuckle** - Swagger/OpenAPI documentation

### Testing
- **xUnit 2.9** - Test framework
- **Moq 4.20** - Mocking library
- **coverlet** - Code coverage

### DevOps
- **Docker** - Containerization with multi-stage builds
- **Docker Compose** - Service orchestration (API + PostgreSQL)
- **GitHub Actions** - CI/CD pipelines (versioning, NuGet publishing, releases, deployment)
- **GitVersion** - Semantic versioning from conventional commits

---

## 📁 Project Structure

```
Ganesha/
├── Ganesha.API/                    # Web API entry point
│   ├── Controllers/             # REST controllers (Store, Invoice)
│   ├── Middlewares/             # TenantMiddleware
│   ├── Startup.cs               # DI, auth, CORS, Swagger, GraphQL endpoints
│   └── Dockerfile               # Multi-stage Docker build
├── Ganesha.GraphQL/                # GraphQL schemas and resolvers
│   ├── Public/                  # Public queries (stores, storeBySlug)
│   ├── Admin/                   # Authenticated queries (myStores, myInvoices)
│   ├── Types/                   # Type extensions (logoUrl, invoice items)
│   ├── GraphQLServiceExtensions.cs  # Schema registration
│   └── GraphQLErrorLogger.cs    # Error diagnostics
├── Ganesha.Application/            # DI bootstrap (ConfigureGanesha)
├── Ganesha.Domain/                 # Business logic layer
│   ├── Interfaces/              # Service contracts
│   ├── Models/                  # Domain models (Store, Invoice, InvoiceItem)
│   ├── Services/                # Service implementations
│   └── Mappers/                 # Model ↔ DTO mappers
├── Ganesha/                        # Shared package (DTOs + ACL)
│   ├── ACL/                     # Anti-Corruption Layer (external API clients)
│   └── DTO/                     # Data Transfer Objects (Store, Invoice)
├── Ganesha.Infra.Interfaces/      # Repository interfaces (IUnitOfWork, IRepository)
├── Ganesha.Infra/                  # Infrastructure implementation
│   ├── Context/                 # EF Core DbContext + entities
│   └── Repository/              # Repository implementations
├── Ganesha.Tests/                  # Unit tests (xUnit + Moq)
├── bruno-collection/            # Bruno API testing collection
├── scripts/                     # Seed scripts
├── docs/                        # Documentation
├── docker-compose.yml           # Development (API + PostgreSQL)
├── docker-compose-prod.yml      # Production
├── ganesha.sql                  # Database creation script
├── .github/workflows/           # CI/CD (versioning, NuGet, releases, deploy)
├── GitVersion.yml               # Semantic versioning config
├── Ganesha.sln                  # Solution file
└── README.md                    # This file
```

---

## 🏗️ System Design

The following diagram illustrates the high-level architecture of **Ganesha**:

![System Design](docs/system-design.png)

**Dependency flow:** `API / GraphQL → Application → Domain → Infra → PostgreSQL`

- **Ganesha.API** receives REST requests and delegates to Domain services
- **Ganesha.GraphQL** handles GraphQL queries directly against EF Core DbContext with type extensions for computed fields
- **Ganesha.Application** bootstraps all DI registrations via `ConfigureGanesha()` and manages multi-tenant context
- **Ganesha.Domain** contains business rules, service implementations, and domain models
- **Ganesha (Shared)** provides DTOs and ACL clients for external consumers
- **Ganesha.Infra** implements repositories using EF Core 9 with PostgreSQL

> 📄 **Source:** The editable Mermaid source is available at [`docs/system-design.mmd`](docs/system-design.mmd).

---

## 📖 Additional Documentation

| Document | Description |
|----------|-------------|
| [API_REFERENCE.md](docs/API_REFERENCE.md) | Complete REST and GraphQL API reference with all endpoints, DTOs, enums, and examples |

---

## ⚙️ Environment Configuration

Before running the application, configure the environment variables:

### 1. Copy the environment template

```bash
cp .env.example .env
```

### 2. Edit the `.env` file

```bash
# Database
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_password_here
POSTGRES_DB=ganesha

# Tenant: emagine
EMAGINE_CONNECTION_STRING=Host=db;Port=5432;Database=ganesha;Username=postgres;Password=your_password_here
EMAGINE_JWT_SECRET=dev-jwt-secret-min-32-chars-long-here

# Tenant: monexup
MONEXUP_CONNECTION_STRING=Host=db;Port=5432;Database=ganesha;Username=postgres;Password=your_password_here
MONEXUP_JWT_SECRET=dev-jwt-secret-min-32-chars-long-here

# Ganesha
GANESHA_BUCKET_NAME=ganesha

# NAuth
NAUTH_API_URL=https://your-nauth-url/auth-api
NAUTH_BUCKET_NAME=nauth

# zTools
ZTOOLS_API_URL=https://your-ztools-url/tools-api

# App
APP_PORT=5000
```

⚠️ **IMPORTANT**:
- Never commit the `.env` file with real credentials
- Only `.env.example` and `.env.prod.example` should be version controlled
- Change all default passwords and secrets before deployment

---

## 🐳 Docker Setup

### Quick Start with Docker Compose

#### 1. Prerequisites

```bash
# Create the external Docker network (shared with other services)
docker network create emagine-network
```

#### 2. Build and Start Services

```bash
docker-compose up -d --build
```

This starts:
- **ganesha-api** - .NET 8 API on port 5000 (configurable via `APP_PORT`)
- **ganesha-db** - PostgreSQL 17 on port 5432

#### 3. Verify Deployment

```bash
docker-compose ps
docker-compose logs -f api
```

### Accessing the Application

| Service | URL |
|---------|-----|
| **API** | `http://localhost:5000` |
| **Swagger UI** | `http://localhost:5000/swagger/ui` |
| **GraphQL Playground** | `http://localhost:5000/graphql` |
| **GraphQL Admin** | `http://localhost:5000/graphql/admin` |
| **Health Check** | `http://localhost:5000/` |

### Docker Compose Commands

| Action | Command |
|--------|---------|
| Start services | `docker-compose up -d` |
| Start with rebuild | `docker-compose up -d --build` |
| Stop services | `docker-compose stop` |
| View status | `docker-compose ps` |
| View logs | `docker-compose logs -f` |
| Remove containers | `docker-compose down` |
| Remove containers and volumes | `docker-compose down -v` |

### Production Deployment

```bash
docker-compose -f docker-compose-prod.yml up -d --build
```

---

## 🔧 Manual Setup (Without Docker)

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 17+](https://www.postgresql.org/download/)

### Setup Steps

#### 1. Clone the repository

```bash
git clone https://github.com/emaginebr/Ganesha.git
cd Ganesha
```

#### 2. Create the database

```bash
psql -U postgres -c "CREATE DATABASE ganesha;"
psql -U postgres -d ganesha -f ganesha.sql
```

#### 3. Build the solution

```bash
dotnet build Ganesha.sln
```

#### 4. Run the API

```bash
dotnet run --project Ganesha.API
```

The API will be available at `https://localhost:44374`.

---

## 🧪 Testing

### Running Tests

**All Tests:**
```bash
dotnet test Ganesha.sln
```

**With Coverage:**
```bash
dotnet test Ganesha.sln --collect:"XPlat Code Coverage"
```

### Test Structure

```
Ganesha.Tests/
├── Domain/
│   ├── Mappers/         # DTO ↔ Model mapping tests
│   │   └── StoreMapperTest.cs
│   └── Services/        # Business logic tests
│       └── StoreServiceTest.cs
```

---

## 📚 API Documentation

### Authentication Flow

```
1. Client sends Bearer token → 2. RemoteAuthHandler validates via NAuth → 3. TenantMiddleware resolves tenant → 4. Request processed
```

### GraphQL Endpoints (Read Operations)

| Endpoint | Auth | Queries |
|----------|------|---------|
| `POST /graphql` | No | `stores`, `storeBySlug` |
| `POST /graphql/admin` | Yes | `myStores`, `myInvoices` |

**Example query:**
```graphql
{
  myInvoices {
    items {
      invoiceId
      invoiceNumber
      status
      total
      dueDate
      items {
        description
        quantity
        unitPrice
        total
      }
    }
  }
}
```

### REST Endpoints (Write Operations)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/store/insert` | Create store | Yes |
| POST | `/store/update` | Update store | Yes |
| POST | `/store/uploadLogo/{storeId}` | Upload logo | Yes |
| DELETE | `/store/delete/{storeId}` | Delete store | Yes |
| POST | `/invoice/insert` | Create invoice with items | Yes |
| POST | `/invoice/update` | Update invoice with items | Yes |
| GET | `/invoice/list` | List user invoices | Yes |
| GET | `/invoice/getById/{invoiceId}` | Get invoice by ID | Yes |
| DELETE | `/invoice/delete/{invoiceId}` | Delete invoice | Yes |

> Full interactive documentation available at `/swagger/ui` when running the API.

> Complete API reference at [`docs/API_REFERENCE.md`](docs/API_REFERENCE.md).

---

## 🔒 Security Features

### Authentication
- **Bearer Token** - All protected endpoints require a valid Bearer token
- **Remote Validation** - Tokens are validated against the NAuth external service
- **Session Management** - User context (userId, session) maintained per request

### Infrastructure
- **Multi-Tenant Isolation** - Tenant-specific database connections and JWT secrets
- **CORS** - Configurable cross-origin resource sharing
- **Request Size Limits** - 100MB limit for file upload endpoints

---

## 💾 Database

### Schema Creation

```bash
psql -U postgres -d ganesha -f ganesha.sql
```

### Backup

```bash
pg_dump -U postgres ganesha > backup_ganesha_$(date +%Y%m%d).sql
```

### Restore

```bash
psql -U postgres -d ganesha < backup_ganesha_20260322.sql
```

---

## 🔄 CI/CD

### GitHub Actions

| Workflow | Trigger | Description |
|----------|---------|-------------|
| **Version & Tag** | Push to `main` | Creates semantic version tags using GitVersion |
| **Create Release** | After version tag | Creates GitHub releases for minor/major versions |
| **Publish NuGet** | After version tag | Builds and publishes the Ganesha NuGet package |
| **Deploy Prod** | Manual dispatch | Deploys to production via SSH |

**Versioning strategy** (GitVersion - ContinuousDelivery):
- `feat:` / `feature:` → Minor version bump
- `fix:` / `patch:` → Patch version bump
- `breaking:` / `major:` → Major version bump

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

### Development Setup

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Make your changes
4. Run tests (`dotnet test Ganesha.sln`)
5. Commit your changes using conventional commits (`git commit -m 'feat: add some AmazingFeature'`)
6. Push to the branch (`git push origin feature/AmazingFeature`)
7. Open a Pull Request

### Coding Standards

- Follow Clean Architecture dependency rules
- Use conventional commits for semantic versioning
- All new endpoints must include authorization where appropriate
- Repository pattern for all data access
- Read operations via GraphQL, write operations via REST

---

## 👨‍💻 Author

Developed by **[Rodrigo Landim Carneiro](https://github.com/landim32)**

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- Built with [.NET 8](https://dotnet.microsoft.com/)
- GraphQL powered by [HotChocolate](https://chillicream.com/docs/hotchocolate)
- Database powered by [PostgreSQL](https://www.postgresql.org/)
- ORM by [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- Payments by [Stripe](https://stripe.com/)
- Image processing by [SixLabors ImageSharp](https://sixlabors.com/products/imagesharp/)

---

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/emaginebr/Ganesha/issues)
- **Discussions**: [GitHub Discussions](https://github.com/emaginebr/Ganesha/discussions)

---

**⭐ If you find this project useful, please consider giving it a star!**
