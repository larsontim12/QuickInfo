using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using static QuickInfo.NodeFactory;

namespace QuickInfo
{
    public class Loan : IProcessor
    {
        public object GetResult(Query query)
        {
            if (query.IsHelp)
            {
                return HelpTable(
                    ("loan", "Show loan calculation examples"),
                    ("loan 20000 at 5% for 5 years", "Calculate monthly payment"),
                    ("mortgage 250000 at 3.5% for 30 years", "Calculate mortgage payment"),
                    ("car loan 35000 at 4.2% for 60 months", "Calculate car payment"));
            }

            var input = query.OriginalInput.Trim();

            // General loan info
            if (string.Equals(input, "loan", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "loan calculator", StringComparison.OrdinalIgnoreCase))
            {
                return new object[]
                {
                    Answer("Loan Payment Calculator"),
                    SectionHeader("Examples:"),
                    NameValueTable(entries: new[]
                    {
                        ("Car Loan:", "loan 25000 at 4.5% for 5 years"),
                        ("Mortgage:", "mortgage 300000 at 3.5% for 30 years"),
                        ("Personal Loan:", "loan 10000 at 8% for 36 months")
                    }),
                    SectionHeader("Supported Keywords:"),
                    FixedParagraph("loan, mortgage, car loan"),
                    FixedParagraph("Time: years, months")
                };
            }

            // Pattern: "loan/mortgage X at Y% for Z years/months"
            var loanPattern = @"^(?:loan|mortgage|car\s+loan|auto\s+loan)\s+\$?(\d+(?:,\d{3})*(?:\.\d+)?)\s+(?:at|@)\s+(\d+(?:\.\d+)?)\s*%?\s+(?:for|over)\s+(\d+)\s+(years?|months?)";
            var loanMatch = Regex.Match(input, loanPattern, RegexOptions.IgnoreCase);

            if (loanMatch.Success)
            {
                string principalStr = loanMatch.Groups[1].Value.Replace(",", "");
                if (!double.TryParse(principalStr, out double principal) ||
                    !double.TryParse(loanMatch.Groups[2].Value, out double annualRate) ||
                    !int.TryParse(loanMatch.Groups[3].Value, out int timePeriod))
                {
                    return null;
                }

                string timeUnit = loanMatch.Groups[4].Value.ToLower();
                bool isYears = timeUnit.StartsWith("year");

                // Convert to months
                int totalMonths = isYears ? timePeriod * 12 : timePeriod;

                if (principal <= 0 || principal > 100000000)
                {
                    return FixedParagraph("Please enter a valid loan amount ($1 - $100,000,000)");
                }

                if (annualRate < 0 || annualRate > 50)
                {
                    return FixedParagraph("Please enter a valid interest rate (0% - 50%)");
                }

                if (totalMonths <= 0 || totalMonths > 360)
                {
                    return FixedParagraph("Please enter a valid loan term (1 - 360 months / 1 - 30 years)");
                }

                // Calculate monthly payment using amortization formula
                double monthlyRate = annualRate / 100.0 / 12.0;
                double monthlyPayment;

                if (monthlyRate == 0)
                {
                    // No interest
                    monthlyPayment = principal / totalMonths;
                }
                else
                {
                    // Standard amortization formula: P * [r(1+r)^n] / [(1+r)^n - 1]
                    monthlyPayment = principal * (monthlyRate * System.Math.Pow(1 + monthlyRate, totalMonths)) /
                                   (System.Math.Pow(1 + monthlyRate, totalMonths) - 1);
                }

                double totalPayment = monthlyPayment * totalMonths;
                double totalInterest = totalPayment - principal;
                double percentInterest = (totalInterest / principal) * 100.0;

                // Calculate first payment breakdown
                double firstMonthInterest = principal * monthlyRate;
                double firstMonthPrincipal = monthlyPayment - firstMonthInterest;

                // Calculate payoff at different points
                double fiveYearBalance = 0;
                double tenYearBalance = 0;

                if (totalMonths > 60)
                {
                    fiveYearBalance = CalculateRemainingBalance(principal, monthlyRate, totalMonths, 60);
                }

                if (totalMonths > 120)
                {
                    tenYearBalance = CalculateRemainingBalance(principal, monthlyRate, totalMonths, 120);
                }

                var result = new object[]
                {
                    Answer($"${monthlyPayment:F2}/month for {totalMonths} months"),
                    SectionHeader("Loan Summary:"),
                    NameValueTable(entries: new[]
                    {
                        ("Loan Amount:", $"${principal:N2}"),
                        ("Interest Rate:", $"{annualRate:F2}% APR"),
                        ("Loan Term:", $"{totalMonths} months ({totalMonths / 12.0:F1} years)"),
                        ("Monthly Payment:", $"${monthlyPayment:F2}"),
                        ("Total Paid:", $"${totalPayment:N2}"),
                        ("Total Interest:", $"${totalInterest:N2} ({percentInterest:F1}% of loan)")
                    }),
                    SectionHeader("First Payment Breakdown:"),
                    NameValueTable(entries: new[]
                    {
                        ("Principal:", $"${firstMonthPrincipal:F2}"),
                        ("Interest:", $"${firstMonthInterest:F2}"),
                        ("Total Payment:", $"${monthlyPayment:F2}")
                    })
                };

                // Generate amortization schedule
                string amortizationSchedule = GenerateAmortizationSchedule(principal, monthlyRate, monthlyPayment, totalMonths);

                // Add balance projections and amortization schedule
                if (totalMonths > 60)
                {
                    var projections = new[]
                    {
                        ("After 5 years:", $"${fiveYearBalance:N2} remaining"),
                        tenYearBalance > 0 ? ("After 10 years:", $"${tenYearBalance:N2} remaining") : ("", "")
                    };

                    result = new object[]
                    {
                        Answer($"${monthlyPayment:F2}/month for {totalMonths} months"),
                        SectionHeader("Loan Summary:"),
                        NameValueTable(entries: new[]
                        {
                            ("Loan Amount:", $"${principal:N2}"),
                            ("Interest Rate:", $"{annualRate:F2}% APR"),
                            ("Loan Term:", $"{totalMonths} months ({totalMonths / 12.0:F1} years)"),
                            ("Monthly Payment:", $"${monthlyPayment:F2}"),
                            ("Total Paid:", $"${totalPayment:N2}"),
                            ("Total Interest:", $"${totalInterest:N2} ({percentInterest:F1}% of loan)")
                        }),
                        SectionHeader("First Payment Breakdown:"),
                        NameValueTable(entries: new[]
                        {
                            ("Principal:", $"${firstMonthPrincipal:F2}"),
                            ("Interest:", $"${firstMonthInterest:F2}"),
                            ("Total Payment:", $"${monthlyPayment:F2}")
                        }),
                        SectionHeader("Balance Projections:"),
                        NameValueTable(entries: projections),
                        SectionHeader("Amortization Schedule:"),
                        FixedParagraph(amortizationSchedule)
                    };
                }
                else
                {
                    result = new object[]
                    {
                        Answer($"${monthlyPayment:F2}/month for {totalMonths} months"),
                        SectionHeader("Loan Summary:"),
                        NameValueTable(entries: new[]
                        {
                            ("Loan Amount:", $"${principal:N2}"),
                            ("Interest Rate:", $"{annualRate:F2}% APR"),
                            ("Loan Term:", $"{totalMonths} months ({totalMonths / 12.0:F1} years)"),
                            ("Monthly Payment:", $"${monthlyPayment:F2}"),
                            ("Total Paid:", $"${totalPayment:N2}"),
                            ("Total Interest:", $"${totalInterest:N2} ({percentInterest:F1}% of loan)")
                        }),
                        SectionHeader("First Payment Breakdown:"),
                        NameValueTable(entries: new[]
                        {
                            ("Principal:", $"${firstMonthPrincipal:F2}"),
                            ("Interest:", $"${firstMonthInterest:F2}"),
                            ("Total Payment:", $"${monthlyPayment:F2}")
                        }),
                        SectionHeader("Amortization Schedule:"),
                        FixedParagraph(amortizationSchedule)
                    };
                }

                return result;
            }

            return null;
        }

        private double CalculateRemainingBalance(double principal, double monthlyRate, int totalMonths, int paymentsMade)
        {
            // Calculate remaining balance after X payments
            // Formula: B = P * [(1+r)^n - (1+r)^p] / [(1+r)^n - 1]
            // where B = balance, P = principal, r = monthly rate, n = total payments, p = payments made

            if (monthlyRate == 0)
            {
                return principal - (principal / totalMonths * paymentsMade);
            }

            double numerator = System.Math.Pow(1 + monthlyRate, totalMonths) - System.Math.Pow(1 + monthlyRate, paymentsMade);
            double denominator = System.Math.Pow(1 + monthlyRate, totalMonths) - 1;

            return principal * (numerator / denominator);
        }

        private string GenerateAmortizationSchedule(double principal, double monthlyRate, double monthlyPayment, int totalMonths)
        {
            StringBuilder schedule = new StringBuilder();

            // Determine how many payments to show based on loan term
            int paymentsToShow;
            int grouping; // Show every Nth payment

            if (totalMonths <= 12)
            {
                // Show all payments for loans 1 year or less
                paymentsToShow = totalMonths;
                grouping = 1;
            }
            else if (totalMonths <= 36)
            {
                // Show every payment for loans up to 3 years
                paymentsToShow = totalMonths;
                grouping = 1;
            }
            else if (totalMonths <= 60)
            {
                // Show every other payment for loans up to 5 years
                paymentsToShow = (totalMonths + 1) / 2;
                grouping = 2;
            }
            else if (totalMonths <= 120)
            {
                // Show quarterly (every 3 months) for loans up to 10 years
                paymentsToShow = (totalMonths + 2) / 3;
                grouping = 3;
            }
            else if (totalMonths <= 180)
            {
                // Show semi-annually (every 6 months) for loans up to 15 years
                paymentsToShow = (totalMonths + 5) / 6;
                grouping = 6;
            }
            else
            {
                // Show annually (every 12 months) for longer loans
                paymentsToShow = (totalMonths + 11) / 12;
                grouping = 12;
            }

            schedule.AppendLine("Payment | Principal  | Interest   | Balance");
            schedule.AppendLine("--------|------------|------------|------------");

            double balance = principal;
            double totalPrincipalPaid = 0;
            double totalInterestPaid = 0;

            for (int month = 1; month <= totalMonths; month++)
            {
                double interestPayment = balance * monthlyRate;
                double principalPayment = monthlyPayment - interestPayment;

                // Adjust last payment for any rounding
                if (month == totalMonths && balance < monthlyPayment)
                {
                    principalPayment = balance;
                    monthlyPayment = principalPayment + interestPayment;
                }

                balance -= principalPayment;
                totalPrincipalPaid += principalPayment;
                totalInterestPaid += interestPayment;

                // Ensure balance doesn't go negative due to rounding
                if (balance < 0.01)
                {
                    balance = 0;
                }

                // Show this payment if it matches our grouping or is the last payment
                if (month % grouping == 0 || month == totalMonths || month == 1)
                {
                    string paymentLabel = month == totalMonths ? $"{month} (Final)" : $"{month}";
                    schedule.AppendLine($"{paymentLabel,-7} | ${principalPayment,9:N2} | ${interestPayment,9:N2} | ${balance,10:N2}");
                }
            }

            schedule.AppendLine("--------|------------|------------|------------");
            schedule.AppendLine($"{"Total",-7} | ${totalPrincipalPaid,9:N2} | ${totalInterestPaid,9:N2} |");

            if (grouping > 1)
            {
                schedule.AppendLine();
                if (grouping == 2)
                    schedule.AppendLine("Note: Showing every other payment");
                else if (grouping == 3)
                    schedule.AppendLine("Note: Showing every 3rd payment (quarterly)");
                else if (grouping == 6)
                    schedule.AppendLine("Note: Showing every 6th payment (semi-annually)");
                else if (grouping == 12)
                    schedule.AppendLine("Note: Showing every 12th payment (annually)");
            }

            return schedule.ToString();
        }
    }
}
