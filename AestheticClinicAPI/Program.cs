using AestheticClinicAPI.Data;
using AestheticClinicAPI.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

// Clients
using AestheticClinicAPI.Modules.Clients.Repositories;
using AestheticClinicAPI.Modules.Clients.Services;

// Treatments
using AestheticClinicAPI.Modules.Treatments.Repositories;
using AestheticClinicAPI.Modules.Treatments.Services;

// Appointments
using AestheticClinicAPI.Modules.Appointments.Repositories;
using AestheticClinicAPI.Modules.Appointments.Services;
using AestheticClinicAPI.Modules.Appointments.StateTransitionService;
using AestheticClinicAPI.Modules.Appointments.Subscribers;

// Billing
using AestheticClinicAPI.Modules.Billing.Repositories;
using AestheticClinicAPI.Modules.Billing.Services;

// Notifications
using AestheticClinicAPI.Modules.Notifications.Repositories;
using AestheticClinicAPI.Modules.Notifications.Services;
using AestheticClinicAPI.Modules.Notifications.Channels;

// Photos
using AestheticClinicAPI.Modules.Photos.Repositories;
using AestheticClinicAPI.Modules.Photos.Services;

// Reports
using AestheticClinicAPI.Modules.Reports.Repositories;
using AestheticClinicAPI.Modules.Reports.Services;

// Authentications
using AestheticClinicAPI.Modules.Authentications.Repositories;
using AestheticClinicAPI.Modules.Authentications.Services;

// Staff
using AestheticClinicAPI.Modules.Staff.Repositories;
using AestheticClinicAPI.Modules.Staff.Services;
using AestheticClinicAPI.Middleware;
using AestheticClinicAPI.Modules.Appointments.Models;
using AestheticClinicAPI.Modules.Billing.StateTransitionService;
using AestheticClinicAPI.Modules.Billing.Models;
using AestheticClinicAPI.Modules.Clients.Models;
using AestheticClinicAPI.Modules.Notifications.Models;
using AestheticClinicAPI.Modules.Notifications.StateTransitionService;
using AestheticClinicAPI.Modules.Photos.StateTransitionService;
using AestheticClinicAPI.Modules.Photos.Models;
using AestheticClinicAPI.Modules.Reports.StateTransitionService;
using AestheticClinicAPI.Modules.Reports.Models;
using AestheticClinicAPI.Modules.Staff.Models;
using AestheticClinicAPI.Modules.Staff.StateTransitionService;
using AestheticClinicAPI.Modules.Treatments.StateTransitionService;
using AestheticClinicAPI.Modules.Treatments.Models;
using AestheticClinicAPI.Modules.Authentications.Models;
using AestheticClinicAPI.Modules.Authentications.StateTransitionService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using AestheticClinicAPI.BackgroundServices;
using AestheticClinicAPI.Modules.Dashboard.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using AestheticClinicAPI.Database.Seeders;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ========== Serilog Configuration ==========
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // basahin mula sa appsettings.json (opsyonal)
    .Enrich.FromLogContext()
    .WriteTo.Console()                           // console output
    .WriteTo.File("logs/app-log-.txt",           // file output (daily rolling)
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,               // keep logs for 7 days
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog((context, services, config) =>
{
    config.ReadFrom.Configuration(context.Configuration)
          .ReadFrom.Services(services)
          .Enrich.FromLogContext()
          .WriteTo.Console()
          .WriteTo.File("logs/app-log-.txt",
              rollingInterval: RollingInterval.Day,
              retainedFileCountLimit: 7,
              outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
});

// ========== DbContext ==========
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    var interceptor = sp.GetRequiredService<ModelChangeInterceptor>();
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
           .AddInterceptors(interceptor);
});

// ========== Generic Repository ==========
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// ========== Clients Module ==========
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IClientService, ClientService>();

// ========== Treatments Module ==========
builder.Services.AddScoped<ITreatmentRepository, TreatmentRepository>();
builder.Services.AddScoped<ITreatmentService, TreatmentService>();

// ========== Appointments Module ==========
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<AppointmentStateTransition>();
builder.Services.AddScoped<AppointmentSubscriber>();

// ========== Billing Module ==========
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// ========== Notifications Module ==========
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();
builder.Services.AddScoped<INotifyLogRepository, NotifyLogRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
builder.Services.AddScoped<INotifyLogService, NotifyLogService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<IPushService, PushService>();

// ========== Photos Module ==========
builder.Services.AddScoped<IPhotoRepository, PhotoRepository>();
builder.Services.AddScoped<IPhotoService, PhotoService>();

// ========== Reports Module ==========
builder.Services.AddScoped<IReportLogRepository, ReportLogRepository>();
builder.Services.AddScoped<IReportLogService, ReportLogService>();

// ========== Authentications Module ==========
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ========== Staff Module ==========
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IStaffService, StaffService>();

// ========== Dashboard Service ==========
builder.Services.AddScoped<IDashboardService, DashboardService>();

// ========== MediatR (optional) ==========
// If you have MediatR installed, uncomment the following line:
// builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// ========== EF Core Interceptor ==========
builder.Services.AddSingleton<ModelChangeInterceptor>();

// Palitan ang DbContext registration para isama ang interceptor


// ========== State Transition Services ==========
builder.Services.AddScoped<IStateTransitionService<Appointment>, AppointmentStateTransition>();

// ========== Billing Module State Transitions ==========
builder.Services.AddScoped<IStateTransitionService<Invoice>, InvoiceStateTransition>();
builder.Services.AddScoped<IStateTransitionService<Payment>, PaymentStateTransition>();

// ========== Client Module State Transition ==========
builder.Services.AddScoped<IStateTransitionService<Client>, ClientStateTransition>();

// ========== Notifications Module State Transitions ==========
builder.Services.AddScoped<IStateTransitionService<Notification>, NotificationStateTransition>();
builder.Services.AddScoped<IStateTransitionService<NotificationTemplate>, NotificationTemplateStateTransition>();
builder.Services.AddScoped<IStateTransitionService<NotifyLog>, NotifyLogStateTransition>();

// ========== Photos Module State Transition ==========
builder.Services.AddScoped<IStateTransitionService<Photo>, PhotoStateTransition>();

// ========== Reports Module State Transition ==========
builder.Services.AddScoped<IStateTransitionService<ReportLog>, ReportLogStateTransition>();

// ========== Staff Module State Transition ==========
builder.Services.AddScoped<IStateTransitionService<StaffMember>, StaffMemberStateTransition>();

// ========== Treatments Module State Transition ==========
builder.Services.AddScoped<IStateTransitionService<Treatment>, TreatmentStateTransition>();

// ========== Authentications Module State Transitions ==========
builder.Services.AddScoped<IStateTransitionService<User>, UserStateTransition>();
builder.Services.AddScoped<IStateTransitionService<Role>, RoleStateTransition>();
builder.Services.AddScoped<IStateTransitionService<UserRole>, UserRoleStateTransition>();
builder.Services.AddScoped<IStateTransitionService<RefreshToken>, RefreshTokenStateTransition>();

// ========== JWT Authentication ==========
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key is missing"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});


// ========== AI Reporting Service ==========
builder.Services.AddScoped<IAIReportingService, AIReportingService>();

// ========== Background Service (Weekly Report) ==========
builder.Services.AddHostedService<WeeklyReportBackgroundService>();

// ========== Controllers & Swagger ==========
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Aesthetic Clinic API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        },
        Array.Empty<string>()
    }
});
});

// ========== FluentValidation ==========
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation();

// ========== CORS ==========
builder.Services.AddCors(options =>
  {
      options.AddPolicy("AllowReactApp", policy =>
      {
          policy.WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod();
      });
  });


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.MapControllers();


// Seed database on development startup
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await ApplicationDbInitializer.SeedAsync(dbContext);
}

app.Run();