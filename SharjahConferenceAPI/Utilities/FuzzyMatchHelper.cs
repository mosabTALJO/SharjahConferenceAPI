namespace SharjahConferenceAPI.Utilities;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

/// <summary>
/// Provides bilingual (English and Arabic) string normalization, Damerau-Levenshtein edit distance,
/// and fuzzy token similarity algorithms tailored for conference publisher and participant searches.
/// </summary>
public static class FuzzyMatchHelper
{
    private static readonly char[] Separators = [' ', '-', '_', '.', ',', ':', ';', '(', ')', '[', ']', '/', '\\', '\''];

    /// <summary>
    /// Normalizes Arabic and English text by:
    /// 1. Lowercasing.
    /// 2. Decomposing Latin accents/diacritics (e.g., 'É' -> 'e').
    /// 3. Normalizing Arabic letter variations (Alef variants, Taa Marbuta, Yaa/Alif Maqsura, Hamzas).
    /// 4. Stripping Arabic Tashkeel (harakat) and Tatweel (kashida).
    /// 5. Removing punctuation and collapsing whitespace.
    /// </summary>
    public static string Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        // 1. Lowercase and decompose unicode for Latin diacritic stripping
        var lower = input.Trim().ToLowerInvariant();
        var normalizedString = lower.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalizedString.Length);

        foreach (var c in normalizedString)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            // Skip non-spacing marks (covers Latin accents, Arabic harakat/tashkeel \u064B..\u065F)
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            // Arabic-specific normalizations
            switch (c)
            {
                // Normalize all Alef variants to bare Alef (ا)
                case '\u0622': // آ
                case '\u0623': // أ
                case '\u0625': // إ
                case '\u0671': // ٱ
                    sb.Append('\u0627');
                    break;

                // Normalize Taa Marbuta (ة) to Haa (ه)
                case '\u0629': // ة
                    sb.Append('\u0647');
                    break;

                // Normalize Alif Maqsura (ى) to Yaa (ي)
                case '\u0649': // ى
                    sb.Append('\u064A');
                    break;

                // Normalize Hamza variants (ؤ, ئ)
                case '\u0624': // ؤ
                case '\u0626': // ئ
                    sb.Append('\u0621'); // ء
                    break;

                // Remove Arabic Tatweel / Kashida (ـ)
                case '\u0640':
                    break;

                default:
                    // Keep letters, digits, and whitespace; convert punctuation/separators to spaces
                    if (char.IsLetterOrDigit(c))
                    {
                        sb.Append(c);
                    }
                    else if (char.IsWhiteSpace(c) || IsPunctuationOrSeparator(c))
                    {
                        sb.Append(' ');
                    }
                    break;
            }
        }

        // Return with FormC and collapsed spaces
        var result = sb.ToString().Normalize(NormalizationForm.FormC);
        return CollapseSpaces(result);
    }

    /// <summary>
    /// Calculates a similarity score in the range [0.0, 1.0] between a user search query and a target name.
    /// Combines full-string similarity, substring containment, and token/n-gram level matching.
    /// </summary>
    public static double CalculateMatchScore(string query, string target)
    {
        var normQuery = Normalize(query);
        var normTarget = Normalize(target);

        if (string.IsNullOrEmpty(normQuery) || string.IsNullOrEmpty(normTarget))
        {
            return 0.0;
        }

        // 1. Exact match after normalization
        if (normQuery.Equals(normTarget, StringComparison.Ordinal))
        {
            return 1.0;
        }

        // 2. Substring match
        if (normTarget.Contains(normQuery, StringComparison.Ordinal))
        {
            // Boost if query constitutes most of the target
            var ratio = (double)normQuery.Length / normTarget.Length;
            return Math.Max(0.85, 0.95 * ratio);
        }

        if (normQuery.Contains(normTarget, StringComparison.Ordinal))
        {
            var ratio = (double)normTarget.Length / normQuery.Length;
            return Math.Max(0.80, 0.90 * ratio);
        }

        // 3. Full-string Damerau-Levenshtein similarity
        var fullStringSimilarity = CalculateStringSimilarity(normQuery, normTarget);

        // 4. Token-level matching (matches query words against target words, or handles Arabic "ال" prefix)
        var targetTokens = normTarget.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var queryTokens = normQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        double bestTokenSim = 0.0;

        // Compare single-word query against all tokens in target
        if (queryTokens.Length == 1)
        {
            var singleQuery = queryTokens[0];
            var queryWithoutAl = StripArabicArticle(singleQuery);

            foreach (var token in targetTokens)
            {
                var tokenWithoutAl = StripArabicArticle(token);

                var sim1 = CalculateStringSimilarity(singleQuery, token);
                var sim2 = CalculateStringSimilarity(queryWithoutAl, tokenWithoutAl);
                var maxSim = Math.Max(sim1, sim2);

                if (maxSim > bestTokenSim)
                {
                    bestTokenSim = maxSim;
                }
            }
        }
        else
        {
            // Multi-token query: check sliding window of target tokens of the same length
            int windowSize = queryTokens.Length;
            for (int i = 0; i <= targetTokens.Length - windowSize; i++)
            {
                var window = string.Join(" ", targetTokens.Skip(i).Take(windowSize));
                var sim = CalculateStringSimilarity(normQuery, window);
                if (sim > bestTokenSim)
                {
                    bestTokenSim = sim;
                }
            }

            // Also check token-by-token average of best matches
            double tokenSum = 0.0;
            foreach (var qToken in queryTokens)
            {
                var qWithoutAl = StripArabicArticle(qToken);
                double bestForQ = 0.0;
                foreach (var tToken in targetTokens)
                {
                    var tWithoutAl = StripArabicArticle(tToken);
                    var sim = Math.Max(CalculateStringSimilarity(qToken, tToken), CalculateStringSimilarity(qWithoutAl, tWithoutAl));
                    if (sim > bestForQ) bestForQ = sim;
                }
                tokenSum += bestForQ;
            }
            var avgTokenSim = tokenSum / queryTokens.Length;
            if (avgTokenSim > bestTokenSim)
            {
                bestTokenSim = avgTokenSim;
            }
        }

        // Return highest similarity score found across full string and token match
        return Math.Max(fullStringSimilarity, bestTokenSim);
    }

    /// <summary>
    /// Computes normalized similarity [0.0, 1.0] based on Damerau-Levenshtein distance.
    /// </summary>
    public static double CalculateStringSimilarity(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2)) return 1.0;
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return 0.0;
        if (s1.Equals(s2, StringComparison.Ordinal)) return 1.0;

        int maxLen = Math.Max(s1.Length, s2.Length);
        int distance = DamerauLevenshteinDistance(s1, s2);

        return Math.Max(0.0, 1.0 - (double)distance / maxLen);
    }

    /// <summary>
    /// Calculates Damerau-Levenshtein distance (insertions, deletions, substitutions, and transpositions).
    /// </summary>
    public static int DamerauLevenshteinDistance(string s1, string s2)
    {
        int len1 = s1.Length;
        int len2 = s2.Length;

        if (len1 == 0) return len2;
        if (len2 == 0) return len1;

        var d = new int[len1 + 1, len2 + 1];

        for (int i = 0; i <= len1; i++) d[i, 0] = i;
        for (int j = 0; j <= len2; j++) d[0, j] = j;

        for (int i = 1; i <= len1; i++)
        {
            for (int j = 1; j <= len2; j++)
            {
                int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;

                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1,        // Deletion
                             d[i, j - 1] + 1),       // Insertion
                    d[i - 1, j - 1] + cost           // Substitution
                );

                // Check for transposition
                if (i > 1 && j > 1 && s1[i - 1] == s2[j - 2] && s1[i - 2] == s2[j - 1])
                {
                    d[i, j] = Math.Min(d[i, j], d[i - 2, j - 2] + cost);
                }
            }
        }

        return d[len1, len2];
    }

    private static string StripArabicArticle(string token)
    {
        // If token starts with normalized "ال" (\u0627\u0644) and is at least 4 characters
        if (token.Length >= 4 && token.StartsWith("\u0627\u0644", StringComparison.Ordinal))
        {
            return token[2..];
        }
        return token;
    }

    private static bool IsPunctuationOrSeparator(char c)
    {
        return Array.IndexOf(Separators, c) >= 0;
    }

    private static string CollapseSpaces(string text)
    {
        var sb = new StringBuilder(text.Length);
        bool inSpace = false;

        foreach (var c in text)
        {
            if (char.IsWhiteSpace(c))
            {
                if (!inSpace)
                {
                    sb.Append(' ');
                    inSpace = true;
                }
            }
            else
            {
                sb.Append(c);
                inSpace = false;
            }
        }

        return sb.ToString().Trim();
    }
}
