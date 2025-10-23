using System;
using System.Globalization;
using System.Text.RegularExpressions;
using static QuickInfo.NodeFactory;

namespace QuickInfo
{
    public class Age : IProcessor
    {
        public object GetResult(Query query)
        {
            if (query.IsHelp)
            {
                return HelpTable(
                    ("age", "Show age calculation examples"),
                    ("age 1990-05-15", "Calculate age from birthdate"),
                    ("born 1985-03-20", "Calculate age from birthdate"),
                    ("age from 2000-01-01", "Calculate age from date"),
                    ("how old 1975-12-25", "Calculate age"));
            }

            var input = query.OriginalInput.Trim();

            // General age info
            if (string.Equals(input, "age", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "age calculator", StringComparison.OrdinalIgnoreCase))
            {
                return new object[]
                {
                    Answer("Age Calculator"),
                    SectionHeader("Examples:"),
                    NameValueTable(entries: new[]
                    {
                        ("Calculate Age:", "age 1990-05-15"),
                        ("Born On:", "born 1985-03-20"),
                        ("From Date:", "age from 2000-01-01"),
                        ("How Old:", "how old 1975-12-25")
                    }),
                    SectionHeader("Supported Formats:"),
                    FixedParagraph("YYYY-MM-DD, MM/DD/YYYY, DD-MM-YYYY, or written dates like 'January 15, 1990'")
                };
            }

            // Pattern: "age [date]", "born [date]", "age from [date]", "how old [date]"
            var agePattern = @"^(?:age|born|age\s+from|how\s+old)\s+(.+)$";
            var ageMatch = Regex.Match(input, agePattern, RegexOptions.IgnoreCase);

            if (ageMatch.Success)
            {
                string dateStr = ageMatch.Groups[1].Value.Trim();

                if (!TryParseDate(dateStr, out DateTime birthDate))
                {
                    return FixedParagraph($"Could not parse date: '{dateStr}'. Try formats like: 1990-05-15, 05/15/1990, or 'May 15, 1990'");
                }

                DateTime today = DateTime.Today;

                // Check if date is in the future
                if (birthDate > today)
                {
                    // Calculate time until date
                    TimeSpan timeUntil = birthDate - today;
                    int yearsUntil = today.Year - birthDate.Year;
                    int monthsUntil = today.Month - birthDate.Month;
                    int daysUntil = timeUntil.Days;

                    if (monthsUntil < 0)
                    {
                        yearsUntil--;
                        monthsUntil += 12;
                    }

                    return new object[]
                    {
                        Answer($"{birthDate:yyyy-MM-dd} is in the future"),
                        SectionHeader("Time Until Date:"),
                        NameValueTable(entries: new[]
                        {
                            ("Date:", $"{birthDate:dddd, MMMM d, yyyy}"),
                            ("Days Until:", $"{daysUntil} days"),
                            ("Weeks Until:", $"{daysUntil / 7} weeks"),
                            ("Months Until:", $"~{System.Math.Abs(monthsUntil)} months"),
                            ("Years Until:", $"~{System.Math.Abs(yearsUntil)} years")
                        })
                    };
                }

                // Calculate age
                int years = today.Year - birthDate.Year;
                int months = today.Month - birthDate.Month;
                int days = today.Day - birthDate.Day;

                // Adjust for birthdays not yet reached this year
                if (months < 0 || (months == 0 && days < 0))
                {
                    years--;
                    months += 12;
                }

                if (days < 0)
                {
                    months--;
                    if (months < 0)
                    {
                        years--;
                        months += 12;
                    }
                    // Get days in previous month
                    DateTime previousMonth = today.AddMonths(-1);
                    days += DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);
                }

                // Calculate total days, weeks, months
                TimeSpan totalTime = today - birthDate;
                int totalDays = totalTime.Days;
                int totalWeeks = totalDays / 7;
                int totalMonths = (years * 12) + months;
                double totalYears = totalDays / 365.25;

                // Calculate next birthday
                DateTime nextBirthday = new DateTime(today.Year, birthDate.Month, birthDate.Day);
                if (nextBirthday < today)
                {
                    nextBirthday = nextBirthday.AddYears(1);
                }
                int daysUntilBirthday = (nextBirthday - today).Days;

                // Determine zodiac sign
                string zodiacSign = GetZodiacSign(birthDate);

                // Determine day of week born
                string dayOfWeek = birthDate.ToString("dddd");

                return new object[]
                {
                    Answer($"{years} years old (born {birthDate:yyyy-MM-dd})"),
                    SectionHeader("Age Breakdown:"),
                    NameValueTable(entries: new[]
                    {
                        ("Years:", $"{years}"),
                        ("Months:", $"{months}"),
                        ("Days:", $"{days}"),
                        ("Full Expression:", $"{years} years, {months} months, {days} days")
                    }),
                    SectionHeader("Birth Information:"),
                    NameValueTable(entries: new[]
                    {
                        ("Birth Date:", $"{birthDate:dddd, MMMM d, yyyy}"),
                        ("Day of Week:", dayOfWeek),
                        ("Zodiac Sign:", zodiacSign),
                        ("Next Birthday:", $"{nextBirthday:MMMM d, yyyy} ({daysUntilBirthday} days)"),
                        ("Birthday is:", GetBirthdayRelative(daysUntilBirthday))
                    }),
                    SectionHeader("Total Time Lived:"),
                    NameValueTable(entries: new[]
                    {
                        ("Total Days:", $"{totalDays:N0}"),
                        ("Total Weeks:", $"{totalWeeks:N0}"),
                        ("Total Months:", $"{totalMonths:N0}"),
                        ("Total Years:", $"{totalYears:F2}"),
                        ("Total Hours:", $"{totalDays * 24:N0} (approx)"),
                        ("Total Minutes:", $"{totalDays * 24 * 60:N0} (approx)")
                    })
                };
            }

            return null;
        }

        private bool TryParseDate(string input, out DateTime result)
        {
            input = input.Trim();

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
                "MMMM d, yyyy",
                "MMMM d yyyy",
                "MMM d, yyyy",
                "d MMMM yyyy",
                "d MMM yyyy",
                "MMMM dd, yyyy",
                "MMM dd, yyyy"
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

        private string GetZodiacSign(DateTime date)
        {
            int month = date.Month;
            int day = date.Day;

            if ((month == 3 && day >= 21) || (month == 4 && day <= 19))
                return "Aries ♈";
            else if ((month == 4 && day >= 20) || (month == 5 && day <= 20))
                return "Taurus ♉";
            else if ((month == 5 && day >= 21) || (month == 6 && day <= 20))
                return "Gemini ♊";
            else if ((month == 6 && day >= 21) || (month == 7 && day <= 22))
                return "Cancer ♋";
            else if ((month == 7 && day >= 23) || (month == 8 && day <= 22))
                return "Leo ♌";
            else if ((month == 8 && day >= 23) || (month == 9 && day <= 22))
                return "Virgo ♍";
            else if ((month == 9 && day >= 23) || (month == 10 && day <= 22))
                return "Libra ♎";
            else if ((month == 10 && day >= 23) || (month == 11 && day <= 21))
                return "Scorpio ♏";
            else if ((month == 11 && day >= 22) || (month == 12 && day <= 21))
                return "Sagittarius ♐";
            else if ((month == 12 && day >= 22) || (month == 1 && day <= 19))
                return "Capricorn ♑";
            else if ((month == 1 && day >= 20) || (month == 2 && day <= 18))
                return "Aquarius ♒";
            else
                return "Pisces ♓";
        }

        private string GetBirthdayRelative(int daysUntil)
        {
            if (daysUntil == 0)
                return "Today! 🎉";
            else if (daysUntil == 1)
                return "Tomorrow! 🎂";
            else if (daysUntil <= 7)
                return $"This week ({daysUntil} days)";
            else if (daysUntil <= 14)
                return $"Next week ({daysUntil} days)";
            else if (daysUntil <= 30)
                return $"This month ({daysUntil} days)";
            else if (daysUntil <= 60)
                return $"Next month ({daysUntil} days)";
            else if (daysUntil <= 180)
                return $"In {daysUntil / 30} months";
            else
                return $"In {daysUntil} days";
        }
    }
}
