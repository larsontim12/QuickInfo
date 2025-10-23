using System;
using System.Globalization;
using System.Text.RegularExpressions;
using static QuickInfo.NodeFactory;

namespace QuickInfo
{
    public class TimeUntil : IProcessor
    {
        public object GetResult(Query query)
        {
            if (query.IsHelp)
            {
                return HelpTable(
                    ("time until", "Show countdown/countup examples"),
                    ("days until 2024-12-25", "Count days until a date"),
                    ("time until Christmas", "Count time until Christmas"),
                    ("time since 2020-01-01", "Count time since a date"),
                    ("countdown to 2025-01-01", "Countdown to a date"));
            }

            var input = query.OriginalInput.Trim();

            // General info
            if (string.Equals(input, "time until", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "countdown", StringComparison.OrdinalIgnoreCase))
            {
                return new object[]
                {
                    Answer("Time Until/Since Calculator"),
                    SectionHeader("Examples:"),
                    NameValueTable(entries: new[]
                    {
                        ("Days Until:", "days until 2024-12-25"),
                        ("Time Until:", "time until 2025-01-01"),
                        ("Time Since:", "time since 2020-01-01"),
                        ("Countdown:", "countdown to 2024-06-15"),
                        ("Special Dates:", "days until Christmas, New Year, etc.")
                    })
                };
            }

            // Pattern: "days until [date]", "time until [date]", "countdown to [date]"
            var untilPattern = @"^(?:days?\s+until|time\s+until|countdown\s+to|until)\s+(.+)$";
            var untilMatch = Regex.Match(input, untilPattern, RegexOptions.IgnoreCase);

            if (untilMatch.Success)
            {
                string dateStr = untilMatch.Groups[1].Value.Trim();

                if (TryParseSpecialDate(dateStr, out DateTime targetDate))
                {
                    return CalculateTimeUntil(targetDate, dateStr);
                }
                else if (TryParseDate(dateStr, out targetDate))
                {
                    return CalculateTimeUntil(targetDate, dateStr);
                }
                else
                {
                    return FixedParagraph($"Could not parse date: '{dateStr}'. Try: 2024-12-25, Christmas, New Year, etc.");
                }
            }

            // Pattern: "time since [date]", "days since [date]"
            var sincePattern = @"^(?:days?\s+since|time\s+since|since)\s+(.+)$";
            var sinceMatch = Regex.Match(input, sincePattern, RegexOptions.IgnoreCase);

            if (sinceMatch.Success)
            {
                string dateStr = sinceMatch.Groups[1].Value.Trim();

                if (TryParseSpecialDate(dateStr, out DateTime targetDate))
                {
                    return CalculateTimeSince(targetDate, dateStr);
                }
                else if (TryParseDate(dateStr, out targetDate))
                {
                    return CalculateTimeSince(targetDate, dateStr);
                }
                else
                {
                    return FixedParagraph($"Could not parse date: '{dateStr}'. Try: 2020-01-01, etc.");
                }
            }

            return null;
        }

        private object CalculateTimeUntil(DateTime targetDate, string originalInput)
        {
            DateTime now = DateTime.Now;

            if (targetDate < now)
            {
                // Date is in the past, show how long ago
                TimeSpan timeSince = now - targetDate;
                return new object[]
                {
                    Answer($"{originalInput} was {timeSince.Days} days ago"),
                    SectionHeader("Time Since (Past Date):"),
                    NameValueTable(entries: new[]
                    {
                        ("Target Date:", $"{targetDate:dddd, MMMM d, yyyy}"),
                        ("Current Date:", $"{now:dddd, MMMM d, yyyy}"),
                        ("Time Since:", $"{timeSince.Days} days ago"),
                        ("Status:", "This date has passed")
                    }),
                    FixedParagraph($"Tip: Use 'time since {originalInput}' for past dates")
                };
            }

            TimeSpan timeUntil = targetDate - now;

            int totalDays = timeUntil.Days;
            int totalWeeks = totalDays / 7;
            int totalMonths = (int)(totalDays / 30.44);
            double totalYears = totalDays / 365.25;

            int years = 0;
            int months = 0;
            int days = totalDays;

            DateTime temp = now;
            while (temp.AddYears(1) <= targetDate)
            {
                years++;
                temp = temp.AddYears(1);
            }

            while (temp.AddMonths(1) <= targetDate)
            {
                months++;
                temp = temp.AddMonths(1);
            }

            days = (targetDate - temp).Days;

            // Calculate hours, minutes, seconds
            int hours = timeUntil.Hours;
            int minutes = timeUntil.Minutes;
            int seconds = timeUntil.Seconds;

            // Determine milestone
            string milestone = GetMilestone(totalDays);

            return new object[]
            {
                Answer($"{totalDays} days until {targetDate:yyyy-MM-dd}"),
                SectionHeader("Countdown:"),
                NameValueTable(entries: new[]
                {
                    ("Target Date:", $"{targetDate:dddd, MMMM d, yyyy}"),
                    ("Current Date:", $"{now:dddd, MMMM d, yyyy}"),
                    ("Days Until:", $"{totalDays} days"),
                    ("Full Breakdown:", $"{years} years, {months} months, {days} days"),
                    ("Milestone:", milestone)
                }),
                SectionHeader("Different Time Units:"),
                NameValueTable(entries: new[]
                {
                    ("Weeks:", $"{totalWeeks} weeks"),
                    ("Months:", $"~{totalMonths} months"),
                    ("Years:", $"{totalYears:F2} years"),
                    ("Hours:", $"{(long)timeUntil.TotalHours:N0} hours"),
                    ("Minutes:", $"{(long)timeUntil.TotalMinutes:N0} minutes"),
                    ("Seconds:", $"{(long)timeUntil.TotalSeconds:N0} seconds")
                }),
                SectionHeader("Precise Countdown:"),
                FixedParagraph($"{totalDays} days, {hours} hours, {minutes} minutes, {seconds} seconds")
            };
        }

        private object CalculateTimeSince(DateTime startDate, string originalInput)
        {
            DateTime now = DateTime.Now;

            if (startDate > now)
            {
                // Date is in the future
                TimeSpan timeUntil = startDate - now;
                return new object[]
                {
                    Answer($"{originalInput} is {timeUntil.Days} days in the future"),
                    SectionHeader("Time Until (Future Date):"),
                    NameValueTable(entries: new[]
                    {
                        ("Target Date:", $"{startDate:dddd, MMMM d, yyyy}"),
                        ("Current Date:", $"{now:dddd, MMMM d, yyyy}"),
                        ("Time Until:", $"{timeUntil.Days} days"),
                        ("Status:", "This date is in the future")
                    }),
                    FixedParagraph($"Tip: Use 'time until {originalInput}' for future dates")
                };
            }

            TimeSpan timeSince = now - startDate;

            int totalDays = timeSince.Days;
            int totalWeeks = totalDays / 7;
            int totalMonths = (int)(totalDays / 30.44);
            double totalYears = totalDays / 365.25;

            int years = 0;
            int months = 0;
            int days = totalDays;

            DateTime temp = startDate;
            while (temp.AddYears(1) <= now)
            {
                years++;
                temp = temp.AddYears(1);
            }

            while (temp.AddMonths(1) <= now)
            {
                months++;
                temp = temp.AddMonths(1);
            }

            days = (now - temp).Days;

            // Calculate hours, minutes, seconds
            int hours = timeSince.Hours;
            int minutes = timeSince.Minutes;
            int seconds = timeSince.Seconds;

            // Determine milestone
            string milestone = GetMilestoneSince(totalDays);

            return new object[]
            {
                Answer($"{totalDays} days since {startDate:yyyy-MM-dd}"),
                SectionHeader("Time Elapsed:"),
                NameValueTable(entries: new[]
                {
                    ("Start Date:", $"{startDate:dddd, MMMM d, yyyy}"),
                    ("Current Date:", $"{now:dddd, MMMM d, yyyy}"),
                    ("Days Since:", $"{totalDays} days"),
                    ("Full Breakdown:", $"{years} years, {months} months, {days} days"),
                    ("Milestone:", milestone)
                }),
                SectionHeader("Different Time Units:"),
                NameValueTable(entries: new[]
                {
                    ("Weeks:", $"{totalWeeks} weeks"),
                    ("Months:", $"~{totalMonths} months"),
                    ("Years:", $"{totalYears:F2} years"),
                    ("Hours:", $"{(long)timeSince.TotalHours:N0} hours"),
                    ("Minutes:", $"{(long)timeSince.TotalMinutes:N0} minutes"),
                    ("Seconds:", $"{(long)timeSince.TotalSeconds:N0} seconds")
                }),
                SectionHeader("Precise Duration:"),
                FixedParagraph($"{totalDays} days, {hours} hours, {minutes} minutes, {seconds} seconds")
            };
        }

        private bool TryParseSpecialDate(string input, out DateTime result)
        {
            input = input.ToLower().Trim();
            int currentYear = DateTime.Now.Year;

            // Handle special date keywords
            if (input == "christmas" || input == "xmas")
            {
                result = new DateTime(currentYear, 12, 25);
                if (result < DateTime.Now)
                    result = result.AddYears(1);
                return true;
            }

            if (input == "new year" || input == "new years" || input == "new year's day")
            {
                result = new DateTime(currentYear + 1, 1, 1);
                return true;
            }

            if (input == "halloween")
            {
                result = new DateTime(currentYear, 10, 31);
                if (result < DateTime.Now)
                    result = result.AddYears(1);
                return true;
            }

            if (input == "thanksgiving")
            {
                // Fourth Thursday of November
                result = GetThanksgivingDate(currentYear);
                if (result < DateTime.Now)
                    result = GetThanksgivingDate(currentYear + 1);
                return true;
            }

            if (input == "independence day" || input == "july 4th" || input == "fourth of july")
            {
                result = new DateTime(currentYear, 7, 4);
                if (result < DateTime.Now)
                    result = result.AddYears(1);
                return true;
            }

            if (input == "valentine's day" || input == "valentines day")
            {
                result = new DateTime(currentYear, 2, 14);
                if (result < DateTime.Now)
                    result = result.AddYears(1);
                return true;
            }

            if (input == "easter")
            {
                result = CalculateEaster(currentYear);
                if (result < DateTime.Now)
                    result = CalculateEaster(currentYear + 1);
                return true;
            }

            result = DateTime.MinValue;
            return false;
        }

        private DateTime GetThanksgivingDate(int year)
        {
            // Fourth Thursday of November
            DateTime nov1 = new DateTime(year, 11, 1);
            int daysToThursday = ((int)DayOfWeek.Thursday - (int)nov1.DayOfWeek + 7) % 7;
            DateTime firstThursday = nov1.AddDays(daysToThursday);
            return firstThursday.AddDays(21); // Add 3 weeks
        }

        private DateTime CalculateEaster(int year)
        {
            // Computus algorithm for Easter (Western/Gregorian calendar)
            int a = year % 19;
            int b = year / 100;
            int c = year % 100;
            int d = b / 4;
            int e = b % 4;
            int f = (b + 8) / 25;
            int g = (b - f + 1) / 3;
            int h = (19 * a + b - d - g + 15) % 30;
            int i = c / 4;
            int k = c % 4;
            int l = (32 + 2 * e + 2 * i - h - k) % 7;
            int m = (a + 11 * h + 22 * l) / 451;
            int month = (h + l - 7 * m + 114) / 31;
            int day = ((h + l - 7 * m + 114) % 31) + 1;
            return new DateTime(year, month, day);
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

        private string GetMilestone(int days)
        {
            if (days == 0)
                return "Today!";
            else if (days == 1)
                return "Tomorrow!";
            else if (days <= 7)
                return "Less than a week";
            else if (days <= 14)
                return "About 2 weeks";
            else if (days <= 30)
                return "About a month";
            else if (days <= 60)
                return "About 2 months";
            else if (days <= 90)
                return "About a quarter (3 months)";
            else if (days <= 180)
                return "About half a year";
            else if (days <= 365)
                return "Less than a year";
            else if (days <= 730)
                return "About 1-2 years";
            else
                return $"Over {days / 365} years";
        }

        private string GetMilestoneSince(int days)
        {
            if (days == 0)
                return "Today";
            else if (days == 1)
                return "Yesterday";
            else if (days <= 7)
                return "Within the past week";
            else if (days <= 30)
                return "Within the past month";
            else if (days <= 365)
                return "Within the past year";
            else if (days <= 730)
                return "1-2 years ago";
            else if (days <= 1825)
                return "2-5 years ago";
            else if (days <= 3650)
                return "5-10 years ago";
            else
                return $"Over {days / 365} years ago";
        }
    }
}
