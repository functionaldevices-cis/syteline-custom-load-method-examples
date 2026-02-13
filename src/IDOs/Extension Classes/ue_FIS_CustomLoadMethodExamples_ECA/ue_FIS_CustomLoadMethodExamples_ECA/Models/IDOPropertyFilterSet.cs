using Mongoose.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ue_FIS_CustomLoadMethodExamples_ECA.Models
{

    public interface IIDOPropertyFilterSet
    {
        bool ValueFails(object value);
        void AddFilter(string filterString);

        string GetFilterString();

    }

    public class IDOPropertyFilterSet<T> : IIDOPropertyFilterSet
    {

        public List<IDOPropertyFilter<T>> Filters = new List<IDOPropertyFilter<T>>();

        private bool HasChanged { get; set; } = false;

        public IDOPropertyFilterSet(string defaultFilter = null)
        {
            if (defaultFilter != null)
            {
                this.Filters.Add(new IDOPropertyFilter<T>(defaultFilter));
            }
        }

        public string GetFilterString()
        {
            string joinedFilter = string.Join(" AND ", this.Filters.Where(filter => filter.FilterString != "").Select(filter => "(" + filter.FilterString + ")"));
            return joinedFilter != "" ? $"(" + joinedFilter + ")" : "";
        }

        public void AddFilter(string filterString)
        {

            if (!this.HasChanged && this.Filters.Count > 0)
            {
                this.Filters[0] = new IDOPropertyFilter<T>(filterString);
            }
            else
            {
                this.Filters.Add(new IDOPropertyFilter<T>(filterString));
            }
            this.HasChanged = true;

        }

        bool IIDOPropertyFilterSet.ValueFails(object value)
        {
            // Cast the object to T and call the strongly-typed version
            if (value is T typedValue)
            {
                return this.ValueFails(typedValue);
            }

            // Handle cases where the wrong type is passed (optional)
            return true;
        }

        public bool ValueFails(T value)
        {
            return !this.ValuePasses(value);
        }

        public bool ValuePasses(T value)
        {
            if (typeof(T) == typeof(DateTime))
            {
                return this.ValuePasses_DateTime((DateTime)Convert.ChangeType(value, typeof(DateTime)));
            }
            else if (typeof(T) == typeof(decimal))
            {
                return this.ValuePasses_Decimal((decimal)Convert.ChangeType(value, typeof(decimal)));
            }
            else if (typeof(T) == typeof(int))
            {
                return this.ValuePasses_Int((int)Convert.ChangeType(value, typeof(int)));
            }
            else if (typeof(T) == typeof(string))
            {
                return this.ValuePasses_String((string)Convert.ChangeType(value, typeof(string)));
            }
            return true;
        }

        private bool ValuePasses_DateTime(DateTime value)
        {

            bool addRowToOutputTable = true;

            this.Filters.ForEach(filter =>
            {
                switch (filter.OperatorString)
                {
                    case ">=":
                        if (value < filter.ValueDateTime)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case ">":
                        if (value <= filter.ValueDateTime)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "<=":
                        if (value > filter.ValueDateTime)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "<":
                        if (value >= filter.ValueDateTime)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "=":
                        if (value != filter.ValueDateTime)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "!=":
                    case "<>":
                        if (value == filter.ValueDateTime)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                }
            });

            return addRowToOutputTable;

        }

        private bool ValuePasses_Decimal(decimal value)
        {

            bool addRowToOutputTable = true;

            this.Filters.ForEach(filter =>
            {
                switch (filter.OperatorString)
                {
                    case ">=":
                        if (value < filter.ValueDecimal)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case ">":
                        if (value <= filter.ValueDecimal)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "<=":
                        if (value > filter.ValueDecimal)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "<":
                        if (value >= filter.ValueDecimal)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "=":
                        if (value != filter.ValueDecimal)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "!=":
                    case "<>":
                        if (value == filter.ValueDecimal)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                }
            });

            return addRowToOutputTable;

        }

        private bool ValuePasses_Int(int value)
        {

            bool addRowToOutputTable = true;

            this.Filters.ForEach(filter =>
            {
                switch (filter.OperatorString)
                {
                    case ">=":
                        if (value < filter.ValueInt)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case ">":
                        if (value <= filter.ValueInt)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "<=":
                        if (value > filter.ValueInt)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "<":
                        if (value >= filter.ValueInt)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "=":
                        if (value != filter.ValueInt)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "!=":
                    case "<>":
                        if (value == filter.ValueInt)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                }
            });

            return addRowToOutputTable;

        }

        private bool ValuePasses_String(string value)
        {

            string regexPattern;
            bool addRowToOutputTable = true;

            this.Filters.ForEach(filter =>
            {
                switch (filter.OperatorString)
                {
                    case "=":
                        // 'rib'
                        if (!value.Equals(filter.ValueString, StringComparison.OrdinalIgnoreCase))
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "!=":
                    case "<>":
                        // '<>rib'
                        if (value.Equals(filter.ValueString, StringComparison.OrdinalIgnoreCase))
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "LIKE":
                    case "like":
                        // '<>ri%b'
                        regexPattern = "^" + Regex.Escape(filter.ValueString).Replace("%", ".*").Replace("_", ".") + "$";
                        if (!Regex.IsMatch(value, regexPattern, RegexOptions.IgnoreCase))
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "NOT LIKE":
                    case "not like":
                        // '<>ri%b'
                        regexPattern = "^" + Regex.Escape(filter.ValueString).Replace("%", ".*").Replace("_", ".") + "$";
                        if (Regex.IsMatch(value, regexPattern, RegexOptions.IgnoreCase))
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                }
            });

            return addRowToOutputTable;

        }

    }
}
