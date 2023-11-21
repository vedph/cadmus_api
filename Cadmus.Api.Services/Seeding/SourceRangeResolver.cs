using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Cadmus.Api.Services.Seeding;

/// <summary>
/// Source range resolver. This is used to resolve ranges in source names,
/// e.g. produce a range of source names from a pattern. The pattern
/// is specified as a source name with format
/// <c>* source first last padding</c>, where <c>source</c> is the source
/// name including the <c>{N}</c> placeholder representing a number,
/// <c>first</c> and <c>last</c> define the range (inclusive), and the
/// optional <c>padding</c> specifies the fix number of digits required.
/// For instance, say you are using as sources a set of file paths named
/// like <c>VERG-aene-app_00001.json</c> up to 449. You can specify all
/// of them with a single source name <c>* VERG-aene-app_{N}.json 1 449 5</c>,
/// and use this resolver to get all the source names.
/// </summary>
public static class SourceRangeResolver
{
    /// <summary>
    /// Resolves the specified source name.
    /// </summary>
    /// <param name="source">The source name.</param>
    /// <returns>Resolved source name(s).</returns>
    /// <exception cref="ArgumentNullException">source</exception>
    public static IEnumerable<string> Resolve(string source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return InnerResolve();

        IEnumerable<string> InnerResolve()
        {
            Match m = Regex.Match(source,
              @"^\*\s+(?<m>[^\s]+)\s+(?<f>\d+)\s+(?<l>\d+)(?:\s+(?<p>\d+))?");

            // not a pattern, just ret the literal source
            if (!m.Success)
            {
                yield return source;
                yield break;
            }

            // it's a pattern, resolve it
            int first = int.Parse(m.Groups["f"].Value,
                CultureInfo.InvariantCulture);
            int last = int.Parse(m.Groups["l"].Value,
                CultureInfo.InvariantCulture);
            int padding = m.Groups["p"].Length > 0
                ? int.Parse(m.Groups["p"].Value)
                : 0;

            StringBuilder sb = new();
            int i = m.Groups["m"].Value.IndexOf("{N}",
                StringComparison.Ordinal);

            // defensive: if no {N} just return the mask as a literal
            if (i == -1)
            {
                yield return m.Groups["m"].Value;
                yield break;
            }

            // resolve
            string prefix = m.Groups["m"].Value[..i];
            string? fmt = padding > 0 ? new string('0', padding) : null;
            string suffix = m.Groups["m"].Value[(i + 1)..];

            if (last < first) last = first;

            for (int n = first; n <= last; n++)
            {
                sb.Clear();
                sb.Append(prefix);
                sb.Append(fmt != null
                    ? n.ToString(fmt, CultureInfo.InvariantCulture)
                    : n.ToString(CultureInfo.InvariantCulture));
                sb.Append(suffix);
                yield return sb.ToString();
            }
        }
    }
}
