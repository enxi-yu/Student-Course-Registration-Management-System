using StudentCourse.Infrastructure;
using StudentCourse.Repositories;
using StudentCourse.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllers();

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
