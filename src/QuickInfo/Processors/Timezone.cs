using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static QuickInfo.NodeFactory;

namespace QuickInfo
{
    public class Timezone : IProcessor
    {
        // Common timezone abbreviations to offset mapping (standard time)
        private static readonly Dictionary<string, int> TimezoneOffsets = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            // US timezones
            { "EST", -5 }, { "Eastern", -5 },
            { "EDT", -4 },
            { "CST", -6 }, { "Central", -6 },
            { "CDT", -5 },
            { "MST", -7 }, { "Mountain", -7 },
            { "MDT", -6 },
            { "PST", -8 }, { "Pacific", -8 },
            { "PDT", -7 },
            { "AKST", -9 }, { "Alaska", -9 },
            { "AKDT", -8 },
            { "HST", -10 }, { "Hawaii", -10 },

            // Universal
            { "UTC", 0 }, { "GMT", 0 }, { "Z", 0 },

            // Europe
            { "BST", 1 },  // British Summer Time
            { "CET", 1 },  // Central European Time
            { "CEST", 2 }, // Central European Summer Time
            { "EET", 2 },  // Eastern European Time
            { "EEST", 3 }, // Eastern European Summer Time
            { "WET", 0 },  // Western European Time
            { "WEST", 1 }, // Western European Summer Time

            // Asia
            { "IST", 5 },   // India Standard Time (actually +5:30, simplified)
            { "JST", 9 },   // Japan Standard Time
            { "KST", 9 },   // Korea Standard Time
            { "CST China", 8 }, // China Standard Time
            { "HKT", 8 },   // Hong Kong Time

            // Australia
            { "AEST", 10 }, // Australian Eastern Standard Time
            { "AEDT", 11 }, // Australian Eastern Daylight Time
            { "ACST", 9 },  // Australian Central Standard Time (actually +9:30)
            { "ACDT", 10 }, // Australian Central Daylight Time (actually +10:30)
            { "AWST", 8 },  // Australian Western Standard Time

            // Others
            { "NST", -3 },  // Newfoundland Standard Time (actually -3:30)
            { "NDT", -2 }   // Newfoundland Daylight Time (actually -2:30)
        };

        public object GetResult(Query query)
        {
            if (query.IsHelp)
            {
                return HelpTable(
                    ("timezone", "Show timezone information"),
                    ("3pm EST to PST", "Convert time between timezones"),
                    ("15:00 UTC to EST", "Convert UTC time to EST"),
                    ("now in Tokyo", "Show current time in a timezone"),
                    ("timezones", "List common timezone abbreviations"));
            }

            var input = query.OriginalInput.Trim();

            // General timezone info
            if (string.Equals(input, "timezone", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "timezones", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "time zones", StringComparison.OrdinalIgnoreCase))
            {
                return new object[]
                {
                    Answer("Common Timezones"),
                    SectionHeader("US Timezones:"),
                    NameValueTable(entries: new[]
                    {
                        ("EST/EDT:", "Eastern (UTC-5/-4)"),
                        ("CST/CDT:", "Central (UTC-6/-5)"),
                        ("MST/MDT:", "Mountain (UTC-7/-6)"),
                        ("PST/PDT:", "Pacific (UTC-8/-7)"),
                        ("AKST/AKDT:", "Alaska (UTC-9/-8)"),
                        ("HST:", "Hawaii (UTC-10)")
                    }),
                    SectionHeader("International:"),
                    NameValueTable(entries: new[]
                    {
                        ("UTC/GMT:", "Universal/Greenwich (UTC+0)"),
                        ("CET/CEST:", "Central Europe (UTC+1/+2)"),
                        ("IST:", "India (UTC+5:30)"),
                        ("JST:", "Japan (UTC+9)"),
                        ("AEST/AEDT:", "Australian Eastern (UTC+10/+11)")
                    })
                };
            }

            // Pattern: "now in [timezone]" or "current time in [timezone]"
            var nowInMatch = Regex.Match(input, @"^(?:now|current\s+time)\s+in\s+(.+)$", RegexOptions.IgnoreCase);
            if (nowInMatch.Success)
            {
                string tzStr = nowInMatch.Groups[1].Value.Trim();

                if (TryParseTimezoneOffset(tzStr, out int offsetHours))
                {
                    DateTime utcNow = DateTime.UtcNow;
                    DateTime targetTime = utcNow.AddHours(offsetHours);

                    return new object[]
                    {
                        Answer($"Current time in {tzStr.ToUpper()}: {targetTime:HH:mm:ss}"),
                        SectionHeader("Time Information:"),
                        NameValueTable(entries: new[]
                        {
                            ("Timezone:", tzStr.ToUpper()),
                            ("Current Time:", $"{targetTime:HH:mm:ss}"),
                            ("Full Date/Time:", $"{targetTime:dddd, MMMM d, yyyy HH:mm:ss}"),
                            ("UTC Offset:", $"UTC{(offsetHours >= 0 ? "+" : "")}{offsetHours}"),
                            ("UTC Time:", $"{utcNow:HH:mm:ss} UTC")
                        })
                    };
                }
                else
                {
                    return FixedParagraph($"Unknown timezone: '{tzStr}'. Try: EST, PST, UTC, GMT, etc.");
                }
            }

            // Pattern: "Xam/pm TZ to TZ" or "X:XX TZ to TZ" or "HH:MM TZ to TZ"
            var timeConvertMatch = Regex.Match(input, @"^(\d{1,2})(?::(\d{2}))?\s*(am|pm)?\s+([a-z]+)\s+to\s+([a-z]+)$", RegexOptions.IgnoreCase);
            if (timeConvertMatch.Success)
            {
                int hour = int.Parse(timeConvertMatch.Groups[1].Value);
                int minute = timeConvertMatch.Groups[2].Success ? int.Parse(timeConvertMatch.Groups[2].Value) : 0;
                string ampm = timeConvertMatch.Groups[3].Success ? timeConvertMatch.Groups[3].Value.ToLower() : "";
                string fromTz = timeConvertMatch.Groups[4].Value;
                string toTz = timeConvertMatch.Groups[5].Value;

                // Handle AM/PM
                if (!string.IsNullOrEmpty(ampm))
                {
                    if (ampm == "pm" && hour != 12)
                        hour += 12;
                    else if (ampm == "am" && hour == 12)
                        hour = 0;
                }

                if (hour < 0 || hour > 23 || minute < 0 || minute > 59)
                {
                    return FixedParagraph("Invalid time. Hours must be 0-23 (or 1-12 with AM/PM), minutes 0-59");
                }

                if (!TryParseTimezoneOffset(fromTz, out int fromOffset))
                {
                    return FixedParagraph($"Unknown timezone: '{fromTz}'. Try: EST, PST, UTC, GMT, etc.");
                }

                if (!TryParseTimezoneOffset(toTz, out int toOffset))
                {
                    return FixedParagraph($"Unknown timezone: '{toTz}'. Try: EST, PST, UTC, GMT, etc.");
                }

                // Convert to UTC, then to target timezone
                int hourDifference = toOffset - fromOffset;
                int targetHour = hour + hourDifference;
                int targetMinute = minute;
                int dayAdjustment = 0;

                // Handle day overflow/underflow
                if (targetHour >= 24)
                {
                    targetHour -= 24;
                    dayAdjustment = 1;
                }
                else if (targetHour < 0)
                {
                    targetHour += 24;
                    dayAdjustment = -1;
                }

                string dayNote = dayAdjustment == 1 ? " (next day)" : (dayAdjustment == -1 ? " (previous day)" : "");
                string timeFormat12 = targetHour == 0 ? $"12:{targetMinute:D2} AM" :
                                     targetHour < 12 ? $"{targetHour}:{targetMinute:D2} AM" :
                                     targetHour == 12 ? $"12:{targetMinute:D2} PM" :
                                     $"{targetHour - 12}:{targetMinute:D2} PM";

                return new object[]
                {
                    Answer($"{hour:D2}:{minute:D2} {fromTz.ToUpper()} = {targetHour:D2}:{targetMinute:D2} {toTz.ToUpper()}{dayNote}"),
                    SectionHeader("Timezone Conversion:"),
                    NameValueTable(entries: new[]
                    {
                        ("Original Time:", $"{hour:D2}:{minute:D2} {fromTz.ToUpper()}"),
                        ("Original Offset:", $"UTC{(fromOffset >= 0 ? "+" : "")}{fromOffset}"),
                        ("Target Timezone:", toTz.ToUpper()),
                        ("Target Offset:", $"UTC{(toOffset >= 0 ? "+" : "")}{toOffset}"),
                        ("Converted Time:", $"{targetHour:D2}:{targetMinute:D2} (24-hour)"),
                        ("12-Hour Format:", timeFormat12),
                        ("Day Adjustment:", dayAdjustment == 0 ? "Same day" : (dayAdjustment == 1 ? "Next day" : "Previous day"))
                    })
                };
            }

            return null;
        }

        private bool TryParseTimezoneOffset(string tz, out int offsetHours)
        {
            tz = tz.Trim();

            // Direct lookup
            if (TimezoneOffsets.TryGetValue(tz, out offsetHours))
            {
                return true;
            }

            // Try parsing UTC+X or UTC-X format
            var utcOffsetMatch = Regex.Match(tz, @"^UTC\s*([+-]?\d{1,2})$", RegexOptions.IgnoreCase);
            if (utcOffsetMatch.Success)
            {
                offsetHours = int.Parse(utcOffsetMatch.Groups[1].Value);
                return true;
            }

            // Try parsing +X or -X format
            var offsetMatch = Regex.Match(tz, @"^([+-]?\d{1,2})$");
            if (offsetMatch.Success)
            {
                offsetHours = int.Parse(offsetMatch.Groups[1].Value);
                return true;
            }

            offsetHours = 0;
            return false;
        }
    }
}
