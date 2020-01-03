# CadmusTool

Cadmus configuration and utility tools.

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
