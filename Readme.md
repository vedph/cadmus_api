# CadmusApi

Quick Docker image build: `docker build . -t vedph2020/cadmus_api:1.0.12 -t vedph2020/cadmus_api:latest` (replace with the current version).

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

There seems to be an issue with VS in launching IIS Express at a given port in Windows. If IIS starts complaining that the port is in use, run in an elevated prompt:

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
