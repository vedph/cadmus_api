@echo off
echo PRESS ANY KEY TO INSTALL Cadmus Libraries TO LOCAL NUGET FEED
echo Remember to generate the up-to-date package.
pause
c:\exe\nuget add .\Cadmus.Api.Controllers\bin\Debug\Cadmus.Api.Controllers.1.0.11.nupkg -source C:\Projects\_NuGet
c:\exe\nuget add .\Cadmus.Api.Models\bin\Debug\Cadmus.Api.Models.1.0.7.nupkg -source C:\Projects\_NuGet
c:\exe\nuget add .\Cadmus.Api.Services\bin\Debug\Cadmus.Api.Services.1.0.14.nupkg -source C:\Projects\_NuGet
pause
