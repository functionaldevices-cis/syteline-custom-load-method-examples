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