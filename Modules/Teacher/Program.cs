using StudentCourse.Infrastructure;
using StudentCourse.Repositories;
using StudentCourse.Services;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
builder.Logging.AddConsole();

builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.Cookie.Name = "StudentCourse.Teacher.Auth";
    options.Events.OnRedirectToLogin = context => { context.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; };
    options.Events.OnRedirectToAccessDenied = context => { context.Response.StatusCode = StatusCodes.Status403Forbidden; return Task.CompletedTask; };
});
builder.Services.AddAuthorization();
builder.Services.AddCors(options => options.AddPolicy("LoginPortal", policy => policy.WithOrigins("http://localhost:5100").AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

builder.Services.AddScoped<TeacherRepository>();
builder.Services.AddScoped<TeachingClassRepository>();
builder.Services.AddScoped<StudentListRepository>();
builder.Services.AddScoped<CourseApplicationRepository>();
builder.Services.AddScoped<ScoreRepository>();
builder.Services.AddScoped<AccountRepository>();
builder.Services.AddScoped<TeacherService>();
builder.Services.AddScoped<TeacherStudentService>();
builder.Services.AddScoped<CourseApplicationService>();
builder.Services.AddScoped<ScoreService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<ExportService>();

DbConnectionFactory.Initialize(builder.Configuration);

var app = builder.Build();
UserSessionContext.Configure(app.Services.GetRequiredService<IHttpContextAccessor>());

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    Exception? exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    app.Logger.LogError(exception, "Unhandled request error. TraceId: {TraceId}", context.TraceIdentifier);
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(new { message = "Service is temporarily unavailable.", traceId = context.TraceIdentifier });
}));

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("LoginPortal");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();
