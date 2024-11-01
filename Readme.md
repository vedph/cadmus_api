# Cadmus API

👀 [Cadmus Page](https://myrmex.github.io/overview/cadmus/)

🐋 Quick **Docker** image build: `docker build . -t vedph2020/cadmus-api:9.0.3 -t vedph2020/cadmus-api:latest` (replace with the current version).

API layer for the Cadmus content editor.

This API is the default API serving general and philological parts, and contains all the shared components which can be used to compose your own API:

- `Cadmus.Api.Models`: API data models.
- `Cadmus.Api.Services`: API services.
- `Cadmus.Api.Controllers`: API controllers.
- `Cadmus.Api.Controllers.Import`: API controllers for data import.

The API application proper just adds a couple of application-specific services implementations:

- `AppPartSeederFactoryProvider` implementing `IPartSeederFactoryProvider`;
- `AppRepositoryProvider` implementing `IRepositoryProvider`.

Both these services depend on the parts you choose to support, so they are implemented at the application level.

## History

### 9.0.11

- 2024-10-30:
  - updated packages.
  - added district location part to configuration.

### 9.0.10

- 2024-09-28: added endpoint for generating items from an item used as a template.

### 9.0.9

- 2024-09-28:
  - updated packages.
  - adjusted all the controllers in `Cadmus.Api.Controllers` for their namespace and derivation from `ControllerBase`.
  - added endpoint for checking the existance of a part of given type and role in a specific item.

### 9.0.7

- 2024-07-17: updated packages.
- 2024-06-06: updated packages.

### 9.0.6

- 2024-06-05: updated packages.

### 9.0.5

- 2024-05-22:
  - updated packages. Updating `Swashbuckle.AspNetCore` from 6.5.0 to 6.6.2 implied removing the `[FromFile]` attribute from the thesaurus import controller as [specified here](https://github.com/domaindrivendev/Swashbuckle.AspNetCore#handle-forms-and-file-uploads).
  - added proxy controller example in demo API.

### 9.0.4

- 2024-05-13: updated packages.

### 9.0.3

- 2024-04-12: updated packages.

### 9.0.2

- 2024-01-31: updated packages.
- 2023-12-06: more logging in API app.

### 9.0.1

- 2023-11-21: updated packages.

### 9.0.0

- 2023-11-18: ⚠️ Upgraded to .NET 8.

### 8.0.13

- 2023-11-04:
  - added add flags endpoint.
  - updated packages.
- 2023-10-03: updated packages.

### 8.0.12

- 2023-09-24: added new controllers library for thesauri import. If you want to **enable thesauri import**:
  1. in backend, add `Cadmus.Api.Controllers.Import` to your API project.
  2. in frontend, opt-in import by adding this setting to the environment variables (`env.js`): `window.__env.thesImportEnabled = true;`. At any rate, import is available only to admin users.

### 8.0.11

- 2023-09-22:
  - updated packages.
  - added entry point `GET api/items/groups` to items controller.

### 8.0.9

- 2023-09-04: updated packages.

### 8.0.8

- 2023-09-04: updated packages.

### 8.0.7

- 2023-07-19: fixed [logging](https://myrmex.github.io/overview/cadmus/dev/history/b-logging).

### 8.0.6

- 2023-07-16: updated packages.

### 8.0.5

- 2023-07-10: honor delay request before creating graph store.

### 8.0.4

- 2023-07-01: updated packages.

### 8.0.3

- 2023-06-23:
  - updated packages.
  - added more properties to triple binding model and added supply logic to `GraphController` for triple literal objects.

### 8.0.2

- 2023-06-21: updated packages for service libraries.

### 8.0.1

- 2023-06-21: updated packages.

### 8.0.0

- 2023-06-16: **breaking changes** for index/graph refactoring. See the [documentation page](https://myrmex.github.io/overview/cadmus/dev/history/b-rdbms). Added `Cadmus.Api.Services.Legacy` to support legacy code.

### 7.0.6

- 2023-06-02: updated packages.

### 7.0.5

- 2023-05-26:
  - event types thesauri.
  - updated packages.

### 7.0.4

- 2023-05-26: fix to `GraphController` in building the return route after saving nodes/triples.

### 7.0.3

- 2023-05-26: added deletion for item's parts in graph when item is deleted.

### 7.0.2

-2023-05-26: updated packages.

### 7.0.1

-2023-05-26: updated packages.

### 7.0.0

- 2023-05-23:
  - updated general parts with breaking changes for pin links part and fragment, comments parts and fragments, and historical events, now using `AssertedCompositeId`.
  - added thesaurus `pin-link-settings` for pin lookup settings.

### 6.3.0

- 2023-05-18: updated libraries and API startup code to use DI for `GraphUpdater`. See [this page](https://myrmex.github.io/overview/cadmus/dev/history/b-graph/) for details.

### 6.2.3

- 2023-05-16:
  - updated packages.
  - removed legacy dependencies.

### 6.2.2

- 2023-05-16: updated packages.

### 6.2.1

- 2023-04-21:
  - updated packages.
  - increased length of SecureKey in `appsettings.json` as required by updated MS `CryptoProviderFactory`. Do this in your API settings if you get an error like:

```txt
System.ArgumentOutOfRangeException: IDX10720: Unable to create KeyedHashAlgorithm for algorithm 'HS256', the key size must be greater than: '256' bits, key has '128' bits. (Parameter 'keyBytes')
   at Microsoft.IdentityModel.Tokens.CryptoProviderFactory.ValidateKeySize(Byte[] keyBytes, String algorithm, Int32 expectedNumberOfBytes)
   ...
```

### 6.2.0

- 2023-03-02:
  - moved gallery image annotations part into its own library and updated this demo API accordingly.
  - removed gallery image annotations from profile, as the demo will no more use it. There is now an independent shell for imaging components providing the same functionalities without additional overhead for the base shell. Anyway, the API demo services still mantain a reference to `Cadmus.Img.Parts` so that it will be quicker to introduce additional imaging components in the demo.

### 6.1.3

- 2023-03-02:
  - updated packages (added gallery image annotations part).
  - added new part definition and thesauri to seed profile.

### 6.1.1

- 2023-02-27: updated packages (added `tag` to historical event in general parts).

### 6.1.0

- 2023-02-26: updated packages (multiple chronotopes for historical event in general parts).

### 6.0.3

- 2023-02-24: updated packages (added `PinLinksLayerFragment` in general parts).
- 2023-02-15: updated packages (added `tag` to `PinLink` in general parts).
- 2023-02-07: updated sample thesauri for events.

### 6.0.2

- 2023-02-01: migrated to new components factory. This is a breaking change for backend components, please see [this page](https://myrmex.github.io/overview/cadmus/dev/history/#2023-02-01---backend-infrastructure-upgrade). Anyway, in the end you just have to update your libraries and a single namespace reference. Benefits include:
  - more streamlined component instantiation.
  - more functionality in components factory, including DI.
  - dropped third party dependencies.
  - adopted standard MS technologies for DI.

- 2023-01-25: removed pattern validation in `ThesaurusEntryBindingModel`.
- 2023-01-22: changed event related entities thesaurus.
- 2023-01-16: updated packages.

### 5.0.1

- 2022-11-30: added `comment-categories` thesaurus.
- 2022-11-25: updated packages (added `PinLinksPart`).

### 5.0.0

- 2022-11-10: upgraded to NET 7.
- 2022-11-08: changed sample event types thesaurus and relations.

### 4.3.0

- 2022-11-04:
  - updated packages (nullability enabled in Cadmus core).
  - nullability enabled.

### 4.2.2

- 2022-11-03: updated packages.
- 2022-10-25: updated packages.

### 4.2.1

- 2022-10-11: updated packages.

### 4.2.0

- 2022-10-10: updated packages to incorporate breaking change for `IRepositoryProvider`.

### 4.1.7

- 2022-10-10:
  - updated to `Cadmus.Core.IRepositoryProvider` with database name.
  - fix to comment preview sample.

### 4.1.6

- 2022-10-10:
  - updated packages.
  - fixes to sample previews.

### 4.1.5

- 2022-10-09: updated packages (`Cadmus.Migration`) and version numbers and published to NuGet.
- 2022-10-05: updated version numbers and published to NuGet.

### 4.1.4

- 2022-10-04:
  - updated packages.
  - added apparatus preview in `preview-profile.json`.
  - made HTTPS optional in `Startup.cs`. New environment variables: `Server:UseHSTS` and `Server:UseHttpsRedirection`, both defaulting to false.
  - set connection string of preview factory in `Startup.cs` preview configuration. This is now required to avoid issues with those filters requiring a database connection, so you should add this line to the method in any API using the preview infrastructure:

```cs
factory.ConnectionString = string.Format(CultureInfo.InvariantCulture,
                Configuration.GetConnectionString("Default"),
                Configuration.GetValue<string>("DatabaseNames:Data"));
```

- 2022-09-14: updated packages.

### 4.1.3

- 2022-08-21: updated packages, where item filter has got the new property `FlagMatching`; consequently, its API model and controller have been updated. This is not a breaking change, as the default matching mode for flags is the only one which was implemented until now. Also, the preview `GetKeys` API method signature has been updated.

### 4.1.2

- 2022-08-14: added methods to graph controller from Cadmus Graph sample API.

### 4.1.1

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

- add `GetPreviewer` (see [here](./CadmusApi/Startup.cs)).
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
