using System;
using System.Text.RegularExpressions;
using static QuickInfo.NodeFactory;

namespace QuickInfo
{
    public class FuelEfficiency : IProcessor
    {
        public object GetResult(Query query)
        {
            if (query.IsHelp)
            {
                return HelpTable(
                    ("fuel efficiency", "Show fuel efficiency conversion formulas"),
                    ("25 mpg", "Convert MPG to L/100km"),
                    ("8.5 l/100km", "Convert L/100km to MPG"),
                    ("300 miles at 25 mpg", "Calculate fuel needed for trip"),
                    ("15 gallons at $3.50", "Calculate fuel cost"));
            }

            var input = query.OriginalInput.Trim();

            // General fuel efficiency info
            if (string.Equals(input, "fuel efficiency", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "mpg", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "fuel economy", StringComparison.OrdinalIgnoreCase))
            {
                return new object[]
                {
                    Answer("Fuel Efficiency Conversions"),
                    SectionHeader("Conversion Formula:"),
                    FixedParagraph("L/100km = 235.215 / MPG"),
                    FixedParagraph("MPG = 235.215 / (L/100km)"),
                    SectionHeader("Common Values:"),
                    NameValueTable(entries: new[]
                    {
                        ("20 MPG:", "11.8 L/100km (SUV/Truck)"),
                        ("25 MPG:", "9.4 L/100km (Average Car)"),
                        ("30 MPG:", "7.8 L/100km (Efficient Car)"),
                        ("40 MPG:", "5.9 L/100km (Hybrid)"),
                        ("50 MPG:", "4.7 L/100km (High-Efficiency)")
                    }),
                    FixedParagraph("Note: MPG = Miles per Gallon (US), L/100km = Liters per 100 kilometers")
                };
            }

            // Parse MPG pattern: "25 mpg", "30.5 miles per gallon"
            var mpgPattern = @"^(\d+(?:\.\d+)?)\s*(?:mpg|miles?\s+per\s+gallons?)\b";
            var mpgMatch = Regex.Match(input, mpgPattern, RegexOptions.IgnoreCase);

            if (mpgMatch.Success)
            {
                if (!double.TryParse(mpgMatch.Groups[1].Value, out double mpg))
                {
                    return null;
                }

                if (mpg <= 0 || mpg > 300)
                {
                    return FixedParagraph("Please enter a valid MPG value (0.1 - 300)");
                }

                double lper100km = 235.215 / mpg;
                double kmPerLiter = 100.0 / lper100km;
                double milesPerLiter = mpg / 3.785411784; // 1 US gallon = 3.785411784 liters

                string classification = ClassifyFuelEfficiency(mpg);

                return new object[]
                {
                    Answer($"{mpg:F1} MPG = {lper100km:F2} L/100km"),
                    SectionHeader("Fuel Efficiency Conversions:"),
                    NameValueTable(entries: new[]
                    {
                        ("Miles per Gallon (US):", $"{mpg:F1} MPG"),
                        ("Liters per 100km:", $"{lper100km:F2} L/100km"),
                        ("Kilometers per Liter:", $"{kmPerLiter:F2} km/L"),
                        ("Miles per Liter:", $"{milesPerLiter:F2} mi/L"),
                        ("Classification:", classification)
                    }),
                    SectionHeader("Example Trip:"),
                    FixedParagraph($"For a 300-mile trip: {300.0 / mpg:F2} gallons needed")
                };
            }

            // Parse L/100km pattern: "8.5 l/100km", "7.5 liters per 100 kilometers"
            var lper100kmPattern = @"^(\d+(?:\.\d+)?)\s*(?:l\/100km|liters?\s+per\s+100\s*(?:km|kilometers?))\b";
            var lper100kmMatch = Regex.Match(input, lper100kmPattern, RegexOptions.IgnoreCase);

            if (lper100kmMatch.Success)
            {
                if (!double.TryParse(lper100kmMatch.Groups[1].Value, out double lper100km))
                {
                    return null;
                }

                if (lper100km <= 0 || lper100km > 100)
                {
                    return FixedParagraph("Please enter a valid L/100km value (0.1 - 100)");
                }

                double mpg = 235.215 / lper100km;
                double kmPerLiter = 100.0 / lper100km;
                double milesPerLiter = mpg / 3.785411784;

                string classification = ClassifyFuelEfficiency(mpg);

                return new object[]
                {
                    Answer($"{lper100km:F2} L/100km = {mpg:F1} MPG"),
                    SectionHeader("Fuel Efficiency Conversions:"),
                    NameValueTable(entries: new[]
                    {
                        ("Liters per 100km:", $"{lper100km:F2} L/100km"),
                        ("Miles per Gallon (US):", $"{mpg:F1} MPG"),
                        ("Kilometers per Liter:", $"{kmPerLiter:F2} km/L"),
                        ("Miles per Liter:", $"{milesPerLiter:F2} mi/L"),
                        ("Classification:", classification)
                    }),
                    SectionHeader("Example Trip:"),
                    FixedParagraph($"For 500 km: {lper100km * 5:F2} liters needed")
                };
            }

            // Parse trip calculation: "300 miles at 25 mpg"
            var tripMilesPattern = @"^(\d+(?:\.\d+)?)\s*(?:miles?|mi)\s+(?:at|@)\s+(\d+(?:\.\d+)?)\s*mpg";
            var tripMilesMatch = Regex.Match(input, tripMilesPattern, RegexOptions.IgnoreCase);

            if (tripMilesMatch.Success)
            {
                if (!double.TryParse(tripMilesMatch.Groups[1].Value, out double miles) ||
                    !double.TryParse(tripMilesMatch.Groups[2].Value, out double mpg))
                {
                    return null;
                }

                if (miles <= 0 || miles > 10000)
                {
                    return FixedParagraph("Please enter a valid distance (0.1 - 10,000 miles)");
                }

                if (mpg <= 0 || mpg > 300)
                {
                    return FixedParagraph("Please enter a valid MPG value (0.1 - 300)");
                }

                double gallonsNeeded = miles / mpg;
                double kilometers = miles * 1.60934;
                double liters = gallonsNeeded * 3.785411784;

                return new object[]
                {
                    Answer($"{miles:N1} miles at {mpg:F1} MPG = {gallonsNeeded:F2} gallons"),
                    SectionHeader("Trip Fuel Calculation:"),
                    NameValueTable(entries: new[]
                    {
                        ("Distance:", $"{miles:N1} miles ({kilometers:N1} km)"),
                        ("Fuel Efficiency:", $"{mpg:F1} MPG"),
                        ("Fuel Needed:", $"{gallonsNeeded:F2} gallons ({liters:F2} liters)"),
                        ("Cost at $3.00/gal:", $"${gallonsNeeded * 3.00:F2}"),
                        ("Cost at $3.50/gal:", $"${gallonsNeeded * 3.50:F2}"),
                        ("Cost at $4.00/gal:", $"${gallonsNeeded * 4.00:F2}")
                    })
                };
            }

            // Parse fuel cost calculation: "15 gallons at $3.50", "15 gal at 3.50"
            var fuelCostPattern = @"^(\d+(?:\.\d+)?)\s*(?:gallons?|gal)\s+(?:at|@)\s+\$?(\d+(?:\.\d+)?)";
            var fuelCostMatch = Regex.Match(input, fuelCostPattern, RegexOptions.IgnoreCase);

            if (fuelCostMatch.Success)
            {
                if (!double.TryParse(fuelCostMatch.Groups[1].Value, out double gallons) ||
                    !double.TryParse(fuelCostMatch.Groups[2].Value, out double pricePerGallon))
                {
                    return null;
                }

                if (gallons <= 0 || gallons > 1000)
                {
                    return FixedParagraph("Please enter a valid amount (0.1 - 1,000 gallons)");
                }

                if (pricePerGallon <= 0 || pricePerGallon > 20)
                {
                    return FixedParagraph("Please enter a valid price per gallon ($0.01 - $20.00)");
                }

                double totalCost = gallons * pricePerGallon;
                double liters = gallons * 3.785411784;
                double pricePerLiter = pricePerGallon / 3.785411784;

                // Calculate range at different MPG values
                double range20mpg = gallons * 20;
                double range25mpg = gallons * 25;
                double range30mpg = gallons * 30;

                return new object[]
                {
                    Answer($"{gallons:F2} gallons @ ${pricePerGallon:F2}/gal = ${totalCost:F2}"),
                    SectionHeader("Fuel Cost Calculation:"),
                    NameValueTable(entries: new[]
                    {
                        ("Fuel Amount:", $"{gallons:F2} gallons ({liters:F2} liters)"),
                        ("Price per Gallon:", $"${pricePerGallon:F2}/gal (${pricePerLiter:F2}/L)"),
                        ("Total Cost:", $"${totalCost:F2}")
                    }),
                    SectionHeader("Driving Range:"),
                    NameValueTable(entries: new[]
                    {
                        ("At 20 MPG:", $"{range20mpg:N1} miles"),
                        ("At 25 MPG:", $"{range25mpg:N1} miles"),
                        ("At 30 MPG:", $"{range30mpg:N1} miles")
                    })
                };
            }

            return null;
        }

        private string ClassifyFuelEfficiency(double mpg)
        {
            if (mpg < 15)
                return "Poor (Heavy Truck/SUV)";
            else if (mpg < 20)
                return "Below Average (Large Vehicle)";
            else if (mpg < 25)
                return "Average (Mid-Size Car)";
            else if (mpg < 30)
                return "Good (Compact Car)";
            else if (mpg < 40)
                return "Very Good (Efficient Car)";
            else if (mpg < 50)
                return "Excellent (Hybrid)";
            else
                return "Outstanding (High-Efficiency Hybrid)";
        }
    }
}
