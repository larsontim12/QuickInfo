using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static QuickInfo.NodeFactory;

namespace QuickInfo
{
    public class GrainWeight : IProcessor
    {
        // Standard test weights (lbs/bushel) for various grains
        private static readonly Dictionary<string, double> TestWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            { "corn", 56.0 },
            { "soybeans", 60.0 },
            { "soybean", 60.0 },
            { "wheat", 60.0 },
            { "barley", 48.0 },
            { "oats", 32.0 },
            { "rye", 56.0 },
            { "sorghum", 56.0 },
            { "milo", 56.0 }
        };

        public object GetResult(Query query)
        {
            if (query.IsHelp)
            {
                return HelpTable(
                    ("grain weight", "Show standard test weights for grains"),
                    ("test weight", "Show standard test weights"),
                    ("1000 bu corn to lbs", "Convert bushels to pounds"),
                    ("50000 lbs wheat to bu", "Convert pounds to bushels"),
                    ("corn test weight", "Show corn test weight"));
            }

            var input = query.OriginalInput.Trim();

            // Check for general info requests
            if (string.Equals(input, "grain weight", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "test weight", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "grain test weight", StringComparison.OrdinalIgnoreCase))
            {
                return new object[]
                {
                    Answer("Standard Test Weights (lbs/bushel)"),
                    SectionHeader("Common Grains:"),
                    NameValueTable(entries: new[]
                    {
                        ("Soybeans:", "60 lbs/bu"),
                        ("Wheat:", "60 lbs/bu"),
                        ("Corn:", "56 lbs/bu"),
                        ("Rye:", "56 lbs/bu"),
                        ("Sorghum/Milo:", "56 lbs/bu"),
                        ("Barley:", "48 lbs/bu"),
                        ("Oats:", "32 lbs/bu")
                    }),
                    FixedParagraph("Test weight is the weight of grain per bushel, used to determine quality and market value.")
                };
            }

            // Check for specific grain test weight request
            foreach (var grain in TestWeights.Keys)
            {
                if (Regex.IsMatch(input, $@"^{grain}\s+test\s+weight$", RegexOptions.IgnoreCase))
                {
                    return Answer($"{char.ToUpper(grain[0]) + grain.Substring(1)}: {TestWeights[grain]} lbs/bushel");
                }
            }

            // Parse conversion requests
            // Pattern: "1000 bu corn to lbs" or "1000 bushels of corn to pounds"
            var buToLbsPattern = @"^(\d+(?:\.\d+)?)\s*(?:bu|bushels?)\s+(?:of\s+)?(\w+)\s+(?:to|in)\s+(?:lbs?|pounds?)";
            var buToLbsMatch = Regex.Match(input, buToLbsPattern, RegexOptions.IgnoreCase);

            if (buToLbsMatch.Success)
            {
                if (!double.TryParse(buToLbsMatch.Groups[1].Value, out double bushels))
                {
                    return null;
                }

                string grainType = buToLbsMatch.Groups[2].Value;
                if (!TestWeights.TryGetValue(grainType, out double testWeight))
                {
                    return FixedParagraph($"Unknown grain type '{grainType}'. Supported grains: corn, soybeans, wheat, barley, oats, rye, sorghum/milo");
                }

                if (bushels < 0.01 || bushels > 1000000000)
                {
                    return FixedParagraph("Please enter a valid number of bushels (0.01 - 1,000,000,000)");
                }

                double pounds = bushels * testWeight;
                double tons = pounds / 2000.0;
                double metricTonnes = pounds * 0.453592 / 1000.0;

                return new object[]
                {
                    Answer($"{bushels:N2} bu {grainType} = {pounds:N2} lbs"),
                    SectionHeader("Weight Conversion:"),
                    NameValueTable(entries: new[]
                    {
                        ("Bushels:", $"{bushels:N2} bu"),
                        ("Grain Type:", $"{char.ToUpper(grainType[0]) + grainType.Substring(1)}"),
                        ("Test Weight:", $"{testWeight} lbs/bu"),
                        ("Total Weight (lbs):", $"{pounds:N2} lbs"),
                        ("Total Weight (tons):", $"{tons:N2} tons"),
                        ("Total Weight (metric):", $"{metricTonnes:N2} tonnes")
                    })
                };
            }

            // Pattern: "50000 lbs wheat to bu" or "50000 pounds of wheat to bushels"
            var lbsToBuPattern = @"^(\d+(?:\.\d+)?)\s*(?:lbs?|pounds?)\s+(?:of\s+)?(\w+)\s+(?:to|in)\s+(?:bu|bushels?)";
            var lbsToBuMatch = Regex.Match(input, lbsToBuPattern, RegexOptions.IgnoreCase);

            if (lbsToBuMatch.Success)
            {
                if (!double.TryParse(lbsToBuMatch.Groups[1].Value, out double pounds))
                {
                    return null;
                }

                string grainType = lbsToBuMatch.Groups[2].Value;
                if (!TestWeights.TryGetValue(grainType, out double testWeight))
                {
                    return FixedParagraph($"Unknown grain type '{grainType}'. Supported grains: corn, soybeans, wheat, barley, oats, rye, sorghum/milo");
                }

                if (pounds < 0.01 || pounds > 1000000000)
                {
                    return FixedParagraph("Please enter a valid weight (0.01 - 1,000,000,000 lbs)");
                }

                double bushels = pounds / testWeight;
                double tons = pounds / 2000.0;
                double metricTonnes = pounds * 0.453592 / 1000.0;

                return new object[]
                {
                    Answer($"{pounds:N2} lbs {grainType} = {bushels:N2} bu"),
                    SectionHeader("Weight Conversion:"),
                    NameValueTable(entries: new[]
                    {
                        ("Weight (lbs):", $"{pounds:N2} lbs"),
                        ("Weight (tons):", $"{tons:N2} tons"),
                        ("Weight (metric):", $"{metricTonnes:N2} tonnes"),
                        ("Grain Type:", $"{char.ToUpper(grainType[0]) + grainType.Substring(1)}"),
                        ("Test Weight:", $"{testWeight} lbs/bu"),
                        ("Total Bushels:", $"{bushels:N2} bu")
                    })
                };
            }

            return null;
        }
    }
}
