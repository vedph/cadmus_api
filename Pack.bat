@echo off
echo BUILD Cadmus API packages
del .\Cadmus.Api.Services\bin\Debug\*.nupkg

cd .\Cadmus.Api.Services
dotnet pack -c Debug -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg
cd..
pause
