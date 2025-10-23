using System;
using System.Text.RegularExpressions;
using static QuickInfo.NodeFactory;

namespace QuickInfo
{
    public class ProofConverter : IProcessor
    {
        public object GetResult(Query query)
        {
            if (query.IsHelp)
            {
                return HelpTable(
                    ("190 proof", "Convert proof to ABV"),
                    ("95 abv", "Convert ABV to proof"),
                    ("proof to abv", "Show conversion formula"));
            }

            var input = query.OriginalInput.Trim();

            // Check for general help triggers
            if (string.Equals(input, "proof to abv", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "abv to proof", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "proof", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "alcohol proof", StringComparison.OrdinalIgnoreCase))
            {
                return new object[]
                {
                    Answer("Proof = 2 × ABV%"),
                    SectionHeader("Common Values:"),
                    NameValueTable(entries: new[]
                    {
                        ("200 Proof:", "100% ABV (Pure ethanol)"),
                        ("190 Proof:", "95% ABV (Denatured ethanol)"),
                        ("160 Proof:", "80% ABV (High-proof spirits)"),
                        ("100 Proof:", "50% ABV"),
                        ("80 Proof:", "40% ABV (Standard spirits)")
                    })
                };
            }

            double value = 0;
            bool isProof = false;
            bool isABV = false;

            // Try to parse proof values like "190 proof", "95.5 proof"
            var proofMatch = Regex.Match(input, @"^(\d+(?:\.\d+)?)\s*(?:proof|prf|°)\b", RegexOptions.IgnoreCase);
            if (proofMatch.Success)
            {
                if (double.TryParse(proofMatch.Groups[1].Value, out value))
                {
                    isProof = true;
                }
            }

            // Try to parse ABV values like "95 abv", "40% abv", "50 percent"
            if (!isProof)
            {
                var abvMatch = Regex.Match(input, @"^(\d+(?:\.\d+)?)\s*%?\s*(?:abv|alcohol|percent)\b", RegexOptions.IgnoreCase);
                if (abvMatch.Success)
                {
                    if (double.TryParse(abvMatch.Groups[1].Value, out value))
                    {
                        isABV = true;
                    }
                }
            }

            if (!isProof && !isABV)
            {
                return null;
            }

            // Validate ranges
            if (isProof && (value < 0 || value > 200))
            {
                return FixedParagraph("Proof must be between 0 and 200 (0% to 100% ABV)");
            }

            if (isABV && (value < 0 || value > 100))
            {
                return FixedParagraph("ABV must be between 0% and 100%");
            }

            double proof = isProof ? value : value * 2;
            double abv = isABV ? value : value / 2;

            string classification = GetClassification(abv);

            return new object[]
            {
                Answer($"{proof:F1} Proof = {abv:F1}% ABV"),
                SectionHeader("Conversion Details:"),
                NameValueTable(entries: new[]
                {
                    ("Proof:", $"{proof:F1}°"),
                    ("ABV (Alcohol By Volume):", $"{abv:F1}%"),
                    ("Classification:", classification),
                    ("Formula:", "Proof = 2 × ABV%")
                })
            };
        }

        private string GetClassification(double abv)
        {
            if (abv >= 95)
                return "Pure/Denatured Ethanol";
            else if (abv >= 80)
                return "High-Proof Spirit";
            else if (abv >= 60)
                return "Overproof Spirit";
            else if (abv >= 40)
                return "Standard Spirit";
            else if (abv >= 20)
                return "Fortified Wine";
            else if (abv >= 12)
                return "Wine";
            else if (abv >= 4)
                return "Beer/Ale";
            else if (abv > 0)
                return "Low Alcohol";
            else
                return "Non-Alcoholic";
        }
    }
}
