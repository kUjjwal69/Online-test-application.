using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TestManagementApplication.Data;
using TestManagementApplication.Helpers;
using TestManagementApplication.Middleware;
using TestManagementApplication.Repositories.Implementations;
using TestManagementApplication.Repositories.Interfaces;
using TestManagementApplication.Services.Implementations;
using TestManagementApplication.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════════
//  DATABASE
// ═══════════════════════════════════════════════════════════════
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null)));

// ═══════════════════════════════════════════════════════════════
//  JWT AUTHENTICATION
// ═══════════════════════════════════════════════════════════════
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey is not configured.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                success = false,
                message = "Unauthorized. Please provide a valid JWT token.",
                errors = new[] { "Authentication required." }
            });
            return context.Response.WriteAsync(result);
        },
        OnForbidden = context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                success = false,
                message = "Forbidden. You do not have permission to access this resource.",
                errors = new[] { "Insufficient role." }
            });
            return context.Response.WriteAsync(result);
        }
    };
});

builder.Services.AddAuthorization();

// ═══════════════════════════════════════════════════════════════
//  DEPENDENCY INJECTION — HELPERS
// ═══════════════════════════════════════════════════════════════
builder.Services.AddSingleton<JwtHelper>();

// ═══════════════════════════════════════════════════════════════
//  DEPENDENCY INJECTION — REPOSITORIES
// ═══════════════════════════════════════════════════════════════
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITestRepository, TestRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<ITestAssignmentRepository, TestAssignmentRepository>();
builder.Services.AddScoped<ITestSessionRepository, TestSessionRepository>();
builder.Services.AddScoped<IUserAnswerRepository, UserAnswerRepository>();
builder.Services.AddScoped<IViolationRepository, ViolationRepository>();
builder.Services.AddScoped<ICapturedImageRepository, CapturedImageRepository>();
builder.Services.AddScoped<IVideoRecordingRepository, VideoRecordingRepository>();

// ═══════════════════════════════════════════════════════════════
//  DEPENDENCY INJECTION — SERVICES
// ═══════════════════════════════════════════════════════════════
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ICandidateService, CandidateService>();
builder.Services.AddScoped<IProctoringService, ProctoringService>();

// ═══════════════════════════════════════════════════════════════
//  CONTROLLERS
// ═══════════════════════════════════════════════════════════════
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ═══════════════════════════════════════════════════════════════
//  SWAGGER
// ═══════════════════════════════════════════════════════════════
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Online Test & Proctoring Platform API",
        Version = "v1",
        Description = """
            A production-ready proctored examination platform API.
            
            **Default Credentials (seeded on first run):**
            - Admin → username: `admin` | password: `Admin@123`
            - Candidate → username: `john_doe` | password: `User@123`
            
            **How to authenticate:**
            1. Call `POST /api/auth/login` with credentials above.
            2. Copy the `token` from the response.
            3. Click **Authorize** (top right), enter: `Bearer <your-token>`.
            4. All protected endpoints will now work.
            """,
        Contact = new OpenApiContact
        {
            Name = "Test Management Platform",
            Email = "admin@testplatform.com"
        }
    });

    // JWT Bearer definition for Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. Example: Bearer eyJhbGci..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments for richer Swagger docs
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// ═══════════════════════════════════════════════════════════════
//  CORS (open for dev; restrict in production)
// ═══════════════════════════════════════════════════════════════
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ═══════════════════════════════════════════════════════════════
//  MULTIPART BODY SIZE LIMIT (for video chunk uploads)
// ═══════════════════════════════════════════════════════════════
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 52_428_800; // 50 MB
});
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 52_428_800; // 50 MB
});

// ═══════════════════════════════════════════════════════════════
//  BUILD
// ═══════════════════════════════════════════════════════════════
var app = builder.Build();

// ─── Global Exception Middleware (must be FIRST) ──────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();

// ─── Swagger ──────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Test Management Platform v1");
    c.RoutePrefix = string.Empty; // Swagger at root "/"
    c.DocumentTitle = "Online Test & Proctoring Platform";
    c.DefaultModelsExpandDepth(-1); // Hide schema models by default
    c.DisplayRequestDuration();
});

// ─── Static Files (for serving uploaded screenshots/videos) ───
app.UseStaticFiles();

// ─── HTTPS Redirect ───────────────────────────────────────────
app.UseHttpsRedirection();

// ─── CORS ─────────────────────────────────────────────────────
app.UseCors("AllowAll");

// ─── Auth ─────────────────────────────────────────────────────
app.UseAuthentication();
app.UseAuthorization();

// ─── Controllers ──────────────────────────────────────────────
app.MapControllers();

// ═══════════════════════════════════════════════════════════════
//  DATABASE SEEDING (runs migrations + seeds admin + sample data)
// ═══════════════════════════════════════════════════════════════
var logger = app.Services.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("Starting database initialization and seeding...");

    // Ensure wwwroot/uploads directories exist
    var webRootPath = app.Environment.WebRootPath
        ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    Directory.CreateDirectory(Path.Combine(webRootPath, "uploads", "screenshots"));
    Directory.CreateDirectory(Path.Combine(webRootPath, "uploads", "videos"));
    logger.LogInformation("Upload directories verified.");

    await DataSeeder.SeedAsync(app.Services, logger);
    logger.LogInformation("Database seeding completed successfully.");
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred during database seeding. Application will continue.");
}

// ═══════════════════════════════════════════════════════════════
//  RUN
// ═══════════════════════════════════════════════════════════════
logger.LogInformation("Application started. Navigate to http://localhost:5000 to open Swagger UI.");
app.Run();
