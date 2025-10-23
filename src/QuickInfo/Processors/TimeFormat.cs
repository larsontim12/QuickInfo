using System;
using System.Text.RegularExpressions;
using static QuickInfo.NodeFactory;

namespace QuickInfo
{
    public class TimeFormat : IProcessor
    {
        public object GetResult(Query query)
        {
            if (query.IsHelp)
            {
                return HelpTable(
                    ("time format", "Show time format conversion examples"),
                    ("3600 seconds to time", "Convert seconds to time format"),
                    ("1:30:45 to seconds", "Convert time format to seconds"),
                    ("90 minutes to time", "Convert minutes to time format"),
                    ("12345 seconds", "Convert seconds to readable time"));
            }

            var input = query.OriginalInput.Trim();

            // General time format info
            if (string.Equals(input, "time format", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "time conversion", StringComparison.OrdinalIgnoreCase))
            {
                return new object[]
                {
                    Answer("Time Format Conversions"),
                    SectionHeader("Supported Conversions:"),
                    NameValueTable(entries: new[]
                    {
                        ("Seconds to Time:", "3600 seconds → 1:00:00"),
                        ("Minutes to Time:", "90 minutes → 1:30:00"),
                        ("Hours to Time:", "2.5 hours → 2:30:00"),
                        ("Time to Seconds:", "1:30:45 → 5445 seconds"),
                        ("Time to Minutes:", "2:15:00 → 135 minutes")
                    }),
                    SectionHeader("Time Format:"),
                    FixedParagraph("H:MM:SS (hours:minutes:seconds) or HH:MM:SS")
                };
            }

            // Pattern: "X seconds to time" or "X seconds to hours" etc.
            var secondsToTimeMatch = Regex.Match(input, @"^(\d+)\s+seconds?(?:\s+to\s+(?:time|hours?|hms))?$", RegexOptions.IgnoreCase);
            if (secondsToTimeMatch.Success)
            {
                int totalSeconds = int.Parse(secondsToTimeMatch.Groups[1].Value);

                if (totalSeconds < 0 || totalSeconds > 86400 * 365)
                {
                    return FixedParagraph("Please enter a valid number of seconds (0 - 31,536,000 / 1 year)");
                }

                return ConvertSecondsToTime(totalSeconds);
            }

            // Pattern: "X minutes to time" or "X minutes"
            var minutesToTimeMatch = Regex.Match(input, @"^(\d+(?:\.\d+)?)\s+minutes?(?:\s+to\s+(?:time|hours?|hms))?$", RegexOptions.IgnoreCase);
            if (minutesToTimeMatch.Success)
            {
                double totalMinutes = double.Parse(minutesToTimeMatch.Groups[1].Value);

                if (totalMinutes < 0 || totalMinutes > 525600)
                {
                    return FixedParagraph("Please enter a valid number of minutes (0 - 525,600 / 1 year)");
                }

                int totalSeconds = (int)(totalMinutes * 60);
                return ConvertSecondsToTime(totalSeconds);
            }

            // Pattern: "X hours to time" or "X hours"
            var hoursToTimeMatch = Regex.Match(input, @"^(\d+(?:\.\d+)?)\s+hours?(?:\s+to\s+(?:time|hms))?$", RegexOptions.IgnoreCase);
            if (hoursToTimeMatch.Success)
            {
                double totalHours = double.Parse(hoursToTimeMatch.Groups[1].Value);

                if (totalHours < 0 || totalHours > 8760)
                {
                    return FixedParagraph("Please enter a valid number of hours (0 - 8,760 / 1 year)");
                }

                int totalSeconds = (int)(totalHours * 3600);
                return ConvertSecondsToTime(totalSeconds);
            }

            // Pattern: "H:MM:SS to seconds" or "HH:MM:SS to seconds/minutes"
            var timeToSecondsMatch = Regex.Match(input, @"^(\d{1,2}):(\d{2}):(\d{2})(?:\s+to\s+(seconds?|minutes?|mins?))?$", RegexOptions.IgnoreCase);
            if (timeToSecondsMatch.Success)
            {
                int hours = int.Parse(timeToSecondsMatch.Groups[1].Value);
                int minutes = int.Parse(timeToSecondsMatch.Groups[2].Value);
                int seconds = int.Parse(timeToSecondsMatch.Groups[3].Value);
                string targetUnit = timeToSecondsMatch.Groups[4].Success ? timeToSecondsMatch.Groups[4].Value : "seconds";

                if (hours < 0 || hours > 999 || minutes < 0 || minutes > 59 || seconds < 0 || seconds > 59)
                {
                    return FixedParagraph("Invalid time format. Minutes and seconds must be 0-59, hours 0-999");
                }

                int totalSeconds = (hours * 3600) + (minutes * 60) + seconds;
                double totalMinutes = totalSeconds / 60.0;
                double totalHours = totalSeconds / 3600.0;

                return new object[]
                {
                    Answer($"{hours:D2}:{minutes:D2}:{seconds:D2} = {totalSeconds:N0} seconds"),
                    SectionHeader("Time to Seconds Conversion:"),
                    NameValueTable(entries: new[]
                    {
                        ("Time Format:", $"{hours:D2}:{minutes:D2}:{seconds:D2}"),
                        ("Total Seconds:", $"{totalSeconds:N0}"),
                        ("Total Minutes:", $"{totalMinutes:F2}"),
                        ("Total Hours:", $"{totalHours:F2}")
                    }),
                    SectionHeader("Breakdown:"),
                    NameValueTable(entries: new[]
                    {
                        ("Hours:", $"{hours} × 3600 = {hours * 3600:N0} seconds"),
                        ("Minutes:", $"{minutes} × 60 = {minutes * 60:N0} seconds"),
                        ("Seconds:", $"{seconds}"),
                        ("Total:", $"{totalSeconds:N0} seconds")
                    })
                };
            }

            // Pattern: "MM:SS to seconds" (without hours)
            var mmssMatch = Regex.Match(input, @"^(\d{1,2}):(\d{2})(?:\s+to\s+(seconds?|minutes?|mins?))?$", RegexOptions.IgnoreCase);
            if (mmssMatch.Success)
            {
                int minutes = int.Parse(mmssMatch.Groups[1].Value);
                int seconds = int.Parse(mmssMatch.Groups[2].Value);

                if (minutes < 0 || minutes > 999 || seconds < 0 || seconds > 59)
                {
                    return FixedParagraph("Invalid time format. Seconds must be 0-59, minutes 0-999");
                }

                int totalSeconds = (minutes * 60) + seconds;
                double totalMinutes = totalSeconds / 60.0;

                return new object[]
                {
                    Answer($"{minutes}:{seconds:D2} = {totalSeconds:N0} seconds"),
                    SectionHeader("Time to Seconds Conversion:"),
                    NameValueTable(entries: new[]
                    {
                        ("Time Format:", $"{minutes}:{seconds:D2} (MM:SS)"),
                        ("Total Seconds:", $"{totalSeconds:N0}"),
                        ("Total Minutes:", $"{totalMinutes:F2}"),
                        ("Hours Format:", $"{totalSeconds / 3600}:{(totalSeconds % 3600) / 60:D2}:{totalSeconds % 60:D2}")
                    })
                };
            }

            return null;
        }

        private object ConvertSecondsToTime(int totalSeconds)
        {
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;

            int days = hours / 24;
            int hoursInDay = hours % 24;

            double totalMinutes = totalSeconds / 60.0;
            double totalHours = totalSeconds / 3600.0;
            double totalDays = totalSeconds / 86400.0;

            string timeFormat = $"{hours}:{minutes:D2}:{seconds:D2}";
            string dayFormat = days > 0 ? $"{days} day{(days != 1 ? "s" : "")} {hoursInDay:D2}:{minutes:D2}:{seconds:D2}" : timeFormat;

            return new object[]
            {
                Answer($"{totalSeconds:N0} seconds = {timeFormat}"),
                SectionHeader("Time Format Conversion:"),
                NameValueTable(entries: new[]
                {
                    ("Total Seconds:", $"{totalSeconds:N0}"),
                    ("Time Format:", timeFormat),
                    ("With Days:", dayFormat),
                    ("12-Hour Format:", ConvertTo12Hour(hours, minutes, seconds))
                }),
                SectionHeader("Breakdown:"),
                NameValueTable(entries: new[]
                {
                    ("Days:", $"{days}"),
                    ("Hours:", $"{hours}"),
                    ("Minutes:", $"{minutes}"),
                    ("Seconds:", $"{seconds}"),
                    ("Total Minutes:", $"{totalMinutes:F2}"),
                    ("Total Hours:", $"{totalHours:F2}"),
                    ("Total Days:", $"{totalDays:F2}")
                })
            };
        }

        private string ConvertTo12Hour(int hours, int minutes, int seconds)
        {
            if (hours == 0)
                return $"12:{minutes:D2}:{seconds:D2} AM";
            else if (hours < 12)
                return $"{hours}:{minutes:D2}:{seconds:D2} AM";
            else if (hours == 12)
                return $"12:{minutes:D2}:{seconds:D2} PM";
            else
                return $"{hours - 12}:{minutes:D2}:{seconds:D2} PM";
        }
    }
}
