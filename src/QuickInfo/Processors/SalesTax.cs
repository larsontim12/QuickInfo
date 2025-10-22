using System;
using System.Text.RegularExpressions;
using static QuickInfo.NodeFactory;

namespace QuickInfo
{
    public class SalesTax : IProcessor
    {
        public object GetResult(Query query)
        {
            if (query.IsHelp)
            {
                return HelpTable(
                    ("sales tax", "Show sales tax calculation examples"),
                    ("100 with 8.5% tax", "Calculate total with tax"),
                    ("50 at 6% tax", "Calculate price with tax"),
                    ("tax on 75 at 7.5%", "Calculate tax amount"),
                    ("85.50 before tax at 6%", "Calculate final price"));
            }

            var input = query.OriginalInput.Trim();

            // General sales tax info
            if (string.Equals(input, "sales tax", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "tax", StringComparison.OrdinalIgnoreCase))
            {
                return new object[]
                {
                    Answer("Sales Tax Calculator"),
                    SectionHeader("Examples:"),
                    NameValueTable(entries: new[]
                    {
                        ("Price with Tax:", "100 with 8.5% tax"),
                        ("Calculate Tax:", "tax on 50 at 6%"),
                        ("At Tax Rate:", "75 at 7.5% tax"),
                        ("Before Tax:", "85.50 before tax at 6%")
                    }),
                    SectionHeader("Common US Tax Rates:"),
                    NameValueTable(entries: new[]
                    {
                        ("California:", "7.25% - 10.75%"),
                        ("Texas:", "6.25% - 8.25%"),
                        ("New York:", "4% - 8.875%"),
                        ("Florida:", "6% - 7.5%"),
                        ("No Sales Tax:", "AK, DE, MT, NH, OR")
                    })
                };
            }

            // Pattern: "X with/at Y% tax" or "X with/at Y% sales tax"
            var withTaxPattern = @"^\$?(\d+(?:\.\d+)?)\s+(?:with|at|@)\s+(\d+(?:\.\d+)?)\s*%?\s*(?:sales\s+)?tax$";
            var withTaxMatch = Regex.Match(input, withTaxPattern, RegexOptions.IgnoreCase);

            if (withTaxMatch.Success)
            {
                if (!double.TryParse(withTaxMatch.Groups[1].Value, out double price) ||
                    !double.TryParse(withTaxMatch.Groups[2].Value, out double taxRate))
                {
                    return null;
                }

                if (price < 0 || price > 1000000)
                {
                    return FixedParagraph("Please enter a valid price ($0.01 - $1,000,000)");
                }

                if (taxRate < 0 || taxRate > 20)
                {
                    return FixedParagraph("Please enter a valid tax rate (0% - 20%)");
                }

                double taxAmount = price * (taxRate / 100.0);
                double totalPrice = price + taxAmount;

                return new object[]
                {
                    Answer($"${price:F2} + {taxRate}% tax = ${totalPrice:F2}"),
                    SectionHeader("Sales Tax Calculation:"),
                    NameValueTable(entries: new[]
                    {
                        ("Subtotal (Before Tax):", $"${price:F2}"),
                        ("Tax Rate:", $"{taxRate}%"),
                        ("Tax Amount:", $"${taxAmount:F2}"),
                        ("Total (After Tax):", $"${totalPrice:F2}")
                    }),
                    SectionHeader("Breakdown:"),
                    FixedParagraph($"${price:F2} × {taxRate / 100:F4} = ${taxAmount:F2} tax"),
                    FixedParagraph($"${price:F2} + ${taxAmount:F2} = ${totalPrice:F2} total")
                };
            }

            // Pattern: "tax on X at Y%" or "calculate tax X at Y%"
            var taxOnPattern = @"^(?:tax\s+on|calculate\s+tax)\s+\$?(\d+(?:\.\d+)?)\s+(?:at|@)\s+(\d+(?:\.\d+)?)\s*%?$";
            var taxOnMatch = Regex.Match(input, taxOnPattern, RegexOptions.IgnoreCase);

            if (taxOnMatch.Success)
            {
                if (!double.TryParse(taxOnMatch.Groups[1].Value, out double price) ||
                    !double.TryParse(taxOnMatch.Groups[2].Value, out double taxRate))
                {
                    return null;
                }

                if (price < 0 || price > 1000000)
                {
                    return FixedParagraph("Please enter a valid price ($0.01 - $1,000,000)");
                }

                if (taxRate < 0 || taxRate > 20)
                {
                    return FixedParagraph("Please enter a valid tax rate (0% - 20%)");
                }

                double taxAmount = price * (taxRate / 100.0);
                double totalPrice = price + taxAmount;

                return new object[]
                {
                    Answer($"Tax on ${price:F2} at {taxRate}% = ${taxAmount:F2}"),
                    SectionHeader("Tax Calculation:"),
                    NameValueTable(entries: new[]
                    {
                        ("Subtotal:", $"${price:F2}"),
                        ("Tax Rate:", $"{taxRate}%"),
                        ("Tax Amount:", $"${taxAmount:F2}"),
                        ("Total Price:", $"${totalPrice:F2}")
                    }),
                    SectionHeader("Formula:"),
                    FixedParagraph($"Tax = ${price:F2} × {taxRate}% = ${taxAmount:F2}")
                };
            }

            // Pattern: "X before tax at Y%" - calculate pre-tax price from total
            var beforeTaxPattern = @"^\$?(\d+(?:\.\d+)?)\s+(?:before\s+tax|pretax|pre-tax)\s+(?:at|@)\s+(\d+(?:\.\d+)?)\s*%?$";
            var beforeTaxMatch = Regex.Match(input, beforeTaxPattern, RegexOptions.IgnoreCase);

            if (beforeTaxMatch.Success)
            {
                if (!double.TryParse(beforeTaxMatch.Groups[1].Value, out double price) ||
                    !double.TryParse(beforeTaxMatch.Groups[2].Value, out double taxRate))
                {
                    return null;
                }

                if (price < 0 || price > 1000000)
                {
                    return FixedParagraph("Please enter a valid price ($0.01 - $1,000,000)");
                }

                if (taxRate < 0 || taxRate > 20)
                {
                    return FixedParagraph("Please enter a valid tax rate (0% - 20%)");
                }

                double taxAmount = price * (taxRate / 100.0);
                double totalPrice = price + taxAmount;

                return new object[]
                {
                    Answer($"${price:F2} before tax → ${totalPrice:F2} after {taxRate}% tax"),
                    SectionHeader("Price with Tax:"),
                    NameValueTable(entries: new[]
                    {
                        ("Price Before Tax:", $"${price:F2}"),
                        ("Tax Rate:", $"{taxRate}%"),
                        ("Tax Amount:", $"${taxAmount:F2}"),
                        ("Price After Tax:", $"${totalPrice:F2}")
                    })
                };
            }

            // Pattern: "remove tax from X at Y%" - calculate original price from total with tax
            var removeTaxPattern = @"^(?:remove\s+tax\s+from|reverse\s+tax)\s+\$?(\d+(?:\.\d+)?)\s+(?:at|@)\s+(\d+(?:\.\d+)?)\s*%?$";
            var removeTaxMatch = Regex.Match(input, removeTaxPattern, RegexOptions.IgnoreCase);

            if (removeTaxMatch.Success)
            {
                if (!double.TryParse(removeTaxMatch.Groups[1].Value, out double totalPrice) ||
                    !double.TryParse(removeTaxMatch.Groups[2].Value, out double taxRate))
                {
                    return null;
                }

                if (totalPrice < 0 || totalPrice > 1000000)
                {
                    return FixedParagraph("Please enter a valid total price ($0.01 - $1,000,000)");
                }

                if (taxRate < 0 || taxRate > 20)
                {
                    return FixedParagraph("Please enter a valid tax rate (0% - 20%)");
                }

                // Calculate original price: total / (1 + tax_rate)
                double originalPrice = totalPrice / (1 + (taxRate / 100.0));
                double taxAmount = totalPrice - originalPrice;

                return new object[]
                {
                    Answer($"${totalPrice:F2} with tax → ${originalPrice:F2} before tax"),
                    SectionHeader("Reverse Tax Calculation:"),
                    NameValueTable(entries: new[]
                    {
                        ("Total (with Tax):", $"${totalPrice:F2}"),
                        ("Tax Rate:", $"{taxRate}%"),
                        ("Original Price:", $"${originalPrice:F2}"),
                        ("Tax Amount:", $"${taxAmount:F2}")
                    }),
                    SectionHeader("Formula:"),
                    FixedParagraph($"Original = ${totalPrice:F2} / (1 + {taxRate / 100:F4}) = ${originalPrice:F2}")
                };
            }

            return null;
        }
    }
}
