using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Attendance_Management_System.DBCONTEXT;
using Attendance_Management_System.Helpers;
using Attendance_Management_System.Hubs;
using Attendance_Management_System.Interfacess;
using Attendance_Management_System.Middlewares;
using Attendance_Management_System.Repositories.Interfaces;
using Attendance_Management_System.Repositories.Implementations;
using Attendance_Management_System.Services;

var builder = WebApplication.CreateBuilder(args);

// =============================================
// Validation Error Response Format
// =============================================
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );
        return new BadRequestObjectResult(new
        {
            statusCode = 400,
            message = "Validation failed.",
            errors
        });
    };
});

builder.Services.AddControllers();

// =============================================
// Database — Neon PostgreSQL with Retry Logic
// =============================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(120);
        }));

// =============================================
// EmailJS
// =============================================
builder.Services.Configure<EmailJSOptions>(
    builder.Configuration.GetSection("EmailJS"));
builder.Services.AddHttpClient<EmailJSHelper>();

// =============================================
// Register Repositories
// =============================================
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<AttendanceRepository>();
builder.Services.AddScoped<IAttendanceRepository>(sp => sp.GetRequiredService<AttendanceRepository>());
builder.Services.AddScoped<IAttendanceFilterRepository>(sp => sp.GetRequiredService<AttendanceRepository>());
builder.Services.AddScoped<IAttendanceBulkRepository>(sp => sp.GetRequiredService<AttendanceRepository>());

builder.Services.AddScoped<IQRSessionRepository, QRSessionRepository>();

// =============================================
// Register Services
// =============================================
builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IQRService, QRService>();

// =============================================
// JWT Authentication
// =============================================
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var cookieToken = context.Request.Cookies["accessToken"];
                if (!string.IsNullOrEmpty(cookieToken))
                {
                    context.Token = cookieToken;
                    return Task.CompletedTask;
                }

                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs/attendance"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

// =============================================
// CORS — Allow frontend connections
// =============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionCors", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7033",           // Your local frontend
                "https://localhost:5001",           // Local frontend alternative
                "http://localhost:3000",            // React
                "http://localhost:5173",            // Vite
                "http://localhost:4200",            // Angular
                "https://your-frontend.onrender.com" // Replace with your actual frontend URL
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("Content-Disposition");
    });
});

// =============================================
// Rate Limiting
// =============================================
builder.Services.AddAppRateLimiting();

// =============================================
// SignalR
// =============================================
builder.Services.AddSignalR();

// =============================================
// Swagger
// =============================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "3TagaPayu Attendance Management API",
        Version = "v1",
        Description = "REST API for the 3TagaPayu Attendance Management System."
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// =============================================
// Build App
// =============================================
var app = builder.Build();

// Show detailed errors for debugging
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Seed admin
using (var scope = app.Services.CreateScope())
{
    try
    {
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        await authService.SeedAdminAsync();
        Console.WriteLine("Admin seeded successfully (if no admin existed)");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error seeding admin: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }
}

// Middleware pipeline
app.UseMiddleware<ExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "3TagaPayu AMS API v1");
    c.DocumentTitle = "3TagaPayu AMS – API Docs";
});

app.UseCors("ProductionCors");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<AttendanceHub>("/hubs/attendance");

app.Run();