using System;
using System.Globalization;
using System.Text.RegularExpressions;
using static QuickInfo.NodeFactory;

namespace QuickInfo
{
    public class UnixTimestamp : IProcessor
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public object GetResult(Query query)
        {
            if (query.IsHelp)
            {
                return HelpTable(
                    ("unix timestamp", "Show Unix timestamp information"),
                    ("1234567890", "Convert Unix timestamp to date"),
                    ("timestamp 2024-01-15", "Convert date to Unix timestamp"),
                    ("unix now", "Get current Unix timestamp"),
                    ("epoch", "Show Unix epoch information"));
            }

            var input = query.OriginalInput.Trim();

            // General Unix timestamp info
            if (string.Equals(input, "unix timestamp", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "unix time", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "epoch", StringComparison.OrdinalIgnoreCase))
            {
                long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                return new object[]
                {
                    Answer($"Unix Epoch: January 1, 1970 00:00:00 UTC"),
                    SectionHeader("Current Unix Timestamp:"),
                    NameValueTable(entries: new[]
                    {
                        ("Seconds:", $"{currentTimestamp}"),
                        ("Milliseconds:", $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"),
                        ("Current UTC Time:", $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC"),
                        ("Current Local Time:", $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                    }),
                    SectionHeader("About Unix Timestamps:"),
                    FixedParagraph("Unix time is the number of seconds that have elapsed since January 1, 1970 (midnight UTC/GMT), not counting leap seconds.")
                };
            }

            // Get current timestamp: "unix now" or "timestamp now"
            if (Regex.IsMatch(input, @"^(?:unix|timestamp)\s+now$", RegexOptions.IgnoreCase))
            {
                long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                long currentMillis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                DateTime nowUtc = DateTime.UtcNow;
                DateTime nowLocal = DateTime.Now;

                return new object[]
                {
                    Answer($"Current Unix Timestamp: {currentTimestamp}"),
                    SectionHeader("Current Timestamps:"),
                    NameValueTable(entries: new[]
                    {
                        ("Seconds:", $"{currentTimestamp}"),
                        ("Milliseconds:", $"{currentMillis}"),
                        ("Microseconds:", $"{currentMillis}000 (approx)")
                    }),
                    SectionHeader("Current Time:"),
                    NameValueTable(entries: new[]
                    {
                        ("UTC:", $"{nowUtc:yyyy-MM-dd HH:mm:ss} UTC"),
                        ("Local:", $"{nowLocal:yyyy-MM-dd HH:mm:ss}"),
                        ("ISO 8601:", $"{nowUtc:yyyy-MM-ddTHH:mm:ssZ}")
                    })
                };
            }

            // Parse Unix timestamp to date: just a number
            var timestampMatch = Regex.Match(input, @"^(\d{9,13})$");
            if (timestampMatch.Success)
            {
                string timestampStr = timestampMatch.Groups[1].Value;
                long timestamp = long.Parse(timestampStr);

                DateTime dateTime;
                string precision;

                // Determine if it's seconds or milliseconds based on length
                if (timestampStr.Length <= 10)
                {
                    // Seconds
                    dateTime = UnixEpoch.AddSeconds(timestamp);
                    precision = "seconds";
                }
                else
                {
                    // Milliseconds
                    dateTime = UnixEpoch.AddMilliseconds(timestamp);
                    precision = "milliseconds";
                }

                // Validate reasonable date range (1970 - 2100)
                if (dateTime.Year < 1970 || dateTime.Year > 2100)
                {
                    return FixedParagraph($"Timestamp results in unreasonable date: {dateTime:yyyy-MM-dd}. Check if timestamp is in seconds or milliseconds.");
                }

                DateTime localTime = dateTime.ToLocalTime();
                long timestampSeconds = timestamp <= 9999999999 ? timestamp : timestamp / 1000;
                long timestampMillis = timestamp <= 9999999999 ? timestamp * 1000 : timestamp;

                // Calculate relative time
                TimeSpan timeSince = DateTime.UtcNow - dateTime;
                string relativeTime = GetRelativeTime(timeSince);

                return new object[]
                {
                    Answer($"{timestamp} = {dateTime:yyyy-MM-dd HH:mm:ss} UTC"),
                    SectionHeader("Timestamp Conversion:"),
                    NameValueTable(entries: new[]
                    {
                        ("Unix Timestamp:", $"{timestamp} ({precision})"),
                        ("UTC Time:", $"{dateTime:dddd, MMMM d, yyyy HH:mm:ss} UTC"),
                        ("Local Time:", $"{localTime:dddd, MMMM d, yyyy HH:mm:ss}"),
                        ("ISO 8601:", $"{dateTime:yyyy-MM-ddTHH:mm:ssZ}"),
                        ("Relative:", relativeTime)
                    }),
                    SectionHeader("All Formats:"),
                    NameValueTable(entries: new[]
                    {
                        ("Seconds:", $"{timestampSeconds}"),
                        ("Milliseconds:", $"{timestampMillis}"),
                        ("RFC 2822:", $"{dateTime:ddd, dd MMM yyyy HH:mm:ss} GMT"),
                        ("Day of Year:", $"Day {dateTime.DayOfYear} of {dateTime.Year}")
                    })
                };
            }

            // Convert date to Unix timestamp: "timestamp YYYY-MM-DD" or "unix YYYY-MM-DD"
            var dateToTimestampMatch = Regex.Match(input, @"^(?:timestamp|unix)\s+(.+)$", RegexOptions.IgnoreCase);
            if (dateToTimestampMatch.Success)
            {
                string dateStr = dateToTimestampMatch.Groups[1].Value.Trim();

                if (!TryParseDate(dateStr, out DateTime dateTime))
                {
                    return FixedParagraph($"Could not parse date: '{dateStr}'. Try formats like: 2024-01-15, 01/15/2024, or '2024-01-15 14:30:00'");
                }

                // Convert to UTC for timestamp calculation
                DateTime utcDateTime = dateTime.Kind == DateTimeKind.Utc
                    ? dateTime
                    : DateTime.SpecifyKind(dateTime, DateTimeKind.Local).ToUniversalTime();

                long timestampSeconds = new DateTimeOffset(utcDateTime).ToUnixTimeSeconds();
                long timestampMillis = new DateTimeOffset(utcDateTime).ToUnixTimeMilliseconds();

                // Calculate relative time
                TimeSpan timeSince = DateTime.UtcNow - utcDateTime;
                string relativeTime = GetRelativeTime(timeSince);

                return new object[]
                {
                    Answer($"{dateTime:yyyy-MM-dd HH:mm:ss} = {timestampSeconds}"),
                    SectionHeader("Date to Timestamp:"),
                    NameValueTable(entries: new[]
                    {
                        ("Input Date:", $"{dateTime:dddd, MMMM d, yyyy HH:mm:ss}"),
                        ("UTC Time:", $"{utcDateTime:yyyy-MM-dd HH:mm:ss} UTC"),
                        ("Unix Timestamp:", $"{timestampSeconds}"),
                        ("Milliseconds:", $"{timestampMillis}"),
                        ("Relative:", relativeTime)
                    }),
                    SectionHeader("All Formats:"),
                    NameValueTable(entries: new[]
                    {
                        ("ISO 8601:", $"{utcDateTime:yyyy-MM-ddTHH:mm:ssZ}"),
                        ("RFC 2822:", $"{utcDateTime:ddd, dd MMM yyyy HH:mm:ss} GMT"),
                        ("Short Date:", $"{dateTime:yyyy-MM-dd}"),
                        ("Long Date:", $"{dateTime:dddd, MMMM d, yyyy}")
                    })
                };
            }

            return null;
        }

        private bool TryParseDate(string input, out DateTime result)
        {
            input = input.Trim();

            // Handle special keywords
            if (string.Equals(input, "now", StringComparison.OrdinalIgnoreCase))
            {
                result = DateTime.Now;
                return true;
            }

            if (string.Equals(input, "today", StringComparison.OrdinalIgnoreCase))
            {
                result = DateTime.Today;
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
                "yyyy-MM-dd HH:mm:ss",
                "yyyy/MM/dd HH:mm:ss",
                "yyyy-MM-dd HH:mm",
                "yyyy/MM/dd HH:mm",
                "MM/dd/yyyy",
                "MM-dd-yyyy",
                "MM/dd/yyyy HH:mm:ss",
                "MM/dd/yyyy HH:mm",
                "M/d/yyyy",
                "M-d-yyyy",
                "M/d/yyyy h:mm:ss tt",
                "M/d/yyyy h:mm tt",
                "dd/MM/yyyy",
                "dd-MM-yyyy",
                "d/M/yyyy",
                "d-M-yyyy",
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

        private string GetRelativeTime(TimeSpan span)
        {
            bool isPast = span.TotalSeconds > 0;
            TimeSpan absSpan = span.Duration();

            string result;
            if (absSpan.TotalSeconds < 60)
            {
                result = "just now";
            }
            else if (absSpan.TotalMinutes < 60)
            {
                int minutes = (int)absSpan.TotalMinutes;
                result = $"{minutes} minute{(minutes != 1 ? "s" : "")}";
            }
            else if (absSpan.TotalHours < 24)
            {
                int hours = (int)absSpan.TotalHours;
                result = $"{hours} hour{(hours != 1 ? "s" : "")}";
            }
            else if (absSpan.TotalDays < 30)
            {
                int days = (int)absSpan.TotalDays;
                result = $"{days} day{(days != 1 ? "s" : "")}";
            }
            else if (absSpan.TotalDays < 365)
            {
                int months = (int)(absSpan.TotalDays / 30.44);
                result = $"{months} month{(months != 1 ? "s" : "")}";
            }
            else
            {
                int years = (int)(absSpan.TotalDays / 365.25);
                result = $"{years} year{(years != 1 ? "s" : "")}";
            }

            if (result == "just now")
                return result;

            return isPast ? $"{result} ago" : $"in {result}";
        }
    }
}
