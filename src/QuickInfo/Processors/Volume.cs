using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static QuickInfo.NodeFactory;

namespace QuickInfo
{
    public class Volume : IProcessor
    {
        private static readonly Dictionary<string, double> ToLiters = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            // Metric
            { "ml", 0.001 },
            { "milliliter", 0.001 },
            { "milliliters", 0.001 },
            { "millilitre", 0.001 },
            { "millilitres", 0.001 },
            { "l", 1.0 },
            { "liter", 1.0 },
            { "liters", 1.0 },
            { "litre", 1.0 },
            { "litres", 1.0 },
            { "m3", 1000.0 },
            { "cubic meter", 1000.0 },
            { "cubic meters", 1000.0 },
            { "cubic metre", 1000.0 },
            { "cubic metres", 1000.0 },

            // US liquid measures
            { "tsp", 0.00492892 },
            { "teaspoon", 0.00492892 },
            { "teaspoons", 0.00492892 },
            { "tbsp", 0.0147868 },
            { "tablespoon", 0.0147868 },
            { "tablespoons", 0.0147868 },
            { "fl oz", 0.0295735 },
            { "fluid ounce", 0.0295735 },
            { "fluid ounces", 0.0295735 },
            { "cup", 0.236588 },
            { "cups", 0.236588 },
            { "pint", 0.473176 },
            { "pints", 0.473176 },
            { "pt", 0.473176 },
            { "quart", 0.946353 },
            { "quarts", 0.946353 },
            { "qt", 0.946353 },
            { "gallon", 3.78541 },
            { "gallons", 3.78541 },
            { "gal", 3.78541 },

            // UK Imperial measures
            { "uk gallon", 4.54609 },
            { "uk gallons", 4.54609 },
            { "imperial gallon", 4.54609 },
            { "imperial gallons", 4.54609 },
            { "uk pint", 0.568261 },
            { "uk pints", 0.568261 },
            { "imperial pint", 0.568261 },
            { "imperial pints", 0.568261 },

            // Other
            { "barrel", 158.987 },
            { "barrels", 158.987 },
            { "bbl", 158.987 },
            { "cubic foot", 28.3168 },
            { "cubic feet", 28.3168 },
            { "ft3", 28.3168 },
            { "cubic inch", 0.0163871 },
            { "cubic inches", 0.0163871 },
            { "in3", 0.0163871 }
        };

        public object GetResult(Query query)
        {
            if (query.IsHelp)
            {
                return HelpTable(
                    ("volume", "Show volume conversion units"),
                    ("10 gallons to liters", "Convert gallons to liters"),
                    ("500 ml to cups", "Convert milliliters to cups"),
                    ("2.5 liters to gallons", "Convert liters to gallons"),
                    ("1 barrel to gallons", "Convert barrels to gallons"));
            }

            var input = query.OriginalInput.Trim();

            // General volume info
            if (string.Equals(input, "volume", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "volume conversion", StringComparison.OrdinalIgnoreCase))
            {
                return new object[]
                {
                    Answer("Volume Conversions"),
                    SectionHeader("Supported Units:"),
                    FixedParagraph("Metric: ml, l, m³"),
                    FixedParagraph("US: tsp, tbsp, fl oz, cup, pint, quart, gallon"),
                    FixedParagraph("Imperial: UK/Imperial gallon, UK/Imperial pint"),
                    FixedParagraph("Other: barrel (bbl), cubic foot, cubic inch"),
                    SectionHeader("Common Conversions:"),
                    NameValueTable(entries: new[]
                    {
                        ("1 cup:", "236.6 ml / 8 fl oz"),
                        ("1 pint:", "473.2 ml / 2 cups"),
                        ("1 quart:", "946.4 ml / 4 cups"),
                        ("1 gallon (US):", "3.785 liters"),
                        ("1 gallon (UK):", "4.546 liters"),
                        ("1 liter:", "1000 ml / 33.8 fl oz"),
                        ("1 barrel (oil):", "42 gallons / 159 liters")
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

            if (!ToLiters.TryGetValue(fromUnit, out double fromMultiplier))
            {
                return FixedParagraph($"Unknown volume unit '{fromUnit}'. Supported: ml, l, gal, qt, pt, cup, fl oz, tbsp, tsp, bbl, etc.");
            }

            if (!ToLiters.TryGetValue(toUnit, out double toMultiplier))
            {
                return FixedParagraph($"Unknown volume unit '{toUnit}'. Supported: ml, l, gal, qt, pt, cup, fl oz, tbsp, tsp, bbl, etc.");
            }

            // Convert through liters
            double liters = value * fromMultiplier;
            double result = liters / toMultiplier;

            // Normalize unit names for display
            string fromUnitDisplay = NormalizeUnitName(fromUnit, value);
            string toUnitDisplay = NormalizeUnitName(toUnit, result);

            // Calculate additional common conversions
            double milliliters = liters * 1000;
            double gallons = liters / 3.78541;
            double cups = liters / 0.236588;
            double fluidOunces = liters / 0.0295735;

            return new object[]
            {
                Answer($"{value:F2} {fromUnitDisplay} = {result:F2} {toUnitDisplay}"),
                SectionHeader("Conversion Result:"),
                NameValueTable(entries: new[]
                {
                    ("Original:", $"{value:F2} {fromUnitDisplay}"),
                    ("Converted:", $"{result:F2} {toUnitDisplay}"),
                    ("In Liters:", $"{liters:F4} L")
                }),
                SectionHeader("Additional Conversions:"),
                NameValueTable(entries: new[]
                {
                    ("Milliliters:", $"{milliliters:F2} ml"),
                    ("Liters:", $"{liters:F4} L"),
                    ("Fluid Ounces:", $"{fluidOunces:F2} fl oz"),
                    ("Cups:", $"{cups:F2} cups"),
                    ("Gallons (US):", $"{gallons:F4} gal"),
                    ("Gallons (UK):", $"{liters / 4.54609:F4} gal"),
                    ("Barrels:", $"{liters / 158.987:F4} bbl")
                })
            };
        }

        private string NormalizeUnitName(string unit, double value)
        {
            unit = unit.ToLower().Trim();

            // Return proper unit names with singular/plural
            if (unit == "ml" || unit.StartsWith("millil"))
                return value == 1 ? "milliliter" : "milliliters";
            if (unit == "l" || unit == "liter" || unit == "litre")
                return value == 1 ? "liter" : "liters";
            if (unit == "liters" || unit == "litres")
                return "liters";
            if (unit == "m3" || unit.Contains("cubic meter") || unit.Contains("cubic metre"))
                return value == 1 ? "cubic meter" : "cubic meters";
            if (unit == "tsp" || unit == "teaspoon")
                return value == 1 ? "teaspoon" : "teaspoons";
            if (unit == "teaspoons")
                return "teaspoons";
            if (unit == "tbsp" || unit == "tablespoon")
                return value == 1 ? "tablespoon" : "tablespoons";
            if (unit == "tablespoons")
                return "tablespoons";
            if (unit == "fl oz" || unit.Contains("fluid ounce"))
                return value == 1 ? "fluid ounce" : "fluid ounces";
            if (unit == "cup")
                return value == 1 ? "cup" : "cups";
            if (unit == "cups")
                return "cups";
            if (unit == "pt" || unit == "pint")
                return value == 1 ? "pint" : "pints";
            if (unit == "pints")
                return "pints";
            if (unit == "qt" || unit == "quart")
                return value == 1 ? "quart" : "quarts";
            if (unit == "quarts")
                return "quarts";
            if (unit == "gal" || unit == "gallon")
                return value == 1 ? "gallon" : "gallons";
            if (unit == "gallons")
                return "gallons";
            if (unit.Contains("uk") || unit.Contains("imperial"))
            {
                if (unit.Contains("gallon"))
                    return value == 1 ? "UK gallon" : "UK gallons";
                if (unit.Contains("pint"))
                    return value == 1 ? "UK pint" : "UK pints";
            }
            if (unit == "bbl" || unit == "barrel")
                return value == 1 ? "barrel" : "barrels";
            if (unit == "barrels")
                return "barrels";
            if (unit == "ft3" || unit.Contains("cubic f"))
                return value == 1 ? "cubic foot" : "cubic feet";
            if (unit == "in3" || unit.Contains("cubic i"))
                return value == 1 ? "cubic inch" : "cubic inches";

            return unit;
        }
    }
}
