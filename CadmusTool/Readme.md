# CadmusTool

Cadmus configuration and utility tools.

**Warning**: ensure that plugins in the CLI tool folder have been updated (using the provided batch) before executing!

## Seed Database Command

This command is used to create a new Cadmus MongoDB database (if the specified database does not already exists), and seed it with a specified number of random items.

This command relies on plugins in the `Plugins` folder, so be sure to run the `UpdatePlugins` batch, or manually update the plugins before executing it.

For a sample seed profile see `Assets/SeedProfile.json`.

Syntax:

```ps1
./CadmusTool.exe seed <MongoDBName> <SeedProfileFilePath> [-c ItemCount] [-d] [-h]
```

where:

- `MongoDBName` is the name of the MongoDB database to be seeded (and created, if it does not exist).
- `SeedProfileFilePath` is the path to the seed profile JSON file.

Options:

- `-c N` or `--count N`: the number of items to be seeded. Default is 100.
- `-d` or `--dry`: dry run, i.e. create the items and parts, but do not create the database nor store anything into it. This is used to test for seeder issues before actually running it.
- `-h` or `--history`: add history items and parts together with the seeded items and parts. Default is `false`. In a real-world database you should set this to `true`.

## Legacy Commands

### Import LEX

Import into a Cadmus database an essential subset of roughly filtered data to be used as seed data. This is a very minimal conversion from Zingarelli LEX files, just to have some fake data to work with.

	CadmusTool import-lex <lexDirectory> <databaseName> <profileXmlFilePath> [-p|--preflight]

The profile JSON file defines items facets and flags. You can find a sample in `CadmusTool/Assets/Profile-lex.json`. Note that this profile is used only to provide a better editing experience, and does not reflect a real limitation for allowed parts in the database.

	CadmusTool import-lex c:\users\dfusi\desktop\lex cadmuslex c:\users\dfusi\desktop\Profile.json -p

### Legacy Seed

Seed a Cadmus database (creating it if it does not exist) with a specified number of random items with their parts.

	CadmusTool seed <databaseName> <profileXmlFilePath> <facetsCsvList> [-c|--count itemCount]

The profile JSON file defines items facets and flags. You can find a sample in `CadmusTool/Assets/Profile.json`. Note that this profile is used only to provide a better editing experience, and does not reflect a real limitation for allowed parts in the database.

The items count defaults to 100. Example:

	.\CadmusTool.exe seed cadmus \Projects\Core20\CadmusApi\CadmusTool\Assets\Profile.json facet-default -c 100

### Import LEX

Create and seed a Cadmus MongoDB database with the specified profile, importing LEX files from a folder.

	CadmusTool import-lex inputDirectory databaseName profilePath [-p]

Option `-p` = preflight, i.e. do not touch the target database.

Example:

	dotnet .\CadmusTool.dll import-lex c:\users\dfusi\desktop\lex cadmuslex c:\users\dfusi\desktop\Profile.json -p
