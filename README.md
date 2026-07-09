# ToDoApp - Enterprise Task Manager Platform

A high-performance Fullstack task management application built using the latest Microsoft .NET technologies and advanced architectural patterns. This repository showcases a production-ready enterprise solution focusing on security, reactive state management, clean boundaries, and dynamic localization.

Dashboard
<img width="1904" height="944" alt="slika" src="https://github.com/user-attachments/assets/965325f6-2b45-4fe6-9d8f-f6b300f59399" />

List of Todo's
<img width="1898" height="942" alt="slika" src="https://github.com/user-attachments/assets/6cf06a40-8111-4d63-b9c6-0c043c5319dd" />

Task Details with subtasks:
<img width="1909" height="948" alt="slika" src="https://github.com/user-attachments/assets/6ac7bf9d-01ae-47fe-bb03-f32ae8da028f" />

## 🏗️ Architecture & Technology Stack

The application is engineered strictly on the principles of **Clean Architecture** and separation of concerns. The system decouples backend service orchestration from a highly responsive Frontend Single Page Application (SPA).

### Backend (Web API)
* **Core Framework:** .NET 10.0 Core Web API utilizing optimized **Minimal Endpoints**
* **Persistence Layer:** Entity Framework Core with Microsoft SQL Server or PostgreSQL integration (optimized tracking models).
* **Identity & Security:** ASP.NET Core Identity engine coupled with full **JWT Bearer token** authentication and role-based authorization policy gates
* **API Versioning:** Robust endpoint contract versioning via `Asp.Versioning.Http` supporting URL segment transitions (`/api/v1/todos`), custom HTTP headers (`x-api-version`), and query parameters
* **Data Validation:** Strict inbound payload filtration via pipeline-integrated `FluentValidation`
* **Error Resilience:** Centralized middleware exception capture (`GlobalExceptionHandler`) emitting rich, standardized RFC 7807 `ProblemDetails` structures
* **Interactive Documentation:** Native Integration with `.NET OpenApi` rendering structured metadata discoverable via Swagger/OpenAPI specifications

### Frontend (Client UI)
* **Core Framework:** Blazor WebAssembly (WASM SPA)
* **Ecosystem UI:** MudBlazor component wrapper implementation providing comprehensive Material Design primitives
* **State Management:** Tailored asynchronous **AppState Event-Driven Pattern** decoupling application states from UI lifecycles
* **Dynamic Localization (i18n):** Multi-language satelite orchestration (Slovenian, English, German) utilizing compiled `.resx` resources exposed via public code-generators, enabling instant text switching in real time without forcing an assembly reload
* **Local Storage Storage:** Transparent client state serialization (Theme toggles, selected cultures, drawer positions) into browser persistent storage via a custom unified `IDataStorage` interface

---

## 📂 Solution Structural Topography

```text
📂 ToDoApp
├── 📂 ToDoApp.WebApi               # Executable Host / API Bootstrap layer (Program.cs, Middleware pipeline)
├── 📂 ToDoApp.Application          # Agnostic Use Cases, service orchestrations, DTO mappers
├── 📂 ToDoApp.Domain               # Pure domain specifications, decoupled database schemas, entities
├── 📂 ToDoApp.Infrastructure       # Infrastructure blueprints (ApplicationDbContext, Repositories, JWT Engines)
├── 📂 ToDoApp.Shared               # DTOs which are used on backend and frontend (with fluent validation).
├── 📂 ToDoApp.Web                  # Client-side Blazor WebAssembly (Web) bootstrapper (Client-Program.cs)
├── 📂 ToDoApp.Maui                 # Client-side Blazor WebAssembly (Mobile) bootstrapper (Client-Program.cs)
└── 📂 ToDoApp.SharedUI             # Visual layout domain (Razor components, AppState manager, .resx files). Used for mobile and web.
```
⚡ Key Architecture & Implementation Details
1. High-Fidelity Reactive Localization

By extracting target structures from standard .resx resource manager files via custom factory adapters inside the AppState, the user interface instantly renders text shifts using explicit context lookups (@State.L("Key")). This fully matches the seamless behavior of theme transitions without triggering typical Blazor client-side pipeline crashes.

2. Resilient API Exception Interception

The system includes a dedicated global middleware wrapper handling unexpected data exceptions. Unhandled technical faults are parsed away from client exposure, turning database timeouts, validation failures, and illegal cross-origin mutations into precise JSON error models.

🚀 Local Deployment Checklist
Environmental Primitives

    .NET 10.0 Software Development Kit (SDK)

    MS SQL Server Instance (LocalDB or Docker Container instance) or PostgreSQL

    Microsoft Visual Studio 2022 (v17.12+) or Visual Studio Code

1. Database Connection and Configuration

Navigate into ToDoApp.API and modify appsettings.json. Align target local connection parameters and authorize Cross-Origin Resource Sharing (CORS) access endpoints:


```
{
  "ConnectionStrings": {
    "DbConnection": "Server=YOUR_SQL_INSTANCE;Database=ToDoAppDb;Trusted_Connection=True;TrustServerCertificate=True;"
    "PostgresConnection": "Host=servername;Port=port;Database=db;Username=username;Password=password",
  },
  "AllowedOrigins": [ "https://localhost:7176" ],
  "Jwt": {
    "Issuer": "ToDoAppBackend",
    "Audience": "ToDoAppFrontend",
    "Key": "YourSecure32ByteSymmetricSigningKeyMustBeConfiguredHere!"
  }
}
```
2. Running Schema Migrations

Execute the target database mapping parameters directly through the Package Manager Console to set up your entity graphs:

```
Update-Database -Project ToDoApp.Infrastructure -StartupProject ToDoApp.API
```

3. Orchestrating Multi-Project Launch

Configure Multiple Startup Projects inside your solution properties or execute simultaneously from separate terminal contexts:

```
# Terminal 1 - API Bootstrapper
dotnet run --project ToDoApp.API

# Terminal 2 - Blazor WebAssembly Client
dotnet run --project ToDoApp.WasmClient
```
📝 License

This system project is built for educational demonstration purposes, validating high-fidelity clean architecture paradigms within modern C# .NET environments.
