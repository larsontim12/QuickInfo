using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static QuickInfo.NodeFactory;

namespace QuickInfo
{
    public class Pressure : IProcessor
    {
        private static readonly Dictionary<string, double> ToPascals = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            // SI units
            { "pa", 1.0 },
            { "pascal", 1.0 },
            { "pascals", 1.0 },
            { "kpa", 1000.0 },
            { "kilopascal", 1000.0 },
            { "kilopascals", 1000.0 },
            { "mpa", 1000000.0 },
            { "megapascal", 1000000.0 },
            { "megapascals", 1000000.0 },

            // Common pressure units
            { "bar", 100000.0 },
            { "bars", 100000.0 },
            { "mbar", 100.0 },
            { "millibar", 100.0 },
            { "millibars", 100.0 },
            { "atm", 101325.0 },
            { "atmosphere", 101325.0 },
            { "atmospheres", 101325.0 },

            // Imperial/US units
            { "psi", 6894.76 },
            { "psig", 6894.76 },  // Gauge pressure (same conversion, but meaning differs)
            { "pound per square inch", 6894.76 },
            { "pounds per square inch", 6894.76 },

            // Other units
            { "torr", 133.322 },
            { "mmhg", 133.322 },
            { "mm hg", 133.322 },
            { "millimeter of mercury", 133.322 },
            { "millimeters of mercury", 133.322 },
            { "inhg", 3386.39 },
            { "in hg", 3386.39 },
            { "inch of mercury", 3386.39 },
            { "inches of mercury", 3386.39 }
        };

        public object GetResult(Query query)
        {
            if (query.IsHelp)
            {
                return HelpTable(
                    ("pressure", "Show pressure conversion units"),
                    ("30 psi to bar", "Convert PSI to bar"),
                    ("1 atm to psi", "Convert atmospheres to PSI"),
                    ("100 kpa to psi", "Convert kilopascals to PSI"),
                    ("29.92 inhg to psi", "Convert inches of mercury to PSI"));
            }

            var input = query.OriginalInput.Trim();

            // General pressure info
            if (string.Equals(input, "pressure", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "pressure conversion", StringComparison.OrdinalIgnoreCase))
            {
                return new object[]
                {
                    Answer("Pressure Conversions"),
                    SectionHeader("Supported Units:"),
                    FixedParagraph("SI: Pa, kPa, MPa"),
                    FixedParagraph("Common: bar, mbar, atm (atmosphere)"),
                    FixedParagraph("Imperial: psi, psig"),
                    FixedParagraph("Mercury: torr, mmHg, inHg"),
                    SectionHeader("Standard Reference Values:"),
                    NameValueTable(entries: new[]
                    {
                        ("1 atm:", "101,325 Pa / 14.7 psi / 1.013 bar"),
                        ("Sea Level:", "~1 atm / 14.7 psi / 101.3 kPa"),
                        ("1 bar:", "100,000 Pa / 14.5 psi"),
                        ("1 psi:", "6,894.76 Pa / 0.0689 bar"),
                        ("Tire Pressure:", "~32 psi / 2.2 bar (typical car)"),
                        ("Vacuum:", "0 psi / 0 bar (absolute)")
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

            if (!ToPascals.TryGetValue(fromUnit, out double fromMultiplier))
            {
                return FixedParagraph($"Unknown pressure unit '{fromUnit}'. Supported: pa, kpa, bar, atm, psi, torr, mmhg, inhg");
            }

            if (!ToPascals.TryGetValue(toUnit, out double toMultiplier))
            {
                return FixedParagraph($"Unknown pressure unit '{toUnit}'. Supported: pa, kpa, bar, atm, psi, torr, mmhg, inhg");
            }

            // Convert through Pascals
            double pascals = value * fromMultiplier;
            double result = pascals / toMultiplier;

            // Normalize unit names for display
            string fromUnitDisplay = NormalizeUnitName(fromUnit, value);
            string toUnitDisplay = NormalizeUnitName(toUnit, result);

            // Calculate additional common conversions
            double kilopascals = pascals / 1000.0;
            double bar = pascals / 100000.0;
            double psi = pascals / 6894.76;
            double atmospheres = pascals / 101325.0;

            // Determine context
            string context = GetPressureContext(psi);

            return new object[]
            {
                Answer($"{value:F2} {fromUnitDisplay} = {result:F2} {toUnitDisplay}"),
                SectionHeader("Conversion Result:"),
                NameValueTable(entries: new[]
                {
                    ("Original:", $"{value:F2} {fromUnitDisplay}"),
                    ("Converted:", $"{result:F2} {toUnitDisplay}"),
                    ("In Pascals:", $"{pascals:F2} Pa"),
                    ("Context:", context)
                }),
                SectionHeader("Additional Conversions:"),
                NameValueTable(entries: new[]
                {
                    ("Pascals:", $"{pascals:F2} Pa"),
                    ("Kilopascals:", $"{kilopascals:F2} kPa"),
                    ("Bar:", $"{bar:F4} bar"),
                    ("PSI:", $"{psi:F2} psi"),
                    ("Atmospheres:", $"{atmospheres:F4} atm"),
                    ("mmHg (Torr):", $"{pascals / 133.322:F2} mmHg"),
                    ("inHg:", $"{pascals / 3386.39:F2} inHg")
                })
            };
        }

        private string NormalizeUnitName(string unit, double value)
        {
            unit = unit.ToLower().Trim();

            // Return proper unit names with singular/plural
            if (unit == "pa" || unit == "pascal")
                return value == 1 ? "pascal" : "pascals";
            if (unit == "pascals")
                return "pascals";
            if (unit == "kpa" || unit == "kilopascal")
                return value == 1 ? "kilopascal" : "kilopascals";
            if (unit == "kilopascals")
                return "kilopascals";
            if (unit == "mpa" || unit == "megapascal")
                return value == 1 ? "megapascal" : "megapascals";
            if (unit == "megapascals")
                return "megapascals";
            if (unit == "bar")
                return value == 1 ? "bar" : "bar";
            if (unit == "bars")
                return "bar";
            if (unit == "mbar" || unit == "millibar")
                return value == 1 ? "millibar" : "millibars";
            if (unit == "millibars")
                return "millibars";
            if (unit == "atm" || unit == "atmosphere")
                return value == 1 ? "atmosphere" : "atmospheres";
            if (unit == "atmospheres")
                return "atmospheres";
            if (unit == "psi" || unit == "psig" || unit.Contains("pound"))
                return "psi";
            if (unit == "torr")
                return "torr";
            if (unit == "mmhg" || unit == "mm hg" || unit.Contains("millimeter"))
                return "mmHg";
            if (unit == "inhg" || unit == "in hg" || unit.Contains("inch"))
                return "inHg";

            return unit;
        }

        private string GetPressureContext(double psi)
        {
            if (psi < 0.1)
                return "Near vacuum / Very low pressure";
            else if (psi < 5)
                return "Low pressure (HVAC, vacuum systems)";
            else if (psi < 10)
                return "Low pressure (natural gas lines)";
            else if (psi < 14.7)
                return "Below atmospheric pressure";
            else if (System.Math.Abs(psi - 14.7) < 0.5)
                return "Standard atmospheric pressure (sea level)";
            else if (psi < 20)
                return "Slightly above atmospheric";
            else if (psi < 40)
                return "Typical tire pressure range";
            else if (psi < 100)
                return "Medium pressure (pneumatic tools)";
            else if (psi < 200)
                return "High pressure (hydraulic systems)";
            else if (psi < 1000)
                return "Very high pressure (industrial)";
            else if (psi < 10000)
                return "Extreme pressure (specialized equipment)";
            else
                return "Ultra-high pressure";
        }
    }
}
