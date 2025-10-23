using System;
using System.Text.RegularExpressions;
using static QuickInfo.NodeFactory;

namespace QuickInfo
{
    public class Discount : IProcessor
    {
        public object GetResult(Query query)
        {
            if (query.IsHelp)
            {
                return HelpTable(
                    ("discount", "Show discount calculation examples"),
                    ("100 with 25% off", "Calculate sale price"),
                    ("50 - 20%", "Calculate discounted price"),
                    ("discount 20% on 80", "Calculate sale price"),
                    ("save on 150 at 30% off", "Calculate savings"));
            }

            var input = query.OriginalInput.Trim();

            // General discount info
            if (string.Equals(input, "discount", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "sale", StringComparison.OrdinalIgnoreCase))
            {
                return new object[]
                {
                    Answer("Discount Calculator"),
                    SectionHeader("Examples:"),
                    NameValueTable(entries: new[]
                    {
                        ("With Percent Off:", "100 with 25% off"),
                        ("Minus Percent:", "50 - 20%"),
                        ("Discount On:", "discount 20% on 80"),
                        ("Calculate Savings:", "save on 150 at 30% off")
                    }),
                    SectionHeader("Common Discounts:"),
                    NameValueTable(entries: new[]
                    {
                        ("10% off:", "Save $10 on $100"),
                        ("25% off:", "Save $25 on $100"),
                        ("50% off:", "Half price"),
                        ("Buy 2 Get 1:", "~33% off (on 3 items)")
                    })
                };
            }

            // Pattern: "X with Y% off" or "X - Y%"
            var withOffPattern = @"^\$?(\d+(?:\.\d+)?)\s+(?:with|at|-)\s+(\d+(?:\.\d+)?)\s*%?\s*(?:off|discount)?$";
            var withOffMatch = Regex.Match(input, withOffPattern, RegexOptions.IgnoreCase);

            if (withOffMatch.Success)
            {
                if (!double.TryParse(withOffMatch.Groups[1].Value, out double originalPrice) ||
                    !double.TryParse(withOffMatch.Groups[2].Value, out double discountPercent))
                {
                    return null;
                }

                if (originalPrice < 0 || originalPrice > 1000000)
                {
                    return FixedParagraph("Please enter a valid price ($0.01 - $1,000,000)");
                }

                if (discountPercent < 0 || discountPercent > 100)
                {
                    return FixedParagraph("Please enter a valid discount (0% - 100%)");
                }

                double discountAmount = originalPrice * (discountPercent / 100.0);
                double salePrice = originalPrice - discountAmount;
                double finalPercent = 100 - discountPercent;

                return new object[]
                {
                    Answer($"${originalPrice:F2} - {discountPercent}% = ${salePrice:F2}"),
                    SectionHeader("Discount Calculation:"),
                    NameValueTable(entries: new[]
                    {
                        ("Original Price:", $"${originalPrice:F2}"),
                        ("Discount:", $"{discountPercent}% off"),
                        ("You Save:", $"${discountAmount:F2}"),
                        ("Sale Price:", $"${salePrice:F2}"),
                        ("Final Percentage:", $"{finalPercent}% of original")
                    }),
                    SectionHeader("Breakdown:"),
                    FixedParagraph($"Discount: ${originalPrice:F2} × {discountPercent / 100:F2} = ${discountAmount:F2}"),
                    FixedParagraph($"Sale Price: ${originalPrice:F2} - ${discountAmount:F2} = ${salePrice:F2}")
                };
            }

            // Pattern: "discount X% on Y" or "X% off Y"
            var discountOnPattern = @"^(?:discount\s+)?(\d+(?:\.\d+)?)\s*%?\s+(?:off|on|discount\s+on)\s+\$?(\d+(?:\.\d+)?)$";
            var discountOnMatch = Regex.Match(input, discountOnPattern, RegexOptions.IgnoreCase);

            if (discountOnMatch.Success)
            {
                if (!double.TryParse(discountOnMatch.Groups[1].Value, out double discountPercent) ||
                    !double.TryParse(discountOnMatch.Groups[2].Value, out double originalPrice))
                {
                    return null;
                }

                if (originalPrice < 0 || originalPrice > 1000000)
                {
                    return FixedParagraph("Please enter a valid price ($0.01 - $1,000,000)");
                }

                if (discountPercent < 0 || discountPercent > 100)
                {
                    return FixedParagraph("Please enter a valid discount (0% - 100%)");
                }

                double discountAmount = originalPrice * (discountPercent / 100.0);
                double salePrice = originalPrice - discountAmount;

                return new object[]
                {
                    Answer($"{discountPercent}% off ${originalPrice:F2} = ${salePrice:F2}"),
                    SectionHeader("Sale Price:"),
                    NameValueTable(entries: new[]
                    {
                        ("Original Price:", $"${originalPrice:F2}"),
                        ("Discount:", $"{discountPercent}% off"),
                        ("You Save:", $"${discountAmount:F2}"),
                        ("Sale Price:", $"${salePrice:F2}")
                    })
                };
            }

            // Pattern: "save on X at Y% off" or "savings X at Y%"
            var saveOnPattern = @"^(?:save\s+on|savings\s+on)\s+\$?(\d+(?:\.\d+)?)\s+(?:at|@)\s+(\d+(?:\.\d+)?)\s*%?\s*(?:off)?$";
            var saveOnMatch = Regex.Match(input, saveOnPattern, RegexOptions.IgnoreCase);

            if (saveOnMatch.Success)
            {
                if (!double.TryParse(saveOnMatch.Groups[1].Value, out double originalPrice) ||
                    !double.TryParse(saveOnMatch.Groups[2].Value, out double discountPercent))
                {
                    return null;
                }

                if (originalPrice < 0 || originalPrice > 1000000)
                {
                    return FixedParagraph("Please enter a valid price ($0.01 - $1,000,000)");
                }

                if (discountPercent < 0 || discountPercent > 100)
                {
                    return FixedParagraph("Please enter a valid discount (0% - 100%)");
                }

                double savings = originalPrice * (discountPercent / 100.0);
                double salePrice = originalPrice - savings;

                return new object[]
                {
                    Answer($"Save ${savings:F2} on ${originalPrice:F2} at {discountPercent}% off"),
                    SectionHeader("Savings Breakdown:"),
                    NameValueTable(entries: new[]
                    {
                        ("Original Price:", $"${originalPrice:F2}"),
                        ("Discount Rate:", $"{discountPercent}%"),
                        ("Your Savings:", $"${savings:F2}"),
                        ("Sale Price:", $"${salePrice:F2}"),
                        ("You Pay:", $"{100 - discountPercent}% of original")
                    })
                };
            }

            // Pattern: "original price X sale price Y" - calculate discount percent
            var calculateDiscountPattern = @"^(?:original|was)\s+\$?(\d+(?:\.\d+)?)\s+(?:sale|now|is)\s+\$?(\d+(?:\.\d+)?)$";
            var calculateDiscountMatch = Regex.Match(input, calculateDiscountPattern, RegexOptions.IgnoreCase);

            if (calculateDiscountMatch.Success)
            {
                if (!double.TryParse(calculateDiscountMatch.Groups[1].Value, out double originalPrice) ||
                    !double.TryParse(calculateDiscountMatch.Groups[2].Value, out double salePrice))
                {
                    return null;
                }

                if (originalPrice <= 0 || originalPrice > 1000000)
                {
                    return FixedParagraph("Please enter a valid original price ($0.01 - $1,000,000)");
                }

                if (salePrice < 0 || salePrice > originalPrice)
                {
                    return FixedParagraph("Sale price must be less than or equal to original price");
                }

                double savings = originalPrice - salePrice;
                double discountPercent = (savings / originalPrice) * 100.0;

                return new object[]
                {
                    Answer($"${originalPrice:F2} → ${salePrice:F2} = {discountPercent:F1}% off"),
                    SectionHeader("Discount Calculation:"),
                    NameValueTable(entries: new[]
                    {
                        ("Original Price:", $"${originalPrice:F2}"),
                        ("Sale Price:", $"${salePrice:F2}"),
                        ("Savings:", $"${savings:F2}"),
                        ("Discount Percent:", $"{discountPercent:F1}% off"),
                        ("Sale Ratio:", $"{(salePrice / originalPrice) * 100:F1}% of original")
                    }),
                    SectionHeader("Formula:"),
                    FixedParagraph($"Discount % = (${savings:F2} / ${originalPrice:F2}) × 100 = {discountPercent:F1}%")
                };
            }

            // Pattern: "multiple discounts" - e.g., "100 - 20% - 10%"
            var multipleDiscountsPattern = @"^\$?(\d+(?:\.\d+)?)\s+-\s+(\d+(?:\.\d+)?)\s*%\s+-\s+(\d+(?:\.\d+)?)\s*%$";
            var multipleDiscountsMatch = Regex.Match(input, multipleDiscountsPattern, RegexOptions.IgnoreCase);

            if (multipleDiscountsMatch.Success)
            {
                if (!double.TryParse(multipleDiscountsMatch.Groups[1].Value, out double originalPrice) ||
                    !double.TryParse(multipleDiscountsMatch.Groups[2].Value, out double discount1) ||
                    !double.TryParse(multipleDiscountsMatch.Groups[3].Value, out double discount2))
                {
                    return null;
                }

                // Apply first discount
                double priceAfterFirst = originalPrice * (1 - discount1 / 100.0);
                double savings1 = originalPrice - priceAfterFirst;

                // Apply second discount to already-discounted price
                double finalPrice = priceAfterFirst * (1 - discount2 / 100.0);
                double savings2 = priceAfterFirst - finalPrice;

                double totalSavings = originalPrice - finalPrice;
                double effectiveDiscount = (totalSavings / originalPrice) * 100.0;

                return new object[]
                {
                    Answer($"${originalPrice:F2} - {discount1}% - {discount2}% = ${finalPrice:F2}"),
                    SectionHeader("Multiple Discount Calculation:"),
                    NameValueTable(entries: new[]
                    {
                        ("Original Price:", $"${originalPrice:F2}"),
                        ("After 1st Discount ({discount1}%):", $"${priceAfterFirst:F2} (saved ${savings1:F2})"),
                        ("After 2nd Discount ({discount2}%):", $"${finalPrice:F2} (saved ${savings2:F2})"),
                        ("Total Savings:", $"${totalSavings:F2}"),
                        ("Effective Discount:", $"{effectiveDiscount:F2}%")
                    }),
                    SectionHeader("Note:"),
                    FixedParagraph($"Stacked discounts: {discount1}% + {discount2}% ≠ {discount1 + discount2}%"),
                    FixedParagraph($"Actual combined discount: {effectiveDiscount:F2}%")
                };
            }

            return null;
        }
    }
}
