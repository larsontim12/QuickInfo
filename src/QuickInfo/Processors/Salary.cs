using System;
using System.Text.RegularExpressions;
using static QuickInfo.NodeFactory;

namespace QuickInfo
{
    public class Salary : IProcessor
    {
        public object GetResult(Query query)
        {
            if (query.IsHelp)
            {
                return HelpTable(
                    ("salary", "Show salary conversion examples"),
                    ("50000 yearly", "Convert annual salary to hourly"),
                    ("25/hour", "Convert hourly wage to annual"),
                    ("75000 annual to hourly", "Calculate hourly rate"),
                    ("30/hr annual", "Calculate annual salary"));
            }

            var input = query.OriginalInput.Trim();

            // General salary info
            if (string.Equals(input, "salary", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "salary calculator", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "wage", StringComparison.OrdinalIgnoreCase))
            {
                return new object[]
                {
                    Answer("Salary Calculator"),
                    SectionHeader("Examples:"),
                    NameValueTable(entries: new[]
                    {
                        ("Annual to Hourly:", "50000 yearly"),
                        ("Hourly to Annual:", "25/hour"),
                        ("Explicit Convert:", "75000 annual to hourly"),
                        ("Per Hour:", "30/hr annual")
                    }),
                    SectionHeader("Assumptions:"),
                    NameValueTable(entries: new[]
                    {
                        ("Work Hours/Week:", "40 hours"),
                        ("Work Weeks/Year:", "52 weeks"),
                        ("Work Hours/Year:", "2,080 hours"),
                        ("Work Days/Year:", "260 days (5 days/week)")
                    })
                };
            }

            // Pattern: "X yearly/annual" or "X salary"
            var yearlyPattern = @"^\$?(\d+(?:,\d{3})*(?:\.\d+)?)\s*(?:yearly|annual|annually|per\s+year|\/year|year|salary)(?:\s+to\s+hourly)?$";
            var yearlyMatch = Regex.Match(input, yearlyPattern, RegexOptions.IgnoreCase);

            if (yearlyMatch.Success)
            {
                string salaryStr = yearlyMatch.Groups[1].Value.Replace(",", "");
                if (!double.TryParse(salaryStr, out double annualSalary))
                {
                    return null;
                }

                if (annualSalary < 1000 || annualSalary > 10000000)
                {
                    return FixedParagraph("Please enter a valid annual salary ($1,000 - $10,000,000)");
                }

                // Standard work year assumptions
                const int hoursPerWeek = 40;
                const int weeksPerYear = 52;
                const int hoursPerYear = 2080; // 40 * 52
                const int daysPerYear = 260; // 52 * 5

                double hourlyRate = annualSalary / hoursPerYear;
                double monthlyGross = annualSalary / 12;
                double biweeklyGross = annualSalary / 26;
                double weeklyGross = annualSalary / weeksPerYear;
                double dailyRate = annualSalary / daysPerYear;

                return new object[]
                {
                    Answer($"${annualSalary:N2}/year = ${hourlyRate:F2}/hour"),
                    SectionHeader("Annual Salary Breakdown:"),
                    NameValueTable(entries: new[]
                    {
                        ("Annual Salary:", $"${annualSalary:N2}"),
                        ("Hourly Rate:", $"${hourlyRate:F2}/hour"),
                        ("Monthly Gross:", $"${monthlyGross:N2}"),
                        ("Bi-Weekly Gross:", $"${biweeklyGross:N2}"),
                        ("Weekly Gross:", $"${weeklyGross:N2}"),
                        ("Daily Rate:", $"${dailyRate:F2}/day")
                    }),
                    SectionHeader("Work Schedule:"),
                    NameValueTable(entries: new[]
                    {
                        ("Hours per Week:", $"{hoursPerWeek} hours"),
                        ("Weeks per Year:", $"{weeksPerYear} weeks"),
                        ("Total Hours/Year:", $"{hoursPerYear:N0} hours"),
                        ("Work Days/Year:", $"{daysPerYear} days")
                    }),
                    SectionHeader("Tax Considerations:"),
                    FixedParagraph("Note: These are gross (before tax) amounts. Take-home pay will be lower after taxes, insurance, and other deductions.")
                };
            }

            // Pattern: "X/hour", "X/hr", "X per hour", "X hourly"
            var hourlyPattern = @"^\$?(\d+(?:\.\d+)?)\s*(?:\/hour|\/hr|per\s+hour|hourly|hour|hr)(?:\s+(?:to\s+)?(?:annual|yearly))?$";
            var hourlyMatch = Regex.Match(input, hourlyPattern, RegexOptions.IgnoreCase);

            if (hourlyMatch.Success)
            {
                if (!double.TryParse(hourlyMatch.Groups[1].Value, out double hourlyRate))
                {
                    return null;
                }

                if (hourlyRate < 7 || hourlyRate > 500)
                {
                    return FixedParagraph("Please enter a valid hourly rate ($7 - $500)");
                }

                // Standard work year assumptions
                const int hoursPerWeek = 40;
                const int weeksPerYear = 52;
                const int hoursPerYear = 2080;
                const int daysPerYear = 260;

                double annualSalary = hourlyRate * hoursPerYear;
                double monthlyGross = annualSalary / 12;
                double biweeklyGross = hourlyRate * hoursPerWeek * 2;
                double weeklyGross = hourlyRate * hoursPerWeek;
                double dailyRate = hourlyRate * 8;

                return new object[]
                {
                    Answer($"${hourlyRate:F2}/hour = ${annualSalary:N2}/year"),
                    SectionHeader("Hourly Rate Breakdown:"),
                    NameValueTable(entries: new[]
                    {
                        ("Hourly Rate:", $"${hourlyRate:F2}/hour"),
                        ("Annual Salary:", $"${annualSalary:N2}"),
                        ("Monthly Gross:", $"${monthlyGross:N2}"),
                        ("Bi-Weekly Gross:", $"${biweeklyGross:N2}"),
                        ("Weekly Gross:", $"${weeklyGross:N2}"),
                        ("Daily Rate (8 hrs):", $"${dailyRate:F2}/day")
                    }),
                    SectionHeader("Work Schedule:"),
                    NameValueTable(entries: new[]
                    {
                        ("Hours per Week:", $"{hoursPerWeek} hours"),
                        ("Weeks per Year:", $"{weeksPerYear} weeks"),
                        ("Total Hours/Year:", $"{hoursPerYear:N0} hours"),
                        ("Work Days/Year:", $"{daysPerYear} days")
                    }),
                    SectionHeader("Tax Considerations:"),
                    FixedParagraph("Note: These are gross (before tax) amounts. Take-home pay will be lower after taxes, insurance, and other deductions.")
                };
            }

            // Pattern: "X monthly" or "X per month"
            var monthlyPattern = @"^\$?(\d+(?:,\d{3})*(?:\.\d+)?)\s*(?:\/month|per\s+month|monthly|month)$";
            var monthlyMatch = Regex.Match(input, monthlyPattern, RegexOptions.IgnoreCase);

            if (monthlyMatch.Success)
            {
                string monthlyStr = monthlyMatch.Groups[1].Value.Replace(",", "");
                if (!double.TryParse(monthlyStr, out double monthlyGross))
                {
                    return null;
                }

                if (monthlyGross < 500 || monthlyGross > 1000000)
                {
                    return FixedParagraph("Please enter a valid monthly amount ($500 - $1,000,000)");
                }

                const int hoursPerYear = 2080;
                double annualSalary = monthlyGross * 12;
                double hourlyRate = annualSalary / hoursPerYear;
                double biweeklyGross = annualSalary / 26;
                double weeklyGross = annualSalary / 52;

                return new object[]
                {
                    Answer($"${monthlyGross:N2}/month = ${annualSalary:N2}/year"),
                    SectionHeader("Monthly Salary Breakdown:"),
                    NameValueTable(entries: new[]
                    {
                        ("Monthly Gross:", $"${monthlyGross:N2}"),
                        ("Annual Salary:", $"${annualSalary:N2}"),
                        ("Hourly Rate:", $"${hourlyRate:F2}/hour"),
                        ("Bi-Weekly Gross:", $"${biweeklyGross:N2}"),
                        ("Weekly Gross:", $"${weeklyGross:N2}")
                    })
                };
            }

            return null;
        }
    }
}
