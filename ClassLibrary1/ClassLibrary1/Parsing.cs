using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Las2witsmlLIB
{
    class Parsing
    {
        public static double ParseDouble(string data)
        {
            return double.Parse(data, NumberStyles.Any, NumberFormatInfo.InvariantInfo);
        }

        public static DateTime ParseDateTime(string data)
        {
            var dateTimePattern = new Regex(@"[\d:\/T\-\.\s]+");
            var dateTimeSubstring = dateTimePattern.Match(data);
            if (!dateTimeSubstring.Success)
            {
                throw new ApplicationException("Либо кривая дата, либо кривой регексп");
            }

            var dateTime = DateTime.Parse(dateTimeSubstring.Value, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.RoundtripKind);
            if (data.Length > dateTimeSubstring.Length)
            {
                var timeZoneSubstring = data.Substring(dateTimeSubstring.Length).Trim(' ', '(', ')');
                var timeZone = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(x => x.DaylightName == timeZoneSubstring || x.StandardName == timeZoneSubstring);
                if (timeZone != null)
                {
                    dateTime = dateTime.Add(-timeZone.BaseUtcOffset);
                }
            }

            if (dateTime.Kind == DateTimeKind.Unspecified)
            {
                dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            }

            return dateTime.ToUniversalTime();
        }
    }
}
