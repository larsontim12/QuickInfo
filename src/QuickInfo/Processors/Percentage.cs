using System;
using System.Text.RegularExpressions;
using static QuickInfo.NodeFactory;

namespace QuickInfo
{
    public class Percentage : IProcessor
    {
        public object GetResult(Query query)
        {
            if (query.IsHelp)
            {
                return HelpTable(
                    ("percentage", "Show percentage calculation examples"),
                    ("20% of 150", "Calculate percentage of a number"),
                    ("30 is what % of 120", "Find percentage"),
                    ("50 increased by 20%", "Calculate percentage increase"),
                    ("100 decreased by 15%", "Calculate percentage decrease"),
                    ("tip on 85.50", "Calculate 15%, 18%, 20% tips"));
            }

            var input = query.OriginalInput.Trim();

            // General percentage info
            if (string.Equals(input, "percentage", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "percent", StringComparison.OrdinalIgnoreCase))
            {
                return new object[]
                {
                    Answer("Percentage Calculations"),
                    SectionHeader("Common Operations:"),
                    NameValueTable(entries: new[]
                    {
                        ("Find Percent:", "20% of 150 → 30"),
                        ("Find Percentage:", "30 is what % of 120 → 25%"),
                        ("Increase:", "50 increased by 20% → 60"),
                        ("Decrease:", "100 decreased by 15% → 85"),
                        ("Percent Change:", "from 80 to 100 → +25%"),
                        ("Tip Calculator:", "tip on 85.50")
                    })
                };
            }

            // Pattern: "X% of Y" or "X percent of Y"
            var percentOfPattern = @"^(\d+(?:\.\d+)?)\s*%?\s*(?:percent|%)\s+of\s+(\d+(?:\.\d+)?)";
            var percentOfMatch = Regex.Match(input, percentOfPattern, RegexOptions.IgnoreCase);

            if (percentOfMatch.Success)
            {
                if (!double.TryParse(percentOfMatch.Groups[1].Value, out double percent) ||
                    !double.TryParse(percentOfMatch.Groups[2].Value, out double number))
                {
                    return null;
                }

                double result = (percent / 100.0) * number;
                double remaining = number - result;

                return new object[]
                {
                    Answer($"{percent}% of {number} = {result:F2}"),
                    SectionHeader("Calculation:"),
                    NameValueTable(entries: new[]
                    {
                        ("Original Number:", $"{number:F2}"),
                        ("Percentage:", $"{percent}%"),
                        ("Result:", $"{result:F2}"),
                        ("Remaining:", $"{remaining:F2} ({100 - percent}%)")
                    })
                };
            }

            // Pattern: "X is what % of Y" or "X is what percent of Y"
            var whatPercentPattern = @"^(\d+(?:\.\d+)?)\s+is\s+what\s+%?\s*(?:percent|%|percentage)?\s+of\s+(\d+(?:\.\d+)?)";
            var whatPercentMatch = Regex.Match(input, whatPercentPattern, RegexOptions.IgnoreCase);

            if (whatPercentMatch.Success)
            {
                if (!double.TryParse(whatPercentMatch.Groups[1].Value, out double part) ||
                    !double.TryParse(whatPercentMatch.Groups[2].Value, out double whole))
                {
                    return null;
                }

                if (whole == 0)
                {
                    return FixedParagraph("Cannot calculate percentage: division by zero");
                }

                double percent = (part / whole) * 100.0;

                return new object[]
                {
                    Answer($"{part} is {percent:F2}% of {whole}"),
                    SectionHeader("Calculation:"),
                    NameValueTable(entries: new[]
                    {
                        ("Part:", $"{part:F2}"),
                        ("Whole:", $"{whole:F2}"),
                        ("Percentage:", $"{percent:F2}%"),
                        ("Formula:", $"({part} / {whole}) × 100")
                    })
                };
            }

            // Pattern: "X increased by Y%" or "X plus Y%"
            var increasePattern = @"^(\d+(?:\.\d+)?)\s+(?:increased|increased by|plus|add|\+)\s+(\d+(?:\.\d+)?)\s*%?\s*(?:percent|%)?";
            var increaseMatch = Regex.Match(input, increasePattern, RegexOptions.IgnoreCase);

            if (increaseMatch.Success)
            {
                if (!double.TryParse(increaseMatch.Groups[1].Value, out double original) ||
                    !double.TryParse(increaseMatch.Groups[2].Value, out double percent))
                {
                    return null;
                }

                double increase = (percent / 100.0) * original;
                double result = original + increase;
                double multiplier = 1 + (percent / 100.0);

                return new object[]
                {
                    Answer($"{original} increased by {percent}% = {result:F2}"),
                    SectionHeader("Percentage Increase:"),
                    NameValueTable(entries: new[]
                    {
                        ("Original Value:", $"{original:F2}"),
                        ("Increase Percentage:", $"{percent}%"),
                        ("Increase Amount:", $"+{increase:F2}"),
                        ("New Value:", $"{result:F2}"),
                        ("Multiplier:", $"×{multiplier:F2}")
                    })
                };
            }

            // Pattern: "X decreased by Y%" or "X minus Y%"
            var decreasePattern = @"^(\d+(?:\.\d+)?)\s+(?:decreased|decreased by|minus|subtract|-)\s+(\d+(?:\.\d+)?)\s*%?\s*(?:percent|%)?";
            var decreaseMatch = Regex.Match(input, decreasePattern, RegexOptions.IgnoreCase);

            if (decreaseMatch.Success)
            {
                if (!double.TryParse(decreaseMatch.Groups[1].Value, out double original) ||
                    !double.TryParse(decreaseMatch.Groups[2].Value, out double percent))
                {
                    return null;
                }

                if (percent > 100)
                {
                    return FixedParagraph("Percentage decrease cannot exceed 100%");
                }

                double decrease = (percent / 100.0) * original;
                double result = original - decrease;
                double multiplier = 1 - (percent / 100.0);

                return new object[]
                {
                    Answer($"{original} decreased by {percent}% = {result:F2}"),
                    SectionHeader("Percentage Decrease:"),
                    NameValueTable(entries: new[]
                    {
                        ("Original Value:", $"{original:F2}"),
                        ("Decrease Percentage:", $"{percent}%"),
                        ("Decrease Amount:", $"-{decrease:F2}"),
                        ("New Value:", $"{result:F2}"),
                        ("Multiplier:", $"×{multiplier:F2}")
                    })
                };
            }

            // Pattern: "from X to Y" or "X to Y" (percent change)
            var changePattern = @"^(?:from\s+)?(\d+(?:\.\d+)?)\s+to\s+(\d+(?:\.\d+)?)(?:\s+(?:percent|percentage|%|change))?$";
            var changeMatch = Regex.Match(input, changePattern, RegexOptions.IgnoreCase);

            if (changeMatch.Success && input.Contains("to", StringComparison.OrdinalIgnoreCase))
            {
                if (!double.TryParse(changeMatch.Groups[1].Value, out double oldValue) ||
                    !double.TryParse(changeMatch.Groups[2].Value, out double newValue))
                {
                    return null;
                }

                if (oldValue == 0)
                {
                    return FixedParagraph("Cannot calculate percentage change from zero");
                }

                double change = newValue - oldValue;
                double percentChange = (change / oldValue) * 100.0;
                string direction = change >= 0 ? "Increase" : "Decrease";
                string sign = change >= 0 ? "+" : "";

                return new object[]
                {
                    Answer($"{oldValue} to {newValue} = {sign}{percentChange:F2}% change"),
                    SectionHeader($"Percentage {direction}:"),
                    NameValueTable(entries: new[]
                    {
                        ("Original Value:", $"{oldValue:F2}"),
                        ("New Value:", $"{newValue:F2}"),
                        ("Change Amount:", $"{sign}{change:F2}"),
                        ("Percent Change:", $"{sign}{percentChange:F2}%"),
                        ("Direction:", direction)
                    })
                };
            }

            // Pattern: "tip on X" or "tip for X"
            var tipPattern = @"^(?:tip|tips?)\s+(?:on|for)\s+\$?(\d+(?:\.\d+)?)";
            var tipMatch = Regex.Match(input, tipPattern, RegexOptions.IgnoreCase);

            if (tipMatch.Success)
            {
                if (!double.TryParse(tipMatch.Groups[1].Value, out double bill))
                {
                    return null;
                }

                double tip15 = bill * 0.15;
                double tip18 = bill * 0.18;
                double tip20 = bill * 0.20;
                double tip25 = bill * 0.25;

                return new object[]
                {
                    Answer($"Tip Calculator for ${bill:F2}"),
                    SectionHeader("Standard Tip Amounts:"),
                    NameValueTable(entries: new[]
                    {
                        ("15% (Fair):", $"${tip15:F2} (Total: ${bill + tip15:F2})"),
                        ("18% (Good):", $"${tip18:F2} (Total: ${bill + tip18:F2})"),
                        ("20% (Great):", $"${tip20:F2} (Total: ${bill + tip20:F2})"),
                        ("25% (Excellent):", $"${tip25:F2} (Total: ${bill + tip25:F2})")
                    }),
                    SectionHeader("Split Bill (2 people):"),
                    NameValueTable(entries: new[]
                    {
                        ("Bill per person:", $"${bill / 2:F2}"),
                        ("With 18% tip:", $"${(bill + tip18) / 2:F2} each"),
                        ("With 20% tip:", $"${(bill + tip20) / 2:F2} each")
                    })
                };
            }

            return null;
        }
    }
}
