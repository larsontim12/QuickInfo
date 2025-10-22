using System;
using System.Text.RegularExpressions;
using static QuickInfo.NodeFactory;

namespace QuickInfo
{
    public class CompoundInterest : IProcessor
    {
        public object GetResult(Query query)
        {
            if (query.IsHelp)
            {
                return HelpTable(
                    ("compound interest", "Show compound interest examples"),
                    ("compound 1000 at 5% for 10 years", "Calculate compound interest"),
                    ("invest 5000 at 7% for 20 years", "Calculate investment growth"),
                    ("compound 10000 at 3.5% for 5 years monthly", "Monthly compounding"),
                    ("savings 2500 at 4% for 15 years", "Calculate savings growth"));
            }

            var input = query.OriginalInput.Trim();

            // General compound interest info
            if (string.Equals(input, "compound interest", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "compound", StringComparison.OrdinalIgnoreCase))
            {
                return new object[]
                {
                    Answer("Compound Interest Calculator"),
                    SectionHeader("Examples:"),
                    NameValueTable(entries: new[]
                    {
                        ("Annual Compound:", "compound 1000 at 5% for 10 years"),
                        ("Investment:", "invest 5000 at 7% for 20 years"),
                        ("Monthly Compound:", "compound 10000 at 3% for 5 years monthly"),
                        ("Quarterly:", "invest 2000 at 6% for 8 years quarterly")
                    }),
                    SectionHeader("Compounding Frequency:"),
                    NameValueTable(entries: new[]
                    {
                        ("Annual:", "Once per year (default)"),
                        ("Monthly:", "12 times per year"),
                        ("Quarterly:", "4 times per year"),
                        ("Daily:", "365 times per year")
                    }),
                    SectionHeader("Formula:"),
                    FixedParagraph("A = P(1 + r/n)^(nt)"),
                    FixedParagraph("A = final amount, P = principal, r = rate, n = compounds/year, t = years")
                };
            }

            // Pattern: "compound/invest X at Y% for Z years [frequency]"
            var compoundPattern = @"^(?:compound|invest|investment|savings?)\s+\$?(\d+(?:,\d{3})*(?:\.\d+)?)\s+(?:at|@)\s+(\d+(?:\.\d+)?)\s*%?\s+(?:for|over)\s+(\d+)\s+years?\s*(annual|annually|monthly|quarterly|daily)?$";
            var match = Regex.Match(input, compoundPattern, RegexOptions.IgnoreCase);

            if (match.Success)
            {
                string principalStr = match.Groups[1].Value.Replace(",", "");
                if (!double.TryParse(principalStr, out double principal) ||
                    !double.TryParse(match.Groups[2].Value, out double annualRate) ||
                    !int.TryParse(match.Groups[3].Value, out int years))
                {
                    return null;
                }

                string frequency = match.Groups[4].Success ? match.Groups[4].Value.ToLower() : "annual";

                if (principal <= 0 || principal > 10000000)
                {
                    return FixedParagraph("Please enter a valid principal amount ($1 - $10,000,000)");
                }

                if (annualRate < 0 || annualRate > 30)
                {
                    return FixedParagraph("Please enter a valid interest rate (0% - 30%)");
                }

                if (years <= 0 || years > 100)
                {
                    return FixedParagraph("Please enter a valid time period (1 - 100 years)");
                }

                // Determine compounding frequency
                int compoundsPerYear;
                string frequencyDisplay;
                switch (frequency)
                {
                    case "monthly":
                        compoundsPerYear = 12;
                        frequencyDisplay = "Monthly";
                        break;
                    case "quarterly":
                        compoundsPerYear = 4;
                        frequencyDisplay = "Quarterly";
                        break;
                    case "daily":
                        compoundsPerYear = 365;
                        frequencyDisplay = "Daily";
                        break;
                    default: // annual/annually or not specified
                        compoundsPerYear = 1;
                        frequencyDisplay = "Annual";
                        break;
                }

                // Calculate compound interest
                // A = P(1 + r/n)^(nt)
                double rate = annualRate / 100.0;
                double amount = principal * Math.Pow(1 + (rate / compoundsPerYear), compoundsPerYear * years);
                double interest = amount - principal;
                double percentGain = (interest / principal) * 100.0;

                // Calculate for comparison: simple interest
                double simpleInterest = principal * rate * years;
                double simpleAmount = principal + simpleInterest;
                double compoundAdvantage = amount - simpleAmount;

                // Calculate year-by-year growth for first 5 years
                var yearGrowth = new System.Text.StringBuilder();
                for (int i = 1; i <= Math.Min(5, years); i++)
                {
                    double yearAmount = principal * Math.Pow(1 + (rate / compoundsPerYear), compoundsPerYear * i);
                    yearGrowth.AppendLine($"Year {i}: ${yearAmount:N2}");
                }

                // Calculate effective annual rate
                double effectiveRate = (Math.Pow(1 + (rate / compoundsPerYear), compoundsPerYear) - 1) * 100;

                return new object[]
                {
                    Answer($"${principal:N2} at {annualRate}% for {years} years = ${amount:N2}"),
                    SectionHeader("Compound Interest Calculation:"),
                    NameValueTable(entries: new[]
                    {
                        ("Principal (Start):", $"${principal:N2}"),
                        ("Interest Rate:", $"{annualRate}% per year"),
                        ("Time Period:", $"{years} years"),
                        ("Compounding:", $"{frequencyDisplay} ({compoundsPerYear}x per year)"),
                        ("Final Amount:", $"${amount:N2}"),
                        ("Total Interest:", $"${interest:N2}"),
                        ("Percent Gain:", $"{percentGain:F2}%")
                    }),
                    SectionHeader("Interest Comparison:"),
                    NameValueTable(entries: new[]
                    {
                        ("Compound Interest:", $"${interest:N2}"),
                        ("Simple Interest:", $"${simpleInterest:N2}"),
                        ("Compound Advantage:", $"${compoundAdvantage:N2}"),
                        ("Effective Annual Rate:", $"{effectiveRate:F3}%")
                    }),
                    SectionHeader("Growth Over Time:"),
                    FixedParagraph(yearGrowth.ToString().TrimEnd()),
                    SectionHeader("Formula Used:"),
                    FixedParagraph($"A = ${principal:N2} × (1 + {annualRate}%/{compoundsPerYear})^({compoundsPerYear} × {years})")
                };
            }

            return null;
        }
    }
}
