namespace SharjahConferenceAPI.Tests;

using SharjahConferenceAPI.Utilities;
using Xunit;

public class FuzzyMatchHelperTests
{
    [Theory]
    [InlineData("Obeikan Education", "obeikan education")]
    [InlineData("Éditions Gallimard", "editions gallimard")]
    [InlineData("مجموعة العبيكان للتعليم", "مجموعه العبيكان للتعليم")] // Taa marbuta normalized
    [InlineData("دار المدى", "دار المدي")] // Alif maqsura normalized
    [InlineData("أثر للنشر", "اثر للنشر")] // Alef with hamza normalized
    [InlineData("إكسبو", "اكسبو")] // Alef with kasra normalized
    [InlineData("شَارِقَة", "شارقه")] // Tashkeel removed, Taa marbuta normalized
    public void Normalize_HandlesBilingualDiacriticsAndVariants(string input, string expected)
    {
        var normalized = FuzzyMatchHelper.Normalize(input);
        Assert.Equal(expected, normalized);
    }

    [Theory]
    // English Exact & Typo tests
    [InlineData("Obeikan", "Obeikan Education", true)]
    [InlineData("Obiekan", "Obeikan Education", true)] // Transposition typo (ie vs ei)
    [InlineData("Galimard", "Éditions Gallimard", true)] // Missing 'l' typo + accent stripped
    [InlineData("Gallimard", "Éditions Gallimard", true)] // Exact token match
    [InlineData("Suhrkamp Verlaag", "Suhrkamp Verlag", true)] // Double 'a' typo
    [InlineData("Dar Al Tanwer", "Dar Al Tanweer", true)] // Missing 'e' typo
    [InlineData("Maktabat Al Falah", "Maktabat Al Falah", true)] // Exact match
    [InlineData("Takwen", "Takween Publishing", true)] // Missing 'e' typo
    [InlineData("Completely Unrelated Query", "Obeikan Education", false)]
    // Arabic Exact & Typo tests
    [InlineData("العبيكان", "مجموعة العبيكان للتعليم", true)] // Token match
    [InlineData("عبيكان", "مجموعة العبيكان للتعليم", true)] // Without 'ال'
    [InlineData("ألعبيكان", "مجموعة العبيكان للتعليم", true)] // With Hamza variant
    [InlineData("العبيكن", "مجموعة العبيكان للتعليم", true)] // Typo missing Alef
    [InlineData("دار التنوير", "دار التنوير", true)] // Exact Arabic match
    [InlineData("التنوير", "دار التنوير", true)] // Single word token match
    [InlineData("تنوير", "دار التنوير", true)] // Without 'ال'
    [InlineData("غاليمار", "منشورات غاليمار", true)] // Token match
    [InlineData("منشورات تكوين", "منشورات تكوين", true)] // Exact Arabic match
    [InlineData("تكوين", "منشورات تكوين", true)] // Token match
    [InlineData("كتاب غير موجود", "دار التنوير", false)] // Unrelated Arabic query
    public void CalculateMatchScore_EvaluatesSimilarityCorrectly(string query, string target, bool shouldMatch)
    {
        var score = FuzzyMatchHelper.CalculateMatchScore(query, target);
        if (shouldMatch)
        {
            Assert.True(score >= 0.60, $"Expected match for query '{query}' against target '{target}', but got score {score:F2}");
        }
        else
        {
            Assert.True(score < 0.50, $"Expected no match for query '{query}' against target '{target}', but got score {score:F2}");
        }
    }

    [Fact]
    public void DamerauLevenshteinDistance_ComputesTranspositionsCorrectly()
    {
        // Transposition cost = 1
        var dist1 = FuzzyMatchHelper.DamerauLevenshteinDistance("ca", "ac");
        Assert.Equal(1, dist1);

        // Insertion cost = 1
        var dist2 = FuzzyMatchHelper.DamerauLevenshteinDistance("cat", "cats");
        Assert.Equal(1, dist2);

        // Deletion cost = 1
        var dist3 = FuzzyMatchHelper.DamerauLevenshteinDistance("cats", "cat");
        Assert.Equal(1, dist3);

        // Substitution cost = 1
        var dist4 = FuzzyMatchHelper.DamerauLevenshteinDistance("cat", "bat");
        Assert.Equal(1, dist4);
    }
}
