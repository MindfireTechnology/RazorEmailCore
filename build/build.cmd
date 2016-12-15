cd ..\src
dotnet restore
dotnet build --configuration Release RazorEmailCore 
..\build\nuget.exe pack RazorEmailCore.nuspec
move ..\src\*.nupkg ..\releases
