using StudentCourse.Infrastructure;
using StudentCourse.Repositories;
using StudentCourse.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddDebug();

builder.Services.AddRazorPages();
builder.Services.AddControllers();

builder.Services.AddScoped<TeacherRepository>();
builder.Services.AddScoped<TeachingClassRepository>();
builder.Services.AddScoped<StudentListRepository>();
builder.Services.AddScoped<CourseApplicationRepository>();
builder.Services.AddScoped<ScoreRepository>();
builder.Services.AddScoped<AdminRepository>();
builder.Services.AddScoped<TeacherService>();
builder.Services.AddScoped<TeacherStudentService>();
builder.Services.AddScoped<CourseApplicationService>();
builder.Services.AddScoped<ScoreService>();
builder.Services.AddScoped<SystemLogService>();
builder.Services.AddScoped<AdminAuthService>();
builder.Services.AddScoped<AdminUserService>();
builder.Services.AddScoped<SelectionBatchService>();
builder.Services.AddScoped<AdminClassService>();

DbConnectionFactory.Initialize(builder.Configuration);
UserSessionContext.UseDevelopmentTeacherSession();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();
