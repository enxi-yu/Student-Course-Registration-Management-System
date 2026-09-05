using StudentCourse.Infrastructure;
using StudentCourse.Student.Repositories;
using StudentCourse.Student.Services;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
string sharedLocalSettingsPath = Path.GetFullPath(
    Path.Combine(builder.Environment.ContentRootPath, "..", "..", "appsettings.Local.json"));
string sharedUiRootPath = Path.GetFullPath(
    Path.Combine(builder.Environment.ContentRootPath, "..", "..", "Shared", "wwwroot"));
builder.Configuration.AddJsonFile(sharedLocalSettingsPath, optional: true, reloadOnChange: true);
builder.Logging.ClearProviders();
builder.Logging.AddDebug();
builder.Logging.AddConsole();

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.DataProtectionProvider = new EphemeralDataProtectionProvider();
    options.Cookie.Name = "StudentCourse.Student.Auth";
    options.Events.OnRedirectToLogin = context => { context.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; };
    options.Events.OnRedirectToAccessDenied = context => { context.Response.StatusCode = StatusCodes.Status403Forbidden; return Task.CompletedTask; };
});
builder.Services.AddAuthorization();
builder.Services.AddCors(options => options.AddPolicy("LoginPortal", policy => policy.WithOrigins("http://localhost:5100").AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

// 模块一：基本信息与首页（Person 1）
builder.Services.AddScoped<StudentProfileRepository>();
builder.Services.AddScoped<StudentProfileService>();

// 模块二：选课中心与课表（Person 2）
builder.Services.AddScoped<CourseSelectionRepository>();
builder.Services.AddScoped<CourseSelectionService>();

// 模块三：成绩查询与评价（Person 3）
builder.Services.AddScoped<StudentGradeRepository>();
builder.Services.AddScoped<StudentGradeService>();
builder.Services.AddScoped<StudentEvaluationRepository>();
builder.Services.AddScoped<StudentEvaluationService>();

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
