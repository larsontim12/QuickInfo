using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static QuickInfo.NodeFactory;

namespace QuickInfo
{
    public class Weight : IProcessor
    {
        private static readonly Dictionary<string, double> ToKilograms = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            // Metric
            { "mg", 0.000001 },
            { "milligram", 0.000001 },
            { "milligrams", 0.000001 },
            { "g", 0.001 },
            { "gram", 0.001 },
            { "grams", 0.001 },
            { "kg", 1.0 },
            { "kilogram", 1.0 },
            { "kilograms", 1.0 },
            { "tonne", 1000.0 },
            { "tonnes", 1000.0 },
            { "metric ton", 1000.0 },
            { "metric tons", 1000.0 },

            // Imperial/US
            { "oz", 0.0283495 },
            { "ounce", 0.0283495 },
            { "ounces", 0.0283495 },
            { "lb", 0.453592 },
            { "lbs", 0.453592 },
            { "pound", 0.453592 },
            { "pounds", 0.453592 },
            { "ton", 907.185 },
            { "tons", 907.185 },
            { "short ton", 907.185 },
            { "short tons", 907.185 },
            { "long ton", 1016.05 },
            { "long tons", 1016.05 },

            // Troy weight (for precious metals)
            { "troy oz", 0.0311035 },
            { "troy ounce", 0.0311035 },
            { "troy ounces", 0.0311035 }
        };

        public object GetResult(Query query)
        {
            if (query.IsHelp)
            {
                return HelpTable(
                    ("weight", "Show weight/mass conversion units"),
                    ("150 lbs to kg", "Convert pounds to kilograms"),
                    ("75 kg to lbs", "Convert kilograms to pounds"),
                    ("16 oz to grams", "Convert ounces to grams"),
                    ("2.5 tons to kg", "Convert tons to kilograms"));
            }

            var input = query.OriginalInput.Trim();

            // General weight info
            if (string.Equals(input, "weight", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "mass", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "weight conversion", StringComparison.OrdinalIgnoreCase))
            {
                return new object[]
                {
                    Answer("Weight/Mass Conversions"),
                    SectionHeader("Supported Units:"),
                    FixedParagraph("Metric: mg, g, kg, tonne (metric ton)"),
                    FixedParagraph("Imperial: oz, lb, ton (short ton), long ton"),
                    FixedParagraph("Troy: troy oz (for precious metals)"),
                    SectionHeader("Common Conversions:"),
                    NameValueTable(entries: new[]
                    {
                        ("1 ounce:", "28.35 g"),
                        ("1 pound:", "0.4536 kg / 453.6 g"),
                        ("1 kilogram:", "2.205 lbs"),
                        ("1 ton (US):", "907.2 kg / 2000 lbs"),
                        ("1 metric ton:", "1000 kg / 2204.6 lbs"),
                        ("1 troy ounce:", "31.10 g (precious metals)")
                    })
                };
            }

            // Pattern: "X unit to unit" or "X unit in unit"
            var conversionPattern = @"^(\d+(?:\.\d+)?)\s*([a-z\s]+?)\s+(?:to|in|as)\s+([a-z\s]+)$";
            var match = Regex.Match(input, conversionPattern, RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return null;
            }

            if (!double.TryParse(match.Groups[1].Value, out double value))
            {
                return null;
            }

            string fromUnit = match.Groups[2].Value.Trim();
            string toUnit = match.Groups[3].Value.Trim();

            if (!ToKilograms.TryGetValue(fromUnit, out double fromMultiplier))
            {
                return FixedParagraph($"Unknown weight unit '{fromUnit}'. Supported: mg, g, kg, oz, lb, ton, tonne, troy oz");
            }

            if (!ToKilograms.TryGetValue(toUnit, out double toMultiplier))
            {
                return FixedParagraph($"Unknown weight unit '{toUnit}'. Supported: mg, g, kg, oz, lb, ton, tonne, troy oz");
            }

            // Convert through kilograms
            double kilograms = value * fromMultiplier;
            double result = kilograms / toMultiplier;

            // Normalize unit names for display
            string fromUnitDisplay = NormalizeUnitName(fromUnit, value);
            string toUnitDisplay = NormalizeUnitName(toUnit, result);

            // Calculate additional common conversions
            double grams = kilograms * 1000;
            double pounds = kilograms / 0.453592;
            double ounces = kilograms / 0.0283495;
            double metricTons = kilograms / 1000.0;

            return new object[]
            {
                Answer($"{value:F2} {fromUnitDisplay} = {result:F2} {toUnitDisplay}"),
                SectionHeader("Conversion Result:"),
                NameValueTable(entries: new[]
                {
                    ("Original:", $"{value:F2} {fromUnitDisplay}"),
                    ("Converted:", $"{result:F2} {toUnitDisplay}"),
                    ("In Kilograms:", $"{kilograms:F4} kg")
                }),
                SectionHeader("Additional Conversions:"),
                NameValueTable(entries: new[]
                {
                    ("Milligrams:", $"{kilograms * 1000000:F2} mg"),
                    ("Grams:", $"{grams:F2} g"),
                    ("Kilograms:", $"{kilograms:F4} kg"),
                    ("Ounces:", $"{ounces:F2} oz"),
                    ("Pounds:", $"{pounds:F2} lbs"),
                    ("Metric Tons:", $"{metricTons:F6} tonnes"),
                    ("US Tons:", $"{kilograms / 907.185:F6} tons")
                })
            };
        }

        private string NormalizeUnitName(string unit, double value)
        {
            unit = unit.ToLower().Trim();

            // Return proper unit names with singular/plural
            if (unit == "mg" || unit.StartsWith("millig"))
                return value == 1 ? "milligram" : "milligrams";
            if (unit == "g" || unit == "gram")
                return value == 1 ? "gram" : "grams";
            if (unit == "grams")
                return "grams";
            if (unit == "kg" || unit.StartsWith("kilog"))
                return value == 1 ? "kilogram" : "kilograms";
            if (unit == "oz" || unit == "ounce")
                return value == 1 ? "ounce" : "ounces";
            if (unit == "ounces")
                return "ounces";
            if (unit == "lb" || unit == "lbs" || unit == "pound")
                return value == 1 ? "pound" : "pounds";
            if (unit == "pounds")
                return "pounds";
            if (unit == "ton" || unit == "short ton")
                return value == 1 ? "ton" : "tons";
            if (unit == "tons" || unit == "short tons")
                return "tons";
            if (unit == "long ton")
                return value == 1 ? "long ton" : "long tons";
            if (unit == "long tons")
                return "long tons";
            if (unit == "tonne" || unit == "metric ton")
                return value == 1 ? "tonne" : "tonnes";
            if (unit == "tonnes" || unit == "metric tons")
                return "tonnes";
            if (unit == "troy oz" || unit == "troy ounce")
                return value == 1 ? "troy ounce" : "troy ounces";
            if (unit == "troy ounces")
                return "troy ounces";

            return unit;
        }
    }
}
