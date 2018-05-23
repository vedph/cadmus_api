FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY CadmusApi/CadmusApi.csproj CadmusApi/
COPY CadmusApi/NuGet.config ./
RUN dotnet restore CadmusApi/CadmusApi.csproj
COPY . .
WORKDIR /src/CadmusApi
RUN dotnet build CadmusApi.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish CadmusApi.csproj -c Release -o /app
COPY CadmusApi/bin/Release/netcoreapp2.0/CadmusApi.xml /app
COPY CadmusApi/bin/Release/netcoreapp2.0/Plugins /app/Plugins

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "CadmusApi.dll"]
