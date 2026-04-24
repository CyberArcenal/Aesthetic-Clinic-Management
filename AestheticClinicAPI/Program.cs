using AestheticClinicAPI.Data;
using AestheticClinicAPI.Modules.Shared;
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

var builder = WebApplication.CreateBuilder(args);

// ========== DbContext ==========
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// ========== MediatR (optional) ==========
// If you have MediatR installed, uncomment the following line:
// builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// ========== Controllers & Swagger ==========
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Aesthetic Clinic API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();