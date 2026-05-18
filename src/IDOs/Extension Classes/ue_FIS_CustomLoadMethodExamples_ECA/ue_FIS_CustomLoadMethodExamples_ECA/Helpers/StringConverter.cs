using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ue_FIS_CustomLoadMethodExamples_ECA.Helpers
{
    public static class StringConverter
    {

        public static decimal? TryToParseDecimalString(string input)
        {

            decimal parsedDecimal;

            if (decimal.TryParse(input, out parsedDecimal))
            {
                return parsedDecimal;
            }

            return null;

        }

        public static int TryToParseIntString(string input)
        {

            if (input == "1")
            {
                return 1;
            }

            if (input == "0" || input == "" || input == null)
            {
                return 0;
            }

            return -1;

        }
        public static (DateTime value, int precision) TryToParseDateTimeString(string input)
        {

            DateTime parsedDateTime = DateTime.MinValue;

            if (DateTime.TryParseExact(input, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime))
            {
                return (parsedDateTime, 0);
            }

            if (DateTime.TryParseExact(input, "yyyyMMdd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime))
            {
                return (parsedDateTime, 0);
            }

            if (DateTime.TryParseExact(input, "yyyyMMdd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime))
            {
                return (parsedDateTime, 3);
            }

            if (DateTime.TryParseExact(input, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime))
            {
                return (parsedDateTime, 0);
            }

            if (DateTime.TryParseExact(input, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime))
            {
                return (parsedDateTime, 0);
            }

            if (DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime))
            {
                return (parsedDateTime, 0);
            }

            return (parsedDateTime, 0);

        }

    }

}
