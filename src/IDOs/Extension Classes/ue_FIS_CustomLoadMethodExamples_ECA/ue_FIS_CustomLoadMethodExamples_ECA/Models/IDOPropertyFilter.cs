using Infor.DocumentManagement.ICP;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ue_FIS_CustomLoadMethodExamples_ECA.Helpers;

namespace ue_FIS_CustomLoadMethodExamples_ECA.Models
{
    public class IDOPropertyFilter<T>
    {

        public string PropertyName { get; set; }

        public string FilterString { get; set; }

        public string OperatorString { get; set; }

        public string ValueString { get; set; }

        public int ValueInt { get; set; }

        public decimal ValueDecimal { get; set; }

        public DateTime ValueDateTime { get; set; }

        public IDOPropertyFilter(string filterString, string propertyName = null, string operatorName = null, string value = null)
        {

            this.FilterString = filterString;
            this.OperatorString = operatorName ?? FilterStringParser.ExtractOperator(this.FilterString);
            this.ValueString = value ?? FilterStringParser.ExtractValue(this.FilterString, this.OperatorString);
            this.PropertyName = propertyName ?? FilterStringParser.ExtractPropertyName(this.FilterString, this.OperatorString, this.ValueString);

            // VALIDATE VALUE TYPE AND PROPERTY TYPE

            if (typeof(T) == typeof(int))
            {
                int parsedValueInt = this.TryToParseIntString(value);
                if (parsedValueInt != -1)
                {
                    this.ValueInt = parsedValueInt;
                }
            }
            else if (typeof(T) == typeof(DateTime))
            {
                (DateTime parsedValueDateTime, int precision) = this.TryToParseDateTimeString(value);
                if (parsedValueDateTime != DateTime.MinValue)
                {
                    if (filterString.Contains("dateadd(day")) // THIS FIXES THE ISSUE FROM INFOR WHERE THEY PASS IN A STRING LIKE: "EffectDate < dbo.MidnightOf(dateadd(day, 1, cast('20240219' as datetime))"
                    {
                        parsedValueDateTime = parsedValueDateTime.AddDays(1);
                    }
                    this.ValueDateTime = parsedValueDateTime;
                }
            }
            else if (typeof(T) == typeof(decimal))
            {
                decimal? parsedValueDecimal = this.TryToParseDecimalString(value);
                if (parsedValueDecimal != null)
                {
                    this.ValueDecimal = (decimal)parsedValueDecimal;
                }
            }

        }

        private decimal? TryToParseDecimalString(string input)
        {

            decimal parsedDecimal;

            if (decimal.TryParse(input, out parsedDecimal))
            {
                return parsedDecimal;
            }

            return null;

        }

        private int TryToParseIntString(string input)
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

        private (DateTime value, int precision) TryToParseDateTimeString(string input)
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
