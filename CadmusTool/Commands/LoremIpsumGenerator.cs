using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CadmusTool.Commands
{
    // cfr. http://loremipsumhelper.codeplex.com/SourceControl/latest#LoremIpsum.cs

    internal static class LoremIpsumGenerator
    {
        private static readonly string[] _tokens;
        private static readonly Random _random;

        static LoremIpsumGenerator()
        {
            string text = LoadResourceText("CadmusTool.Assets.LoremIpsum.txt");
            _tokens = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            _random = new Random();
        }

        private static string[] RandomizeLines(string[] lines)
        {
            List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
            foreach (string line in lines)
                list.Add(new KeyValuePair<int, string>(_random.Next(), line));

            // sort the list by the random number
            var sorted = from item in list
                orderby item.Key
                select item;

            string[] result = new string[lines.Length];
            int index = 0;
            foreach (KeyValuePair<int, string> pair in sorted)
            {
                result[index] = pair.Value;
                index++;
            }

            return result;
        }

        private static string LoadResourceText(string name)
        {
            Assembly asm = typeof(LoremIpsumGenerator).GetTypeInfo().Assembly;
            using (StreamReader reader = new StreamReader
                (asm.GetManifestResourceStream(name), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        public static string Generate(int wordCount, int paragraphLength)
        {
            string[] tokens = RandomizeLines(_tokens);
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < wordCount; i++)
            {
                sb.Append(tokens[i]);
                sb.Append(' ');
                if (i > 0 && paragraphLength > 0 && i % paragraphLength == 0) sb.AppendLine();
            }

            return sb.ToString().TrimEnd() + ".";
        }
    }
}
