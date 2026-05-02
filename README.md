# 💆‍♀️ Aesthetic Clinic Management System API

A modular, enterprise‑grade backend REST API built with **ASP.NET Core 8**, **Entity Framework Core**, and **SQLite/SQL Server**.  
Designed as a **modular monolith** – demonstrates clean architecture, DDD patterns, real‑world clinic management features, signal‑based state transitions, background job scheduling, file uploads, and comprehensive logging.

---

## ✨ Features

### Core Modules
- **Clients** – CRUD, soft delete, skin history, allergies
- **Treatments** – service catalog (facial, laser, botox, injectables)
- **Appointments** – booking, rescheduling, staff assignment, status flow (Scheduled → Confirmed → Completed / Cancelled / NoShow)
- **Billing** – invoices, payments, invoice status management
- **Photos** – before/after treatment images (local storage – ready for Cloudinary/AWS S3)

### Notification System (Event‑Driven)
- **Notification Templates** – reusable message formats with placeholders
- **Notifications** – in‑app messages generated automatically
- **NotifyLog** – full audit trail (status, channel, provider response, retry)
- **Email / SMS / Push** – pluggable channels (email via SMTP, SMS/Push stubbed)

### Advanced Backend Features
- **Signal System** – EF Core interceptor that dispatches `OnCreatedAsync`, `OnUpdatedAsync`, `OnStatusChangedAsync`, `OnActiveChangedAsync`, `OnRevokedChangedAsync` – no manual event calls.
- **Background Service** – weekly AI prediction report generation (scheduled via `BackgroundService`).
- **Dashboard Statistics** – advanced KPIs, revenue trends, top services, appointment funnel, client retention, staff performance, and forecast.
- **File Upload** – local storage (`wwwroot/uploads/photos/`) with automatic GUID renaming.
- **JWT Authentication** with refresh token rotation (roles: `Admin`, `Staff`, `Client`).
- **Global Exception Handling** – unified `ApiResponse<T>` format.
- **FluentValidation** – automatic DTO validation.
- **Serilog** – structured logging to console and rolling file.
- **CORS** – configured for React frontend (development: `http://localhost:5173`).
- **Seed Data** – automatic database seeding on development startup (roles, admin user, treatments, staff).

### Tech Stack

| Layer            | Technology |
|------------------|------------|
| Framework        | ASP.NET Core 8 Web API |
| ORM              | Entity Framework Core 8 (SQLite / SQL Server) |
| Authentication   | JWT Bearer + refresh tokens |
| Validation       | FluentValidation |
| Logging          | Serilog (console + file) |
| Background       | `BackgroundService` (no external dependencies) |
| API Documentation| Swagger / OpenAPI |
| Testing          | xUnit + Moq (unit & integration) |
| CORS             | Enabled for React |

---

## 📁 Project Structure (Modular Monolith)

```
Aesthetic-Clinic-Management/
├── Controllers/                # cross‑cutting endpoints (Dashboard)
├── Modules/
│   ├── Clients/                # Client module
│   ├── Treatments/             # Treatment catalog
│   ├── Appointments/           # Appointment + state transitions
│   ├── Billing/                # Invoices, payments
│   ├── Notifications/          # Templates, Notifications, Logs, Channels
│   ├── Photos/                 # Photo upload & serving
│   ├── Reports/                # ReportLog (AI weekly reports)
│   ├── Staff/                  # Staff management
│   ├── Authentications/        # Users, roles, refresh tokens
├── Shared/                     # Shared kernel (BaseEntity, ApiResponse, ServiceResult, IStateTransitionService)
├── Data/                       # DbContext & migrations
├── Middleware/                 # GlobalExceptionMiddleware, ModelChangeInterceptor
├── BackgroundServices/         # WeeklyReportBackgroundService
├── Database/Seeders/           # Role, User, Treatment, Staff seeders
├── wwwroot/uploads/photos/     # Uploaded photo files
└── Program.cs
```

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [SQLite](https://www.sqlite.org/) (included, or switch to SQL Server)
- [Git](https://git-scm.com/)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/CyberArcenal/Aesthetic-Clinic-Management.git
   cd Aesthetic-Clinic-Management/AestheticClinicAPI
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure `appsettings.json`**  
   - Set your JWT secret key (at least 32 characters).
   - (Optional) Configure SMTP for real emails.
   - The default connection string uses SQLite (file `AestheticClinic.db`).

4. **Apply migrations and create database**
   ```bash
   dotnet ef database update
   ```

5. **Run the API**
   ```bash
   dotnet run
   # or watch mode
   dotnet watch run
   ```

6. **Open Swagger UI**  
   Navigate to `https://localhost:5001/swagger` (or `http://localhost:5000/swagger`).

> **Note:** On first run in Development environment, the database will be seeded with:
> - Roles: `Admin`, `Staff`, `Client`
> - Admin user: `admin` / `Admin123!`
> - Sample treatments (HydraFacial, Botox, Laser, etc.)
> - Sample staff members (optional)

---

## 🔐 Authentication (JWT)

The API uses JWT bearer tokens with refresh token rotation.

### Default admin credentials
- **Username:** `admin`
- **Password:** `Admin123!`

### Endpoints
- `POST /api/v1/auth/register` – create a new user (role `Client` automatically)
- `POST /api/v1/auth/login` – obtain access token + refresh token
- `POST /api/v1/auth/refresh` – get new access token using refresh token
- `POST /api/v1/auth/logout` – revoke all refresh tokens
- `GET /api/v1/auth/me` – get current user info
- `POST /api/v1/auth/change-password` – change password

**Swagger Authorization:** Click the `Authorize` button and enter `Bearer <your-token>`.

---

## 📚 Key API Endpoints

| Module        | Method | Endpoint                         | Description                     |
|---------------|--------|----------------------------------|---------------------------------|
| Clients       | GET    | `/api/v1/clients`                | Paginated clients               |
| Clients       | POST   | `/api/v1/clients`                | Create client                   |
| Treatments    | GET    | `/api/v1/treatments`             | List all treatments             |
| Appointments  | POST   | `/api/v1/appointments`           | Book appointment                |
| Appointments  | PATCH  | `/api/v1/appointments/{id}/status` | Update status (triggers signal)|
| Billing       | POST   | `/api/v1/invoices`               | Create invoice (auto‑generates number) |
| Billing       | POST   | `/api/v1/payments`               | Record payment (updates invoice status) |
| Photos        | POST   | `/api/v1/photos` (multipart/form‑data) | Upload photo (local storage)   |
| Dashboard     | GET    | `/api/v1/dashboard/stats`        | Enhanced dashboard statistics   |
| Dashboard     | GET    | `/api/v1/dashboard/enhanced-stats` | More detailed analytics        |

Full documentation available in Swagger.

---

## 🧪 Running Tests

The solution includes unit tests for all major services using xUnit and Moq.

```bash
dotnet test
```

To run tests for a specific module:
```bash
dotnet test --filter "FullyQualifiedName~AuthServiceTests"
dotnet test --filter "FullyQualifiedName~Billing"
```

---

## 🚢 Deployment (Production)

### Option 1: Deploy to Azure App Service (Linux or Windows)

1. **Build the project**
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. **Deploy using Azure CLI**
   ```bash
   az webapp create --resource-group <group> --plan <plan> --name <app-name> --runtime "DOTNET:8"
   az webapp deployment source config-zip --resource-group <group> --name <app-name> --src ./publish.zip
   ```

3. **Set environment variables** in Azure Portal or via CLI:
   - `ASPNETCORE_ENVIRONMENT=Production`
   - `ConnectionStrings__DefaultConnection` (SQL Server connection string)
   - `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience`
   - `Smtp__...` (if email is enabled)

4. **Enable `Always On`** in App Service settings so background services run continuously.

### Option 2: Deploy to a Windows Server with IIS

1. Publish to a folder.
2. Create an IIS Application Pool with **.NET CLR version: No Managed Code** and **Start Mode: AlwaysRunning**.
3. Set **Idle Time-out = 0**.
4. Point the website to the published folder and enable `web.config` (generated automatically).

### Option 3: Run as a Windows Service

You can use `Microsoft.Extensions.Hosting.WindowsServices` to run the API as a Windows Service.

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

CyberArcenal – [@Third1Dz](https://twitter.com/Third1Dz) – cyberarcenal1@gmail.com

Project Link: [https://github.com/CyberArcenal/Aesthetic-Clinic-Management](https://github.com/CyberArcenal/Aesthetic-Clinic-Management)

---

**Built with 💜 for aesthetic clinics.**