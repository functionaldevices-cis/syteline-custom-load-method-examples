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
                int parsedValueInt = StringConverter.TryToParseIntString(value);
                if (parsedValueInt != -1)
                {
                    this.ValueInt = parsedValueInt;
                }
            }
            else if (typeof(T) == typeof(DateTime))
            {
                (DateTime parsedValueDateTime, int precision) = StringConverter.TryToParseDateTimeString(value);
                if (parsedValueDateTime != DateTime.MinValue)
                {
                    this.ValueDateTime = parsedValueDateTime;
                }
            }
            else if (typeof(T) == typeof(decimal))
            {
                decimal? parsedValueDecimal = StringConverter.TryToParseDecimalString(value);
                if (parsedValueDecimal != null)
                {
                    this.ValueDecimal = (decimal)parsedValueDecimal;
                }
            }

        }

    }

}
