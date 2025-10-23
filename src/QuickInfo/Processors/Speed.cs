using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static QuickInfo.NodeFactory;

namespace QuickInfo
{
    public class Speed : IProcessor
    {
        private static readonly Dictionary<string, double> ToMetersPerSecond = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            // Metric
            { "m/s", 1.0 },
            { "mps", 1.0 },
            { "meters per second", 1.0 },
            { "metres per second", 1.0 },
            { "km/h", 0.277778 },
            { "kmh", 0.277778 },
            { "kph", 0.277778 },
            { "kilometers per hour", 0.277778 },
            { "kilometres per hour", 0.277778 },

            // Imperial
            { "mph", 0.44704 },
            { "miles per hour", 0.44704 },
            { "ft/s", 0.3048 },
            { "fps", 0.3048 },
            { "feet per second", 0.3048 },

            // Nautical
            { "knot", 0.514444 },
            { "knots", 0.514444 },
            { "kt", 0.514444 },
            { "kts", 0.514444 },

            // Special
            { "mach", 343.0 },  // Mach 1 at sea level, 20°C
            { "c", 299792458.0 },  // Speed of light
            { "speed of light", 299792458.0 }
        };

        public object GetResult(Query query)
        {
            if (query.IsHelp)
            {
                return HelpTable(
                    ("speed", "Show speed conversion units"),
                    ("60 mph to kmh", "Convert miles per hour to km/h"),
                    ("100 kmh to mph", "Convert km/h to mph"),
                    ("20 knots to mph", "Convert knots to mph"),
                    ("340 m/s to mach", "Convert m/s to Mach number"));
            }

            var input = query.OriginalInput.Trim();

            // General speed info
            if (string.Equals(input, "speed", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "velocity", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "speed conversion", StringComparison.OrdinalIgnoreCase))
            {
                return new object[]
                {
                    Answer("Speed/Velocity Conversions"),
                    SectionHeader("Supported Units:"),
                    FixedParagraph("Metric: m/s, km/h"),
                    FixedParagraph("Imperial: mph, ft/s"),
                    FixedParagraph("Nautical: knots"),
                    FixedParagraph("Special: Mach, c (speed of light)"),
                    SectionHeader("Common Conversions:"),
                    NameValueTable(entries: new[]
                    {
                        ("1 mph:", "1.609 km/h / 0.447 m/s"),
                        ("1 km/h:", "0.621 mph / 0.278 m/s"),
                        ("1 knot:", "1.852 km/h / 1.151 mph"),
                        ("Mach 1:", "343 m/s / 767 mph (at sea level)"),
                        ("Speed of light:", "299,792,458 m/s"),
                        ("Speed limit (US highway):", "~70 mph / 113 km/h")
                    })
                };
            }

            // Pattern: "X unit to unit" or "X unit in unit"
            var conversionPattern = @"^(\d+(?:\.\d+)?)\s*([a-z/]+)\s+(?:to|in|as)\s+([a-z/\s]+)$";
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

            if (!ToMetersPerSecond.TryGetValue(fromUnit, out double fromMultiplier))
            {
                return FixedParagraph($"Unknown speed unit '{fromUnit}'. Supported: m/s, km/h, mph, ft/s, knots, mach, c");
            }

            if (!ToMetersPerSecond.TryGetValue(toUnit, out double toMultiplier))
            {
                return FixedParagraph($"Unknown speed unit '{toUnit}'. Supported: m/s, km/h, mph, ft/s, knots, mach, c");
            }

            // Convert through meters per second
            double metersPerSecond = value * fromMultiplier;
            double result = metersPerSecond / toMultiplier;

            // Normalize unit names for display
            string fromUnitDisplay = NormalizeUnitName(fromUnit);
            string toUnitDisplay = NormalizeUnitName(toUnit);

            // Calculate additional common conversions
            double kmh = metersPerSecond / 0.277778;
            double mph = metersPerSecond / 0.44704;
            double knots = metersPerSecond / 0.514444;
            double mach = metersPerSecond / 343.0;

            // Determine context
            string context = GetSpeedContext(mph);

            return new object[]
            {
                Answer($"{value:F2} {fromUnitDisplay} = {result:F2} {toUnitDisplay}"),
                SectionHeader("Conversion Result:"),
                NameValueTable(entries: new[]
                {
                    ("Original:", $"{value:F2} {fromUnitDisplay}"),
                    ("Converted:", $"{result:F2} {toUnitDisplay}"),
                    ("In m/s:", $"{metersPerSecond:F2} m/s"),
                    ("Context:", context)
                }),
                SectionHeader("Additional Conversions:"),
                NameValueTable(entries: new[]
                {
                    ("Meters/Second:", $"{metersPerSecond:F2} m/s"),
                    ("Kilometers/Hour:", $"{kmh:F2} km/h"),
                    ("Miles/Hour:", $"{mph:F2} mph"),
                    ("Knots:", $"{knots:F2} knots"),
                    ("Mach:", $"Mach {mach:F4}"),
                    ("% of Light Speed:", $"{(metersPerSecond / 299792458.0) * 100:E2}%")
                })
            };
        }

        private string NormalizeUnitName(string unit)
        {
            unit = unit.ToLower().Trim();

            // Return proper unit names
            if (unit == "m/s" || unit == "mps" || unit.Contains("meters per second") || unit.Contains("metres per second"))
                return "m/s";
            if (unit == "km/h" || unit == "kmh" || unit == "kph" || unit.Contains("kilometers per hour") || unit.Contains("kilometres per hour"))
                return "km/h";
            if (unit == "mph" || unit.Contains("miles per hour"))
                return "mph";
            if (unit == "ft/s" || unit == "fps" || unit.Contains("feet per second"))
                return "ft/s";
            if (unit == "knot" || unit == "knots" || unit == "kt" || unit == "kts")
                return "knots";
            if (unit == "mach")
                return "Mach";
            if (unit == "c" || unit.Contains("speed of light"))
                return "c (speed of light)";

            return unit;
        }

        private string GetSpeedContext(double mph)
        {
            if (mph < 1)
                return "Very slow (walking pace or slower)";
            else if (mph < 5)
                return "Walking speed";
            else if (mph < 15)
                return "Running/jogging speed";
            else if (mph < 30)
                return "Bicycle/residential speed";
            else if (mph < 45)
                return "City driving speed";
            else if (mph < 70)
                return "Highway speed";
            else if (mph < 100)
                return "Fast highway/autobahn speed";
            else if (mph < 200)
                return "Race car speed";
            else if (mph < 500)
                return "High-speed train";
            else if (mph < 770)
                return "Approaching sound barrier (subsonic)";
            else if (mph < 1500)
                return "Supersonic (faster than sound)";
            else if (mph < 10000)
                return "Hypersonic (5+ times speed of sound)";
            else if (mph < 17500)
                return "Spacecraft re-entry speed";
            else if (mph < 25000)
                return "Low Earth orbit speed";
            else
                return "Extreme velocity";
        }
    }
}
