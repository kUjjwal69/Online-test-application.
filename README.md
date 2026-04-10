# 🎓 Online Test & Proctored Examination Platform — Backend API

A **production-ready ASP.NET Core Web API (.NET 8)** backend for an online proctored examination system with Admin and Candidate roles, JWT authentication, EF Core + SQL Server, screenshot/video proctoring, and auto-database seeding.

---

## 🧱 Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core Web API (.NET 8) |
| ORM | Entity Framework Core 8 |
| Database | SQL Server (LocalDB for dev) |
| Authentication | JWT Bearer (HS256) |
| Password Hashing | BCrypt.Net-Next |
| API Documentation | Swashbuckle / Swagger UI |
| Architecture | Clean Layered (Controller → Service → Repository → DB) |

---

## 📁 Project Structure

```
TestManagementApplication/
├── Controllers/
│   ├── AuthController.cs         → POST /api/auth/register, /login
│   ├── AdminController.cs        → /api/admin/** (Admin only)
│   ├── CandidateController.cs    → /api/candidate/** (User only)
│   └── ProctoringController.cs   → /api/proctoring/** (User only)
├── Services/
│   ├── Interfaces/IServices.cs
│   └── Implementations/          → AuthService, AdminService, CandidateService, ProctoringService
├── Repositories/
│   ├── Interfaces/IRepositories.cs
│   └── Implementations/Repositories.cs
├── Models/
│   ├── Entities/                 → User, Test, Question, TestAssignment,
│   │                               TestSession, UserAnswer, Violation,
│   │                               CapturedImage, VideoRecording
│   └── DTOs/
│       ├── Auth/AuthDtos.cs
│       ├── Admin/AdminDtos.cs
│       ├── Candidate/CandidateDtos.cs
│       └── Proctoring/ProctoringDtos.cs
├── Data/
│   ├── AppDbContext.cs            → EF Core context with Fluent API config
│   ├── DataSeeder.cs             → Auto-seeds Admin + sample data
│   └── Migrations/               → EF Core InitialCreate migration
├── Helpers/
│   └── JwtHelper.cs              → Token generation + claim extraction
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs
├── Common/
│   └── ApiResponse.cs            → Generic API wrapper
├── wwwroot/
│   └── uploads/
│       ├── screenshots/          → Captured proctoring images
│       └── videos/               → Recorded session videos
├── appsettings.json
├── appsettings.Development.json
└── Program.cs                    → Full DI + Swagger + Middleware bootstrap
```

---

## 🚀 How to Run Locally

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- SQL Server or SQL Server LocalDB (included with Visual Studio)

### Step 1 — Configure the Connection String

Edit `appsettings.json` (or `appsettings.Development.json`) and update:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=TestManagementDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

> **Full SQL Server:** `Server=YOUR_SERVER;Database=TestManagementDb;User Id=sa;Password=YOUR_PASS;TrustServerCertificate=True`

### Step 2 — Restore & Run

```bash
# Restore packages
dotnet restore

# Run the app (migrations + seeding run automatically on startup)
dotnet run
```

The app will:
1. ✅ Apply EF Core migrations automatically
2. ✅ Seed the default Admin user
3. ✅ Seed sample test data (C# quiz + candidate)
4. ✅ Open Swagger UI at `http://localhost:5000`

### Step 3 — Open Swagger UI

Navigate to: **`http://localhost:5000`**

---

## 🔐 Authentication

### Default Seeded Credentials

| Role | Username | Password |
|------|----------|----------|
| **Admin** | `admin` | `Admin@123` |
| **Candidate** | `john_doe` | `User@123` |

### How to Authenticate in Swagger

1. Call `POST /api/auth/login` with credentials above
2. Copy the `token` value from the response
3. Click the **🔓 Authorize** button (top-right in Swagger)
4. Enter: `Bearer <your-token>`
5. Click **Authorize** → all protected endpoints will now work

---

## 📡 Complete API Reference

### 🔑 Auth (`/api/auth`)
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/auth/register` | Register a new candidate | ❌ Public |
| POST | `/api/auth/login` | Login and get JWT token | ❌ Public |

### 👤 Admin (`/api/admin`) — Requires `Admin` role

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/admin/tests` | List all tests |
| POST | `/api/admin/tests` | Create a test |
| PUT | `/api/admin/tests/{testId}` | Update a test |
| DELETE | `/api/admin/tests/{testId}` | Delete a test |
| GET | `/api/admin/tests/{testId}/questions` | Get questions for a test |
| POST | `/api/admin/tests/{testId}/questions` | Add a question |
| PUT | `/api/admin/questions/{questionId}` | Update a question |
| DELETE | `/api/admin/questions/{questionId}` | Delete a question |
| POST | `/api/admin/tests/{testId}/assign` | Assign test to a candidate |
| GET | `/api/admin/sessions` | View all test sessions |
| GET | `/api/admin/sessions/{sessionId}` | View a specific session |
| POST | `/api/admin/sessions/{sessionId}/suspend` | Manually suspend a session |
| GET | `/api/admin/sessions/{sessionId}/violations` | View violations for a session |
| GET | `/api/admin/sessions/{sessionId}/screenshots` | View screenshots for a session |
| GET | `/api/admin/users` | List all candidates |

### 🎓 Candidate (`/api/candidate`) — Requires `User` role

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/candidate/tests` | View assigned tests |
| POST | `/api/candidate/tests/{testId}/start` | Start a test (creates session) |
| GET | `/api/candidate/sessions/{sessionId}/questions` | Fetch questions (no answers exposed) |
| POST | `/api/candidate/sessions/{sessionId}/answers` | Submit / update an answer |
| POST | `/api/candidate/sessions/{sessionId}/submit` | Submit test and get result |
| GET | `/api/candidate/sessions/{sessionId}/result` | View result of completed test |

### 🎥 Proctoring (`/api/proctoring`) — Requires `User` role

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/proctoring/sessions/{sessionId}/screenshot` | Upload Base64 screenshot |
| POST | `/api/proctoring/sessions/{sessionId}/violation` | Report violation event |
| POST | `/api/proctoring/sessions/{sessionId}/video/chunk` | Upload a video chunk (multipart) |
| POST | `/api/proctoring/sessions/{sessionId}/video/complete` | Finalize and merge video |

---

## 🗄️ Database Schema

```
Users ──────────────────── Tests
  │                          │
  │                     Questions
  │
  ├──── TestAssignments (User ↔ Test)
  │
  └──── TestSessions
            │
            ├── UserAnswers
            ├── Violations
            ├── CapturedImages
            └── VideoRecordings
```

---

## 🎥 Proctoring Features

### Screenshot Upload
- Send `imageBase64` (string) in the request body
- Supports `data:image/png;base64,...` prefix automatically stripped
- Saved to `wwwroot/uploads/screenshots/{sessionId}/{guid}.png`

### Violation Tracking
- Types: `TabSwitch`, `WindowBlur`, `FullscreenExit`
- Session auto-suspended when violation count ≥ `ViolationThreshold` (default: 3)

### Video Recording (Chunked Upload)
1. Upload first chunk → get `recordingId`
2. Upload subsequent chunks → pass `recordingId` + incrementing `chunkIndex`
3. Call `/video/complete` → server merges all chunks into one `.webm` file
- Max chunk size: **50 MB**
- Types: `Screen` or `Webcam`

---

## 🧩 Manual Migration Commands (Optional)

If you want to manage migrations manually:

```bash
# Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef --version 8.0.0

# Add a new migration
dotnet-ef migrations add InitialCreate --output-dir Data/Migrations

# Apply migration to database
dotnet-ef database update

# Remove last migration
dotnet-ef migrations remove
```

> **Note:** Migrations are applied **automatically** on startup via `DataSeeder.SeedAsync()`. You don't need to run `database update` manually.

---

## 🔒 Security Features

| Feature | Implementation |
|---------|---------------|
| Password Storage | BCrypt.Net-Next (work factor 10) |
| Token Auth | JWT HS256, 24-hour expiry |
| Role Enforcement | `[Authorize(Roles = "Admin")]` / `"User"` |
| Input Validation | Service-layer guards with descriptive exceptions |
| File Upload Safety | Base64 decode + whitelist extension check |
| Error Handling | Global middleware — no stack traces in responses |
| HTTPS | Enforced via `UseHttpsRedirection()` |

---

## ⚙️ Configuration Reference (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "<your-sql-server-connection-string>"
  },
  "JwtSettings": {
    "SecretKey": "<minimum-32-character-secret>",
    "Issuer": "TestManagementPlatform",
    "Audience": "TestManagementClients",
    "ExpiryMinutes": "1440"
  }
}
```

---

## ✅ Full Exam Flow (End-to-End)

```
1. Candidate registers → POST /api/auth/register
2. Candidate logs in   → POST /api/auth/login          → receives JWT
3. View assigned tests → GET  /api/candidate/tests
4. Start test          → POST /api/candidate/tests/{id}/start  → receives sessionId
5. Fetch questions     → GET  /api/candidate/sessions/{sessionId}/questions
6. Submit answers      → POST /api/candidate/sessions/{sessionId}/answers  (repeat per question)
7. Upload screenshots  → POST /api/proctoring/sessions/{sessionId}/screenshot
8. Report violations   → POST /api/proctoring/sessions/{sessionId}/violation
9. Upload video        → POST /api/proctoring/sessions/{sessionId}/video/chunk  (repeat)
                       → POST /api/proctoring/sessions/{sessionId}/video/complete
10. Submit test        → POST /api/candidate/sessions/{sessionId}/submit  → score + % returned
11. View result        → GET  /api/candidate/sessions/{sessionId}/result
```
