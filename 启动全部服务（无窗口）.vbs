Option Explicit

Dim shell, fso, root
Set shell = CreateObject("WScript.Shell")
Set fso = CreateObject("Scripting.FileSystemObject")
root = fso.GetParentFolderName(WScript.ScriptFullName)
shell.CurrentDirectory = root

shell.Run "cmd.exe /c dotnet run --project Portal\Login.csproj --urls http://localhost:5100", 0, False
shell.Run "cmd.exe /c dotnet run --project Modules\Student\Student-Course.csproj --urls http://localhost:5101", 0, False
shell.Run "cmd.exe /c dotnet run --project Modules\Teacher\Student-Course.csproj --urls http://localhost:5102", 0, False
shell.Run "cmd.exe /c dotnet run --project Modules\Admin\Student-Course.csproj --urls http://localhost:5103", 0, False
