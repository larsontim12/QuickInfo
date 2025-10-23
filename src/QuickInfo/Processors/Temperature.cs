using System;
using System.Text.RegularExpressions;
using static QuickInfo.NodeFactory;

namespace QuickInfo
{
    public class Temperature : IProcessor
    {
        public object GetResult(Query query)
        {
            if (query.IsHelp)
            {
                return HelpTable(
                    ("temperature", "Show temperature conversion formulas"),
                    ("32 f", "Convert Fahrenheit to Celsius and Kelvin"),
                    ("100 c", "Convert Celsius to Fahrenheit and Kelvin"),
                    ("273.15 k", "Convert Kelvin to Celsius and Fahrenheit"),
                    ("0 celsius", "Convert temperature to all scales"));
            }

            var input = query.OriginalInput.Trim();

            // General temperature info
            if (string.Equals(input, "temperature", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "temp conversion", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "temperature conversion", StringComparison.OrdinalIgnoreCase))
            {
                return new object[]
                {
                    Answer("Temperature Conversion Formulas"),
                    SectionHeader("Conversion Formulas:"),
                    FixedParagraph("°C = (°F - 32) × 5/9"),
                    FixedParagraph("°F = (°C × 9/5) + 32"),
                    FixedParagraph("K = °C + 273.15"),
                    SectionHeader("Key Reference Points:"),
                    NameValueTable(entries: new[]
                    {
                        ("Absolute Zero:", "-273.15°C = -459.67°F = 0K"),
                        ("Water Freezes:", "0°C = 32°F = 273.15K"),
                        ("Body Temperature:", "37°C = 98.6°F = 310.15K"),
                        ("Water Boils:", "100°C = 212°F = 373.15K")
                    })
                };
            }

            // Parse temperature input patterns
            // Patterns: "32 f", "100°C", "273.15 kelvin", "-40 fahrenheit"
            var tempPattern = @"^(-?\d+(?:\.\d+)?)\s*°?\s*(f|c|k|fahrenheit|celsius|kelvin)\b";
            var match = Regex.Match(input, tempPattern, RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return null;
            }

            if (!double.TryParse(match.Groups[1].Value, out double value))
            {
                return null;
            }

            string unit = match.Groups[2].Value.ToLower();

            // Normalize unit
            char unitChar = unit[0];
            if (unit == "fahrenheit") unitChar = 'f';
            if (unit == "celsius") unitChar = 'c';
            if (unit == "kelvin") unitChar = 'k';

            double celsius, fahrenheit, kelvin;

            // Convert to all scales
            switch (unitChar)
            {
                case 'f':
                    fahrenheit = value;
                    celsius = (fahrenheit - 32) * 5.0 / 9.0;
                    kelvin = celsius + 273.15;
                    break;

                case 'c':
                    celsius = value;
                    fahrenheit = (celsius * 9.0 / 5.0) + 32;
                    kelvin = celsius + 273.15;
                    break;

                case 'k':
                    kelvin = value;
                    if (kelvin < 0)
                    {
                        return FixedParagraph("Kelvin temperature cannot be negative (absolute zero is 0K)");
                    }
                    celsius = kelvin - 273.15;
                    fahrenheit = (celsius * 9.0 / 5.0) + 32;
                    break;

                default:
                    return null;
            }

            // Validate reasonable range for display (but allow scientific extremes)
            if (kelvin < 0)
            {
                return FixedParagraph($"Temperature below absolute zero is not physically possible ({kelvin:F2}K)");
            }

            string classification = ClassifyTemperature(celsius);
            string scientificContext = GetScientificContext(kelvin);

            var result = new object[]
            {
                Answer($"{value}°{char.ToUpper(unitChar)} = {celsius:F2}°C = {fahrenheit:F2}°F = {kelvin:F2}K"),
                SectionHeader("Temperature Conversions:"),
                NameValueTable(entries: new[]
                {
                    ("Celsius:", $"{celsius:F2}°C"),
                    ("Fahrenheit:", $"{fahrenheit:F2}°F"),
                    ("Kelvin:", $"{kelvin:F2}K"),
                    ("Classification:", classification)
                })
            };

            if (!string.IsNullOrEmpty(scientificContext))
            {
                result = new object[]
                {
                    Answer($"{value}°{char.ToUpper(unitChar)} = {celsius:F2}°C = {fahrenheit:F2}°F = {kelvin:F2}K"),
                    SectionHeader("Temperature Conversions:"),
                    NameValueTable(entries: new[]
                    {
                        ("Celsius:", $"{celsius:F2}°C"),
                        ("Fahrenheit:", $"{fahrenheit:F2}°F"),
                        ("Kelvin:", $"{kelvin:F2}K"),
                        ("Classification:", classification)
                    }),
                    SectionHeader("Context:"),
                    FixedParagraph(scientificContext)
                };
            }

            return result;
        }

        private string ClassifyTemperature(double celsius)
        {
            if (celsius < -200)
                return "Cryogenic (Extreme Cold)";
            else if (celsius < -100)
                return "Ultra-Cold (Dry Ice Range)";
            else if (celsius < -40)
                return "Extreme Cold";
            else if (celsius < -20)
                return "Very Cold (Arctic)";
            else if (celsius < 0)
                return "Freezing";
            else if (celsius < 10)
                return "Cold";
            else if (celsius < 20)
                return "Cool";
            else if (celsius < 25)
                return "Comfortable";
            else if (celsius < 30)
                return "Warm";
            else if (celsius < 35)
                return "Hot";
            else if (celsius < 40)
                return "Very Hot";
            else if (celsius < 100)
                return "Extremely Hot";
            else if (celsius < 200)
                return "Boiling Range";
            else if (celsius < 1000)
                return "High Heat (Industrial)";
            else
                return "Extreme Heat";
        }

        private string GetScientificContext(double kelvin)
        {
            if (System.Math.Abs(kelvin) < 0.01)
                return "Absolute zero - theoretically the lowest possible temperature";
            else if (kelvin < 4.2)
                return "Liquid helium range - used in superconductivity research";
            else if (kelvin < 77)
                return "Liquid nitrogen range - common cryogenic coolant";
            else if (kelvin < 195)
                return "Dry ice sublimation range (CO₂)";
            else if (System.Math.Abs(kelvin - 273.15) < 0.01)
                return "Water freezing point at standard pressure";
            else if (System.Math.Abs(kelvin - 310.15) < 1)
                return "Normal human body temperature";
            else if (System.Math.Abs(kelvin - 373.15) < 0.5)
                return "Water boiling point at standard pressure";
            else if (kelvin > 1000 && kelvin < 2000)
                return "Molten metal range (aluminum, copper)";
            else if (kelvin > 2000 && kelvin < 3700)
                return "Steel melting range, high-temperature furnaces";
            else if (kelvin > 3700)
                return "Extreme industrial temperatures";

            return string.Empty;
        }
    }
}
