using Mongoose.IDO;
using Mongoose.IDO.DataAccess;
using Mongoose.IDO.Metadata;
using Mongoose.IDO.Protocol;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ue_FIS_CustomLoadMethodExamples_ECA.Models;

namespace ue_FIS_CustomLoadMethodExamples_ECA.Helpers
{

    public class ue_FDI_Utilities {

        public IIDOCommands IDOCommands { get; set; }
        public int BGTaskNum { get; set; }
        public int DebugLevel { get; set; }

        public ue_FDI_Utilities(IIDOCommands commands, int bGTaskNum = 0, int debugLevel = 0)
        {
            this.IDOCommands = commands;
            this.BGTaskNum = bGTaskNum;
            this.DebugLevel = debugLevel;
        }

        public string ParseFilterArgument(string arg, string defaultVal)
        {
            if (string.IsNullOrEmpty(arg))
            {
                return defaultVal;
            }
            else
            {
                return arg.Replace("*", "%");
            }
        }

        public string ParseEffectDate(string sRawDate)
        {
            DateTime? parsedDateTime = this.TryToParseDateTime(sRawDate);
            if (parsedDateTime != null)
            {
                return (parsedDateTime ?? new DateTime()).ToString("MM/dd/yyyy");
            }
            return "";
        }

        public DateTime? TryToParseDateTime(string input)
        {

            DateTime parsedDateTime;

            if (DateTime.TryParseExact(input, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime))
            {
                return parsedDateTime;
            }

            if (DateTime.TryParseExact(input, "yyyyMMdd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime))
            {
                return parsedDateTime;
            }

            if (DateTime.TryParseExact(input, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime))
            {
                return parsedDateTime;
            }

            if (DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime))
            {
                return parsedDateTime;
            }

            return null;

        }

        public string ExtractOperator(string sInput)
        {
            List<string> operators = new List<string>() {
                ">=",
                "<=",
                "=",
                "<",
                ">",
                "LIKE",
                "NOT LIKE",
                "!=",
                "<>"
            };
            foreach (string op in operators)
            {
                if (sInput.Contains(op))
                {
                    return op;
                }
            }
            return "=";
        }

        public string ExtractValue(string sInput)
        {

            if (sInput.Contains('\''))
            {

                int iStart = sInput.IndexOf('\'') + 1;
                int iEnd = sInput.LastIndexOf('\'');

                return sInput.Substring(iStart, iEnd - iStart);

            }
            else
            {
                string op = ExtractOperator(sInput);
                sInput = sInput.Replace(")", "").Replace("(", "").Replace(" ", "");
                string[] parts = sInput.Split(new string[] { op }, StringSplitOptions.None);
                if (parts.Count() == 2)
                {
                    return parts[1];
                }
            }
            return "";

        }

        public string GetFiltersContaining(string sKeyword, List<string> lFilters)
        {
            return string.Join(" AND ", lFilters.Where(f => f.Contains(sKeyword)));
        }

        public string AppendFilter(string sOldFilter, string sNewFilter)
        {
            return string.IsNullOrWhiteSpace(sOldFilter) ? sNewFilter : $"{sOldFilter} AND {sNewFilter}";
        }

        public string FormatFilterCondition(string sRawFilter)
        {
            return "( " + sRawFilter
                .Replace(" <> N", " <> ")
                .Replace(" = N", " = ")
                .Replace(" like N", " like ")
                .Replace(" DATEPART( yyyy, ", " YEAR( ")
                .Replace(" DATEPART( mm, ", " MONTH( ")
                .Replace(" DATEPART( dd, ", " DAY( ")
                + " )";
        }

        public string BuildFilterString(List<string> filters, string joiner = "AND")
        {

            string joinedFilter = string.Join(" " + joiner + " ", filters.Where(filter => filter != "").Select(filter => "(" + filter + ")"));

            return joinedFilter != "" ? $"(" + joinedFilter + ")" : "";

        }

        public void WriteLogMessage(string sMessage, int iMinDebugLevel = 0) {

            if ((DebugLevel >= iMinDebugLevel) && (BGTaskNum > 0)) {

                this.IDOCommands?.Invoke(new InvokeRequestData {
                    IDOName = "ProcessErrorLogs",
                    MethodName = "AddProcessErrorLog",
                    Parameters = new InvokeParameterList() {
                        BGTaskNum,
                        sMessage,
                        0
                    }
                });

            }

        }

        public static int ConvertToInt(string sInput) {

            decimal dInput;

            if (string.IsNullOrEmpty(sInput)) {
                sInput = "0";
            }

            dInput = decimal.Parse(sInput);
            dInput = Math.Round(dInput, 2, MidpointRounding.AwayFromZero);

            return Convert.ToInt32(dInput);

        }

        public string FixParenthesis(string input)
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

        public DateTime AddBusinessDays(DateTime date, int days)
        {

            int addedDays = 0;
            if (days <= 0)
            {
                return date;
            }

            if (date.DayOfWeek == DayOfWeek.Saturday)
            {
                date = date.AddDays(2);
            }
            else if (date.DayOfWeek == DayOfWeek.Sunday)
            {
                date = date.AddDays(1);
            }

            while (addedDays < days)
            {
                date = date.AddDays(1); // Move to the next day

                // Check if the new day is a weekday (Monday to Friday)
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    addedDays++; // Only count it if it's a weekday
                }
            }
            return date;

        }

        public int GetBusinessDaysDiff(DateTime startDate, DateTime endDate)
        {
            int days = 0;
            if (startDate < endDate)
            {
                while (startDate < endDate)
                {
                    if (startDate.DayOfWeek != DayOfWeek.Saturday && startDate.DayOfWeek != DayOfWeek.Sunday)
                    {
                        days++;
                    }
                    startDate = startDate.AddDays(1);
                }
            }
            return days;
        }

        public static List<T> ExtractPropertiesAsList<T>(LoadCollectionResponseData oLdResponse, string propertyName) {

            int index = oLdResponse.PropertyList.IndexOf(propertyName);
            if (index > -1) {
                return oLdResponse.Items.Select(record => record.PropertyValues[index].GetValue<T>()).ToList();
            } else {
                return new List<T>();
            }

        }

        public List<Dictionary<string, T>> UnpackRecords<T>(LoadRecordsResponseData records)
        {

            return records.LoadCollectionResponseData.Items.Select(
                record => records.PropertyKeys.ToDictionary(
                    propertyName => propertyName.Key,
                    propertyName => this.ParseIDOPropertyValue<T>(record.PropertyValues[propertyName.Value])
                )
            ).ToList();

        }

        public T ParseIDOPropertyValue<T>(IDOPropertyValue value)
        {

            if (!value.IsNull)
            {
                return value.GetValue<T>();
            }

            return default;

        }


        public LoadRecordsResponseData LoadRecords(string IDOName, string filter, string orderBy, List<string> properties, int recordCap = 0)
        {

            // GENERIC SYSTEM PROPS

            LoadCollectionRequestData oLoadRequest;
            LoadCollectionResponseData oLoadResponse = new LoadCollectionResponseData();

            // SET UP DATA LOAD REQUEST PARAMETERS

            oLoadRequest = new LoadCollectionRequestData()
            {
                IDOName = IDOName,
                RecordCap = recordCap,
                Filter = filter,
                OrderBy = orderBy,
                ReadMode = ReadMode.ReadCommitted
            };
            oLoadRequest.PropertyList.SetProperties(string.Join(", ", properties));

            // LOAD THE RECORD(S)

            oLoadResponse = this.IDOCommands.LoadCollection(oLoadRequest);

            // IF WE HAVE A VALID RECORD

            return new LoadRecordsResponseData(
                queryIDOName: IDOName,
                queryFilter: filter,
                queryOrderBy: orderBy,
                queryProperties: properties,
                loadCollectionResponseData: oLoadResponse,
                loadCollectionRequestData: oLoadRequest
            );

        }

        public LoadRecordsResponseData RefreshRequest(LoadRecordsResponseData loadRecordsResponseData)
        {

            // REFRESH THE RECORD(S)

            loadRecordsResponseData.LoadCollectionResponseData = this.IDOCommands.LoadCollection(loadRecordsResponseData.LoadCollectionRequestData);

            return loadRecordsResponseData;

        }

        public int CreateRecord(string IDOName, IDOUpdateItem record) {

            if (this.IDOCommands != null) {


                // CREATE THE UPDATE REQUEST WRAPPER

                UpdateCollectionRequestData oUpdateRequest = new UpdateCollectionRequestData(IDOName);
                oUpdateRequest.Items.Add(record);

                // SEND THE UPDATE REQUEST

                this.IDOCommands.UpdateCollection(oUpdateRequest);

                return 1;

            }

            return 0;

        }

        public int CreateRecords(string IDOName, List<IDOUpdateItem> records)
        {

            if (this.IDOCommands != null)
            {

                // CREATE THE UPDATE REQUEST WRAPPER

                UpdateCollectionRequestData oUpdateRequest = new UpdateCollectionRequestData(IDOName);
                oUpdateRequest.Items.AddRange(records);

                // SEND THE UPDATE REQUEST

                this.IDOCommands.UpdateCollection(oUpdateRequest);

                return 1;

            }

            return 0;

        }

        public int UpdateRecord(string IDOName, IDOUpdateItem record)
        {

            if (this.IDOCommands != null)
            {

                // GENERIC SYSTEM PROPS

                UpdateCollectionRequestData oUpdateRequest = new UpdateCollectionRequestData(IDOName);
                oUpdateRequest.Items.Add(record);

                this.IDOCommands.UpdateCollection(oUpdateRequest);

                return 1;

            }

            return 0;

        }

        public int UpdateRecords(string IDOName, List<IDOUpdateItem> records) {

            if (this.IDOCommands != null) {

                // GENERIC SYSTEM PROPS

                UpdateCollectionRequestData oUpdateRequest = new UpdateCollectionRequestData(IDOName);
                oUpdateRequest.Items.AddRange(records);

                this.IDOCommands.UpdateCollection(oUpdateRequest);

                return 1;

            }

            return 0;

        }

        public IDOUpdateItem BuildInsertItem(Dictionary<string, object> propertyUpdates)
        {

            // CREATE AN UPDATE ITEM OBJECT

            IDOUpdateItem oUpdateItem = new IDOUpdateItem(UpdateAction.Insert);

            foreach (KeyValuePair<string, object> propertyUpdate in propertyUpdates)
            {

                // IF WE HAVE A VALUE IN THE VALUE PROP, USE THAT. OTHERWISE, USE WHAT IS IN THE LOAD COLLECTION.

                if (propertyUpdate.Value != null)
                {

                    oUpdateItem.Properties.Add(propertyUpdate.Key, propertyUpdate.Value, true);

                }

            }

            return oUpdateItem;

        }

        public IDOUpdateItem BuildUpdateItem(string itemID = null, List<IDOUpdateProperty> propertyUpdates = null)
        {

            IDOUpdateItem oUpdateItem = itemID != null ? new IDOUpdateItem(UpdateAction.Update, itemID)
            {
                ItemID = itemID
            } : new IDOUpdateItem(UpdateAction.Update)
            {
                UseOptimisticLocking = false
            };

            this.WriteLogMessage(
                itemID,
                2
            );

            if (propertyUpdates != null)
            {

                foreach (IDOUpdateProperty propertyUpdate in propertyUpdates)
                {

                    // IF WE HAVE A VALUE IN THE VALUE PROP, USE THAT. OTHERWISE, USE WHAT IS IN THE LOAD COLLECTION.

                    if (propertyUpdate.Value != null)
                    {



                        this.WriteLogMessage(
                            propertyUpdate.Name + "|"+ propertyUpdate.Value + "|true",
                            2
                        );

                        oUpdateItem.Properties.Add(propertyUpdate.Name, propertyUpdate.Value, propertyUpdate.Modified);

                    }

                }

            }

            return oUpdateItem;

        }

    }

    public class ComparerNumericStrings : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            var regex = new Regex("^(d+)");

            // run the regex on both strings
            var xRegexResult = regex.Match(x);
            var yRegexResult = regex.Match(y);

            // check if they are both numbers
            if (xRegexResult.Success && yRegexResult.Success)
            {
                return int.Parse(xRegexResult.Groups[1].Value).CompareTo(int.Parse(yRegexResult.Groups[1].Value));
            }

            // otherwise return as string comparison
            return x.CompareTo(y);
        }
    }

}