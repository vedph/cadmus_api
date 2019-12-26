using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cadmus.Core;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using Cadmus.Mongo;
using Cadmus.Parts.General;
using Cadmus.Parts.Layers;
using Cadmus.Philology.Parts.Layers;
using CadmusTool.Services;
using Fusi.Antiquity.Chronology;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;

namespace CadmusTool.Commands
{
    public sealed class SeedDatabaseCommand : ICommand
    {
        private readonly IConfiguration _config;
        private readonly RepositoryService _repositoryService;
        private readonly string _database;
        private readonly string _profilePath;
        private readonly string _facets;
        private readonly int _count;
        private readonly Random _random;
        private readonly Regex _crLfRegex;
        private DataProfile _profile;
        private bool _textPartAdded;

        public SeedDatabaseCommand(AppOptions options, string database,
            string profilePath, string facets, int count)
        {
            _config = options.Configuration;
            _repositoryService = new RepositoryService(_config);
            _database = database
                ?? throw new ArgumentNullException(nameof(database));
            _profilePath = profilePath
                ?? throw new ArgumentNullException(nameof(profilePath));
            _facets = facets
                ?? throw new ArgumentNullException(nameof(facets));
            _count = count;
            _random = new Random();
            _crLfRegex = new Regex(@"\r?\n");
        }

        public static void Configure(CommandLineApplication command,
            AppOptions options)
        {
            command.Description = "Create and seed a Cadmus MongoDB database " +
                                  "with the specified profile and number of items.";
            command.HelpOption("-?|-h|--help");

            CommandArgument databaseArgument = command.Argument("[database]",
                "The database name");

            CommandArgument profileArgument = command.Argument("[profile]",
                "The profile JSON file path");

            CommandArgument facetsArgument = command.Argument("[facets]",
                "The facet(s) to be applied to inserted items, separated by commas");

            CommandOption countOption = command.Option("-c|--count", "Items count",
                CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {
                int count = 100;
                if (countOption.HasValue()) int.TryParse(countOption.Value(), out count);

                options.Command = new SeedDatabaseCommand(options,
                    databaseArgument.Value,
                    profileArgument.Value,
                    facetsArgument.Value,
                    count);
                return 0;
            });
        }

        private static string LoadProfile(string path)
        {
            using StreamReader reader = File.OpenText(path);
            return reader.ReadToEnd();
        }

        private void AddCategoriesPart(IItem item, int index)
        {
            Thesaurus thesaurus = Array.Find(_profile.Thesauri,
                t => t.Id == "categories@en");
            if (thesaurus == null) return;

            CategoriesPart part = new CategoriesPart
            {
                ItemId = item.Id,
                CreatorId = item.CreatorId,
                UserId = item.UserId
            };

            int desired = (index & 1) == 1 ? 2 : 1;
            IList<ThesaurusEntry> entries = thesaurus.GetEntries();

            while (part.Categories.Count < desired)
            {
                int i = _random.Next(0, entries.Count);
                if (!part.Categories.Contains(entries[i].Id))
                    part.Categories.Add(entries[i].Id);
            }

            item.Parts.Add(part);
        }

        private void AddKeywordsPart(IItem item, int index)
        {
            KeywordsPart part = new KeywordsPart
            {
                ItemId = item.Id,
                CreatorId = item.CreatorId,
                UserId = item.UserId
            };

            int desired = (index & 1) == 1 ? 2 : 1;
            while (part.Keywords.Count < desired)
            {
                int n = _random.Next(1, 20);
                string keyword = NumberToWords.Convert(n);
                if (part.Keywords.All(k => k.Value != keyword))
                {
                    part.Keywords.Add(new Keyword
                    {
                        Language = "eng",
                        Value = keyword
                    });
                }
            }

            item.Parts.Add(part);
        }

        private void AddNotePart(IItem item)
        {
            NotePart part = new NotePart
            {
                ItemId = item.Id,
                CreatorId = item.CreatorId,
                UserId = item.UserId,
                Text = LoremIpsumGenerator.Generate(_random.Next(10, 60), 12)
            };

            item.Parts.Add(part);
        }

        private Datation GetRandomDatation()
        {
            if (_random.Next(0, 5) == 0)
            {
                return new Datation
                {
                    Value = _random.Next(-8, 6),
                    IsCentury = true,
                    IsApproximate = _random.Next(0, 10) == 0
                };
            }

            int year = _random.Next(-753, 477);
            short day = 0, month = 0;
            if (_random.Next(0, 10) == 0)
            {
                day = (short)_random.Next(1, 29);
                month = (short)_random.Next(1, 13);
            }
            return new Datation
            {
                Value = year,
                Day = day,
                Month = month
            };
        }

        private void AddHistoricalDatePart(IItem item)
        {
            HistoricalDate date = new HistoricalDate();

            if (_random.Next(1, 10) == 0)
            {
                date.SetStartPoint(GetRandomDatation());
                Datation b = date.A.Clone();
                b.Value++;
                date.SetEndPoint(b);
            }
            else
            {
                date.SetSinglePoint(GetRandomDatation());
            }

            HistoricalDatePart part = new HistoricalDatePart
            {
                Date = date
            };
            item.Parts.Add(part);
        }

        private string GetRandomLocation(TokenTextPart part)
        {
            int i = _random.Next(0, part.Lines.Count);
            int y = i + 1;
            int x = _random.Next(0, part.Lines[i].GetTokens().Length) + 1;
            return $"{y}.{x}";
        }

        private void AddCommentLayerFragment(string location,
            TokenTextLayerPart<CommentLayerFragment> part)
        {
            part.Fragments.Add(new CommentLayerFragment
            {
                Location = location,
                Text = LoremIpsumGenerator.Generate(_random.Next(5, 20), 12)
            });
        }

        private void AddQuotationLayerFragment(string location,
            TokenTextLayerPart<QuotationLayerFragment> part)
        {
            part.Fragments.Add(new QuotationLayerFragment
            {
                Location = location,
                Author = "au-" + new string((char) (65 + _random.Next(0, 27)), 1),
                Work = "wk-" + new string((char) (65 + _random.Next(0, 27)), 1),
                WorkLoc = $"{_random.Next(1, 24)}.{_random.Next(1, 1001)}",
                VariantOf = _random.Next(1, 10) == 3 ?
                    LoremIpsumGenerator.Generate(_random.Next(3, 20), 12) : null
            });
        }

        private void AddApparatusLayerFragment(string location,
            TokenTextLayerPart<ApparatusLayerFragment> part)
        {
            ApparatusLayerFragment fragment = new ApparatusLayerFragment
            {
                Location = location,
                Type = (LemmaVariantType) _random.Next(0, 4)
            };

            string[] authors = {"Alpha", "Beta", "Gamma"};
            for (int i = 0;
                i < (fragment.Type == LemmaVariantType.Note? _random.Next(0, 3):
                _random.Next(1, 3)); i++)
            {
                fragment.Authors.Add(authors[i]);
            }

            if (fragment.Type == LemmaVariantType.Note)
            {
                fragment.Note = LoremIpsumGenerator.Generate(10, 50);
            }
            else
            {
                fragment.Value = LoremIpsumGenerator.Generate(3, 10);
                fragment.IsAccepted = _random.Next(0, 2) == 1;
            }

            part.Fragments.Add(fragment);
        }

        private void AddTokenTextPart(IItem item)
        {
            TokenTextPart part = new TokenTextPart
            {
                ItemId = item.Id,
                CreatorId = item.CreatorId,
                UserId = item.UserId,
            };
            string text = LoremIpsumGenerator.Generate(_random.Next(12, 36), 6);
            int y = 0;
            foreach (string line in _crLfRegex.Split(text))
            {
                part.Lines.Add(new TextLine
                {
                    Y = ++y,
                    Text = line
                });
            }
            item.Parts.Add(part);

            // layer fragments:
            // comment
            TokenTextLayerPart<CommentLayerFragment> partComments =
                new TokenTextLayerPart<CommentLayerFragment>
            {
                    ItemId = item.Id,
                    CreatorId = item.CreatorId,
                    UserId = item.UserId
                    // RoleId is set by part ctor
            };
            AddCommentLayerFragment(GetRandomLocation(part), partComments);
            item.Parts.Add(partComments);

            // quotation
            if (_random.Next(0, 3) == 0 || !_textPartAdded)
            {
                TokenTextLayerPart<QuotationLayerFragment> partQuotes =
                    new TokenTextLayerPart<QuotationLayerFragment>
                {
                        ItemId = item.Id,
                        CreatorId = item.CreatorId,
                        UserId = item.UserId
                        // RoleId is set by part ctor
                };
                AddQuotationLayerFragment(GetRandomLocation(part), partQuotes);
                item.Parts.Add(partQuotes);
            }

            // apparatus
            if (_random.Next(0, 3) == 0 || !_textPartAdded)
            {
                TokenTextLayerPart<ApparatusLayerFragment> partApparatus =
                    new TokenTextLayerPart<ApparatusLayerFragment>
                    {
                        ItemId = item.Id,
                        CreatorId = item.CreatorId,
                        UserId = item.UserId,
                        // RoleId is set by part ctor
                    };
                AddApparatusLayerFragment(GetRandomLocation(part), partApparatus);
                item.Parts.Add(partApparatus);
            }

            _textPartAdded = true;
        }

        private void AddParts(IItem item, int index)
        {
            // categories, keywords, note, tokentext
            switch (index % 4)
            {
                case 0: // C
                    AddCategoriesPart(item, index);
                    if (_random.Next(0, 3) == 0) AddHistoricalDatePart(item);
                    break;
                case 1: // CK
                    AddCategoriesPart(item, index);
                    AddKeywordsPart(item, index);
                    break;
                case 2: // CKN
                    AddCategoriesPart(item, index);
                    AddKeywordsPart(item, index);
                    AddNotePart(item);
                    break;
                case 3: // CKNT
                    AddCategoriesPart(item, index);
                    AddKeywordsPart(item, index);
                    AddNotePart(item);
                    AddTokenTextPart(item);
                    break;
            }
        }

        public Task Run()
        {
            Console.WriteLine("SEED DATABASE\n" +
                              $"Database: {_database}\n" +
                              $"Profile file: {_profilePath}\n" +
                              $"Facets: {_facets}\n" +
                              $"Count: {_count}\n");
            Serilog.Log.Information("SEED DATABASE: " +
                         $"Database: {_database}, " +
                         $"Profile file: {_profilePath}, " +
                         $"Facets: {_facets}, " +
                         $"Count: {_count}");

            // create database if not exists
            string connection = string.Format(CultureInfo.InvariantCulture,
                _config.GetConnectionString("Mongo"),
                _database);

            IDatabaseManager manager = new MongoDatabaseManager();

            string profileContent = LoadProfile(_profilePath);
            IDataProfileSerializer serializer = new JsonDataProfileSerializer();
            _profile = serializer.Read(profileContent);

            if (!manager.DatabaseExists(connection))
            {
                Console.WriteLine("Creating database...");
                Serilog.Log.Information($"Creating database {_database}...");

                manager.CreateDatabase(connection, _profile);

                Console.WriteLine("Database created.");
                Serilog.Log.Information("Database created.");
            }

            Console.WriteLine("Creating repository...");
            Serilog.Log.Information("Creating repository...");

            ICadmusRepository repository =
                _repositoryService.CreateRepository(_database);

            Console.Write("Seeding items (.=10): ");
            int facetIndex = 0;
            string[] facets = _facets.Split(',');
            try
            {
                for (int i = 0; i < _count; i++)
                {
                    string oddEven = (i & 1) == 1 ? "odd" : "even";
                    Item item = new Item
                    {
                        Title = $"Item #{i + 1}",
                        Description = $"Description for {oddEven} " +
                                      $"item number {NumberToWords.Convert(i + 1)}.",
                        FacetId = facets[facetIndex++],
                        SortKey = $"item-{i + 1:00000}",
                        CreatorId = oddEven,
                        UserId = oddEven,
                        Flags = (i & 1) == 1 ? 1 : 0
                    };

                    // add item and its parts
                    repository.AddItem(item, false);
                    AddParts(item, i);
                    foreach (IPart part in item.Parts)
                        repository.AddPart(part, false);

                    if (facetIndex >= facets.Length) facetIndex = 0;
                    if (i % 10 == 0) Console.Write('.');
                }

                Console.WriteLine(" completed.");
                Serilog.Log.Information("Seed completed");
                return Task.FromResult(0);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, ex.Message);
                throw;
            }
        }
    }
}
