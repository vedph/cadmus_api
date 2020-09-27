@echo off
echo PRESS ANY KEY TO INSTALL Cadmus Libraries TO LOCAL NUGET FEED
echo Remember to generate the up-to-date package.
pause
c:\exe\nuget add .\Cadmus.Api.Services\bin\Debug\Cadmus.Api.Services.1.0.3.nupkg -source C:\Projects\_NuGet
pause
