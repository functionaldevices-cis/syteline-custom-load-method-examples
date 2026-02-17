using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ue_FIS_CustomLoadMethodExamples_ECA.Helpers
{
    public static class FilterStringParser
    {

        public static string ExtractPropertyName(string filterString, string operatorName, string value)
        {
            if (operatorName != "")
            {
                filterString = filterString.Replace(operatorName, "");
            }
            if (value != "")
            {
                filterString = filterString.Replace(value, "");
            }
            return filterString.Replace("(", "").Replace(")", "").Replace("'", "").Replace("dbo.MidnightOfdateaddday, 1,", "").Replace("cast as datetime", "").Trim();
        }

        public static string ExtractOperator(string filterString)
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
                if (filterString.Contains(op))
                {
                    return op.Trim();
                }
            }
            return "=";
        }
        public static string ExtractValue(string filterString, string operatorName)
        {

            if (filterString.Contains('\''))
            {

                int iStart = filterString.IndexOf('\'') + 1;
                int iEnd = filterString.LastIndexOf('\'');

                return filterString.Substring(iStart, iEnd - iStart);

            }
            else
            {
                filterString = filterString.Replace(")", "").Replace("(", "").Replace(" ", "");
                string[] parts = filterString.Split(new string[] { operatorName }, StringSplitOptions.None);
                if (parts.Count() == 2)
                {
                    return parts[1];
                }
            }
            return "";

        }


    }
}
