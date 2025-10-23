using System;
using System.Text.RegularExpressions;
using static QuickInfo.NodeFactory;

namespace QuickInfo
{
    public class CornMoisture : IProcessor
    {
        private const double STANDARD_MOISTURE = 15.5;

        public object GetResult(Query query)
        {
            if (query.IsHelp)
            {
                return HelpTable(
                    ("corn moisture", "Show standard moisture information"),
                    ("1000 bu at 18%", "Convert bushels to standard moisture"),
                    ("1000 bushels at 18% to 15.5%", "Convert between moisture levels"),
                    ("moisture conversion", "Show conversion formula"));
            }

            var input = query.OriginalInput.Trim();

            // Check for general help triggers
            if (string.Equals(input, "corn moisture", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "moisture conversion", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "moisture adjustment", StringComparison.OrdinalIgnoreCase))
            {
                return new object[]
                {
                    Answer($"Standard corn moisture: {STANDARD_MOISTURE}%"),
                    FixedParagraph("Corn moisture affects weight and pricing. Grain contracts typically specify delivery at standard moisture."),
                    SectionHeader("Conversion Formula:"),
                    FixedParagraph("Adjusted Bushels = Original Bushels × (100 - Original Moisture%) / (100 - Target Moisture%)"),
                    SectionHeader("Example:"),
                    FixedParagraph($"1000 bu at 18% → 1000 × (100-18)/(100-15.5) = 970.4 bu at {STANDARD_MOISTURE}%")
                };
            }

            // Parse input patterns like:
            // "1000 bushels at 18%"
            // "1000 bu at 18% moisture"
            // "1000 bu at 18% to 15.5%"
            // "1000 bushels at 18 percent"
            var pattern = @"^(\d+(?:\.\d+)?)\s*(?:bushels?|bu)\s+(?:at|@)\s+(\d+(?:\.\d+)?)%?\s*(?:moisture|percent)?(?:\s+(?:to)\s+(\d+(?:\.\d+)?)%?)?";
            var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return null;
            }

            if (!double.TryParse(match.Groups[1].Value, out double originalBushels))
            {
                return null;
            }

            if (!double.TryParse(match.Groups[2].Value, out double originalMoisture))
            {
                return null;
            }

            double targetMoisture = STANDARD_MOISTURE;
            if (match.Groups[3].Success)
            {
                if (!double.TryParse(match.Groups[3].Value, out targetMoisture))
                {
                    targetMoisture = STANDARD_MOISTURE;
                }
            }

            // Validate ranges
            if (originalBushels < 0.01 || originalBushels > 1000000000)
            {
                return FixedParagraph("Please enter a valid number of bushels (0.01 - 1,000,000,000)");
            }

            if (originalMoisture < 0 || originalMoisture >= 100)
            {
                return FixedParagraph("Original moisture must be between 0% and 99.9%");
            }

            if (targetMoisture < 0 || targetMoisture >= 100)
            {
                return FixedParagraph("Target moisture must be between 0% and 99.9%");
            }

            // Calculate adjusted bushels
            double adjustedBushels = originalBushels * (100 - originalMoisture) / (100 - targetMoisture);
            double bushelsDifference = adjustedBushels - originalBushels;
            double percentChange = (bushelsDifference / originalBushels) * 100;

            string changeDescription;
            if (originalMoisture > targetMoisture)
            {
                double absDiff = bushelsDifference < 0 ? -bushelsDifference : bushelsDifference;
                double absPercent = percentChange < 0 ? -percentChange : percentChange;
                changeDescription = $"Loss of {absDiff:N2} bu ({absPercent:N2}% reduction)";
            }
            else if (originalMoisture < targetMoisture)
            {
                double absDiff = bushelsDifference < 0 ? -bushelsDifference : bushelsDifference;
                double absPercent = percentChange < 0 ? -percentChange : percentChange;
                changeDescription = $"Gain of {absDiff:N2} bu ({absPercent:N2}% increase)";
            }
            else
            {
                changeDescription = "No adjustment needed (same moisture)";
            }

            // Calculate dry matter
            double dryMatterOriginal = originalBushels * (100 - originalMoisture) / 100;
            double dryMatterAdjusted = adjustedBushels * (100 - targetMoisture) / 100;

            return new object[]
            {
                Answer($"{originalBushels:N2} bu at {originalMoisture}% → {adjustedBushels:N2} bu at {targetMoisture}%"),
                SectionHeader("Moisture Adjustment:"),
                NameValueTable(entries: new[]
                {
                    ("Original Bushels:", $"{originalBushels:N2} bu"),
                    ("Original Moisture:", $"{originalMoisture}%"),
                    ("Target Moisture:", $"{targetMoisture}%"),
                    ("Adjusted Bushels:", $"{adjustedBushels:N2} bu"),
                    ("Adjustment:", changeDescription)
                }),
                SectionHeader("Dry Matter (Constant):"),
                FixedParagraph($"{dryMatterOriginal:N2} bu dry matter"),
                SectionHeader("Formula:"),
                FixedParagraph($"{originalBushels:N2} × (100 - {originalMoisture}) / (100 - {targetMoisture}) = {adjustedBushels:N2} bu")
            };
        }
    }
}
