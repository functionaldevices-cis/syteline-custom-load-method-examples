using System;
using System.Linq;
using Microsoft.VisualBasic;
using Mongoose.IDO.Protocol;
using Mongoose.Scripting;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Linq.Expressions;

namespace Mongoose.GlobalScripts
{

    public class ue_FIS_GenerateFilterString : GlobalScript
    {

        /**********************************************************************************************************/
        /**********************************************************************************************************/
        /*
        /* Name:     ue_FIS_GenerateFilterString
        /* Date:     2025-11-18
        /* Authors:  Andy Mercer
        /* Purpose:  This script will build an SQL filter string that can be sent to Custom Load Methods using the
        /*           values which the user has entered while in FilterInPlace mode on a form. It should be triggered
        /*           from the StdFormFilterInPlaceExecute event. For an example, look at the Resources form, which
        /*           uses the Vendor script GenerateFilterStringXML, which is what this script is based on. The
        /*           difference between this and the vendor XML version is that this script will give the exact same
        /*           SQL string that you'd get from ThisForm.PrimaryIDOCollection.Filter.
        /*
        /* Copyright 2025, Functional Devices, Inc
        /*
        /**********************************************************************************************************/
        /**********************************************************************************************************/

        public void Main()
        {

            try
            {

                // RETRIEVE AND VALIDATE THE PARAMETERS

                string variableName = this.GetParameterWithDefault(0, "vFilterString");

                // LOOP THROUGH ALL PROPERTIES AND GENERATE THE SQL STRING

                string SQL = this.GenerateFromCollection();

                // INSERT THE SQL STRING INTO THE FORM VARIABLE

                this.ThisForm.Variables(variableName).SetValue(SQL);

                // SET RETURN VALUE TO 0 FOR SUCCESS

                this.ReturnValue = "0";

            }
            catch (Exception ex)
            {

                // SHOW ERROR

                this.Application.ShowMessage("FDI_GenerateFilterStringSQL Error: " + ex.Message);
                this.ReturnValue = "1";

            }

        }

        public string GenerateFromCollection()
        {

            // SET UP VARIABLES

            IWSIDOCollection currentCollection = this.ThisForm.CurrentIDOCollection;
            IDictionary<string, IWSFormComponent> components = this.ThisForm.Components;
            List<Filter> filters = new List<Filter>();
            Filter filter = new Filter();
            string propertyName;
            string propertyValue;
            int counter;

            // LOOP THROUGH THE PROPERTIES ON THE PRIMARY IDO COLLECTION

            for (counter = 0; counter <= currentCollection.GetNumProperties() - 1; counter++)
            {

                // GET THE CURRENT PROPERTY NAME AND VALUE

                propertyName = currentCollection.GetPropertyName(counter);
                propertyValue = currentCollection.CurrentItem[propertyName].Value.Trim();

                // IF THE PROPERTY HASN'T BEEN INTERACTED WITH BY THE USER OR ISN'T EVEN ON THE FORM, THEN SKIP TO NEXT PROPERTY

                if (propertyValue.Length > 0 && currentCollection.GetComponentsBoundToProperty(propertyName).Count > 0)
                {

                    // PASS IN THE PROPERTY NAME, VALUE, AND TYPE TO THE FILTER CLASS, WHICH WILL CONVERT IT TO THE 

                    filter = new Filter(
                        propertyName: propertyName,
                        propertyValue: propertyValue.Replace(this.Application.WildCardCharacter, "%"),
                        propertyType: components[currentCollection.GetComponentsBoundToProperty(propertyName).First()].DataType.ToLower()
                    );

                    // BUILD THE FILTER STRING(S) AND ADD TO LIST

                    filters.Add(filter);

                }
            }

            // JOIN THE FILTERS LIST AND RETURN

            return string.Join(" AND ", filters.SelectMany(f => f.FilterStrings));

        }

        public string GetParameterWithDefault(int index, string defaultValue = "")
        {

            string parameterValue = defaultValue;
            string rawParameterValue;

            // VALIDATE THE PARAMETER COUNT

            if (this.ParameterCount() > index)
            {

                rawParameterValue = this.GetParameter(0);

                // VALIDATE THAT THE VALUE ISN'T NULL OR EMPTY

                if (rawParameterValue != null && rawParameterValue.Length > 0)
                {
                    parameterValue = rawParameterValue;
                }

            }

            return parameterValue;

        }

    }
    public class Filter {

        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
        public string OperatorString { get; set; }
        public string MatchMethod { get; set; }
        public string PropertyType { get; set; }

        public List<string> FilterStrings {
            get {

                List<string> filters = new List<string>();

                switch (this.PropertyType) {

                    // decimal | char | date | i1 | i2

                    case "date":

                        if (this.MatchMethod == "fuzzy") {

                            // TRY TO PARSE OUT THE DATE INTO THE MONTH, DAY, AND YEAR PARTS

                            Dictionary<string, string> datePartPair = this.ParseDateWithWildcard(this.PropertyValue);

                            // IF WE HAVE ALL THREE PARTS

                            if (datePartPair.ContainsKey("DATE.YEAR") & datePartPair.ContainsKey("DATE.MONTH") & datePartPair.ContainsKey("DATE.DAY")) {

                                // IF THE MONTH IS NOT A WILDCARD, THEN APPEND IT TO THE FILTER USING THE "DATEPART(mm, PROPERTYNAME)" SYNTAX

                                if (datePartPair["DATE.MONTH"] != "%") {

                                    filters.Add(" ( " + "DATEPART( mm, " + this.PropertyName + " ) " + this.OperatorString + " N'" + datePartPair["DATE.MONTH"] + "' ) ");

                                }

                                // IF THE DAY IS NOT A WILDCARD, THEN APPEND IT TO THE FILTER USING THE "DATEPART(dd, PROPERTYNAME)" SYNTAX

                                if (datePartPair["DATE.DAY"] != "%") {

                                    filters.Add(" ( " + "DATEPART( dd, " + this.PropertyName + " ) " + this.OperatorString + " N'" + datePartPair["DATE.DAY"] + "' ) ");

                                }

                                // IF THE YEAR IS NOT A WILDCARD, THEN APPEND IT TO THE FILTER USING THE "DATEPART(yyyy, PROPERTYNAME)" SYNTAX

                                if (datePartPair["DATE.YEAR"] != "%") {

                                    filters.Add(" ( " + "DATEPART( yyyy, " + this.PropertyName + " ) " + this.OperatorString + " N'" + datePartPair["DATE.YEAR"] + "' ) ");

                                }

                            }

                        } else {

                            switch (this.OperatorString) {

                                case ">=":
                                    if (this.PropertyValue.Length < 10) {
                                       this.PropertyValue = DateTime.Parse(this.PropertyValue).ToString("yyyyMMdd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                                    }
                                    filters.Add(" ( " + this.PropertyName + " " + this.OperatorString + " '" + this.PropertyValue + "' ) ");
                                    break;

                                case "<=":
                                    if (this.PropertyValue.Length < 10) {
                                       this.PropertyValue = DateTime.Parse(this.PropertyValue).AddDays(1).AddMilliseconds(-3).ToString("yyyyMMdd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                                    }
                                    filters.Add(" ( " + this.PropertyName + " " + this.OperatorString + " '" + this.PropertyValue + "' ) ");
                                    break;

                                case ">":
                                    if (this.PropertyValue.Length < 10) {
                                       this.PropertyValue = DateTime.Parse(this.PropertyValue).AddDays(1).AddMilliseconds(-3).ToString("yyyyMMdd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                                    }
                                    filters.Add(" ( " + this.PropertyName + " " + this.OperatorString + " '" + this.PropertyValue + "' ) ");
                                    break;

                                case "<":
                                    if (this.PropertyValue.Length < 10) {
                                       this.PropertyValue = DateTime.Parse(this.PropertyValue).ToString("yyyyMMdd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                                    }
                                    filters.Add(" ( " + this.PropertyName + " " + this.OperatorString + " '" + this.PropertyValue + "' ) ");
                                    break;

                                case "<>":
                                    this.PropertyValue = DateTime.Parse(this.PropertyValue).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                                    filters.Add(
                                        " ( " + this.PropertyName + " < CAST('" + this.PropertyValue + "' as DateTime)"
                                        + " OR " 
                                        + this.PropertyName + " > DATEADD(ms, -3, dbo.MidnightOf(DATEADD(day, 1, CAST('" + this.PropertyValue  + "' as DateTime)))) ) "
                                    );
                                    break;

                                default:
                                    this.PropertyValue = DateTime.Parse(this.PropertyValue).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                                    filters.Add(
                                        " ( " + this.PropertyName + " >= CAST('" + this.PropertyValue + "' as DateTime)"
                                        + " AND "
                                        + this.PropertyName + " < dbo.MidnightOf(DATEADD(DAY, 1, CAST('" + this.PropertyValue  + "' as DateTime))) ) "
                                    );
                                    break;

                            }

                        }

                        break;

                    case "numsortchar":
                    case "char":

                        filters.Add(" ( " + this.PropertyName + " " + ( this.MatchMethod == "fuzzy" ? this.OperatorString.Replace("=", "like").Replace("<>", "not like") : this.OperatorString ) + " N'" + this.PropertyValue + "' ) ");
                        break;

                    case "guid":

                        string temp = this.PropertyName + " " + ( this.MatchMethod == "fuzzy" ? this.OperatorString.Replace("=", "like").Replace("<>", "not like") : this.OperatorString );

                        filters.Add(" ( " + temp + " '{" + this.PropertyValue + "}' ) ");
                        break;

                    default:

                        filters.Add(" ( " + this.PropertyName + " " + ( this.MatchMethod == "fuzzy" ? this.OperatorString.Replace("=", "like").Replace("<>", "not like") : this.OperatorString ) + " " + this.PropertyValue + " ) ");
                        break;

                }


                return filters;

            }
        }

        public Filter(string propertyName = "", string propertyValue = "", string propertyType = "") {

            this.PropertyName = propertyName;
            this.PropertyValue = propertyValue;

            // DETERMINE THE OPERATOR

            if (this.PropertyValue.StartsWith("<>")) {
                this.OperatorString = "<>";
                this.PropertyValue = this.PropertyValue.Remove(0, 2);
            } else if (this.PropertyValue.StartsWith(">=")) {
                this.OperatorString = ">=";
                this.PropertyValue = this.PropertyValue.Remove(0, 2);
            } else if (this.PropertyValue.StartsWith("<=")) {
                this.OperatorString = "<=";
                this.PropertyValue = this.PropertyValue.Remove(0, 2);
            } else if (this.PropertyValue.StartsWith(">")) {
                this.OperatorString = ">";
                this.PropertyValue = this.PropertyValue.Remove(0, 1);
            } else if (this.PropertyValue.StartsWith("<")) {
                this.OperatorString = "<";
                this.PropertyValue = this.PropertyValue.Remove(0, 1);
            } else {
                this.OperatorString = "=";
            }

            this.MatchMethod = this.PropertyValue.Contains("%") ? "fuzzy" : "exact";
            this.PropertyType = this.PropertyValue == "null" ? "null" : propertyType;

        }

        private Dictionary<string, string> ParseDateWithWildcard(string filterDate) {

            // THIS PARSES THE DATE INTO A DICTIONARY OF PARTS. AS AN EXAMPLE, 9/1/2023 WOULD BECOME
            // {
            //     "DATE.YEAR" : "2023"
            //     "DATE.MONTH" : "9"
            //     "DATE.DAY" : "1"
            // }

            string[] dateFormatParts = Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortDatePattern.Split(new char[] { '/', ' ', '-' });
            string[] dateValueParts = filterDate.Split(new char[] { '/', ' ', '-' });

            Dictionary<string, string> parsedDateParts = new Dictionary<string, string>();

            if (dateFormatParts.Length >= 3 & dateValueParts.Length >= 3) {
                for (int i = 0; i <= 2; i++) {
                    if (dateFormatParts[i].ToLower().Contains("y")) {
                        parsedDateParts.Add("DATE.YEAR", dateValueParts[i]);
                    } else if (dateFormatParts[i].ToLower().Contains("m")) {
                        parsedDateParts.Add("DATE.MONTH", dateValueParts[i]);
                    } else if (dateFormatParts[i].ToLower().Contains("d")) {
                        parsedDateParts.Add("DATE.DAY", dateValueParts[i]);
                    }
                }
            }

            return parsedDateParts;

        }

    }

}