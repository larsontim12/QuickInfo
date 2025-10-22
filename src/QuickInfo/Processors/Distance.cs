using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static QuickInfo.NodeFactory;

namespace QuickInfo
{
    public class Distance : IProcessor
    {
        private static readonly Dictionary<string, double> ToMeters = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            // Metric
            { "mm", 0.001 },
            { "millimeter", 0.001 },
            { "millimeters", 0.001 },
            { "millimetre", 0.001 },
            { "millimetres", 0.001 },
            { "cm", 0.01 },
            { "centimeter", 0.01 },
            { "centimeters", 0.01 },
            { "centimetre", 0.01 },
            { "centimetres", 0.01 },
            { "m", 1.0 },
            { "meter", 1.0 },
            { "meters", 1.0 },
            { "metre", 1.0 },
            { "metres", 1.0 },
            { "km", 1000.0 },
            { "kilometer", 1000.0 },
            { "kilometers", 1000.0 },
            { "kilometre", 1000.0 },
            { "kilometres", 1000.0 },

            // Imperial/US
            { "in", 0.0254 },
            { "inch", 0.0254 },
            { "inches", 0.0254 },
            { "ft", 0.3048 },
            { "foot", 0.3048 },
            { "feet", 0.3048 },
            { "yd", 0.9144 },
            { "yard", 0.9144 },
            { "yards", 0.9144 },
            { "mi", 1609.344 },
            { "mile", 1609.344 },
            { "miles", 1609.344 },

            // Nautical
            { "nm", 1852.0 },
            { "nmi", 1852.0 },
            { "nautical mile", 1852.0 },
            { "nautical miles", 1852.0 }
        };

        public object GetResult(Query query)
        {
            if (query.IsHelp)
            {
                return HelpTable(
                    ("distance", "Show distance/length conversion units"),
                    ("100 miles to km", "Convert miles to kilometers"),
                    ("5 feet to meters", "Convert feet to meters"),
                    ("2.5 km to miles", "Convert kilometers to miles"),
                    ("12 inches to cm", "Convert inches to centimeters"));
            }

            var input = query.OriginalInput.Trim();

            // General distance info
            if (string.Equals(input, "distance", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "length", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "distance conversion", StringComparison.OrdinalIgnoreCase))
            {
                return new object[]
                {
                    Answer("Distance/Length Conversions"),
                    SectionHeader("Supported Units:"),
                    FixedParagraph("Metric: mm, cm, m, km"),
                    FixedParagraph("Imperial: in, ft, yd, mi"),
                    FixedParagraph("Nautical: nm, nautical miles"),
                    SectionHeader("Common Conversions:"),
                    NameValueTable(entries: new[]
                    {
                        ("1 inch:", "2.54 cm"),
                        ("1 foot:", "30.48 cm / 0.3048 m"),
                        ("1 yard:", "0.9144 m"),
                        ("1 mile:", "1.609 km"),
                        ("1 km:", "0.621 miles"),
                        ("1 nautical mile:", "1.852 km")
                    })
                };
            }

            // Pattern: "X unit to unit" or "X unit in unit"
            var conversionPattern = @"^(\d+(?:\.\d+)?)\s*([a-z]+)\s+(?:to|in|as)\s+([a-z\s]+)";
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

            if (!ToMeters.TryGetValue(fromUnit, out double fromMultiplier))
            {
                return FixedParagraph($"Unknown distance unit '{fromUnit}'. Supported: mm, cm, m, km, in, ft, yd, mi, nm");
            }

            if (!ToMeters.TryGetValue(toUnit, out double toMultiplier))
            {
                return FixedParagraph($"Unknown distance unit '{toUnit}'. Supported: mm, cm, m, km, in, ft, yd, mi, nm");
            }

            // Convert through meters
            double meters = value * fromMultiplier;
            double result = meters / toMultiplier;

            // Normalize unit names for display
            string fromUnitDisplay = NormalizeUnitName(fromUnit, value);
            string toUnitDisplay = NormalizeUnitName(toUnit, result);

            // Calculate additional common conversions
            double kilometers = meters / 1000.0;
            double miles = meters / 1609.344;
            double feet = meters / 0.3048;
            double inches = meters / 0.0254;

            return new object[]
            {
                Answer($"{value:F2} {fromUnitDisplay} = {result:F2} {toUnitDisplay}"),
                SectionHeader("Conversion Result:"),
                NameValueTable(entries: new[]
                {
                    ("Original:", $"{value:F2} {fromUnitDisplay}"),
                    ("Converted:", $"{result:F2} {toUnitDisplay}"),
                    ("In Meters:", $"{meters:F4} m")
                }),
                SectionHeader("Additional Conversions:"),
                NameValueTable(entries: new[]
                {
                    ("Millimeters:", $"{meters * 1000:F2} mm"),
                    ("Centimeters:", $"{meters * 100:F2} cm"),
                    ("Meters:", $"{meters:F2} m"),
                    ("Kilometers:", $"{kilometers:F4} km"),
                    ("Inches:", $"{inches:F2} in"),
                    ("Feet:", $"{feet:F2} ft"),
                    ("Miles:", $"{miles:F4} mi")
                })
            };
        }

        private string NormalizeUnitName(string unit, double value)
        {
            unit = unit.ToLower();

            // Return proper unit names with singular/plural
            if (unit == "mm" || unit.StartsWith("millim"))
                return value == 1 ? "millimeter" : "millimeters";
            if (unit == "cm" || unit.StartsWith("centim"))
                return value == 1 ? "centimeter" : "centimeters";
            if (unit == "m" || unit == "meter" || unit == "metre")
                return value == 1 ? "meter" : "meters";
            if (unit == "km" || unit.StartsWith("kilom"))
                return value == 1 ? "kilometer" : "kilometers";
            if (unit == "in" || unit.StartsWith("inch"))
                return value == 1 ? "inch" : "inches";
            if (unit == "ft" || unit == "foot" || unit == "feet")
                return value == 1 ? "foot" : "feet";
            if (unit == "yd" || unit.StartsWith("yard"))
                return value == 1 ? "yard" : "yards";
            if (unit == "mi" || unit.StartsWith("mile"))
                return value == 1 ? "mile" : "miles";
            if (unit == "nm" || unit == "nmi" || unit.Contains("nautical"))
                return value == 1 ? "nautical mile" : "nautical miles";

            return unit;
        }
    }
}
