# Cadmus API

- [Cadmus API](#cadmus-api)
	- [History](#history)
	    - [4.1.0](#410)
		- [4.0.3](#403)
		- [4.0.2](#402)
		- [4.0.1](#401)
		- [4.0.0](#400)
		- [1.1.0](#110)
		- [1.0.14](#1014)
		- [1.0.13](#1013)
	- [Developer Note About Port](#developer-note-about-port)
	- [Profile](#profile)
		- [Importing Data From JSON Dumps](#importing-data-from-json-dumps)

Quick Docker image build: `docker build . -t vedph2020/cadmus_api:4.1.0 -t vedph2020/cadmus_api:latest` (replace with the current version).

API layer for the Cadmus content editor.

This API is the default API serving general and philological parts, and contains all the shared components which can be used to compose your own API:

- `Cadmus.Api.Models`: API data models.
- `Cadmus.Api.Services`: API services.
- `Cadmus.Api.Controllers`: API controllers.

The API application proper just adds a couple of application-specific services implementations:

- `AppPartSeederFactoryProvider` implementing `IPartSeederFactoryProvider`;
- `AppRepositoryProvider` implementing `IRepositoryProvider`.

Both these services depend on the parts you choose to support, so they are implemented at the application level.

## History

- 2022-08-11: updated migration package to include filters and added argument to preview build blocks API.
- 2022-08-08: updated migration package to include Markdown support.

### 4.1.0

- 2022-08-07: added [preview feature](https://github.com/vedph/cadmus-migration). This feature must be explicitly opted in. To add preview capabilities to an existing API:

1. in `appsettings.json`, add this entry:

```json
"Preview": {
  "IsEnabled": true
}
```

2. eventually add a different preview factory provider. The standard one is already found in `Cadmus.Api.Services` (`StandardCadmusPreviewFactoryProvider`), and usually this is enough.

3. in `Startup.cs`:

- add `GetPreviewerAsync` (see [here](./CadmusApi/Startup.cs)).
- in `ConfigureServices`, add this line:

```cs
// previewer
services.AddSingleton(p => GetPreviewer(p));
```

This configures the previewer service. When preview is not enabled, this will just return a do-nothing service.

4. in `wwwroot` add `preview-profile.json` with your profile. Note that in most cases here I've not added any true transformation, but I just use null renderers for a few objects. The only transformation used exempli gratia is for `it.vedph.note` (note part), which gets rendered into HTML via XSLT followed by Markdown processing.

### 4.0.3

- 2022-08-04: updated `Cadmus.General.Parts` package where `ExternalId` has been replaced with `AssertedId`.
- 2022-08-01: updated packages.
- 2022-07-14: updated packages.

### 4.0.2

- 2022-06-11: updated packages.

### 4.0.1

- 2022-05-31: fixed missing graph assembly in item factory.

### 4.0.0

- 2022-05-31: replaced graph mapping with [new library](https://github.com/vedph/cadmus-graph). The new library no more relies on data pins (that was a legacy hack to allow for projection when there was no time for a more powerful solution). The graph mapping library is still experimental, but it allows for a more robust approach to RDF-like projection. As no production project is currently using the graph, they will not be affected anyway. Conversely, projects willing to use it will be able to start from a more powerful and future-proof codebase. Note that the Docker version number has been aligned with the library version number.

### 1.1.0

- 2022-04-29: upgraded Cadmus packages to NET 6.

### 1.0.14

- 2022-03-10: upgraded packages.

### 1.0.13

- 2022-01-16: added `ChronotopesPart`. Image: 1.0.13.
- 2022-01-09: upgraded including new `NamesPart`.
- 2022-01-02: upgraded to use part libraries moved out of the original Cadmus core solution.
- 2021-12-22: fix to part/item deletion when graph is disabled.
- 2021-12-18: updated packages.
- 2021-11-22: refactored API endpoints removing the legacy database name. This bumped API library versions to 3.0.0.
- 2021-11-15: graph controller API.
- 2021-10-24: integrated graph in database initializer.
- 2021-10-15: API breaking change for Mongo authentication database, introduced by AspNetCore.Identity.Mongo 8.3.1 which deleted two properties from `MongoUser`. You can realign existing databases with this update command:

```js
/*
Removed fields:
AuthenticatorKey = null
RecoveryCodes = []
*/
db.Users.updateMany({}, { $unset: {"AuthenticatorKey": 1, "RecoveryCodes": 1} });
```

This applies since Cadmus.Api.Controllers 1.3.0, Cadmus.Api.Services 1.2.0.

## Developer Note About Port

Sometimes, there seems to be an issue with VS in launching IIS Express at a given port in Windows. If IIS starts complaining that the port is in use, run in an elevated prompt:

```ps1
net stop winnat
net start winnat
```

## Profile

A Cadmus profile is a JSON document used to define items facets, parts, flags, and taxonomies (thesauri).

When the backend API starts, the profile is used to seed a not-existing database. The location of this profile is read from the environment variable named `SEED__PROFILESOURCE` (Linux; `SEED:PROFILESOURCE` in Windows).

If this is not specified, a default mock profile is used. This typically happens when launching the API just for the sake of experimenting with a mock database. This profile is named `seed-profile.json`, and is located under `wwwroot` in the `cadmus_api` repository.

The profile source can either be a file name, or an HTTP(S) resource; in the latter case, it is assumed that it starts with `http`.

When the profile source is a file name, it may contain directory variables between `%`. Currently, the variable `%wwwroot%` is reserved to resolve to the web content root directory; any other variable name is searched in the configuration.

### Importing Data From JSON Dumps

At startup you have the option of importing data (items and parts) from JSON dumps, after seeding has completed.

You can use this option to fully create a database when it does not exists, by allowing the seeder to seed the Cadmus profile but no items, and then importing JSON dumps from one or more files or HTTP(S) resources.

The import sources are specified in the `imports` section of the profile, just after the `seed` section, as a sibling of it; for instance:

```json
"imports": [
  "https://www.mysite.org/dumps/cadmus01.json",
  "https://www.mysite.org/dumps/cadmus02.json"
]
```

You are free to mix file-based and HTTP-based resources; they will be synchronously processed in the order they are specified. As specified above, when the profile source is a file name it may contain directory variables between `%`.

These dumps are JSON files whose root element is always an array. The objects in the array can be either items with their `parts` property (which can be empty), or just parts.
