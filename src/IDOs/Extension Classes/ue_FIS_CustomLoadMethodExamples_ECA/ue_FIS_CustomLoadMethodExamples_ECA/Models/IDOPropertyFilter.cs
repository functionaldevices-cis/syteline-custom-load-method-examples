using Infor.DocumentManagement.ICP;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ue_FIS_CustomLoadMethodExamples_ECA.Models
{
    public class IDOPropertyFilter<T>
    {

        public string PropertyName { get; set; }

        public string FilterString { get; set; }

        public string OperatorString { get; set; }

        public int ValueInt { get; set; }

        public decimal ValueDecimal { get; set; }

        public string ValueString { get; set; }

        public DateTime ValueDateTime { get; set; }

        public IDOPropertyFilter(string filterString)
        {
            this.FilterString = this.FixParenthesis(filterString);
            this.PropertyName = this.ExtractPropertyName(this.FilterString);

            // VALIDATE VALUE TYPE AND PROPERTY TYPE

            string value = this.ExtractValue(this.FilterString);

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
            else if (typeof(T) == typeof(string))
            {
                this.ValueString = value;
            }

            this.OperatorString = this.ExtractOperator(this.FilterString);
        }

        private string FixParenthesis(string input)
        {

            int counter;
            int openCount = input.Count(f => f == '(');
            int closeCount = input.Count(f => f == ')');

            if (openCount > closeCount)
            {
                for (counter = 0; counter < openCount - closeCount; counter++)
                {
                    input = input + ')';
                }
            }
            else if (closeCount > openCount)
            {
                for (counter = 0; counter < closeCount - openCount; counter++)
                {
                    input = '(' + input;
                }
            }

            return input;

        }

        private string ExtractPropertyName(string sInput)
        {

            return sInput.Replace("(", "").Replace(")", "").Replace(this.ExtractOperator(sInput), "").Replace(this.ExtractValue(sInput), "").Replace("'", "").Replace("dbo.MidnightOfdateaddday, 1,", "").Replace("cast as datetime", "").Trim();

        }

        private string ExtractOperator(string sInput)
        {
            List<string> operators = new List<string>() {
                ">=",
                "<=",
                "=",
                "<",
                ">",
                " NOT LIKE ",
                " not like ",
                " LIKE ",
                " like ",
                "!=",
                "<>"
            };
            foreach (string op in operators)
            {
                if (sInput.Contains(op))
                {
                    return op.Trim();
                }
            }
            return "=";
        }

        private string ExtractValue(string sInput)
        {

            if (sInput.Contains('\''))
            {

                int iStart = sInput.IndexOf('\'') + 1;
                int iEnd = sInput.LastIndexOf('\'');

                return sInput.Substring(iStart, iEnd - iStart);

            }
            else
            {
                string op = this.ExtractOperator(sInput);
                sInput = sInput.Replace(")", "").Replace("(", "").Replace(" ", "");
                string[] parts = sInput.Split(new string[] { op }, StringSplitOptions.None);
                if (parts.Count() == 2)
                {
                    return parts[1];
                }
            }
            return "";

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
