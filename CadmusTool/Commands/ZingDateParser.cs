using Fusi.Antiquity;
using Fusi.Antiquity.Chronology;
using Fusi.Tools;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CadmusTool.Commands
{
    /// <summary>
    /// Zingarelli date parser.
    /// </summary>
    /// <remarks>
    /// Zingarelli dates come in these patterns:
    /// <list type="numbered">
    ///		<item>
    ///			<term>1 (prefixes)</term>
    ///			<description>: av. or pt. (defining a terminus ante or post, imply 1 non-century value),
    ///   OR sec. (implies 1 or 2 century values).</description>
    ///		</item>
    ///		<item>
    ///			<term>2* (values)</term>
    ///			<description>year or century divided by -.
    ///			</description>
    ///		</item>
    ///		<item>
    ///			<term>3 (suffix)</term>
    ///			<description>: ca. (=about).</description>
    ///		</item>
    /// </list>
    /// <para>Samples:</para>
    /// <code>--1 point:
    ///  1263 [ca.]
    ///  av.1263 [ca.]
    ///  pt.1263 [ca.]
    ///  sec.XII [ca.]
    /// --2 points:
    ///  1263-1321 [ca.]
    ///  sec.XII-XIII [ca.]</code>
    /// </remarks>
    public static class ZingDateParser
    {
        // (?:
        //   (?:sec\.?\s*
        //    (?<cent>[IVX]+)
        //    (?:
        //       \s*-\s*(?<cent2>[IVX]+)
        //    )?
        // )
        // |
        // (?:(?<ap>(?:av|pt)\.?)?\s*
        //    (?<val>\d+)
        //    (?:
        //       \s*-\s*(?<val2>\d+)
        //    )?
        // )
        // )
        // (?:\s*(?<ca>ca\.?))?
        static private Regex _dateRegex = new Regex(
            @"(?:" +
            @"  (?:sec\.?\s*" +
            @"   (?<cent>[IVX]+)" +
            @"   (?:" +
            @"      \s*-\s*(?<cent2>[IVX]+)" +
            @"   )?" +
            @")" +
            @"|" +
            @"(?:(?<ap>(?:av|pt)\.?)?\s*" +
            @"   (?<val>\d+)" +
            @"   (?:" +
            @"      \s*-\s*(?<val2>\d+)" +
            @"   )?" +
            @")" +
            @")" +
            @"(?:\s*(?<ca>ca\.?))?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        public static HistoricalDate Parse(string text)
        {
            if (String.IsNullOrEmpty(text)) return null;

            // --1 term:
            //  1263 [ca.]
            //  av.1263 [ca.]
            //  pt.1263 [ca.]
            //  sec.XII [ca.]
            // --2 terms:
            //  1263 [ca.] - 1321 [ca.]
            //  sec.XII [ca.] - XIII [ca.]</code>

            Match m = _dateRegex.Match(text);
            if (!m.Success) return null;

            HistoricalDate date = new HistoricalDate();

            // ap: implies 1 term only, non-century
            if (m.Groups["ap"].Length > 0)
            {
                // av
                if (m.Groups["ap"].Value.ToLowerInvariant().StartsWith("av", StringComparison.OrdinalIgnoreCase))
                    date.SetEndPoint(new Datation(Int16.Parse(m.Groups["val"].Value, CultureInfo.InvariantCulture)));
                // pt
                else
                    date.SetStartPoint(new Datation(Int16.Parse(m.Groups["val"].Value, CultureInfo.InvariantCulture)));
            } // (ap)

            else
            {
                int n1, n2 = 0;
                bool cent1 = false, cent2 = false;

                // century or centuries
                if (m.Groups["cent"].Length > 0)
                {
                    // get 1st century
                    n1 = (short)RomanNumber.FromRoman(m.Groups["cent"].Value);
                    cent1 = true;

                    // get 2nd century if any
                    if (m.Groups["cent2"].Length > 0)
                    {
                        n2 = (short)RomanNumber.FromRoman(m.Groups["cent2"].Value);
                        cent2 = true;
                    }
                } // (cent)
                else
                {
                    // val
                    n1 = Int16.Parse(m.Groups["val"].Value, CultureInfo.InvariantCulture);
                    // val2, if any
                    if (m.Groups["val2"].Length > 0)
                        n2 = Int16.Parse(m.Groups["val2"].Value, CultureInfo.InvariantCulture);
                } // (!cent)

                if (n2 != 0)
                {
                    date.SetStartPoint(new Datation(n1, cent1));
                    date.SetEndPoint(new Datation(n2, cent2));
                }
                else date.SetSinglePoint(new Datation(n1, cent1));
            } // (!ap)

            // about modifier
            if (m.Groups["ca"].Length > 0)
            {
                if (date.GetDateType() == HistoricalDateType.Range)
                {
                    date.A.IsApproximate = true;
                    date.B.IsApproximate = true;
                }
                else date.A.IsApproximate = true;
            }

            return date;
        }
    }
}
