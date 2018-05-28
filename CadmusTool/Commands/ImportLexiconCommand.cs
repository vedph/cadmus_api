using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Cadmus.Core.Blocks;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using Cadmus.Lexicon.Parts;
using Cadmus.Mongo;
using Cadmus.Parts.General;
using Fusi.Antiquity.Chronology;
using Fusi.Text.Unicode;
using Fusi.Tools;
using Fusi.Tools.Text;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using NLog;

namespace CadmusTool.Commands
{
    public sealed class ImportLexiconCommand : ICommand
    {
        private const string USERID = "zeus";

        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly string _inputDir;
        private readonly string _database;
        private readonly string _profileText;
        private readonly bool _preflight;
        private readonly UniData _ud;
        private readonly HashSet<string> _tAllowedChildren;
        private readonly HashSet<string> _exAllowedChildren;
        private readonly HashSet<string> _extAllowedChildren;
        private readonly Regex _lemmaHomRegex;
        private readonly Regex _tagRegex;
        private readonly Regex _wsRegex;
        private readonly TextCutterOptions _textCutterOptions;
        private DataProfile _profile;

        public ImportLexiconCommand(AppOptions options, string inputDir, string database,
            string profile, bool preflight)
        {
            _config = options.Configuration;
            _logger = LogManager.GetCurrentClassLogger();
            _inputDir = inputDir ?? throw new ArgumentNullException(nameof(inputDir));
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _profileText = profile ?? throw new ArgumentNullException(nameof(profile));
            _preflight = preflight;
            _ud = new UniData();

            _tAllowedChildren = new HashSet<string>
            {
                "hi", "qf", "x", "xr"
            };
            _exAllowedChildren = new HashSet<string>
            {
                "hi", "q", "x"
            };
            _extAllowedChildren = new HashSet<string>
            {
                "hi", "x"
            };

            _lemmaHomRegex = new Regex(@"^(?<l>[^(]+)(?:\s*\((?<h>\d+)\))?");
            _tagRegex = new Regex(@"<[^>]+>");
            _wsRegex = new Regex(@"\s+");
            _textCutterOptions = new TextCutterOptions
            {
                LimitAsPercents = false,
                LineFlattening = true,
                MaxLength = 200,
                Ellipsis = "\u2026"
            };
        }

        public static void Configure(CommandLineApplication command, AppOptions options)
        {
            command.Description = "Create and seed a Cadmus MongoDB database " +
                                  "with the specified profile importing LEX files.";
            command.HelpOption("-?|-h|--help");

            CommandArgument inputArgument = command.Argument("[input]",
              "The input directory");

            CommandArgument databaseArgument = command.Argument("[database]",
              "The database name");

            CommandArgument profileArgument = command.Argument("[profile]",
              "The profile XML file path");

            CommandOption preflightOption = command.Option("-p|--preflight",
                "Preflight: do not write to database", CommandOptionType.NoValue);

            command.OnExecute(() =>
            {
                options.Command = new ImportLexiconCommand(options,
                    inputArgument.Value,
                    databaseArgument.Value,
                    profileArgument.Value,
                    preflightOption.HasValue());
                return 0;
            });
        }

        private static string LoadProfile(string path)
        {
            using (StreamReader reader = File.OpenText(path))
            {
                return reader.ReadToEnd();
            }
        }

        private static int GetHomographNumber(XElement item)
        {
            string hom = item.Descendants("hom").FirstOrDefault()?.Value;
            return hom != null ? Int32.Parse(hom, CultureInfo.InvariantCulture) : 0;
        }

        private string BuildWordKey(string lemma, int hom)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in lemma.Trim())
            {
                if (Char.IsWhiteSpace(c) &&
                    (sb.Length == 0 || sb[sb.Length - 1] != '_')) sb.Append('_');
                if (Char.IsLetterOrDigit(c) || c == '\'')
                  sb.Append(_ud.GetSegment(Char.ToLowerInvariant(c), true));
            }

            if (hom > 0) sb.Append('-').Append(hom);

            return sb.ToString();
        }

        private void ReadWordFormPart(XElement itemElement, Item item)
        {
            // item/lemma, item/hom, item/prelem, item/postlem
            WordFormPart part = new WordFormPart
            {
                ItemId = item.Id,
                UserId = USERID,
                Lemma = itemElement.Element("lemma").Value,
                Homograph = GetHomographNumber(itemElement),
                Prelemma = itemElement.Element("prelem")?.Value,
                Postlemma = itemElement.Element("postlem")?.Value
            };

            // /item/phon/pron?
            XElement pron = itemElement.Element("phon")?.Element("pron");
            if (pron != null)
            {
                part.Pronunciations.Add(new UsedWord
                {
                    Usage = pron.ReadOptionalAttribute("t", null),
                    Value = pron.Value
                });
            }

            // /item/var* @t? postlem? prelem?
            foreach (XElement var in itemElement.Elements("var"))
            {
                string usage = var.ReadOptionalAttribute("t", null);
                if (var.HasElements)
                {
                    part.Variants.Add(new VariantWord
                    {
                        Usage = usage,
                        Prelemma = var.Element("prelem")?.Value,
                        Value = var.Nodes().OfType<XText>().First().Value,
                        Postlemma = var.Element("postlem")?.Value
                    });
                }
                else
                {
                    part.Variants.Add(new VariantWord
                    {
                        Usage = usage,
                        Value = var.Value
                    });
                }
            }

            item.Parts.Add(part);
        }

        private XElement FilterElement(XElement element, HashSet<string> allowedChildren)
        {
            XElement result = new XElement(element.Name, element.Attributes());

            foreach (XNode node in element.Nodes())
            {
                if (node is XText txt)
                {
                    result.Add(txt);
                    continue;
                }
                if (node is XElement child && allowedChildren.Contains(child.Name.LocalName))
                    result.Add(child);
            }

            return result;
        }

        private Tuple<string,int> ParseLemmaHom(string text)
        {
            Match m = _lemmaHomRegex.Match(text);
            if (!m.Success) return Tuple.Create(text, 0);

            return Tuple.Create(
                m.Groups["l"].Value,
                m.Groups["h"].Length > 0?
                Int32.Parse(m.Groups["h"].Value, CultureInfo.InvariantCulture) : 0);
        }

        private void ReadWordSenseParts(XElement itemElement, Item item)
        {
            // item/gc*
            int gcLetter = (char)('a' - 1);
            int rank = 0;
            foreach (XElement gc in itemElement.Elements("gc"))
            {
                gcLetter++;
                string pos = gc.Element("pos")?.Value;

                // item/gc/sns+
                foreach (XElement sns in gc.Elements("sns"))
                {
                    rank++;
                    WordSensePart part = new WordSensePart
                    {
                        ItemId = item.Id,
                        UserId = USERID,
                        Rank = rank,
                        SenseId = $"{(char)gcLetter}.{rank}",
                        // assume for sns the 1st tip/usg child if any
                        Tip = sns.Element("tip")?.Value,
                        Usage = sns.Element("usg")?.Value
                    };

                    // item/gc/sns/t+
                    // Ideally we should have only 1 t for 1 sns, but who knows
                    StringBuilder sb = new StringBuilder();
                    foreach (XElement t in sns.Elements("t"))
                    {
                        // by now, as a best guess, just keep only hi, qf, x, xr, and text
                        XElement filtered = FilterElement(t, _tAllowedChildren);
                        if (sb.Length > 0) sb.Append(" | ");
                        sb.Append(filtered.Value);
                    }
                    // TODO determine when it's a list of words
                    part.Explanation.Value = sb.ToString();

                    // item/gc/sns/ex+ and its ext if any
                    string lemma = itemElement.Element("lemma").Value;
                    foreach (XElement ex in sns.Elements("ex"))
                    {
                        WordExample example = new WordExample
                        {
                            Usage = ex.Element("usg")?.Value,
                            Tip = ex.Element("tip")?.Value,
                            Source = ex.Element("src")?.Value
                        };
                        // filter ex
                        XElement filtered = FilterElement(ex, _exAllowedChildren);
                        // expand q in ex (assume expansion is lemma)
                        foreach (XElement q in filtered.Elements("q").ToList()) q.Value = lemma;
                        example.Value = filtered.Value;

                        // ext, filtered
                        XElement next = ex.ElementsAfterSelf().FirstOrDefault();
                        if (next?.Name.LocalName == "ext")
                            example.Explanation = FilterElement(next, _extAllowedChildren).Value;

                        part.Examples.Add(example);
                    }

                    // item/gc/sns/related*
                    foreach (XElement related in sns.Elements("related"))
                    {
                        string type = related.ReadOptionalAttribute("t", null);
                        foreach (XElement qf in related.Elements("qf"))
                        {
                            var t = ParseLemmaHom(qf.Value);
                            part.Links.Add(new WordLink
                            {
                                Type = type ?? "-",
                                TargetId = BuildWordKey(t.Item1, t.Item2),
                                TargetLabel = qf.Value
                            });
                        }
                    }

                    item.Parts.Add(part);
                }
            }
        }

        private void ReadWordEtymologyPart(XElement itemElement, Item item)
        {
            XElement etym = itemElement.Element("etym");
            if (etym == null) return;

            // etym/dat
            string dateText = etym.Element("dat")?.Value.Trim();
            HistoricalDate date = ZingDateParser.Parse(dateText);

            StringBuilder sb = new StringBuilder(etym.ToString(SaveOptions.DisableFormatting));
            sb.Replace("<qf>", "_");
            sb.Replace("</qf>", "_");
            string discussion = Regex.Replace(sb.ToString(), @"<[^>]+>", "").Trim();
            if (discussion == "$") discussion = null;

            WordEtymologyPart part = new WordEtymologyPart
            {
                ItemId = item.Id,
                UserId = USERID,
                Date = date,
                Discussion = discussion == dateText? null : discussion
            };

            item.Parts.Add(part);
        }

        private string HiRenderingToMd(string rendering, bool open)
        {
            string sorted = new string(rendering.ToCharArray().OrderBy(c => c).ToArray());
            switch (sorted)
            {
                case "b":
                    return "__";
                case "i":
                    return "_";
                case "u":
                    return "~~";
                case "bi":
                    return open? "__*" : "*__";
                case "bu":
                    return open ? "__~~" : "~~__";
                case "biu":
                    return open ? "__~~_" : "_~~__";
                case "iu":
                    return open ? "~~_" : "_~~";
                default:
                    return "";
            }
        }

        private void RenderAside(XElement element, StringBuilder sb)
        {
            foreach (XNode node in element.Nodes())
            {
                if (node is XElement e)
                {
                    switch (e.Name.LocalName)
                    {
                        case "head":
                            sb.AppendLine();
                            sb.Append('#');
                            RenderAside(e, sb);
                            break;
                        case "p":
                            sb.AppendLine();
                            if (e.Ancestors().Any(a => a.Name.LocalName == "list"))
                                sb.Append("- ");
                            RenderAside(e, sb);
                            break;
                        case "hi":
                            sb.Append(HiRenderingToMd(e.Attribute("r").Value, true));
                            RenderAside(e, sb);
                            sb.Append(HiRenderingToMd(e.Attribute("r").Value, false));
                            break;
                        case "qf":
                            if (e.Ancestors().All(a => a.Name.LocalName != "hi"))
                                sb.Append('_').Append(e.Value).Append('_');
                            else
                                RenderAside(e, sb);
                            break;
                        default:
                            RenderAside(e, sb);
                            break;
                    }
                    continue;
                }
                if (node is XText txt) sb.Append(txt.Value);
            }
        }

        private void ReadNotePart(XElement itemElement, Item item)
        {
            XElement aside = itemElement.Element("aside");
            if (aside == null) return;

            StringBuilder sb = new StringBuilder();
            RenderAside(aside, sb);

            NotePart part = new NotePart
            {
                ItemId = item.Id,
                UserId = USERID,
                Tag = aside.ReadOptionalAttribute("t", null),
                Text = sb.ToString().Trim()
            };

            item.Parts.Add(part);
        }

        private void ReadParts(XElement itemElement, Item item)
        {
            ReadWordFormPart(itemElement, item);
            ReadWordSenseParts(itemElement, item);
            ReadWordEtymologyPart(itemElement, item);
            ReadNotePart(itemElement, item);
        }

        private string BuildItemDescription(XElement item)
        {
            XElement prepared = XElement.Parse(
                item.ToString(SaveOptions.DisableFormatting),
                LoadOptions.PreserveWhitespace);

            // ensure there is a space before gc/sns
            foreach (XElement gcOrSns in prepared.Descendants()
                .Where(e => e.Name.LocalName == "gc" || e.Name.LocalName == "sns")
                .ToList())
            {
                gcOrSns.AddBeforeSelf(" ");
            }

            // remove any element beginning with _ (_bm etc),
            // or containing only $ (there is a deplorable habit of
            // inserting empty elements with this placeholder)
            foreach (XElement meta in prepared.Descendants()
                .Where(e =>
                    e.Name.LocalName.StartsWith("_", StringComparison.Ordinal) ||
                    (!e.HasElements && e.Value.Trim() == "$"))
                .ToList())
            {
                meta.Remove();
            }

            // extract text and normalize its spaces
            string xml = prepared.ToString(SaveOptions.DisableFormatting);
            string txt = _tagRegex.Replace(xml, "");
            txt = _wsRegex.Replace(txt, " ");

            // cut the result
            return TextCutter.Cut(txt, _textCutterOptions);
        }

        public Task Run()
        {
            Console.WriteLine("IMPORT DATABASE\n" +
                              $"Input directory: {_inputDir}\n" +
                              $"Database: {_database}\n" +
                              $"Profile file: {_profileText}\n" +
                              $"Preflight: {(_preflight? "yes" : "no")}");
            _logger.Info("IMPORT DATABASE: " +
                         $"Input directory: {_inputDir}, " +
                         $"Database: {_database}, " +
                         $"Profile file: {_profileText}, " +
                         $"Preflight: {_preflight}");

            try
            {
                // create database if not exists
                string connection = String.Format(CultureInfo.InvariantCulture,
                    _config.GetConnectionString("Mongo"),
                    _database);
                ICadmusManager manager = new MongoCadmusManager();
                string profileContent = LoadProfile(_profileText);

                if (!_preflight)
                {
                    if (!manager.DatabaseExists(connection))
                    {
                        Console.WriteLine("Creating database...");
                        _logger.Info($"Creating database {_database}...");

                        manager.CreateDatabase(connection, profileContent);

                        Console.WriteLine("Database created.");
                        _logger.Info("Database created.");
                    }
                }
                _profile = new DataProfile(XElement.Parse(profileContent));

                Console.WriteLine("Creating repository...");
                _logger.Info("Creating repository...");

                ICadmusRepository repository = RepositoryService.CreateRepository(_database,
                    _config.GetConnectionString("Mongo"));

                foreach (string lexFile in Directory.GetFiles(_inputDir, "??.xml").OrderBy(s => s))
                {
                    XDocument doc = XDocument.Load(lexFile, LoadOptions.PreserveWhitespace);
                    if (doc.Root == null) continue;

                    foreach (XElement itemElement in doc.Root.Elements("item"))
                    {
                        // read essential metadata
                        string id = itemElement.Attribute("id").Value;
                        string sid = itemElement.Attribute("sid").Value;
                        string lemma = itemElement.Element("lemma").Value;
                        int hom = GetHomographNumber(itemElement);
                        int flags = itemElement.ReadOptionalAttribute("flags", 0);
                        // TODO parent...

                        // item
                        Item item = new Item
                        {
                            Id = id,
                            Title = BuildWordKey(lemma, hom),
                            Description = BuildItemDescription(itemElement),
                            FacetId = "facet-lex-word",
                            SortKey = sid,
                            UserId = USERID,
                            Flags = flags
                        };
                        Console.WriteLine(item.Title);

                        // read parts
                        // first remove all the _bm so we don't accidentally include metatext
                        XElement filtered = XElement.Parse(itemElement.ToString(SaveOptions.DisableFormatting),
                            LoadOptions.PreserveWhitespace);
                        foreach (XElement bm in filtered.Descendants("_bm").ToList())
                            bm.Remove();

                        ReadParts(filtered, item);

                        if (!_preflight)
                        {
                            repository.AddItem(item, false);
                            foreach (IPart part in item.Parts)
                                repository.AddPart(part, false);
                        }
                    }
                }

                Console.WriteLine(" completed.");
                _logger.Info("Import completed");
                return Task.FromResult(0);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.ToString);
                throw;
            }
        }
    }
}
