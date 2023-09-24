@echo off
echo BUILD Cadmus API packages
del .\Cadmus.Api.Controllers\bin\Debug\*.nupkg
del .\Cadmus.Api.Controllers\bin\Debug\*.snupkg
del .\Cadmus.Api.Controllers.Import\bin\Debug\*.nupkg
del .\Cadmus.Api.Controllers.Import\bin\Debug\*.snupkg
del .\Cadmus.Api.Models\bin\Debug\*.nupkg
del .\Cadmus.Api.Models\bin\Debug\*.snupkg
del .\Cadmus.Api.Services\bin\Debug\*.nupkg
del .\Cadmus.Api.Services\bin\Debug\*.snupkg

cd .\Cadmus.Api.Models
dotnet pack -c Debug -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg
cd..

cd .\Cadmus.Api.Services
dotnet pack -c Debug -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg
cd..

cd .\Cadmus.Api.Controllers
dotnet pack -c Debug -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg
cd..

cd .\Cadmus.Api.Controllers.Import
dotnet pack -c Debug -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg
cd..

pause
