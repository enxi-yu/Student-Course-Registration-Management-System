using StudentCourse.Infrastructure;
using StudentCourse.Student.Repositories;
using StudentCourse.Student.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllers();

// 模块一：基本信息与首页（Person 1）
builder.Services.AddScoped<StudentProfileRepository>();
builder.Services.AddScoped<StudentProfileService>();

// 模块二：选课中心与课表（Person 2）
builder.Services.AddScoped<CourseSelectionRepository>();
builder.Services.AddScoped<CourseSelectionService>();

// 模块三：成绩查询与评价（Person 3）
builder.Services.AddScoped<StudentGradeRepository>();
builder.Services.AddScoped<StudentGradeService>();

DbConnectionFactory.Initialize(builder.Configuration);

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
