using System;
using System.Text.RegularExpressions;
using static QuickInfo.NodeFactory;

namespace QuickInfo
{
    public class EthanolYield : IProcessor
    {
        private const double GALLONS_PER_BUSHEL = 2.75;
        private const double DDGS_POUNDS_PER_BUSHEL = 17.0;

        public object GetResult(Query query)
        {
            if (query.IsHelp)
            {
                return HelpTable(
                    ("ethanol yield", "Show ethanol yield conversion rates"),
                    ("100 bushels", "Convert bushels to gallons of ethanol"),
                    ("100 bu", "Convert bushels to gallons of ethanol"),
                    ("bushels to gallons", "Show conversion formula"));
            }

            var input = query.OriginalInput.Trim();

            // Check for trigger phrases
            if (string.Equals(input, "ethanol yield", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "bushels to gallons", StringComparison.OrdinalIgnoreCase))
            {
                return new object[]
                {
                    Answer($"Ethanol Yield: {GALLONS_PER_BUSHEL} gallons per bushel"),
                    FixedParagraph($"1 bushel of corn produces approximately {GALLONS_PER_BUSHEL} gallons of ethanol and {DDGS_POUNDS_PER_BUSHEL} lbs of DDGS (Distillers Dried Grains with Solubles)")
                };
            }

            // Parse bushels value from input
            double bushels = 0;
            bool foundBushels = false;

            // Try patterns like "100 bushels", "56 bu", "1000 bushels to ethanol"
            var patterns = new[]
            {
                @"^(\d+(?:\.\d+)?)\s*(?:bushels?|bu)\b",
                @"^(\d+(?:\.\d+)?)\s+(?:bushels?|bu)?\s+(?:to|of)\s+(?:ethanol|corn)",
                @"^(\d+(?:\.\d+)?)\s*$"  // Just a number, if preceded by context
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    if (double.TryParse(match.Groups[1].Value, out bushels))
                    {
                        foundBushels = true;
                        break;
                    }
                }
            }

            // Only respond if we have a bushels-related query
            if (!foundBushels ||
                (!input.Contains("bushel", StringComparison.OrdinalIgnoreCase) &&
                 !input.Contains("bu", StringComparison.OrdinalIgnoreCase) &&
                 !input.Contains("ethanol", StringComparison.OrdinalIgnoreCase) &&
                 !input.Contains("yield", StringComparison.OrdinalIgnoreCase)))
            {
                return null;
            }

            if (bushels < 0.01 || bushels > 1000000000)
            {
                return FixedParagraph("Please enter a valid number of bushels (0.01 - 1,000,000,000)");
            }

            double gallons = bushels * GALLONS_PER_BUSHEL;
            double ddgsPounds = bushels * DDGS_POUNDS_PER_BUSHEL;
            double ddgsTons = ddgsPounds / 2000.0;

            return new object[]
            {
                Answer($"{bushels:N2} bushels → {gallons:N2} gallons of ethanol"),
                SectionHeader("Ethanol Production:"),
                NameValueTable(entries: new[]
                {
                    ("Bushels of Corn:", $"{bushels:N2} bu"),
                    ("Ethanol Produced:", $"{gallons:N2} gallons"),
                    ("Conversion Rate:", $"{GALLONS_PER_BUSHEL} gal/bu"),
                    ("DDGS Produced:", $"{ddgsPounds:N2} lbs ({ddgsTons:N2} tons)")
                })
            };
        }
    }
}
