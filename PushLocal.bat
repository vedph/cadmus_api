@echo off
echo PRESS ANY KEY TO INSTALL Cadmus Libraries TO LOCAL NUGET FEED
echo Remember to generate the up-to-date package.
c:\exe\nuget add .\Cadmus.Api.Controllers\bin\Debug\Cadmus.Api.Controllers.9.0.11.nupkg -source C:\Projects\_NuGet
c:\exe\nuget add .\Cadmus.Api.Controllers.Import\bin\Debug\Cadmus.Api.Controllers.Import.9.0.11.nupkg -source C:\Projects\_NuGet
c:\exe\nuget add .\Cadmus.Api.Models\bin\Debug\Cadmus.Api.Models.9.0.11.nupkg -source C:\Projects\_NuGet
c:\exe\nuget add .\Cadmus.Api.Services\bin\Debug\Cadmus.Api.Services.9.0.11.nupkg -source C:\Projects\_NuGet
pause
