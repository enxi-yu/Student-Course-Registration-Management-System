@echo off
setlocal
cd /d "%~dp0"

where dotnet >nul 2>nul
if errorlevel 1 goto :no_dotnet

echo Building all services first...
dotnet build StudentCourse.sln --configuration Debug --nologo
if errorlevel 1 goto :build_failed

echo Starting login portal and three services...
start "Login Portal - http://localhost:5100/Login" /D "%~dp0" cmd /k "dotnet run --no-build --configuration Debug --project Portal\Login.csproj --urls http://localhost:5100"
start "Student - http://localhost:5101" /D "%~dp0" cmd /k "dotnet run --no-build --configuration Debug --project Modules\Student\StudentCourse.Student.csproj --urls http://localhost:5101"
start "Teacher - http://localhost:5102" /D "%~dp0" cmd /k "dotnet run --no-build --configuration Debug --project Modules\Teacher\StudentCourse.Teacher.csproj --urls http://localhost:5102"
start "Admin - http://localhost:5103" /D "%~dp0" cmd /k "dotnet run --no-build --configuration Debug --project Modules\Admin\StudentCourse.Admin.csproj --urls http://localhost:5103"

echo.
echo Login portal: http://localhost:5100/Login
echo Four command windows should now be open.
echo Close each command window to stop its service.
pause
exit /b 0

:build_failed
echo Build failed. Close any running service windows, then run this file again.
pause
exit /b 1

:no_dotnet
echo .NET SDK was not found. Install .NET 8 SDK and run this file again.
pause
exit /b 1
