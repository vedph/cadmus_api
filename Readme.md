# CadmusApi

This is an essential API for the server side of the Cadmus content editor.

This solution is related to:

- Cadmus
- Memoria

Remember to update the plugins directories with the batches included in this solution before executing. Note that for `CadmusTool` this directory should not include `Cadmus.Parts`, as this is already loaded by that project in order to seed the parts. Should you miss any plugin, a common issue is that you will see only those item's parts which are included in the plugins found.

## History

2018-06-14: replaced MSSQL with MongoDB store in authentication.

## MongoDB

**Dump/restore** uses BSON. JSON is a subset of BSON and is used for import/export with 3rd parties.

- dump a database:

	.\mongodump.exe --db cadmuslex --out c:\users\dfusi\desktop\cadmuslex-dump

- dump a database to a single, compressed archive:

	.\mongodump.exe --db cadmuslex --archive=c:\users\dfusi\desktop\cadmuslex.tar.gz --gzip

- restore a database from a single, compressed archive:

	.\mongorestore.exe --archive=c:\users\dfusi\desktop\cadmuslex.tar.gz --gzip

## Docker

Quick steps for __building an image__:

1.ensure to __update your MyGet.org repository__. Use `MyNugetSweep` to sweep old packages and upload:

	dotnet .\MyNugetSweep.dll sweep C:\Projects\_NuGet -c 3
	dotnet .\MyNugetSweep.dll push C:\Projects\_NuGet https://www.myget.org/F/fusi/api/v2/package MYNUGETKEY

2.switch to `Release` and __build__.
3.__push the image__ to the private registry:

	docker login --username naftis --password XXX
	docker push naftis/fusi:cadmusapi

In the consumer __Linux machine__:

- login: `sudo docker login --username naftis`: then insert your Linux username (sudo) password, and the Docker password;
- copy the `docker-compose.prod.yml` file in the Linux machine;
- open a terminal in the same folder of the Docker compose file just copied, and execute `sudo docker-composer up`.

To connect to databases from the Linux Docker host, using e.g. Compass (<https://www.mongodb.com/download-center?jmp=nav#compass>):

- server: 127.0.0.1
- port: 27017
- no authentication

Note: you should *remove the 2.0 version number* from the generated Dockerfile dotnet images, or you will have issues because of mismatched dotnet versions: <https://github.com/aspnet/IISIntegration/issues/476> (you can check your development machine version of dotnet with `dotnet --version`). The issue I encountered was this error when doing a docker-compose up on the Linux target: `Error: An assembly specified in the application dependencies manifest (CadmusApi.deps.json) was not found: package: 'Microsoft.AspNetCore.Antiforgery', version: '2.0.3' path: 'lib/netstandard2.0/Microsoft.AspNetCore.Antiforgery.dll' This assembly was expected to be in the local runtime store as the application was published using the following target manifest files: aspnetcore-store-2.0.8.xml`.

The web API project was created with Docker support enabled, **targeting Linux**. To this end, you must ensure that your Docker mode is switched to Linux, and that you have shared your disk from Docker settings (the disk sharing option appears only in Linux mode, as in Windows disks are pre-shared).

As we drag NuGet packages from additional sources, the Dockerfile must include the `NuGet.config` file when copying source to the development container. As for the `myget.org` source, ensure that you have updated it to the latest packages from your local development machine (use my `MyNuGetSweep` to clean and push packages).

When building the app, just use IIS and Windows as usual, setting the API project as the startup project. When you want to build the Docker image, switch to Release and build. This builds the image executing Docker compose, and stores it in your machine. You can then list the images (`docker image ls`) to check that the newly created image is there. Its tag is already defined for uploading to my private registry (`naftis/fusi`). Otherwise, you should tag it with `docker tag imageid naftis/fusi:cadmusapi`. **Note**: remember to update the MyGet registry before building!

To push the image to the private registry:

- login: `docker login --username naftis --password XXX`
- push: `docker push naftis/fusi:cadmusapi`

In the Linux machine:

- login: `sudo docker login --username naftis`: then insert your Linux username (sudo) password, and the Docker password;
- copy the `docker-compose.prod.yml` file in the Linux machine;
- open a terminal in the same folder of the Docker compose file just copied, and execute `sudo docker-composer up`.

To connect to databases from the Linux Docker host:

1.SQL Server (e.g. SQL Operations Studio: <https://docs.microsoft.com/en-us/sql/sql-operations-studio/download?view=sql-server-2017>):

- server: 127.0.0.1\sqlexpress,1433
- user: SA
- password: P4ss-W0rd!

2.MongoDB (e.g. Compass: <https://www.mongodb.com/download-center?jmp=nav#compass>):

- server: 127.0.0.1
- port: 27017
- no authentication

### Obsolete Docker Config

This was changed after switching to MongoDB for authentication.

```yml
version: '3.4'

services:
  # SQL Server
  cadmusmssql:
    image: microsoft/mssql-server-linux
    container_name: cadmussqlserver
    environment:
      ACCEPT_EULA: Y
      SA_PASSWORD: "P4ss-W0rd!"
    ports:
      - 1433:1433
    networks:
      - cadmusnetwork

  # MongoDB
  cadmusmongo:
    image: mongo
    container_name: cadmusmongo
    environment:
      - MONGO_DATA_DIR=/data/db
      - MONGO_LOG_DIR=/dev/null
    ports:
      - 27017:27017
    command: mongod --smallfiles --logpath=/dev/null # --quiet
    networks:
      - cadmusnetwork

  cadmusapi:
    image: naftis/fusi:cadmusapi
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    ports:
      - 60304:60304
    depends_on:
      - cadmusmssql
      - cadmusmongo
    environment:
      # for Windows use : as separator, for non Windows use __
      # (see https://github.com/aspnet/Configuration/issues/469)
      DATA__DEFAULTCONNECTION__CONNECTIONSTRING: "Server=127.0.0.1\\sqlexpress,1433;Database=cadmusapi;User Id=SA;Password=P4ss-W0rd!;MultipleActiveResultSets=true"
      SERILOG__CONNECTIONSTRING: "Server=127.0.0.1\\sqlexpress,1433;Database=cadmusapi;User Id=SA;Password=P4ss-W0rd!;MultipleActiveResultSets=true"
    networks:
      - cadmusnetwork

networks:
  cadmusnetwork:
    driver: bridge
```

## CadmusTool

Command line tool for Cadmus.

### CadmusTool - Import LEX

Import into a Cadmus database an essential subset of roughly filtered data to be used as seed data. This is a very minimal conversion from Zingarelli LEX files, just to have some fake data to work with.

	CadmusTool import-lex <lexDirectory> <databaseName> <profileXmlFilePath> [-p|--preflight]

The profile XML file defines items facets and flags. You can find a sample in `CadmusTool/Assets/Profile-lex.xml`. Note that this profile is used only to provide a better editing experience, and does not reflect a real limitation for allowed parts in the database.

	CadmusTool import-lex c:\users\dfusi\desktop\lex cadmuslex c:\users\dfusi\desktop\Profile.xml -p

### CadmusTool - Seed

Seed a Cadmus database (creating it if it does not exist) with a specified number of random items with their parts.

	CadmusTool seed <databaseName> <profileXmlFilePath> <facetsCsvList> [-c|--count itemCount]

The profile XML file defines items facets and flags. You can find a sample in `CadmusTool/Assets/Profile.xml`. Note that this profile is used only to provide a better editing experience, and does not reflect a real limitation for allowed parts in the database.

The items count defaults to 100. Example:

	dotnet .\CadmusTool.dll seed cadmusapi \Projects\Core20\CadmusApi\CadmusTool\Assets\Profile.xml facet-default -c 100

### CadmusTool - Import LEX

Create and seed a Cadmus MongoDB database with the specified profile, importing LEX files from a folder.

	CadmusTool import-lex inputDirectory databaseName profilePath [-p]

Option `-p` = preflight, i.e. do not touch the target database.

Example:

	dotnet .\CadmusTool.dll import-lex c:\users\dfusi\desktop\lex cadmuslex c:\users\dfusi\desktop\Profile.xml -p

## CadmusApi

### Serilog

- configure WEB app for Serilog: <https://stackoverflow.com/questions/35736437/how-to-log-to-sql-server-using-serilog-with-asp-net-5-dotnet-core>

1.add these packages (or later versions):

- Serilog
- Serilog.Sinks.MSSqlServer
- Serilog.AspNetCore

2.update `appsettings.json` to include all the required Serilog SQL Server Sink configuration by adding the following JSON at the end of the file and before the last closing curly braces:

```json
"Serilog": {
    "ConnectionString": "Server=(local)\\sqlexpress;Database=lexmin;Trusted_Connection=True;MultipleActiveResultSets=true",
    "TableName": "Logs"
  }
```

3.update the `Startup` class to configure Serilog.ILogger ASP.NET Core has a new builtin Dependency Injection feature that can be used by registering the services and their implementations through the ConfigureServices method inside Startup class so add the following section add the end of the ConfigureServices method. The Dependency Injection feature provide three types of registrations Transient, Scoped and Singleton.

```c#
services.AddSingleton<Serilog.ILogger>(x=>
{
    return new LoggerConfiguration().WriteTo.MSSqlServer(Configuration["Serilog:ConnectionString"], Configuration["Serilog:TableName"],autoCreateSqlTable:true).CreateLogger();
});
```

Now you can get a reference to the `Serilog.ILogger` via the building constructor injection feature by simply adding variable of type `Serilog.ILogger` in the constructor of your controller.

Note: should you want to log it, you can get the IP address using `HttpContext.Connection.RemoteIpAddress` (<http://www.tech-coder.com/2016/09/how-to-get-remote-ip-address-in-aspnet.html>).

To connect the ASPNET logging system to Serilog: <https://github.com/serilog/serilog-aspnetcore>:

1.in `Program.cs` change the Main method by adding configuration retrieval and then configuring the Serilog logger with your sink(s):

```cs
public static int Main(string[] args)
{
    // see http://www.carlrippon.com/?p=1118
    IConfiguration configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile(
            $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
            optional: true)
        .Build();

    // https://github.com/serilog/serilog-aspnetcore
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .WriteTo.MSSqlServer(configuration["Serilog:ConnectionString"],
            configuration["Serilog:TableName"],
            autoCreateSqlTable: true)
        .CreateLogger();

    try
    {
        BuildWebHost(args).Run();
        return 0;
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Host terminated unexpectedly");
        return 1;
    }
    finally
    {
        Log.CloseAndFlush();
    }
}
```

2.in the same file, add `.UseSerilog()` to the `BuildWebHost` method `WebHost.CreateDefaultBuilder` call.

3.in `appsettings.json`, replace the `Logger` configuration with `Serilog`:

```json
  "Serilog": {
    "ConnectionString": "Server=(local)\\sqlexpress;Database=lexmin;Trusted_Connection=True;MultipleActiveResultSets=true",
    "TableName": "Log",
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Information",
        "System": "Warning"
      }
    }
  },
```

Sample SQL query:

```sql
SELECT TOP (100) [Id]
      ,[Message]
      ,[MessageTemplate]
      ,[Level]
      ,[TimeStamp]
      ,[Exception]
      ,[Properties]
  FROM [lexmin].[dbo].[Log]
  WHERE [message] LIKE '\[LEX\]%' ESCAPE '\'
  ORDER BY TimeStamp DESC
```
