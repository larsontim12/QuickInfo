using System;
using System.Text.RegularExpressions;
using static QuickInfo.NodeFactory;

namespace QuickInfo
{
    public class ScientificCalculator : IProcessor
    {
        private const double PI = System.Math.PI;
        private const double E = System.Math.E;

        public object GetResult(Query query)
        {
            if (query.IsHelp)
            {
                return HelpTable(
                    ("calculator", "Show scientific calculator functions"),
                    ("sqrt(144)", "Square root"),
                    ("sin(45)", "Sine (in degrees)"),
                    ("log(100)", "Logarithm base 10"),
                    ("ln(2.718)", "Natural logarithm (base e)"),
                    ("2^10", "Power/exponent"),
                    ("abs(-5)", "Absolute value"),
                    ("factorial 10", "Factorial (10!)"));
            }

            var input = query.OriginalInput.Trim();

            // General calculator info
            if (string.Equals(input, "calculator", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "scientific calculator", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "calc", StringComparison.OrdinalIgnoreCase))
            {
                return new object[]
                {
                    Answer("Scientific Calculator Functions"),
                    SectionHeader("Supported Operations:"),
                    NameValueTable(entries: new[]
                    {
                        ("Square Root:", "sqrt(x)"),
                        ("Power:", "x^y or pow(x,y)"),
                        ("Absolute Value:", "abs(x)"),
                        ("Factorial:", "factorial x or x!"),
                        ("Ceiling/Floor:", "ceil(x), floor(x), round(x)")
                    }),
                    SectionHeader("Trigonometry (degrees):"),
                    NameValueTable(entries: new[]
                    {
                        ("Sine:", "sin(x)"),
                        ("Cosine:", "cos(x)"),
                        ("Tangent:", "tan(x)"),
                        ("Arc Sine:", "asin(x)"),
                        ("Arc Cosine:", "acos(x)"),
                        ("Arc Tangent:", "atan(x)")
                    }),
                    SectionHeader("Logarithms & Exponentials:"),
                    NameValueTable(entries: new[]
                    {
                        ("Natural Log:", "ln(x)"),
                        ("Log Base 10:", "log(x)"),
                        ("Exponential:", "exp(x) = e^x"),
                        ("Constants:", "pi = 3.14159..., e = 2.71828...")
                    })
                };
            }

            // Try to parse and evaluate mathematical expression
            try
            {
                var result = EvaluateExpression(input);
                if (result != null)
                {
                    return result;
                }
            }
            catch
            {
                // If parsing fails, return null to let other processors handle it
                return null;
            }

            return null;
        }

        private object EvaluateExpression(string input)
        {
            input = input.Trim().ToLower();

            // Replace constants
            input = input.Replace("pi", PI.ToString());
            input = input.Replace("π", PI.ToString());

            // Square root: sqrt(X)
            var sqrtMatch = Regex.Match(input, @"^sqrt\s*\(?\s*(\d+(?:\.\d+)?)\s*\)?$");
            if (sqrtMatch.Success)
            {
                double value = double.Parse(sqrtMatch.Groups[1].Value);
                if (value < 0)
                {
                    return FixedParagraph("Cannot calculate square root of negative number");
                }
                double result = System.Math.Sqrt(value);
                return CreateMathResult($"√{value}", result, "Square Root");
            }

            // Power: X^Y or pow(X,Y)
            var powerMatch1 = Regex.Match(input, @"^(\d+(?:\.\d+)?)\s*\^\s*(\d+(?:\.\d+)?)$");
            var powerMatch2 = Regex.Match(input, @"^pow\s*\(\s*(\d+(?:\.\d+)?)\s*,\s*(\d+(?:\.\d+)?)\s*\)$");
            var powerMatch = powerMatch1.Success ? powerMatch1 : powerMatch2;

            if (powerMatch.Success)
            {
                double baseNum = double.Parse(powerMatch.Groups[1].Value);
                double exponent = double.Parse(powerMatch.Groups[2].Value);
                double result = System.Math.Pow(baseNum, exponent);

                return new object[]
                {
                    Answer($"{baseNum}^{exponent} = {result:F6}"),
                    SectionHeader("Power Calculation:"),
                    NameValueTable(entries: new[]
                    {
                        ("Base:", $"{baseNum}"),
                        ("Exponent:", $"{exponent}"),
                        ("Result:", $"{result:F6}"),
                        ("Scientific:", $"{result:E6}")
                    })
                };
            }

            // Absolute value: abs(X)
            var absMatch = Regex.Match(input, @"^abs\s*\(?\s*(-?\d+(?:\.\d+)?)\s*\)?$");
            if (absMatch.Success)
            {
                double value = double.Parse(absMatch.Groups[1].Value);
                double result = System.Math.Abs(value);
                return CreateMathResult($"abs({value})", result, "Absolute Value");
            }

            // Factorial: factorial X or X!
            var factorialMatch1 = Regex.Match(input, @"^factorial\s+(\d+)$");
            var factorialMatch2 = Regex.Match(input, @"^(\d+)\s*!$");
            var factorialMatch = factorialMatch1.Success ? factorialMatch1 : factorialMatch2;

            if (factorialMatch.Success)
            {
                int n = int.Parse(factorialMatch.Groups[1].Value);
                if (n < 0)
                {
                    return FixedParagraph("Factorial is not defined for negative numbers");
                }
                if (n > 170)
                {
                    return FixedParagraph("Factorial too large (maximum 170!)");
                }

                double result = Factorial(n);
                return new object[]
                {
                    Answer($"{n}! = {result:E6}"),
                    SectionHeader("Factorial Calculation:"),
                    NameValueTable(entries: new[]
                    {
                        ("Expression:", $"{n}!"),
                        ("Result:", $"{result:E6}"),
                        ("First few:", n <= 10 ? $"1×2×...×{n}" : $"1×2×3×...×{n}")
                    })
                };
            }

            // Ceiling: ceil(X)
            var ceilMatch = Regex.Match(input, @"^ceil(?:ing)?\s*\(?\s*(-?\d+(?:\.\d+)?)\s*\)?$");
            if (ceilMatch.Success)
            {
                double value = double.Parse(ceilMatch.Groups[1].Value);
                double result = System.Math.Ceiling(value);
                return CreateMathResult($"ceil({value})", result, "Ceiling (Round Up)");
            }

            // Floor: floor(X)
            var floorMatch = Regex.Match(input, @"^floor\s*\(?\s*(-?\d+(?:\.\d+)?)\s*\)?$");
            if (floorMatch.Success)
            {
                double value = double.Parse(floorMatch.Groups[1].Value);
                double result = System.Math.Floor(value);
                return CreateMathResult($"floor({value})", result, "Floor (Round Down)");
            }

            // Round: round(X) or round(X,Y)
            var roundMatch = Regex.Match(input, @"^round\s*\(\s*(-?\d+(?:\.\d+)?)(?:\s*,\s*(\d+))?\s*\)$");
            if (roundMatch.Success)
            {
                double value = double.Parse(roundMatch.Groups[1].Value);
                int decimals = roundMatch.Groups[2].Success ? int.Parse(roundMatch.Groups[2].Value) : 0;
                double result = System.Math.Round(value, decimals);
                return CreateMathResult($"round({value}, {decimals})", result, "Round");
            }

            // Trigonometric functions (in degrees)
            // Sin
            var sinMatch = Regex.Match(input, @"^sin\s*\(?\s*(-?\d+(?:\.\d+)?)\s*\)?$");
            if (sinMatch.Success)
            {
                double degrees = double.Parse(sinMatch.Groups[1].Value);
                double radians = degrees * (PI / 180.0);
                double result = System.Math.Sin(radians);
                return CreateTrigResult($"sin({degrees}°)", result, degrees, "Sine");
            }

            // Cos
            var cosMatch = Regex.Match(input, @"^cos\s*\(?\s*(-?\d+(?:\.\d+)?)\s*\)?$");
            if (cosMatch.Success)
            {
                double degrees = double.Parse(cosMatch.Groups[1].Value);
                double radians = degrees * (PI / 180.0);
                double result = System.Math.Cos(radians);
                return CreateTrigResult($"cos({degrees}°)", result, degrees, "Cosine");
            }

            // Tan
            var tanMatch = Regex.Match(input, @"^tan\s*\(?\s*(-?\d+(?:\.\d+)?)\s*\)?$");
            if (tanMatch.Success)
            {
                double degrees = double.Parse(tanMatch.Groups[1].Value);
                double radians = degrees * (PI / 180.0);
                double result = System.Math.Tan(radians);
                return CreateTrigResult($"tan({degrees}°)", result, degrees, "Tangent");
            }

            // Arc Sin (inverse sine)
            var asinMatch = Regex.Match(input, @"^a?sin(?:inv|-1)?\s*\(?\s*(-?\d+(?:\.\d+)?)\s*\)?$");
            if (input.StartsWith("asin") && asinMatch.Success)
            {
                double value = double.Parse(asinMatch.Groups[1].Value);
                if (value < -1 || value > 1)
                {
                    return FixedParagraph("Arc sine domain error: value must be between -1 and 1");
                }
                double radians = System.Math.Asin(value);
                double degrees = radians * (180.0 / PI);
                return CreateInverseTrigResult($"asin({value})", degrees, value, "Arc Sine");
            }

            // Arc Cos (inverse cosine)
            var acosMatch = Regex.Match(input, @"^a?cos(?:inv|-1)?\s*\(?\s*(-?\d+(?:\.\d+)?)\s*\)?$");
            if (input.StartsWith("acos") && acosMatch.Success)
            {
                double value = double.Parse(acosMatch.Groups[1].Value);
                if (value < -1 || value > 1)
                {
                    return FixedParagraph("Arc cosine domain error: value must be between -1 and 1");
                }
                double radians = System.Math.Acos(value);
                double degrees = radians * (180.0 / PI);
                return CreateInverseTrigResult($"acos({value})", degrees, value, "Arc Cosine");
            }

            // Arc Tan (inverse tangent)
            var atanMatch = Regex.Match(input, @"^a?tan(?:inv|-1)?\s*\(?\s*(-?\d+(?:\.\d+)?)\s*\)?$");
            if (input.StartsWith("atan") && atanMatch.Success)
            {
                double value = double.Parse(atanMatch.Groups[1].Value);
                double radians = System.Math.Atan(value);
                double degrees = radians * (180.0 / PI);
                return CreateInverseTrigResult($"atan({value})", degrees, value, "Arc Tangent");
            }

            // Natural log: ln(X)
            var lnMatch = Regex.Match(input, @"^ln\s*\(?\s*(\d+(?:\.\d+)?)\s*\)?$");
            if (lnMatch.Success)
            {
                double value = double.Parse(lnMatch.Groups[1].Value);
                if (value <= 0)
                {
                    return FixedParagraph("Logarithm domain error: value must be positive");
                }
                double result = System.Math.Log(value);
                return new object[]
                {
                    Answer($"ln({value}) = {result:F6}"),
                    SectionHeader("Natural Logarithm (base e):"),
                    NameValueTable(entries: new[]
                    {
                        ("Expression:", $"ln({value})"),
                        ("Result:", $"{result:F6}"),
                        ("Verification:", $"e^{result:F6} = {System.Math.Exp(result):F6}"),
                        ("Base:", $"e = {E:F6}")
                    })
                };
            }

            // Log base 10: log(X)
            var logMatch = Regex.Match(input, @"^log\s*\(?\s*(\d+(?:\.\d+)?)\s*\)?$");
            if (logMatch.Success)
            {
                double value = double.Parse(logMatch.Groups[1].Value);
                if (value <= 0)
                {
                    return FixedParagraph("Logarithm domain error: value must be positive");
                }
                double result = System.Math.Log10(value);
                return new object[]
                {
                    Answer($"log₁₀({value}) = {result:F6}"),
                    SectionHeader("Logarithm Base 10:"),
                    NameValueTable(entries: new[]
                    {
                        ("Expression:", $"log₁₀({value})"),
                        ("Result:", $"{result:F6}"),
                        ("Verification:", $"10^{result:F6} = {System.Math.Pow(10, result):F6}")
                    })
                };
            }

            // Exponential: exp(X) = e^X
            var expMatch = Regex.Match(input, @"^exp\s*\(?\s*(-?\d+(?:\.\d+)?)\s*\)?$");
            if (expMatch.Success)
            {
                double value = double.Parse(expMatch.Groups[1].Value);
                double result = System.Math.Exp(value);
                return new object[]
                {
                    Answer($"e^{value} = {result:F6}"),
                    SectionHeader("Exponential Function:"),
                    NameValueTable(entries: new[]
                    {
                        ("Expression:", $"e^{value}"),
                        ("Result:", $"{result:F6}"),
                        ("Scientific:", $"{result:E6}"),
                        ("Base:", $"e = {E:F6}")
                    })
                };
            }

            return null;
        }

        private object CreateMathResult(string expression, double result, string operation)
        {
            return new object[]
            {
                Answer($"{expression} = {result:F6}"),
                SectionHeader($"{operation}:"),
                NameValueTable(entries: new[]
                {
                    ("Expression:", expression),
                    ("Result:", $"{result:F6}"),
                    ("Scientific:", $"{result:E6}")
                })
            };
        }

        private object CreateTrigResult(string expression, double result, double degrees, string operation)
        {
            double radians = degrees * (PI / 180.0);
            return new object[]
            {
                Answer($"{expression} = {result:F6}"),
                SectionHeader($"{operation}:"),
                NameValueTable(entries: new[]
                {
                    ("Expression:", expression),
                    ("Degrees:", $"{degrees}°"),
                    ("Radians:", $"{radians:F6} rad"),
                    ("Result:", $"{result:F6}")
                })
            };
        }

        private object CreateInverseTrigResult(string expression, double degrees, double value, string operation)
        {
            double radians = degrees * (PI / 180.0);
            return new object[]
            {
                Answer($"{expression} = {degrees:F6}°"),
                SectionHeader($"{operation}:"),
                NameValueTable(entries: new[]
                {
                    ("Expression:", expression),
                    ("Input Value:", $"{value:F6}"),
                    ("Result (degrees):", $"{degrees:F6}°"),
                    ("Result (radians):", $"{radians:F6} rad")
                })
            };
        }

        private double Factorial(int n)
        {
            if (n == 0 || n == 1)
                return 1;

            double result = 1;
            for (int i = 2; i <= n; i++)
            {
                result *= i;
            }
            return result;
        }
    }
}
