using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using static QuickInfo.NodeFactory;

namespace QuickInfo
{
    public class BusinessDays : IProcessor
    {
        // US Federal Holidays (fixed dates and calculation methods)
        private static readonly List<(int Month, int Day)> FixedHolidays = new List<(int, int)>
        {
            (1, 1),    // New Year's Day
            (7, 4),    // Independence Day
            (11, 11),  // Veterans Day
            (12, 25)   // Christmas Day
        };

        public object GetResult(Query query)
        {
            if (query.IsHelp)
            {
                return HelpTable(
                    ("business days", "Show business days calculation examples"),
                    ("business days 2024-01-01 to 2024-12-31", "Calculate business days between dates"),
                    ("workdays 2024-06-01 to 2024-06-30", "Calculate workdays in a period"),
                    ("business days from 2024-01-15 to 2024-03-20", "Calculate business days"));
            }

            var input = query.OriginalInput.Trim();

            // General business days info
            if (string.Equals(input, "business days", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "workdays", StringComparison.OrdinalIgnoreCase))
            {
                return new object[]
                {
                    Answer("Business Days Calculator"),
                    SectionHeader("Examples:"),
                    NameValueTable(entries: new[]
                    {
                        ("Calculate Range:", "business days 2024-01-01 to 2024-12-31"),
                        ("Workdays:", "workdays 2024-06-01 to 2024-06-30"),
                        ("With 'from':", "business days from 2024-01-15 to 2024-03-20")
                    }),
                    SectionHeader("Business Days:"),
                    FixedParagraph("Business days exclude weekends (Saturday and Sunday)."),
                    FixedParagraph("US Federal holidays are also excluded from the count."),
                    SectionHeader("Holidays Excluded:"),
                    FixedParagraph("New Year's Day, MLK Day, Presidents Day, Memorial Day, Independence Day, Labor Day, Columbus Day, Veterans Day, Thanksgiving, Christmas")
                };
            }

            // Pattern: "business days [from] date1 to date2" or "workdays date1 to date2"
            var businessDaysPattern = @"^(?:business\s+days?|workdays?)\s+(?:from\s+)?(.+?)\s+to\s+(.+?)$";
            var match = Regex.Match(input, businessDaysPattern, RegexOptions.IgnoreCase);

            if (match.Success)
            {
                string date1Str = match.Groups[1].Value.Trim();
                string date2Str = match.Groups[2].Value.Trim();

                if (!TryParseDate(date1Str, out DateTime date1))
                {
                    return FixedParagraph($"Could not parse start date: '{date1Str}'. Try: 2024-01-15, 01/15/2024");
                }

                if (!TryParseDate(date2Str, out DateTime date2))
                {
                    return FixedParagraph($"Could not parse end date: '{date2Str}'. Try: 2024-12-31, 12/31/2024");
                }

                // Ensure date1 is earlier
                if (date1 > date2)
                {
                    DateTime temp = date1;
                    date1 = date2;
                    date2 = temp;
                }

                // Calculate business days
                int totalDays = (date2 - date1).Days + 1; // Include both start and end dates
                int businessDays = 0;
                int weekendDays = 0;
                int holidayDays = 0;
                List<string> holidaysFound = new List<string>();

                DateTime current = date1;
                while (current <= date2)
                {
                    if (current.DayOfWeek == DayOfWeek.Saturday || current.DayOfWeek == DayOfWeek.Sunday)
                    {
                        weekendDays++;
                    }
                    else if (IsUSFederalHoliday(current, out string holidayName))
                    {
                        holidayDays++;
                        if (!holidaysFound.Contains(holidayName))
                        {
                            holidaysFound.Add(holidayName);
                        }
                    }
                    else
                    {
                        businessDays++;
                    }

                    current = current.AddDays(1);
                }

                int calendarDays = totalDays;
                double workWeeks = businessDays / 5.0;
                double workMonths = businessDays / 22.0; // Average ~22 work days per month

                string holidaysList = holidaysFound.Count > 0 ? string.Join(", ", holidaysFound) : "None";

                return new object[]
                {
                    Answer($"{businessDays} business days from {date1:yyyy-MM-dd} to {date2:yyyy-MM-dd}"),
                    SectionHeader("Date Range:"),
                    NameValueTable(entries: new[]
                    {
                        ("Start Date:", $"{date1:dddd, MMMM d, yyyy}"),
                        ("End Date:", $"{date2:dddd, MMMM d, yyyy}"),
                        ("Calendar Days:", $"{calendarDays} days"),
                        ("Business Days:", $"{businessDays} days"),
                        ("Weekend Days:", $"{weekendDays} days"),
                        ("Holiday Days:", $"{holidayDays} days")
                    }),
                    SectionHeader("Work Time Estimates:"),
                    NameValueTable(entries: new[]
                    {
                        ("Work Weeks:", $"{workWeeks:F2} weeks"),
                        ("Work Months:", $"{workMonths:F2} months"),
                        ("Work Hours (8h/day):", $"{businessDays * 8:N0} hours"),
                        ("Percentage Workdays:", $"{(businessDays * 100.0 / calendarDays):F1}%")
                    }),
                    SectionHeader("Holidays in Range:"),
                    FixedParagraph(holidaysList)
                };
            }

            return null;
        }

        private bool TryParseDate(string input, out DateTime result)
        {
            input = input.Trim();

            string[] formats = new[]
            {
                "yyyy-MM-dd",
                "yyyy/MM/dd",
                "MM/dd/yyyy",
                "MM-dd-yyyy",
                "M/d/yyyy",
                "M-d-yyyy",
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

            if (DateTime.TryParse(input, out result))
            {
                return true;
            }

            result = DateTime.MinValue;
            return false;
        }

        private bool IsUSFederalHoliday(DateTime date, out string holidayName)
        {
            int year = date.Year;
            int month = date.Month;
            int day = date.Day;

            // Check fixed holidays
            if (FixedHolidays.Contains((month, day)))
            {
                if (month == 1 && day == 1)
                {
                    holidayName = "New Year's Day";
                    return true;
                }
                if (month == 7 && day == 4)
                {
                    holidayName = "Independence Day";
                    return true;
                }
                if (month == 11 && day == 11)
                {
                    holidayName = "Veterans Day";
                    return true;
                }
                if (month == 12 && day == 25)
                {
                    holidayName = "Christmas Day";
                    return true;
                }
            }

            // MLK Day - Third Monday of January
            if (month == 1 && IsNthDayOfWeek(date, DayOfWeek.Monday, 3))
            {
                holidayName = "Martin Luther King Jr. Day";
                return true;
            }

            // Presidents Day - Third Monday of February
            if (month == 2 && IsNthDayOfWeek(date, DayOfWeek.Monday, 3))
            {
                holidayName = "Presidents Day";
                return true;
            }

            // Memorial Day - Last Monday of May
            if (month == 5 && IsLastDayOfWeek(date, DayOfWeek.Monday))
            {
                holidayName = "Memorial Day";
                return true;
            }

            // Labor Day - First Monday of September
            if (month == 9 && IsNthDayOfWeek(date, DayOfWeek.Monday, 1))
            {
                holidayName = "Labor Day";
                return true;
            }

            // Columbus Day - Second Monday of October
            if (month == 10 && IsNthDayOfWeek(date, DayOfWeek.Monday, 2))
            {
                holidayName = "Columbus Day";
                return true;
            }

            // Thanksgiving - Fourth Thursday of November
            if (month == 11 && IsNthDayOfWeek(date, DayOfWeek.Thursday, 4))
            {
                holidayName = "Thanksgiving Day";
                return true;
            }

            holidayName = null;
            return false;
        }

        private bool IsNthDayOfWeek(DateTime date, DayOfWeek targetDayOfWeek, int n)
        {
            if (date.DayOfWeek != targetDayOfWeek)
                return false;

            // Check if this is the nth occurrence of targetDayOfWeek in the month
            DateTime firstOfMonth = new DateTime(date.Year, date.Month, 1);
            int daysToTarget = ((int)targetDayOfWeek - (int)firstOfMonth.DayOfWeek + 7) % 7;
            DateTime nthDay = firstOfMonth.AddDays(daysToTarget + (n - 1) * 7);

            return date == nthDay;
        }

        private bool IsLastDayOfWeek(DateTime date, DayOfWeek targetDayOfWeek)
        {
            if (date.DayOfWeek != targetDayOfWeek)
                return false;

            // Check if this is the last occurrence of targetDayOfWeek in the month
            DateTime nextWeek = date.AddDays(7);
            return nextWeek.Month != date.Month;
        }
    }
}
