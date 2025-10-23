using System;
using System.Globalization;
using System.Text.RegularExpressions;
using static QuickInfo.NodeFactory;

namespace QuickInfo
{
    public class Duration : IProcessor
    {
        public object GetResult(Query query)
        {
            if (query.IsHelp)
            {
                return HelpTable(
                    ("duration", "Show duration calculation examples"),
                    ("2024-01-01 to 2024-12-31", "Calculate time between dates"),
                    ("from 2024-01-15 to 2024-03-20", "Calculate time between dates"),
                    ("2024-06-15 plus 30 days", "Add time to a date"),
                    ("2024-06-15 minus 2 weeks", "Subtract time from a date"),
                    ("today plus 90 days", "Add time to today's date"));
            }

            var input = query.OriginalInput.Trim();

            // General duration info
            if (string.Equals(input, "duration", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "date calculation", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "time calculation", StringComparison.OrdinalIgnoreCase))
            {
                return new object[]
                {
                    Answer("Duration and Date Calculations"),
                    SectionHeader("Supported Operations:"),
                    NameValueTable(entries: new[]
                    {
                        ("Time Between Dates:", "2024-01-01 to 2024-12-31"),
                        ("Add Time:", "2024-06-15 plus 30 days"),
                        ("Subtract Time:", "2024-06-15 minus 2 weeks"),
                        ("Use 'today':", "today plus 90 days"),
                        ("Use 'now':", "now plus 3 hours")
                    }),
                    SectionHeader("Supported Time Units:"),
                    FixedParagraph("seconds, minutes, hours, days, weeks, months, years"),
                    SectionHeader("Date Formats:"),
                    FixedParagraph("YYYY-MM-DD, MM/DD/YYYY, DD-MM-YYYY, or 'today'/'now'")
                };
            }

            // Parse "date1 to date2" or "from date1 to date2"
            var betweenPattern = @"^(?:from\s+)?(.+?)\s+to\s+(.+?)$";
            var betweenMatch = Regex.Match(input, betweenPattern, RegexOptions.IgnoreCase);

            if (betweenMatch.Success)
            {
                string date1Str = betweenMatch.Groups[1].Value.Trim();
                string date2Str = betweenMatch.Groups[2].Value.Trim();

                if (TryParseDate(date1Str, out DateTime date1) && TryParseDate(date2Str, out DateTime date2))
                {
                    return CalculateDurationBetween(date1, date2, date1Str, date2Str);
                }
            }

            // Parse "date plus/add X units" or "date minus/subtract X units"
            var addSubPattern = @"^(.+?)\s+(plus|add|\+|minus|subtract|-)\s+(\d+)\s+(second|minute|hour|day|week|month|year)s?";
            var addSubMatch = Regex.Match(input, addSubPattern, RegexOptions.IgnoreCase);

            if (addSubMatch.Success)
            {
                string dateStr = addSubMatch.Groups[1].Value.Trim();
                string operation = addSubMatch.Groups[2].Value.ToLower();
                if (!int.TryParse(addSubMatch.Groups[3].Value, out int amount))
                {
                    return null;
                }
                string unit = addSubMatch.Groups[4].Value.ToLower();

                if (!TryParseDate(dateStr, out DateTime startDate))
                {
                    return FixedParagraph($"Could not parse date: '{dateStr}'. Try formats like: 2024-01-15, 01/15/2024, or 'today'");
                }

                bool isAdd = operation == "plus" || operation == "add" || operation == "+";
                if (!isAdd)
                {
                    amount = -amount;
                }

                DateTime resultDate;
                try
                {
                    resultDate = unit switch
                    {
                        "second" => startDate.AddSeconds(amount),
                        "minute" => startDate.AddMinutes(amount),
                        "hour" => startDate.AddHours(amount),
                        "day" => startDate.AddDays(amount),
                        "week" => startDate.AddDays(amount * 7),
                        "month" => startDate.AddMonths(amount),
                        "year" => startDate.AddYears(amount),
                        _ => startDate
                    };
                }
                catch (ArgumentOutOfRangeException)
                {
                    return FixedParagraph("The resulting date is outside the valid range (01/01/0001 to 12/31/9999)");
                }

                TimeSpan duration = resultDate - startDate;
                string operationDesc = isAdd ? "Adding" : "Subtracting";
                int absAmount = System.Math.Abs(amount);
                string unitPlural = absAmount == 1 ? unit : unit + "s";

                return new object[]
                {
                    Answer($"{startDate:yyyy-MM-dd} {(isAdd ? "+" : "-")} {absAmount} {unitPlural} = {resultDate:yyyy-MM-dd}"),
                    SectionHeader($"{operationDesc} {absAmount} {unitPlural}:"),
                    NameValueTable(entries: new[]
                    {
                        ("Start Date:", $"{startDate:dddd, MMMM d, yyyy}"),
                        ("Operation:", $"{(isAdd ? "Add" : "Subtract")} {absAmount} {unitPlural}"),
                        ("Result Date:", $"{resultDate:dddd, MMMM d, yyyy}"),
                        ("Days Difference:", $"{System.Math.Abs(duration.Days)} days")
                    }),
                    SectionHeader("Result Details:"),
                    NameValueTable(entries: new[]
                    {
                        ("Short Format:", $"{resultDate:yyyy-MM-dd}"),
                        ("Long Format:", $"{resultDate:dddd, MMMM d, yyyy}"),
                        ("Day of Year:", $"Day {resultDate.DayOfYear} of {resultDate.Year}"),
                        ("Week of Year:", $"Week {GetWeekOfYear(resultDate)}")
                    })
                };
            }

            return null;
        }

        private object CalculateDurationBetween(DateTime date1, DateTime date2, string date1Str, string date2Str)
        {
            // Ensure date1 is earlier
            DateTime startDate = date1 < date2 ? date1 : date2;
            DateTime endDate = date1 < date2 ? date2 : date1;
            bool wasReversed = date1 > date2;

            TimeSpan duration = endDate - startDate;

            // Calculate years, months, days
            int years = 0;
            int months = 0;
            int days = duration.Days;

            DateTime tempDate = startDate;
            while (tempDate.AddYears(1) <= endDate)
            {
                years++;
                tempDate = tempDate.AddYears(1);
            }

            while (tempDate.AddMonths(1) <= endDate)
            {
                months++;
                tempDate = tempDate.AddMonths(1);
            }

            int remainingDays = (endDate - tempDate).Days;

            // Calculate various time units
            long totalSeconds = (long)duration.TotalSeconds;
            long totalMinutes = (long)duration.TotalMinutes;
            long totalHours = (long)duration.TotalHours;
            int totalDays = duration.Days;
            int totalWeeks = totalDays / 7;

            // Build human-readable duration
            string humanReadable = "";
            if (years > 0)
                humanReadable += $"{years} year{(years != 1 ? "s" : "")}";
            if (months > 0)
            {
                if (humanReadable.Length > 0) humanReadable += ", ";
                humanReadable += $"{months} month{(months != 1 ? "s" : "")}";
            }
            if (remainingDays > 0 || humanReadable.Length == 0)
            {
                if (humanReadable.Length > 0) humanReadable += ", ";
                humanReadable += $"{remainingDays} day{(remainingDays != 1 ? "s" : "")}";
            }

            return new object[]
            {
                Answer($"{totalDays} days between {startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd}"),
                SectionHeader("Duration Breakdown:"),
                NameValueTable(entries: new[]
                {
                    ("Start Date:", $"{startDate:dddd, MMMM d, yyyy}"),
                    ("End Date:", $"{endDate:dddd, MMMM d, yyyy}"),
                    ("Duration:", humanReadable),
                    ("Total Days:", $"{totalDays:N0} days"),
                    ("Total Weeks:", $"{totalWeeks:N0} weeks"),
                    ("Total Months:", $"{(totalDays / 30.44):F1} months (avg)"),
                    ("Total Years:", $"{(totalDays / 365.25):F2} years")
                }),
                SectionHeader("Precise Breakdown:"),
                NameValueTable(entries: new[]
                {
                    ("Years:", $"{years}"),
                    ("Months:", $"{months}"),
                    ("Days:", $"{remainingDays}"),
                    ("Total Hours:", $"{totalHours:N0}"),
                    ("Total Minutes:", $"{totalMinutes:N0}"),
                    ("Total Seconds:", $"{totalSeconds:N0}")
                })
            };
        }

        private bool TryParseDate(string input, out DateTime result)
        {
            input = input.Trim();

            // Handle special keywords
            if (string.Equals(input, "today", StringComparison.OrdinalIgnoreCase))
            {
                result = DateTime.Today;
                return true;
            }

            if (string.Equals(input, "now", StringComparison.OrdinalIgnoreCase))
            {
                result = DateTime.Now;
                return true;
            }

            if (string.Equals(input, "yesterday", StringComparison.OrdinalIgnoreCase))
            {
                result = DateTime.Today.AddDays(-1);
                return true;
            }

            if (string.Equals(input, "tomorrow", StringComparison.OrdinalIgnoreCase))
            {
                result = DateTime.Today.AddDays(1);
                return true;
            }

            // Try parsing various date formats
            string[] formats = new[]
            {
                "yyyy-MM-dd",
                "yyyy/MM/dd",
                "MM/dd/yyyy",
                "MM-dd-yyyy",
                "dd/MM/yyyy",
                "dd-MM-yyyy",
                "M/d/yyyy",
                "M-d-yyyy",
                "d/M/yyyy",
                "d-M-yyyy",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy/MM/dd HH:mm:ss",
                "MM/dd/yyyy HH:mm:ss",
                "MM/dd/yyyy h:mm:ss tt",
                "M/d/yyyy h:mm tt",
                "MMMM d, yyyy",
                "MMMM d yyyy",
                "MMM d, yyyy",
                "d MMMM yyyy",
                "d MMM yyyy"
            };

            if (DateTime.TryParseExact(input, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return true;
            }

            // Try general parse as fallback
            if (DateTime.TryParse(input, out result))
            {
                return true;
            }

            result = DateTime.MinValue;
            return false;
        }

        private int GetWeekOfYear(DateTime date)
        {
            CultureInfo culture = CultureInfo.CurrentCulture;
            Calendar calendar = culture.Calendar;
            return calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
        }
    }
}
