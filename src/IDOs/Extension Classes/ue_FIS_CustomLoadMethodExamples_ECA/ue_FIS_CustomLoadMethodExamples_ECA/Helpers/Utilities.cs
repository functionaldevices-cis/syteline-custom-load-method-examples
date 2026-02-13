using Mongoose.IDO;
using Mongoose.IDO.DataAccess;
using Mongoose.IDO.Metadata;
using Mongoose.IDO.Protocol;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ue_FIS_CustomLoadMethodExamples_ECA.Models;

namespace ue_FIS_CustomLoadMethodExamples_ECA.Helpers
{

    public class Utilities {

        public IIDOCommands IDOCommands { get; set; }
        public int BGTaskNum { get; set; }
        public int DebugLevel { get; set; }

        public Utilities(IIDOCommands commands, int bGTaskNum = 0, int debugLevel = 0)
        {
            this.IDOCommands = commands;
            this.BGTaskNum = bGTaskNum;
            this.DebugLevel = debugLevel;
        }

        public string ReverseString(string input)
        {
            char[] charArray = input.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
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
            (DateTime parsedDateTime, int precision) = this.TryToParseDateTime(sRawDate);
            if (parsedDateTime != DateTime.MinValue)
            {
                return parsedDateTime.ToString("MM/dd/yyyy");
            }
            return "";
        }

        public string ConvertDateTimeFilterToPostFilterFormat(string filterPropertyName, string filter)
        {

            string parsedFilter = "";
            string filterValue = this.ExtractValue(filter);
            string filterOperator = this.ExtractOperator(filter);
            (DateTime parsedDateTime, int precision) = this.TryToParseDateTime(filterValue);

            if (parsedDateTime != DateTime.MinValue)
            {
                if (precision == 0 && filterOperator == "=")
                {
                    parsedFilter += filterPropertyName + " >= #" + parsedDateTime.ToString("MM/dd/yyyy HH:mm:ss.fff") + "#";
                    parsedFilter += " AND ";
                    parsedFilter += filterPropertyName + " < #" + parsedDateTime.AddSeconds(1).ToString("MM/dd/yyyy HH:mm:ss.fff") + "#";
                }
                else
                {
                    if (filter.Contains("dateadd"))
                    {
                        parsedDateTime = parsedDateTime.AddDays(1);
                    }
                    parsedFilter = filterPropertyName + " " + filterOperator + " #" + parsedDateTime.ToString("MM/dd/yyyy HH:mm:ss.fff") + "#";
                }
            }

            return parsedFilter;
        }

        public decimal? TryToParseNumberString(string input)
        {

            decimal parsedDecimal;

            if (decimal.TryParse(input, out parsedDecimal))
            {
                return parsedDecimal;
            }

            return null;

        }

        public int TryToParseCheckboxString(string input)
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

        public (DateTime value, int precision) TryToParseDateTime(string input)
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

        public string FilterExtractPropertyName(string sInput)
        {

            return sInput.Replace("(", "").Replace(")", "").Replace(this.ExtractOperator(sInput), "").Replace(this.ExtractValue(sInput), "").Replace("'", "").Replace("dbo.MidnightOfdateaddday, 1,", "").Replace("cast as datetime", "").Trim();

        }

        public string ExtractOperator(string sInput) // FOR BACKWARDS COMPAT. REMOVE EVENTUALLY
        {
            return this.FilterExtractOperator(sInput);
        }

        public string FilterExtractOperator(string sInput)
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

        public string ExtractValue(string sInput) // FOR BACKWARDS COMPAT. REMOVE EVENTUALLY
        {
            return this.FilterExtractValue(sInput);
        }

        public string FilterExtractValue(string sInput)
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

        public bool CheckToSeeIfValuePassesFilters_DateTime(List<(DateTime value, string operatorString)> filters, DateTime value)
        {

            bool addRowToOutputTable = true;

            filters.ForEach(filter =>
            {
                switch (filter.operatorString)
                {
                    case ">=":
                        if (value < filter.value)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case ">":
                        if (value <= filter.value)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "<=":
                        if (value > filter.value)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "<":
                        if (value >= filter.value)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "=":
                        if (value != filter.value)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "!=":
                    case "<>":
                        if (value == filter.value)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                }
            });

            return addRowToOutputTable;

        }

        public bool CheckToSeeIfValuePassesFilters_Decimal(List<(decimal value, string operatorString)> filters, decimal value)
        {

            bool addRowToOutputTable = true;

            filters.ForEach(filter =>
            {
                switch (filter.operatorString)
                {
                    case ">=":
                        if (value < filter.value)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case ">":
                        if (value <= filter.value)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "<=":
                        if (value > filter.value)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "<":
                        if (value >= filter.value)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "=":
                        if (value != filter.value)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "!=":
                    case "<>":
                        if (value == filter.value)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                }
            });

            return addRowToOutputTable;

        }

        public bool CheckToSeeIfValuePassesFilters_Int(List<(int value, string operatorString)> filters, int value)
        {

            bool addRowToOutputTable = true;

            filters.ForEach(filter =>
            {
                switch (filter.operatorString)
                {
                    case ">=":
                        if (value < filter.value)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case ">":
                        if (value <= filter.value)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "<=":
                        if (value > filter.value)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "<":
                        if (value >= filter.value)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "=":
                        if (value != filter.value)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "!=":
                    case "<>":
                        if (value == filter.value)
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                }
            });

            return addRowToOutputTable;

        }

        public bool CheckToSeeIfValuePassesFilters_String(List<(string value, string operatorString)> filters, string value)
        {

            string regexPattern;
            bool addRowToOutputTable = true;

            filters.ForEach(filter =>
            {
                switch (filter.operatorString)
                {
                    case "=":
                        // 'rib'
                        if (!value.Equals(filter.value, StringComparison.OrdinalIgnoreCase))
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "!=":
                    case "<>":
                        // '<>rib'
                        if (value.Equals(filter.value, StringComparison.OrdinalIgnoreCase))
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "LIKE":
                    case "like":
                        // '<>ri%b'
                        regexPattern = "^" + Regex.Escape(filter.value).Replace("%", ".*").Replace("_", ".") + "$";
                        if (!Regex.IsMatch(value, regexPattern, RegexOptions.IgnoreCase))
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                    case "NOT LIKE":
                    case "not like":
                        // '<>ri%b'
                        regexPattern = "^" + Regex.Escape(filter.value).Replace("%", ".*").Replace("_", ".") + "$";
                        if (Regex.IsMatch(value, regexPattern, RegexOptions.IgnoreCase))
                        {
                            addRowToOutputTable = false;
                        }
                        break;
                }
            });

            return addRowToOutputTable;

        }

        public bool CheckToSeeIfValuePassesFilters_Checkbox(List<(int value, string operatorString)> filters, int value)
        {

            bool addRowToOutputTable = true;

            filters.ForEach(filter =>
            {
                if ((filter.value == 1 && value == 0) || (filter.value == 0 && value == 1))
                {
                    addRowToOutputTable = false;
                }
            });

            return addRowToOutputTable;

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

        public DataTable ApplyPostFilters(DataTable fullTable, string userPostQueryFilterString)
        {

            DataTable filteredTable;

            filteredTable = fullTable.Clone();
            DataRow[] filteredRows = fullTable.Select(userPostQueryFilterString);
            foreach (DataRow row in filteredRows)
            {
                filteredTable.ImportRow(row);
            }
 
            return filteredTable;

        }

        public DataTable ApplyPaging(DataTable filteredTable, LoadRecordsRequestData userRequest)
        {

            if (filteredTable.Rows.Count > 0)
            {

                if (userRequest.RecordCap != 0 && filteredTable.Rows.Count > userRequest.RecordCap)
                {

                    filteredTable.PrimaryKey = new DataColumn[] { filteredTable.Columns[filteredTable.Columns["RowPointer"].Ordinal] }; // SET TO THE ROWPOINTER COLUMN
                    int bookmarkIndex = 0;

                    // CHECK IF THERE IS A BOOKMARK

                    if (userRequest.Bookmark == "<B/>")
                    {

                        // THERE IS NO BOOKMARK SO THIS IS THE FIRST REQUEST

                        if (filteredTable.Rows.Count > userRequest.RecordCap + 1)
                        {
                            filteredTable = filteredTable.AsEnumerable().Take(userRequest.RecordCap + 1).CopyToDataTable();
                        }
                        else
                        {
                            // NOTHING BECAUSE WE PRECAPPED
                        }

                        userRequest.Bookmark = filteredTable.Rows[filteredTable.Rows.Count - 2]["RowPointer"].ToString();
                    }
                    else
                    {

                        // THERE IS A BOOKMARK SO WE NEED TO SKIP AHEAD

                        bookmarkIndex = filteredTable.Rows.IndexOf(filteredTable.Rows.Find(new object[1] { userRequest.Bookmark }));

                        // CHECK TO SEE IF THE BOOKMARK IS THE LAST ROW

                        if (bookmarkIndex == filteredTable.Rows.Count)
                        {

                            // CLEAR THE TABLE

                            filteredTable.Clear();
                            userRequest.Bookmark = "<B/>";

                        }
                        else
                        {

                            // SKIP AHEAD OF THE BOOKMARK AND GRAB THE NEXT SET OF RECORDS

                            filteredTable = filteredTable.AsEnumerable().Skip(bookmarkIndex + 1).Take(userRequest.RecordCap + 1).CopyToDataTable();

                            // CHECK TO SEE IF THE NEW SET OF RECORDS IS GREATER THAN THE CAP

                            if (filteredTable.Rows.Count > userRequest.RecordCap)
                            {
                                userRequest.Bookmark = filteredTable.Rows[filteredTable.Rows.Count - 2]["RowPointer"].ToString();
                            }
                            else
                            {
                                userRequest.Bookmark = filteredTable.Rows[filteredTable.Rows.Count - 1]["RowPointer"].ToString();
                            }

                        }
                    }

                }
                else
                {
                    userRequest.Bookmark = filteredTable.Rows[filteredTable.Rows.Count - 1]["RowPointer"].ToString();
                }

            }
            else
            {
                userRequest.Bookmark = "<B/>";
            }

            return filteredTable;

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