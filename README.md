# 💆‍♀️ Aesthetic Clinic Management System API

A modular, enterprise‑grade backend REST API built with **ASP.NET Core 8**, **Entity Framework Core**, and **SQL Server**.
Designed as a **modular monolith** – perfect for a portfolio that demonstrates clean architecture, DDD patterns, and real‑world clinic management features.

---

## ✨ Features

### Core Modules
- **Clients** – CRUD, soft delete, skin history, allergies
- **Treatments** – service catalog (facial, laser, botox, etc.)
- **Appointments** – booking, rescheduling, staff assignment
- **Billing** – invoices, packages, promo handling
- **Photos** – before/after treatment images (local/Azure blob)

### Notification System
- **Notification Templates** – reusable message formats
- **Notifications** – actual messages generated per event/client
- **NotifyLog** – audit trail (status, channel, provider response)
- Integration with appointments & billing

### Advanced Features
- JWT Authentication with role‑based access (Admin, Staff, Client)
- Soft delete & global query filters
- Generic repository + Unit of Work
- DTOs + AutoMapper (optional)
- Global exception handling & structured API responses
- Pagination, filtering, sorting on list endpoints
- Report exports (CSV/PDF)
- Swagger / OpenAPI documentation

### Tech Stack
| Layer            | Technology |
|------------------|------------|
| Framework        | ASP.NET Core 8 Web API |
| ORM              | Entity Framework Core 8 |
| Database         | SQL Server (localdb/Express/Azure) |
| Authentication   | JWT Bearer |
| Validation       | FluentValidation (optional) |
| Logging          | Serilog |
| API Documentation| Swagger / OpenAPI |
| Caching          | In‑Memory / Redis (optional) |
| Background Jobs  | Hangfire / IHostedService (for notifications) |

---

## 📁 Project Structure (Modular Monolith)

```
Aesthetic-Clinic-Management/
├── Controllers/            # API endpoints (grouped by module)
├── Modules/
│   ├── Clients/            # Client module
│   │   ├── Models/         # Entities & DTOs
│   │   ├── Repositories/   # Data access
│   │   ├── Services/       # Business logic
│   │   └── Controllers/    # API routes
│   ├── Treatments/
│   ├── Appointments/
│   ├── Billing/
│   ├── Notifications/
│   └── Shared/             # Shared kernel
│       ├── BaseEntity.cs
│       ├── IRepository.cs
│       ├── ApiResponse.cs
│       └── ServiceResult.cs
├── Data/                   # DbContext & migrations
├── Middleware/             # Global error handling
├── Extensions/             # DI & service configuration
└── Program.cs
```

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or SQL Server LocalDB)
- [Git](https://git-scm.com/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) / VS Code / Rider

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/CyberArcenal/Aesthetic-Clinic-Management.git
   cd Aesthetic-Clinic-Management
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Update connection string** in `appsettings.json`
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AestheticClinicDB;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```

4. **Apply migrations and create database**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

5. **Run the API**
   ```bash
   dotnet run
   ```

6. **Open Swagger UI**  
   Navigate to `https://localhost:5001/swagger` (or `http://localhost:5000/swagger`)

---

## 🔐 Authentication (to be implemented)

The API will use **JWT Bearer tokens**. Sample configuration:

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });
```

Roles: `Admin`, `Staff`, `Client`. Endpoints will be decorated with `[Authorize(Roles = "...")]`.

---

## 📚 API Endpoints (preview)

| Module      | Method | Endpoint                     | Description                |
|-------------|--------|------------------------------|----------------------------|
| Clients     | GET    | `/api/v1/clients`            | Get all clients (paginated)|
| Clients     | GET    | `/api/v1/clients/{id}`       | Get client by ID           |
| Clients     | POST   | `/api/v1/clients`            | Create new client          |
| Clients     | PUT    | `/api/v1/clients/{id}`       | Update client              |
| Clients     | DELETE | `/api/v1/clients/{id}`       | Soft delete client         |
| Treatments  | GET    | `/api/v1/treatments`         | List treatments            |
| Appointments| POST   | `/api/v1/appointments`       | Book an appointment        |
| ...         | ...    | ...                          | ...                        |

Full Swagger documentation available after running the project.

---

## 🧪 Testing

Run unit tests (when implemented):

```bash
dotnet test
```

---

## 🤝 Contributing

1. Fork the repository.
2. Create a feature branch (`git checkout -b feature/amazing-feature`).
3. Commit your changes (`git commit -m 'Add some feature'`).
4. Push to the branch (`git push origin feature/amazing-feature`).
5. Open a Pull Request.

---

## 📄 License

This project is licensed under the **GNU General Public License v3.0** – see the [LICENSE](LICENSE) file for details.

---

## 📬 Contact

Your Name – [@Third1Dz](https://twitter.com/Third1Dz) – cyberarcenal1@gmail.com

Project Link: [https://github.com/CyberArcenal/Aesthetic-Clinic-Management](https://github.com/CyberArcenal/Aesthetic-Clinic-Management)

---

**Built with 💜 for aesthetic clinics.**
```