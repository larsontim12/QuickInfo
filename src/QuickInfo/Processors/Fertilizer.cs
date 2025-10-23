using System;
using System.Text.RegularExpressions;
using static QuickInfo.NodeFactory;

namespace QuickInfo
{
    public class Fertilizer : IProcessor
    {
        public object GetResult(Query query)
        {
            if (query.IsHelp)
            {
                return HelpTable(
                    ("fertilizer", "Show fertilizer information and N-P-K explanation"),
                    ("10-10-10", "Show analysis for N-P-K fertilizer grade"),
                    ("46-0-0", "Show analysis for N-P-K fertilizer grade"),
                    ("100 lbs 10-10-10", "Calculate actual nutrients in fertilizer"),
                    ("50 lbs N per acre at 46-0-0", "Calculate application rate"));
            }

            var input = query.OriginalInput.Trim();

            // General fertilizer info
            if (string.Equals(input, "fertilizer", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "npk", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "n-p-k", StringComparison.OrdinalIgnoreCase))
            {
                return new object[]
                {
                    Answer("N-P-K: Nitrogen - Phosphorus - Potassium"),
                    SectionHeader("Fertilizer Grade:"),
                    FixedParagraph("The three numbers on fertilizer represent the percentage by weight of:"),
                    NameValueTable(entries: new[]
                    {
                        ("N (Nitrogen):", "Promotes leaf and stem growth"),
                        ("P (Phosphorus):", "Promotes root and flower development"),
                        ("K (Potassium):", "Overall plant health and disease resistance")
                    }),
                    SectionHeader("Common Fertilizer Grades:"),
                    NameValueTable(entries: new[]
                    {
                        ("46-0-0:", "Urea (high nitrogen)"),
                        ("18-46-0:", "DAP (Diammonium phosphate)"),
                        ("0-0-60:", "Potash (high potassium)"),
                        ("10-10-10:", "Balanced all-purpose"),
                        ("20-20-20:", "Balanced water-soluble")
                    })
                };
            }

            // Parse N-P-K grade pattern (e.g., "10-10-10", "46-0-0")
            var gradePattern = @"^(\d+(?:\.\d+)?)-(\d+(?:\.\d+)?)-(\d+(?:\.\d+)?)$";
            var gradeMatch = Regex.Match(input, gradePattern);

            if (gradeMatch.Success)
            {
                if (!double.TryParse(gradeMatch.Groups[1].Value, out double n) ||
                    !double.TryParse(gradeMatch.Groups[2].Value, out double p) ||
                    !double.TryParse(gradeMatch.Groups[3].Value, out double k))
                {
                    return null;
                }

                if (n < 0 || n > 100 || p < 0 || p > 100 || k < 0 || k > 100)
                {
                    return FixedParagraph("N-P-K values must be between 0 and 100");
                }

                double total = n + p + k;
                double filler = 100 - total;
                string classification = ClassifyFertilizer(n, p, k);

                return new object[]
                {
                    Answer($"{n}-{p}-{k} Fertilizer Analysis"),
                    SectionHeader("Nutrient Content (per 100 lbs):"),
                    NameValueTable(entries: new[]
                    {
                        ("Nitrogen (N):", $"{n} lbs ({n}%)"),
                        ("Phosphorus (P₂O₅):", $"{p} lbs ({p}%)"),
                        ("Potassium (K₂O):", $"{k} lbs ({k}%)"),
                        ("Total Nutrients:", $"{total} lbs ({total}%)"),
                        ("Filler/Carrier:", $"{filler:F1} lbs ({filler:F1}%)"),
                        ("Classification:", classification)
                    }),
                    SectionHeader("Application Example:"),
                    FixedParagraph($"1 ton (2000 lbs) contains: {n * 20:F0} lbs N, {p * 20:F0} lbs P₂O₅, {k * 20:F0} lbs K₂O")
                };
            }

            // Parse "100 lbs 10-10-10" pattern
            var amountGradePattern = @"^(\d+(?:\.\d+)?)\s*(?:lbs?|pounds?)\s+(?:of\s+)?(\d+(?:\.\d+)?)-(\d+(?:\.\d+)?)-(\d+(?:\.\d+)?)";
            var amountMatch = Regex.Match(input, amountGradePattern, RegexOptions.IgnoreCase);

            if (amountMatch.Success)
            {
                if (!double.TryParse(amountMatch.Groups[1].Value, out double amount) ||
                    !double.TryParse(amountMatch.Groups[2].Value, out double n) ||
                    !double.TryParse(amountMatch.Groups[3].Value, out double p) ||
                    !double.TryParse(amountMatch.Groups[4].Value, out double k))
                {
                    return null;
                }

                if (amount < 0.01 || amount > 1000000)
                {
                    return FixedParagraph("Please enter a valid amount (0.01 - 1,000,000 lbs)");
                }

                double actualN = amount * (n / 100.0);
                double actualP = amount * (p / 100.0);
                double actualK = amount * (k / 100.0);
                double totalNutrients = actualN + actualP + actualK;

                return new object[]
                {
                    Answer($"{amount:N2} lbs of {n}-{p}-{k} contains {actualN:N2} lbs N"),
                    SectionHeader("Actual Nutrient Content:"),
                    NameValueTable(entries: new[]
                    {
                        ("Fertilizer Amount:", $"{amount:N2} lbs"),
                        ("Grade:", $"{n}-{p}-{k}"),
                        ("Actual Nitrogen (N):", $"{actualN:N2} lbs"),
                        ("Actual Phosphorus (P₂O₅):", $"{actualP:N2} lbs"),
                        ("Actual Potassium (K₂O):", $"{actualK:N2} lbs"),
                        ("Total Nutrients:", $"{totalNutrients:N2} lbs")
                    })
                };
            }

            // Parse application rate calculation: "50 lbs N per acre at 46-0-0"
            var ratePattern = @"^(\d+(?:\.\d+)?)\s*(?:lbs?|pounds?)\s+([npkNPK])\s+(?:per|/)\s*(?:acre|ac)\s+(?:at|with|using)\s+(\d+(?:\.\d+)?)-(\d+(?:\.\d+)?)-(\d+(?:\.\d+)?)";
            var rateMatch = Regex.Match(input, ratePattern, RegexOptions.IgnoreCase);

            if (rateMatch.Success)
            {
                if (!double.TryParse(rateMatch.Groups[1].Value, out double targetLbs) ||
                    !double.TryParse(rateMatch.Groups[3].Value, out double n) ||
                    !double.TryParse(rateMatch.Groups[4].Value, out double p) ||
                    !double.TryParse(rateMatch.Groups[5].Value, out double k))
                {
                    return null;
                }

                string nutrient = rateMatch.Groups[2].Value.ToUpper();
                double nutrientPercent = nutrient == "N" ? n : (nutrient == "P" ? p : k);
                string nutrientName = nutrient == "N" ? "Nitrogen" : (nutrient == "P" ? "Phosphorus" : "Potassium");

                if (nutrientPercent == 0)
                {
                    return FixedParagraph($"The fertilizer {n}-{p}-{k} contains 0% {nutrientName}");
                }

                double fertilizerNeeded = (targetLbs / nutrientPercent) * 100.0;
                double actualN = fertilizerNeeded * (n / 100.0);
                double actualP = fertilizerNeeded * (p / 100.0);
                double actualK = fertilizerNeeded * (k / 100.0);

                return new object[]
                {
                    Answer($"Apply {fertilizerNeeded:N2} lbs/acre of {n}-{p}-{k}"),
                    SectionHeader("Application Rate Calculation:"),
                    NameValueTable(entries: new[]
                    {
                        ("Target Nutrient:", $"{targetLbs:N2} lbs {nutrientName}/acre"),
                        ("Fertilizer Grade:", $"{n}-{p}-{k}"),
                        ("Application Rate:", $"{fertilizerNeeded:N2} lbs/acre"),
                        ("Actual N Applied:", $"{actualN:N2} lbs/acre"),
                        ("Actual P₂O₅ Applied:", $"{actualP:N2} lbs/acre"),
                        ("Actual K₂O Applied:", $"{actualK:N2} lbs/acre")
                    }),
                    SectionHeader("For a 40-acre field:"),
                    FixedParagraph($"Total fertilizer needed: {fertilizerNeeded * 40:N2} lbs ({fertilizerNeeded * 40 / 2000.0:N2} tons)")
                };
            }

            return null;
        }

        private string ClassifyFertilizer(double n, double p, double k)
        {
            if (n > 40 && p < 10 && k < 10)
                return "High Nitrogen (e.g., Urea, Ammonium Nitrate)";
            if (p > 40 && n < 25 && k < 10)
                return "High Phosphorus (e.g., DAP, MAP)";
            if (k > 40 && n < 10 && p < 10)
                return "High Potassium (e.g., Potash, Muriate of Potash)";
            if (System.Math.Abs(n - p) <= 5 && System.Math.Abs(n - k) <= 5)
                return "Balanced (All-Purpose)";
            if (n > p && n > k)
                return "Nitrogen-Heavy";
            if (p > n && p > k)
                return "Phosphorus-Heavy";
            if (k > n && k > p)
                return "Potassium-Heavy";
            return "Custom Blend";
        }
    }
}
