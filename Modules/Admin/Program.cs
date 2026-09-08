using StudentCourse.Infrastructure;
using StudentCourse.Repositories;
using StudentCourse.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
string sharedLocalSettingsPath = Path.GetFullPath(
    Path.Combine(builder.Environment.ContentRootPath, "..", "..", "appsettings.Local.json"));
string sharedUiRootPath = Path.GetFullPath(
    Path.Combine(builder.Environment.ContentRootPath, "..", "..", "Shared", "wwwroot"));
string dataProtectionKeysPath = Path.Combine(Path.GetTempPath(), "StudentCourse.Admin.DataProtectionKeys");
Directory.CreateDirectory(dataProtectionKeysPath);
builder.Configuration.AddJsonFile(sharedLocalSettingsPath, optional: true, reloadOnChange: true);

builder.Logging.ClearProviders();
builder.Logging.AddDebug();
builder.Logging.AddConsole();

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
// 本地演示服务可能由终端、IDE 或 Codex 等不同 Windows 身份启动。
// 使用临时目录保存 Cookie 密钥，避免默认用户密钥目录权限异常导致正确账号登录时返回 500。
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
    .SetApplicationName("StudentCourse.Admin");
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "StudentCourse.Admin.Auth";
        options.LoginPath = "/admin.html";
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdmin", policy =>
    {
        policy.RequireRole("Admin");
        policy.RequireClaim("admin_level", AdminAuthService.SuperAdminLevel.ToString());
    });
});
builder.Services.AddCors(options => options.AddPolicy("LoginPortal", policy => policy.WithOrigins("http://localhost:5100").AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

builder.Services.AddScoped<AdminRepository>();
builder.Services.AddScoped<BatchOfferingRepository>();
builder.Services.AddScoped<AdminSelectionRepository>();
builder.Services.AddScoped<SchedulingRepository>();
builder.Services.AddScoped<EvaluationRepository>();
builder.Services.AddScoped<AdminCourseService>();
builder.Services.AddScoped<AdminApplicationService>();
builder.Services.AddScoped<AdminSelectionService>();
builder.Services.AddScoped<SystemLogService>();
builder.Services.AddScoped<SystemLogExportService>();
builder.Services.AddScoped<AdminAuthService>();
builder.Services.AddScoped<AdminUserService>();
builder.Services.AddScoped<SelectionBatchService>();
builder.Services.AddScoped<AdminClassService>();
builder.Services.AddScoped<SchedulingService>();
builder.Services.AddScoped<EvaluationService>();
builder.Services.AddScoped<EvaluationExportService>();

DbConnectionFactory.Initialize(builder.Configuration);

var app = builder.Build();
UserSessionContext.Configure(app.Services.GetRequiredService<IHttpContextAccessor>());

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    Exception? exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    app.Logger.LogError(exception, "Unhandled request error. TraceId: {TraceId}", context.TraceIdentifier);
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    string message = exception is Oracle.ManagedDataAccess.Client.OracleException
        ? "数据库连接失败，请稍后重试。"
        : "服务暂时不可用，请稍后重试。";
    await context.Response.WriteAsJsonAsync(new { message, traceId = context.TraceIdentifier });
}));

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(sharedUiRootPath),
    RequestPath = "/shared-ui"
});
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("LoginPortal");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
