using Mongoose.IDO;
using Mongoose.IDO.DataAccess;
using Mongoose.IDO.Metadata;
using Mongoose.IDO.Protocol;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using ue_FIS_CustomLoadMethodExamples_ECA.Helpers;
using ue_FIS_CustomLoadMethodExamples_ECA.Models;

namespace ue_FIS_CustomLoadMethodExamples_ECA
{

    /**********************************************************************************************************/
    /**********************************************************************************************************/
    /*
    /* Name:     ue_FIS_CustomLoadMethodExamples_ECA
    /* Date:     2025-11-14
    /* Authors:  Andy Mercer
    /* Purpose:  A series of example CLMs, getting progressively more complex. This is the example code meant
    /*           to go along with Andy Mercer's 2026 SUN Conference session.
    /*
    /* Copyright 2025, Functional Devices, Inc
    /*
    /**********************************************************************************************************/
    /**********************************************************************************************************/

    [IDOExtensionClass("ue_FIS_CustomLoadMethodExamples_ECA")]
    #pragma warning disable IDE1006 // Naming Styles
    public class ue_FIS_CustomLoadMethodExamples_ECA : ExtensionClassBase
    {

        /**********************************************************************************************************/
        /**********************************************************************************************************/
        /*
        /* Name:     Example_01A_LoadItemPrices_Base
        /* Date:     2025-11-14
        /* Authors:  Andy Mercer
        /* Purpose:  This example loads 10 records from the SLPricecodes IDO. It returns the bound properties of Item,
        /*           UnitPrice1, UnitPrice2, EffectDate, RecordDate, and RowPointer,, plus calculated properties of,,
        /*           ItemReversed, UnitPriceDoubled1, UnitPriceDoubled2, EffectDateMinus1Day, EffectDateIsWeekday, and
        /*           EffectDateIsWeekday. The calculated properties range in type, to demonstrate different possibilities
        /*           that could be calculated.
        /*
        /*           Version A has hardcoded sort order and record cap, and no filtering.
        /*
        /* Copyright 2025, Functional Devices, Inc
        /*
        /**********************************************************************************************************/
        /**********************************************************************************************************/

        [IDOMethod(MethodFlags.CustomLoad)]
        public DataTable Example_01A_LoadItemPrices_Base()
        {

            /********************************************************************/
            /* SET UP HELPER VARIABLES
            /********************************************************************/

            Utilities utils = new Utilities(
                commands: this.Context.Commands
            );



            /********************************************************************/
            /* CREATE EMPTY TABLE
            /********************************************************************/

            DataTable outputTable = new DataTable("FullTable");
            DataRow outputRow;

            // ADD COLUMN STRUCTURE

            outputTable.Columns.Add("Item", typeof(string));
            outputTable.Columns.Add("ItemReversed", typeof(string));
            outputTable.Columns.Add("UnitPrice1", typeof(decimal));
            outputTable.Columns.Add("UnitPriceDoubled1", typeof(decimal));
            outputTable.Columns.Add("UnitPrice2", typeof(decimal));
            outputTable.Columns.Add("UnitPriceDoubled2", typeof(decimal));
            outputTable.Columns.Add("EffectDate", typeof(DateTime));
            outputTable.Columns.Add("EffectDateMinus1Day", typeof(DateTime));
            outputTable.Columns.Add("EffectDateIsWeekday", typeof(int));
            outputTable.Columns.Add("EffectDateIsWeekend", typeof(int));
            outputTable.Columns.Add("RecordDate", typeof(DateTime));
            outputTable.Columns.Add("RowPointer", typeof(string));



            /********************************************************************/
            /* QUERY ITEM PRICES TO GET BASE RECORDS
            /********************************************************************/

            LoadRecordsResponseData itemPriceRecords = utils.LoadRecords(
                IDOName: "SLItemprices",
                properties: new List<string>() {
                    { "Item" },
                    { "UnitPrice1" },
                    { "UnitPrice2" },
                    { "EffectDate" },
                    { "RecordDate" },
                    { "RowPointer" }
                },
                filter: "",
                orderBy: "Item ASC, EffectDate DESC",
                recordCap: 10
            );



            /********************************************************************/
            /* LOOP THROUGH THE ITEM PRICE RECORDS AND FILL IN THE DATA TABLE
            /********************************************************************/

            itemPriceRecords.Items.ForEach(itemPriceRecord =>
            {

                // EXTRACT AND CALCULATE OUTPUT DATA

                string item = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["Item"]]);
                string itemReversed = utils.ReverseString(item);
                decimal unitPrice1 = utils.ParseIDOPropertyValue<decimal>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["UnitPrice1"]]);
                decimal unitPriceDoubled1 = unitPrice1 * 2;
                decimal unitPrice2 = utils.ParseIDOPropertyValue<decimal>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["UnitPrice2"]]);
                decimal unitPriceDoubled2 = unitPrice2 * 2;
                DateTime effectDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["EffectDate"]]);
                DateTime effectDateMinus1Day = effectDate.AddDays(-1);
                int effectDateIsWeekend = (effectDateMinus1Day.DayOfWeek == DayOfWeek.Saturday || effectDateMinus1Day.DayOfWeek == DayOfWeek.Sunday) ? 1 : 0;
                int effectDateIsWeekday = effectDateIsWeekend == 0 ? 1 : 0;
                DateTime recordDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["RecordDate"]]);
                string rowPointer = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["RowPointer"]]);

                // CREATE OUTPUT ROW

                outputRow = outputTable.NewRow();

                // FILL IN OUTPUT ROW

                outputRow["Item"] = item;
                outputRow["ItemReversed"] = itemReversed;
                outputRow["UnitPrice1"] = unitPrice1;
                outputRow["UnitPriceDoubled1"] = unitPriceDoubled1;
                outputRow["UnitPrice2"] = unitPrice2;
                outputRow["UnitPriceDoubled2"] = unitPriceDoubled2;
                outputRow["EffectDate"] = effectDate;
                outputRow["EffectDateMinus1Day"] = effectDateMinus1Day;
                outputRow["EffectDateIsWeekday"] = effectDateIsWeekday;
                outputRow["EffectDateIsWeekend"] = effectDateIsWeekend;
                outputRow["RecordDate"] = recordDate;
                outputRow["RowPointer"] = rowPointer;

                // ADD ROW TO OUTPUT
                outputTable.Rows.Add(outputRow);


            });

            // FILTER THE TABLE

            return outputTable;

        }



        /**********************************************************************************************************/
        /**********************************************************************************************************/
        /*
        /* Name:     Example_01B_LoadItemPrices_AddPagination
        /* Date:     2025-11-14
        /* Authors:  Andy Mercer
        /* Purpose:  This example loads 10 records from the SLPricecodes IDO. It returns the bound properties of Item,
        /*           UnitPrice1, UnitPrice2, EffectDate, RecordDate, and RowPointer,, plus calculated properties of,,
        /*           ItemReversed, UnitPriceDoubled1, UnitPriceDoubled2, EffectDateMinus1Day, EffectDateIsWeekday, and
        /*           EffectDateIsWeekday. The calculated properties range in type, to demonstrate different possibilities
        /*           that could be calculated.
        /*
        /*           Version B introduces record capping and standard pagination using bookmarks.
        /*
        /*
        /* Copyright 2025, Functional Devices, Inc
        /*
        /**********************************************************************************************************/
        /**********************************************************************************************************/

        [IDOMethod(MethodFlags.CustomLoad)]
        public DataTable Example_01B_LoadItemPrices_AddPagination(string sFilter = null, string sOrderBy = null, string sRecordCap = null, string sBookmark = null)
        {

            /********************************************************************/
            /* SET UP HELPER VARIABLES
            /********************************************************************/

            Utilities utils = new Utilities(
                commands: this.Context.Commands
            );



            /********************************************************************/
            /* CREATE EMPTY TABLE
            /********************************************************************/

            DataTable outputTable = new DataTable("FullTable");
            DataRow outputRow;

            // ADD COLUMN STRUCTURE

            outputTable.Columns.Add("Item", typeof(string));
            outputTable.Columns.Add("ItemReversed", typeof(string));
            outputTable.Columns.Add("UnitPrice1", typeof(decimal));
            outputTable.Columns.Add("UnitPriceDoubled1", typeof(decimal));
            outputTable.Columns.Add("UnitPrice2", typeof(decimal));
            outputTable.Columns.Add("UnitPriceDoubled2", typeof(decimal));
            outputTable.Columns.Add("EffectDate", typeof(DateTime));
            outputTable.Columns.Add("EffectDateMinus1Day", typeof(DateTime));
            outputTable.Columns.Add("EffectDateIsWeekday", typeof(int));
            outputTable.Columns.Add("EffectDateIsWeekend", typeof(int));
            outputTable.Columns.Add("RecordDate", typeof(DateTime));
            outputTable.Columns.Add("RowPointer", typeof(string));



            /********************************************************************/
            /* LOAD USER INPUT FROM THE REQUEST OBJECT AND PARAMETERS IF SET
            /********************************************************************/

            (bool haveBookmark, bool areCappingResults) flags = (false, false);

            int iStartingCounterItems = 0;
            int iCounterItems = 0;

            LoadRecordsRequestData userRequest = new LoadRecordsRequestData(
                contextRequest: this.Context.Request as LoadCollectionRequestData,
                filterOverride: sFilter,
                orderByOverride: sOrderBy,
                recordCapOverride: sRecordCap,
                bookmarkOverride: sBookmark
            );

            if (userRequest.RecordCap > 20000)
            {
                userRequest.RecordCap = 20000;
            }
            if (userRequest.OrderBy == "")
            {
                userRequest.OrderBy = "Item ASC, EffectDate DESC";
            }

            flags.haveBookmark = userRequest.Bookmark != "<B/>";
            flags.areCappingResults = userRequest.RecordCap != 0;



            /********************************************************************/
            /* QUERY ITEM PRICES TO GET BASE RECORDS
            /********************************************************************/

            LoadRecordsResponseData itemPriceRecords = utils.LoadRecords(
                IDOName: "SLItemprices",
                properties: new List<string>() {
                    { "Item" },
                    { "UnitPrice1" },
                    { "UnitPrice2" },
                    { "EffectDate" },
                    { "RecordDate" },
                    { "RowPointer" }
                },
                filter: "",
                orderBy: userRequest.OrderBy,
                recordCap: userRequest.RecordCap
            );



            /********************************************************************/
            /* PARSE BOOKMARK TO DETERMINE WHERE TO START
            /********************************************************************/

            if (flags.haveBookmark)
            {

                string startingItem = userRequest.Bookmark.Substring(userRequest.Bookmark.IndexOf(',') + 1);
                iStartingCounterItems = itemPriceRecords.Items.FindIndex(record => utils.ParseIDOPropertyValue<string>(record.PropertyValues[itemPriceRecords.PropertyKeys["Item"]]) == startingItem);

                if (iStartingCounterItems == -1)
                {
                    throw new Exception("Error: The provided bookmark refers to a Item ('" + startingItem + "') that doesn't exist in the queried record set.");
                }

                iStartingCounterItems++;

            }



            /********************************************************************/
            /* LOOP THROUGH THE ITEM PRICE RECORDS AND FILL IN THE DATA TABLE
            /********************************************************************/

            for (iCounterItems = iStartingCounterItems; iCounterItems < itemPriceRecords.Items.Count; iCounterItems++)
            {

                // GRAB THE ITEM

                IDOItem itemPriceRecord = itemPriceRecords.Items[iCounterItems];

                // EXTRACT AND CALCULATE OUTPUT DATA

                string item = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["Item"]]);
                string itemReversed = utils.ReverseString(item);
                decimal unitPrice1 = utils.ParseIDOPropertyValue<decimal>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["UnitPrice1"]]);
                decimal unitPriceDoubled1 = unitPrice1 * 2;
                decimal unitPrice2 = utils.ParseIDOPropertyValue<decimal>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["UnitPrice2"]]);
                decimal unitPriceDoubled2 = unitPrice2 * 2;
                DateTime effectDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["EffectDate"]]);
                DateTime effectDateMinus1Day = effectDate.AddDays(-1);
                int effectDateIsWeekend = (effectDateMinus1Day.DayOfWeek == DayOfWeek.Saturday || effectDateMinus1Day.DayOfWeek == DayOfWeek.Sunday) ? 1 : 0;
                int effectDateIsWeekday = effectDateIsWeekend == 0 ? 1 : 0;
                DateTime recordDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["RecordDate"]]);
                string rowPointer = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["RowPointer"]]);

                // CREATE OUTPUT ROW

                outputRow = outputTable.NewRow();

                // FILL IN OUTPUT ROW

                outputRow["Item"] = item;
                outputRow["ItemReversed"] = itemReversed;
                outputRow["UnitPrice1"] = unitPrice1;
                outputRow["UnitPriceDoubled1"] = unitPriceDoubled1;
                outputRow["UnitPrice2"] = unitPrice2;
                outputRow["UnitPriceDoubled2"] = unitPriceDoubled2;
                outputRow["EffectDate"] = effectDate;
                outputRow["EffectDateMinus1Day"] = effectDateMinus1Day;
                outputRow["EffectDateIsWeekday"] = effectDateIsWeekday;
                outputRow["EffectDateIsWeekend"] = effectDateIsWeekend;
                outputRow["RecordDate"] = recordDate;
                outputRow["RowPointer"] = rowPointer;

                // ADD ROW TO OUTPUT

                outputTable.Rows.Add(outputRow);

                if (userRequest.RecordCap > 0 && outputTable.Rows.Count == userRequest.RecordCap + 1)
                {
                    iCounterItems = itemPriceRecords.Items.Count;
                }

            }

            if (outputTable.Rows.Count > 0)
            {
                int bookmarkRowIndex = outputTable.Rows.Count > userRequest.RecordCap ? outputTable.Rows.Count - 2 : outputTable.Rows.Count - 1;
                userRequest.Bookmark = outputTable.Rows[bookmarkRowIndex]["Item"].ToString();
            }

            return outputTable;

        }



        /**********************************************************************************************************/
        /**********************************************************************************************************/
        /*
        /* Name:     Example_01C_LoadItemPrices_QueryFiltering
        /* Date:     2025-11-14
        /* Authors:  Andy Mercer
        /* Purpose:  This example loads 10 records from the SLPricecodes IDO. It returns the bound properties of Item,
        /*           UnitPrice1, UnitPrice2, EffectDate, RecordDate, and RowPointer,, plus calculated properties of,,
        /*           ItemReversed, UnitPriceDoubled1, UnitPriceDoubled2, EffectDateMinus1Day, EffectDateIsWeekday, and
        /*           EffectDateIsWeekday. The calculated properties range in type, to demonstrate different possibilities
        /*           that could be calculated.
        /*
        /*           Version C introduces query filtering, ie the ability to filter on properties that are page of the
        /*           base IDO.
        /*
        /* Copyright 2025, Functional Devices, Inc
        /*
        /**********************************************************************************************************/
        /**********************************************************************************************************/

        [IDOMethod(MethodFlags.CustomLoad)]
        public DataTable Example_01C_LoadItemPrices_QueryFiltering(string sFilter = null, string sOrderBy = null, string sRecordCap = null, string sBookmark = null)
        {

            /********************************************************************/
            /* SET UP HELPER VARIABLES
            /********************************************************************/

            Utilities utils = new Utilities(
                commands: this.Context.Commands
            );



            /********************************************************************/
            /* CREATE EMPTY TABLE
            /********************************************************************/

            DataTable outputTable = new DataTable("FullTable");
            DataRow outputRow;

            // ADD COLUMN STRUCTURE

            outputTable.Columns.Add("Item", typeof(string));
            outputTable.Columns.Add("ItemReversed", typeof(string));
            outputTable.Columns.Add("UnitPrice1", typeof(decimal));
            outputTable.Columns.Add("UnitPriceDoubled1", typeof(decimal));
            outputTable.Columns.Add("UnitPrice2", typeof(decimal));
            outputTable.Columns.Add("UnitPriceDoubled2", typeof(decimal));
            outputTable.Columns.Add("EffectDate", typeof(DateTime));
            outputTable.Columns.Add("EffectDateMinus1Day", typeof(DateTime));
            outputTable.Columns.Add("EffectDateIsWeekday", typeof(int));
            outputTable.Columns.Add("EffectDateIsWeekend", typeof(int));
            outputTable.Columns.Add("RecordDate", typeof(DateTime));
            outputTable.Columns.Add("RowPointer", typeof(string));



            /********************************************************************/
            /* LOAD USER INPUT FROM THE REQUEST OBJECT AND PARAMETERS IF SET
            /********************************************************************/

            (bool haveBookmark, bool areCappingResults) flags = (false, false);

            int iStartingCounterItems = 0;
            int iCounterItems = 0;

            LoadRecordsRequestData userRequest = new LoadRecordsRequestData(
                contextRequest: this.Context.Request as LoadCollectionRequestData,
                filterOverride: sFilter,
                orderByOverride: sOrderBy,
                recordCapOverride: sRecordCap,
                bookmarkOverride: sBookmark
            );

            if (userRequest.RecordCap > 20000)
            {
                userRequest.RecordCap = 20000;
            }
            if (userRequest.OrderBy == "")
            {
                userRequest.OrderBy = "Item ASC, EffectDate DESC";
            }

            flags.haveBookmark = userRequest.Bookmark != "<B/>";
            flags.areCappingResults = userRequest.RecordCap != 0;



            /********************************************************************/
            /* PARSE FILTERS
            /********************************************************************/

            string userFilterPropertyName;

            Dictionary<string, IIDOPropertyFilterSet> itempriceQueryFilters = new Dictionary<string, IIDOPropertyFilterSet>() {
                { "Item", new IDOPropertyFilterSet<string>() },
                { "UnitPrice1", new IDOPropertyFilterSet<decimal>() },
                { "UnitPrice2", new IDOPropertyFilterSet<decimal>() },
                { "EffectDate", new IDOPropertyFilterSet<DateTime>() },
                { "RecordDate", new IDOPropertyFilterSet<DateTime>() },
                { "RowPointer", new IDOPropertyFilterSet<string>() }
            };

            userRequest.Filters.ForEach(userFilter =>
            {

                userFilterPropertyName = utils.FilterExtractPropertyName(userFilter);

                if (itempriceQueryFilters.Keys.Contains(userFilterPropertyName))
                {
                    itempriceQueryFilters[userFilterPropertyName].AddFilter(userFilter);
                }

            });



            /********************************************************************/
            /* QUERY ITEM PRICES TO GET BASE RECORDS
            /********************************************************************/

            LoadRecordsResponseData itemPriceRecords = utils.LoadRecords(
                IDOName: "SLItemprices",
                properties: new List<string>() {
                    { "Item" },
                    { "UnitPrice1" },
                    { "UnitPrice2" },
                    { "EffectDate" },
                    { "RecordDate" },
                    { "RowPointer" }
                },
                filter: utils.BuildFilterString(itempriceQueryFilters.Values.Select(filter => filter.GetFilterString()).ToList()),
                orderBy: userRequest.OrderBy,
                recordCap: userRequest.Bookmark == "<B/>" ? userRequest.RecordCap + 1 : 0
            );



            /********************************************************************/
            /* PARSE BOOKMARK TO DETERMINE WHERE TO START
            /********************************************************************/

            if (flags.haveBookmark)
            {

                string startingItem = userRequest.Bookmark.Substring(userRequest.Bookmark.IndexOf(',') + 1);
                iStartingCounterItems = itemPriceRecords.Items.FindIndex(record => utils.ParseIDOPropertyValue<string>(record.PropertyValues[itemPriceRecords.PropertyKeys["Item"]]) == startingItem);

                if (iStartingCounterItems == -1)
                {
                    throw new Exception("Error: The provided bookmark refers to a Item ('" + startingItem + "') that doesn't exist in the queried record set.");
                }

                iStartingCounterItems++;

            }



            /********************************************************************/
            /* LOOP THROUGH THE ITEM PRICE RECORDS AND FILL IN THE DATA TABLE
            /********************************************************************/

            for (iCounterItems = iStartingCounterItems; iCounterItems < itemPriceRecords.Items.Count; iCounterItems++)
            {

                // GRAB THE ITEM

                IDOItem itemPriceRecord = itemPriceRecords.Items[iCounterItems];

                // EXTRACT AND CALCULATE OUTPUT DATA

                string item = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["Item"]]);
                string itemReversed = utils.ReverseString(item);
                decimal unitPrice1 = utils.ParseIDOPropertyValue<decimal>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["UnitPrice1"]]);
                decimal unitPriceDoubled1 = unitPrice1 * 2;
                decimal unitPrice2 = utils.ParseIDOPropertyValue<decimal>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["UnitPrice2"]]);
                decimal unitPriceDoubled2 = unitPrice2 * 2;
                DateTime effectDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["EffectDate"]]);
                DateTime effectDateMinus1Day = effectDate.AddDays(-1);
                int effectDateIsWeekend = (effectDateMinus1Day.DayOfWeek == DayOfWeek.Saturday || effectDateMinus1Day.DayOfWeek == DayOfWeek.Sunday) ? 1 : 0;
                int effectDateIsWeekday = effectDateIsWeekend == 0 ? 1 : 0;
                DateTime recordDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["RecordDate"]]);
                string rowPointer = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["RowPointer"]]);

                // CREATE OUTPUT ROW

                outputRow = outputTable.NewRow();

                // FILL IN OUTPUT ROW

                outputRow["Item"] = item;
                outputRow["ItemReversed"] = itemReversed;
                outputRow["UnitPrice1"] = unitPrice1;
                outputRow["UnitPriceDoubled1"] = unitPriceDoubled1;
                outputRow["UnitPrice2"] = unitPrice2;
                outputRow["UnitPriceDoubled2"] = unitPriceDoubled2;
                outputRow["EffectDate"] = effectDate;
                outputRow["EffectDateMinus1Day"] = effectDateMinus1Day;
                outputRow["EffectDateIsWeekday"] = effectDateIsWeekday;
                outputRow["EffectDateIsWeekend"] = effectDateIsWeekend;
                outputRow["RecordDate"] = recordDate;
                outputRow["RowPointer"] = rowPointer;

                // ADD ROW TO OUTPUT

                outputTable.Rows.Add(outputRow);

                if (userRequest.RecordCap > 0 && outputTable.Rows.Count == userRequest.RecordCap + 1)
                {
                    iCounterItems = itemPriceRecords.Items.Count;
                }

            }

            if (outputTable.Rows.Count > 0)
            {
                int bookmarkRowIndex = outputTable.Rows.Count > userRequest.RecordCap ? outputTable.Rows.Count - 2 : outputTable.Rows.Count - 1;
                userRequest.Bookmark = outputTable.Rows[bookmarkRowIndex]["Item"].ToString();
            }

            return outputTable;

        }



        /**********************************************************************************************************/
        /**********************************************************************************************************/
        /*
        /* Name:     Example_01D_LoadItemPrices_InlineFiltering
        /* Date:     2025-11-14
        /* Authors:  Andy Mercer
        /* Purpose:  This example loads 10 records from the SLPricecodes IDO. It returns the bound properties of Item,
        /*           UnitPrice1, UnitPrice2, EffectDate, RecordDate, and RowPointer,, plus calculated properties of,,
        /*           ItemReversed, UnitPriceDoubled1, UnitPriceDoubled2, EffectDateMinus1Day, EffectDateIsWeekday, and
        /*           EffectDateIsWeekday. The calculated properties range in type, to demonstrate different possibilities
        /*           that could be calculated.
        /*
        /*           Version D introduces inline filtering, which adds the ability to filter on properties that are
        /*           calculated.
        /*
        /* Copyright 2025, Functional Devices, Inc
        /*
        /**********************************************************************************************************/
        /**********************************************************************************************************/

        [IDOMethod(MethodFlags.CustomLoad)]
        public DataTable Example_01D_LoadItemPrices_InlineFiltering(string sFilter = null, string sOrderBy = null, string sRecordCap = null, string sBookmark = null)
        {

            /********************************************************************/
            /* SET UP HELPER VARIABLES
            /********************************************************************/

            Utilities utils = new Utilities(
                commands: this.Context.Commands
            );



            /********************************************************************/
            /* CREATE EMPTY TABLE
            /********************************************************************/

            DataTable outputTable = new DataTable("FullTable");
            DataRow outputRow;

            // ADD COLUMN STRUCTURE

            outputTable.Columns.Add("Item", typeof(string));
            outputTable.Columns.Add("ItemReversed", typeof(string));
            outputTable.Columns.Add("UnitPrice1", typeof(decimal));
            outputTable.Columns.Add("UnitPriceDoubled1", typeof(decimal));
            outputTable.Columns.Add("UnitPrice2", typeof(decimal));
            outputTable.Columns.Add("UnitPriceDoubled2", typeof(decimal));
            outputTable.Columns.Add("EffectDate", typeof(DateTime));
            outputTable.Columns.Add("EffectDateMinus1Day", typeof(DateTime));
            outputTable.Columns.Add("EffectDateIsWeekday", typeof(int));
            outputTable.Columns.Add("EffectDateIsWeekend", typeof(int));
            outputTable.Columns.Add("RecordDate", typeof(DateTime));
            outputTable.Columns.Add("RowPointer", typeof(string));



            /********************************************************************/
            /* LOAD USER INPUT FROM THE REQUEST OBJECT AND PARAMETERS IF SET
            /********************************************************************/

            (bool haveBookmark, bool areCappingResults) flags = (false, false);

            int iStartingCounterItems = 0;
            int iCounterItems = 0;

            LoadRecordsRequestData userRequest = new LoadRecordsRequestData(
                contextRequest: this.Context.Request as LoadCollectionRequestData,
                filterOverride: sFilter,
                orderByOverride: sOrderBy,
                recordCapOverride: sRecordCap,
                bookmarkOverride: sBookmark
            );

            if (userRequest.RecordCap > 20000)
            {
                userRequest.RecordCap = 20000;
            }
            if (userRequest.OrderBy == "")
            {
                userRequest.OrderBy = "Item ASC, EffectDate DESC";
            }

            flags.haveBookmark = userRequest.Bookmark != "<B/>";
            flags.areCappingResults = userRequest.RecordCap != 0;



            /********************************************************************/
            /* PARSE FILTERS
            /********************************************************************/

            string userFilterPropertyName;

            Dictionary<string, IIDOPropertyFilterSet> itempriceQueryFilters = new Dictionary<string, IIDOPropertyFilterSet>() {
                { "Item", new IDOPropertyFilterSet<string>() },
                { "UnitPrice1", new IDOPropertyFilterSet<decimal>() },
                { "UnitPrice2", new IDOPropertyFilterSet<decimal>() },
                { "EffectDate", new IDOPropertyFilterSet<DateTime>() },
                { "RecordDate", new IDOPropertyFilterSet<DateTime>() },
                { "RowPointer", new IDOPropertyFilterSet<string>() }
            };

            Dictionary<string, IIDOPropertyFilterSet> inlineFilters = new Dictionary<string, IIDOPropertyFilterSet>() {
                { "ItemReversed", new IDOPropertyFilterSet<string>() },
                { "UnitPriceDoubled1", new IDOPropertyFilterSet<decimal>() },
                { "UnitPriceDoubled2", new IDOPropertyFilterSet<decimal>() },
                { "EffectDateMinus1Day", new IDOPropertyFilterSet<DateTime>() },
                { "EffectDateIsWeekday", new IDOPropertyFilterSet<int>() },
                { "EffectDateIsWeekend", new IDOPropertyFilterSet<int>() }
            };


            userRequest.Filters.ForEach(userFilter =>
            {

                userFilterPropertyName = utils.FilterExtractPropertyName(userFilter);

                if (itempriceQueryFilters.Keys.Contains(userFilterPropertyName))
                {
                    itempriceQueryFilters[userFilterPropertyName].AddFilter(userFilter);
                }

                if (inlineFilters.Keys.Contains(userFilterPropertyName))
                {
                    inlineFilters[userFilterPropertyName].AddFilter(userFilter);
                }

            });



            /********************************************************************/
            /* QUERY ITEM PRICES TO GET BASE RECORDS
            /********************************************************************/

            LoadRecordsResponseData itemPriceRecords = utils.LoadRecords(
                IDOName: "SLItemprices",
                properties: new List<string>() {
                    { "Item" },
                    { "UnitPrice1" },
                    { "UnitPrice2" },
                    { "EffectDate" },
                    { "RecordDate" },
                    { "RowPointer" }
                },
                filter: utils.BuildFilterString(itempriceQueryFilters.Values.Select(filter => filter.GetFilterString()).ToList()),
                orderBy: userRequest.OrderBy,
                recordCap: userRequest.Bookmark == "<B/>" ? userRequest.RecordCap + 1 : 0
            );



            /********************************************************************/
            /* PARSE BOOKMARK TO DETERMINE WHERE TO START
            /********************************************************************/

            if (flags.haveBookmark)
            {

                string startingItem = userRequest.Bookmark.Substring(userRequest.Bookmark.IndexOf(',') + 1);
                iStartingCounterItems = itemPriceRecords.Items.FindIndex(record => utils.ParseIDOPropertyValue<string>(record.PropertyValues[itemPriceRecords.PropertyKeys["Item"]]) == startingItem);

                if (iStartingCounterItems == -1)
                {
                    throw new Exception("Error: The provided bookmark refers to a Item ('" + startingItem + "') that doesn't exist in the queried record set.");
                }

                iStartingCounterItems++;

            }



            /********************************************************************/
            /* LOOP THROUGH THE ITEM PRICE RECORDS AND FILL IN THE DATA TABLE
            /********************************************************************/

            for (iCounterItems = iStartingCounterItems; iCounterItems < itemPriceRecords.Items.Count; iCounterItems++)
            {

                // GRAB THE ITEM

                IDOItem itemPriceRecord = itemPriceRecords.Items[iCounterItems];

                // EXTRACT AND CALCULATE OUTPUT DATA

                string item = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["Item"]]);
                string itemReversed = utils.ReverseString(item);
                decimal unitPrice1 = utils.ParseIDOPropertyValue<decimal>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["UnitPrice1"]]);
                decimal unitPriceDoubled1 = unitPrice1 * 2;
                decimal unitPrice2 = utils.ParseIDOPropertyValue<decimal>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["UnitPrice2"]]);
                decimal unitPriceDoubled2 = unitPrice2 * 2;
                DateTime effectDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["EffectDate"]]);
                DateTime effectDateMinus1Day = effectDate.AddDays(-1);
                int effectDateIsWeekend = (effectDateMinus1Day.DayOfWeek == DayOfWeek.Saturday || effectDateMinus1Day.DayOfWeek == DayOfWeek.Sunday) ? 1 : 0;
                int effectDateIsWeekday = effectDateIsWeekend == 0 ? 1 : 0;
                DateTime recordDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["RecordDate"]]);
                string rowPointer = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["RowPointer"]]);

                // RUN ALL INLINE FILTERS

                bool passesInlineFilters = true;

                if (inlineFilters["ItemReversed"].ValueFails(itemReversed))
                {
                    passesInlineFilters = false;
                }

                if (inlineFilters["UnitPriceDoubled1"].ValueFails(unitPriceDoubled1))
                {
                    passesInlineFilters = false;
                }

                if (inlineFilters["UnitPriceDoubled2"].ValueFails(unitPriceDoubled2))
                {
                    passesInlineFilters = false;
                }

                if (inlineFilters["EffectDateMinus1Day"].ValueFails(effectDateMinus1Day))
                {
                    passesInlineFilters = false;
                }

                if (inlineFilters["EffectDateIsWeekday"].ValueFails(effectDateIsWeekday))
                {
                    passesInlineFilters = false;
                }

                if (inlineFilters["EffectDateIsWeekend"].ValueFails(effectDateIsWeekend))
                {
                    passesInlineFilters = false;
                }

                if (passesInlineFilters)
                {

                    // CREATE OUTPUT ROW

                    outputRow = outputTable.NewRow();

                    outputRow["Item"] = item;
                    outputRow["ItemReversed"] = itemReversed;
                    outputRow["UnitPrice1"] = unitPrice1;
                    outputRow["UnitPriceDoubled1"] = unitPriceDoubled1;
                    outputRow["UnitPrice2"] = unitPrice2;
                    outputRow["UnitPriceDoubled2"] = unitPriceDoubled2;
                    outputRow["EffectDate"] = effectDate;
                    outputRow["EffectDateMinus1Day"] = effectDateMinus1Day;
                    outputRow["EffectDateIsWeekday"] = effectDateIsWeekday;
                    outputRow["EffectDateIsWeekend"] = effectDateIsWeekend;
                    outputRow["RecordDate"] = recordDate;
                    outputRow["RowPointer"] = rowPointer;

                    // ADD ROW TO OUTPUT

                    outputTable.Rows.Add(outputRow);

                }

                if (userRequest.RecordCap > 0 && outputTable.Rows.Count == userRequest.RecordCap + 1)
                {
                    iCounterItems = itemPriceRecords.Items.Count;
                }

            }

            if (outputTable.Rows.Count > 0)
            {
                int bookmarkRowIndex = outputTable.Rows.Count > userRequest.RecordCap ? outputTable.Rows.Count - 2 : outputTable.Rows.Count - 1;
                userRequest.Bookmark = outputTable.Rows[bookmarkRowIndex]["Item"].ToString();
            }

            return outputTable;

        }



        /**********************************************************************************************************/
        /**********************************************************************************************************/
        /*
        /* Name:     Example_02_LoadCurrentItemPrices
        /* Date:     2025-11-14
        /* Authors:  Andy Mercer
        /* Purpose:  This example loads all 'current' records from the SLPricecodes IDO, meaning the record with the
        /*           highest non-future effective date for each unique Item value.
        /*
        /* Copyright 2025, Functional Devices, Inc
        /*
        /**********************************************************************************************************/
        /**********************************************************************************************************/

        [IDOMethod(MethodFlags.CustomLoad)]
        public DataTable Example_02_LoadCurrentItemPrices(string sFilter = null, string sOrderBy = null, string sRecordCap = null, string sBookmark = null)
        {

            /********************************************************************/
            /* SET UP HELPER VARIABLES
            /********************************************************************/

            Utilities utils = new Utilities(
                commands: this.Context.Commands
            );



            /********************************************************************/
            /* CREATE EMPTY TABLE
            /********************************************************************/

            DataTable outputTable = new DataTable("FullTable");
            Dictionary<string, int> itemIndices = new Dictionary<string, int>();

            // ADD COLUMN STRUCTURE

            outputTable.Columns.Add("Item", typeof(string));
            outputTable.Columns.Add("ItemReversed", typeof(string));
            outputTable.Columns.Add("UnitPrice1", typeof(decimal));
            outputTable.Columns.Add("UnitPriceDoubled1", typeof(decimal));
            outputTable.Columns.Add("UnitPrice2", typeof(decimal));
            outputTable.Columns.Add("UnitPriceDoubled2", typeof(decimal));
            outputTable.Columns.Add("EffectDate", typeof(DateTime));
            outputTable.Columns.Add("EffectDateMinus1Day", typeof(DateTime));
            outputTable.Columns.Add("EffectDateIsWeekday", typeof(int));
            outputTable.Columns.Add("EffectDateIsWeekend", typeof(int));
            outputTable.Columns.Add("RecordDate", typeof(DateTime));
            outputTable.Columns.Add("RowPointer", typeof(string));



            /********************************************************************/
            /* LOAD USER INPUT FROM THE REQUEST OBJECT AND PARAMETERS IF SET
            /********************************************************************/

            (bool haveBookmark, bool areCappingResults) flags = (false, false);

            int iStartingCounterItems = 0;
            int iCounterItems = 0;
            string debug1 = "";
            string debug2 = "";

            LoadRecordsRequestData userRequest = new LoadRecordsRequestData(
                contextRequest: this.Context.Request as LoadCollectionRequestData,
                filterOverride: sFilter,
                orderByOverride: sOrderBy,
                recordCapOverride: sRecordCap,
                bookmarkOverride: sBookmark
            );

            if (userRequest.RecordCap > 20000)
            {
                userRequest.RecordCap = 20000;
            }
            if (userRequest.OrderBy == "")
            {
                userRequest.OrderBy = "Item ASC, EffectDate DESC";
            }

            flags.haveBookmark = userRequest.Bookmark != "<B/>";
            flags.areCappingResults = userRequest.RecordCap != 0;



            /********************************************************************/
            /* PARSE FILTERS
            /********************************************************************/

            string userFilterPropertyName;

            Dictionary<string, IIDOPropertyFilterSet> itempriceQueryFilters = new Dictionary<string, IIDOPropertyFilterSet>() {
                { "Item", new IDOPropertyFilterSet<string>() }
            };

            Dictionary<string, IIDOPropertyFilterSet> inlineFilters = new Dictionary<string, IIDOPropertyFilterSet>() {
                { "ItemReversed", new IDOPropertyFilterSet<string>() },
                { "UnitPrice1", new IDOPropertyFilterSet<decimal>() },
                { "UnitPriceDoubled1", new IDOPropertyFilterSet<decimal>() },
                { "UnitPrice2", new IDOPropertyFilterSet<decimal>() },
                { "UnitPriceDoubled2", new IDOPropertyFilterSet<decimal>() },
                { "EffectDate", new IDOPropertyFilterSet<DateTime>() },
                { "EffectDateMinus1Day", new IDOPropertyFilterSet<DateTime>() },
                { "EffectDateIsWeekday", new IDOPropertyFilterSet<int>() },
                { "EffectDateIsWeekend", new IDOPropertyFilterSet<int>() },
                { "RecordDate", new IDOPropertyFilterSet<DateTime>() },
                { "RowPointer", new IDOPropertyFilterSet<string>() }
            };

            userRequest.Filters.ForEach(userFilter =>
            {

                userFilterPropertyName = utils.FilterExtractPropertyName(userFilter);

                if (itempriceQueryFilters.Keys.Contains(userFilterPropertyName))
                {
                    itempriceQueryFilters[userFilterPropertyName].AddFilter(userFilter);
                }

                if (inlineFilters.Keys.Contains(userFilterPropertyName))
                {
                    inlineFilters[userFilterPropertyName].AddFilter(userFilter);
                }

            });



            /********************************************************************/
            /* QUERY ITEM PRICES TO GET BASE RECORDS
            /********************************************************************/

            LoadRecordsResponseData itemPriceRecords = utils.LoadRecords(
                IDOName: "SLItemprices",
                properties: new List<string>() {
                    { "Item" },
                    { "UnitPrice1" },
                    { "UnitPrice2" },
                    { "EffectDate" },
                    { "RecordDate" },
                    { "RowPointer" }
                },
                filter: utils.BuildFilterString(itempriceQueryFilters.Values.Select(filter => filter.GetFilterString()).ToList()),
                orderBy: "Item ASC, EffectDate DESC",
                recordCap: 0
            );



            /********************************************************************/
            /* PARSE BOOKMARK TO DETERMINE WHERE TO START
            /********************************************************************/

            if (flags.haveBookmark)
            {

                string startingItem = userRequest.Bookmark.Substring(userRequest.Bookmark.IndexOf(',') + 1);
                iStartingCounterItems = itemPriceRecords.Items.FindIndex(record => utils.ParseIDOPropertyValue<string>(record.PropertyValues[itemPriceRecords.PropertyKeys["Item"]]) == startingItem);

                if (iStartingCounterItems == -1)
                {
                    throw new Exception("Error: The provided bookmark refers to a Item ('" + startingItem + "') that doesn't exist in the queried record set.");
                }

                iStartingCounterItems++;

            }



            /********************************************************************/
            /* LOOP THROUGH THE ITEM PRICE RECORDS AND FILL IN THE DATA TABLE
            /********************************************************************/

            for (iCounterItems = iStartingCounterItems; iCounterItems < itemPriceRecords.Items.Count; iCounterItems++)
            {

                // GRAB THE ITEM

                IDOItem itemPriceRecord = itemPriceRecords.Items[iCounterItems];
                string item = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["Item"]]);

                if (item != null && !itemIndices.ContainsKey(item))
                {

                    // SAVE INDEX

                    itemIndices[item] = outputTable.Rows.Count;

                    // LOAD DATA FROM THE ITEM PRICE RECORD AND RUN CUSTOM LOGIC

                    string itemReversed = utils.ReverseString(item);
                    decimal unitPrice1 = utils.ParseIDOPropertyValue<decimal>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["UnitPrice1"]]);
                    decimal unitPriceDoubled1 = unitPrice1 * 2;
                    decimal unitPrice2 = utils.ParseIDOPropertyValue<decimal>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["UnitPrice2"]]);
                    decimal unitPriceDoubled2 = unitPrice2 * 2;
                    DateTime effectDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["EffectDate"]]);
                    DateTime effectDateMinus1Day = effectDate.AddDays(-1);
                    int effectDateIsWeekend = (effectDateMinus1Day.DayOfWeek == DayOfWeek.Saturday || effectDateMinus1Day.DayOfWeek == DayOfWeek.Sunday) ? 1 : 0;
                    int effectDateIsWeekday = effectDateIsWeekend == 0 ? 1 : 0;
                    DateTime recordDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["RecordDate"]]);
                    string rowPointer = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["RowPointer"]]);

                    // RUN ALL INLINE FILTERS

                    bool passesInlineFilters = true;

                    if (inlineFilters["ItemReversed"].ValueFails(itemReversed))
                    {
                        passesInlineFilters = false;
                    }

                    if (inlineFilters["UnitPrice1"].ValueFails(unitPrice1))
                    {
                        passesInlineFilters = false;
                    }

                    if (inlineFilters["UnitPriceDoubled1"].ValueFails(unitPriceDoubled1))
                    {
                        passesInlineFilters = false;
                    }

                    if (inlineFilters["UnitPrice2"].ValueFails(unitPrice2))
                    {
                        passesInlineFilters = false;
                    }

                    if (inlineFilters["UnitPriceDoubled2"].ValueFails(unitPriceDoubled2))
                    {
                        passesInlineFilters = false;
                    }

                    if (inlineFilters["EffectDate"].ValueFails(effectDate))
                    {
                        passesInlineFilters = false;
                    }

                    if (inlineFilters["EffectDateMinus1Day"].ValueFails(effectDateMinus1Day))
                    {
                        passesInlineFilters = false;
                    }

                    if (inlineFilters["EffectDateIsWeekday"].ValueFails(effectDateIsWeekday))
                    {
                        passesInlineFilters = false;
                    }

                    if (inlineFilters["EffectDateIsWeekend"].ValueFails(effectDateIsWeekend))
                    {
                        passesInlineFilters = false;
                    }

                    if (inlineFilters["RecordDate"].ValueFails(recordDate))
                    {
                        passesInlineFilters = false;
                    }

                    if (inlineFilters["RowPointer"].ValueFails(rowPointer))
                    {
                        passesInlineFilters = false;
                    }

                    if (passesInlineFilters)
                    {

                        // CREATE OUTPUT ROW

                        DataRow outputRow = outputTable.NewRow();

                        outputRow["Item"] = item;
                        outputRow["ItemReversed"] = itemReversed;
                        outputRow["UnitPrice1"] = unitPrice1;
                        outputRow["UnitPriceDoubled1"] = unitPriceDoubled1;
                        outputRow["UnitPrice2"] = unitPrice2;
                        outputRow["UnitPriceDoubled2"] = unitPriceDoubled2;
                        outputRow["EffectDate"] = effectDate;
                        outputRow["EffectDateMinus1Day"] = effectDateMinus1Day;
                        outputRow["EffectDateIsWeekday"] = effectDateIsWeekday;
                        outputRow["EffectDateIsWeekend"] = effectDateIsWeekend;
                        outputRow["RecordDate"] = recordDate;
                        outputRow["RowPointer"] = rowPointer;

                        outputTable.Rows.Add(outputRow);

                    }

                }

                if (userRequest.RecordCap > 0 && outputTable.Rows.Count == userRequest.RecordCap + 1)
                {
                    iCounterItems = itemPriceRecords.Items.Count;
                }

            }


            if (outputTable.Rows.Count > 0)
            {
                int bookmarkRowIndex = outputTable.Rows.Count > userRequest.RecordCap ? outputTable.Rows.Count - 2 : outputTable.Rows.Count - 1;
                userRequest.Bookmark = outputTable.Rows[bookmarkRowIndex]["Item"].ToString();
                if (debug1 != "")
                {
                    outputTable.Rows[0]["Item"] = debug1;
                }
                if (debug2 != "")
                {
                    outputTable.Rows[0]["ItemReversed"] = debug2;
                }
            }

            return outputTable;

        }



        /**********************************************************************************************************/
        /**********************************************************************************************************/
        /*
        /* Name:     Example_03_LoadPricesForPricecode
        /* Date:     2025-11-14
        /* Authors:  Andy Mercer
        /* Purpose:  This example loads a set of prices based on a specific PriceCode. There are a number of
        /*           assumptions here. It assumes that you are using Matrix/Formula based pricing in your Syteline
        /*           instance. If you are, then all you need to do is change the default filter value in the 
        /*           pricecodesQueryFilters dictionary to whatever price code you want to use of your own. 
        /*           
        /*           If you are NOT using Matrix/Formula pricing, you can load the example records which are attached
        /*           to this GitHub repo. They are provided as CSV files that can be imported:
        /*
        /*           * Four price code records and four price formula records, with the following structure:
        /*
        /*             * 'Y00' = List * 1
        /*             * 'Y75' = List * 0.75
        /*             * 'Y50' = List * 0.5
        /*             * 'Y25' = List * 0.25'
        /*
        /*             'Y' was chosen as a prefix because it is not likely to have been used in a production system.
        /*           
        /*           Also provided will be four price matrix records. HOWEVER, they will likely not work without tweaking,
        /*           because item price codes can be anything. The simplest way to set up the matrix records is to have
        /*           all of your item price records use a single item price code. It could be called "ALL", and so that
        /*           is what the example will assume. You will need to change it though, if you don't have that item price
        /*           code.
        /*           
        /*
        /* Copyright 2025, Functional Devices, Inc
        /*
        /**********************************************************************************************************/
        /**********************************************************************************************************/

        [IDOMethod(MethodFlags.CustomLoad)]
        public DataTable Example_03_LoadPricesForPricecode(string sFilter = null, string sOrderBy = null, string sRecordCap = null, string sBookmark = null)
        {

            /********************************************************************/
            /* SET UP HELPER VARIABLES
            /********************************************************************/

            Utilities utils = new Utilities(
                commands: this.Context.Commands
            );

            (bool haveBookmark, bool arePostFiltering, bool orderingByRowPointer, bool areCappingResults) flags = (false, false, false, false);



            /********************************************************************/
            /* LOAD USER INPUT FROM THE REQUEST OBJECT AND PARAMETERS IF SET
            /********************************************************************/

            LoadRecordsRequestData userRequest = new LoadRecordsRequestData(
                contextRequest: this.Context.Request as LoadCollectionRequestData,
                filterOverride: sFilter,
                orderByOverride: sOrderBy,
                recordCapOverride: sRecordCap,
                bookmarkOverride: sBookmark
            );

            userRequest.OrderBy = userRequest.OrderBy == "" ? "Item ASC, EffectDate DESC" : userRequest.OrderBy;

            flags.haveBookmark = userRequest.Bookmark != "<B/>";
            flags.orderingByRowPointer = userRequest.OrderBy == "RowPointer" || userRequest.OrderBy == "RowPointer ASC";
            flags.areCappingResults = userRequest.RecordCap != 0;



            /********************************************************************/
            /* PARSE FILTERS
            /********************************************************************/

            string custPriceCode = "Y00"; // !! CHANGE THIS TO WHATEVER YOUR BASE LIST PRICE CODE IS !!
            string userFilterValue;
            string userFilterOperator;

            Dictionary<string, string> itempriceQueryFilters = new Dictionary<string, string>() {
                { "Item", "" },
                { "EffectDate", "" },
                { "RecordDate", "" },
                { "RowPointer", "" },
                { "ListPrice", "" }
            };

            Dictionary<string, string> priceMatrixQueryFilters = new Dictionary<string, string>() {
                { "CustPricecode", "CustPricecode = '" + custPriceCode + "'" }
            };

            Dictionary<string, string> postQueryFilters = new Dictionary<string, string>() {
                { "CustomerPrice", "" }
            };

            userRequest.Filters.ForEach(userFilter =>
            {

                userFilter = utils.FixParenthesis(userFilter);
                userFilterValue = utils.ExtractValue(userFilter);
                userFilterOperator = utils.ExtractOperator(userFilter);

                if (userFilter.Contains("PriceCode"))
                {
                    priceMatrixQueryFilters["CustPricecode"] = userFilter.Replace("PriceCode", "CustPricecode");
                    custPriceCode = userFilterValue;
                }

                if (userFilter.Contains("Item"))
                {
                    itempriceQueryFilters["Item"] = userFilter;
                }

                if (userFilter.Contains("EffectDate"))
                {
                    itempriceQueryFilters["EffectDate"] = userFilter;
                }

                if (userFilter.Contains("ListPrice"))
                {
                    itempriceQueryFilters["ListPrice"] = userFilter;
                }

                if (userFilter.Contains("CustomerPrice"))
                {
                    postQueryFilters["CustomerPrice"] = userFilter.Replace(" null ", " '' ");
                }

                if (userFilter.Contains("RecordDate"))
                {
                    postQueryFilters["RecordDate"] = userFilter;
                }

                if (userFilter.Contains("RowPointer"))
                {
                    itempriceQueryFilters["RowPointer"] = userFilter;
                }

            });

            if (flags.orderingByRowPointer && flags.haveBookmark)
            {
                postQueryFilters["RowPointer"] = "RowPointer > '" + userRequest.Bookmark + "'";
            }

            string userPostQueryFilterString = utils.BuildFilterString(postQueryFilters.Values.ToList());
            flags.arePostFiltering = userPostQueryFilterString != "";



            /********************************************************************/
            /* LOAD THE PRICE MATRIX RECORDS
            /********************************************************************/

            LoadRecordsResponseData priceMatrixRecords = utils.LoadRecords(
                IDOName: "SLPricematrixs",
                properties: new List<string>() {
                    { "CustPricecode" },
                    { "ItemPricecode" },
                    { "Priceformula" },
                    { "RecordDate" }
                },
                filter: utils.BuildFilterString(priceMatrixQueryFilters.Values.ToList()),
                orderBy: "CustPricecode, ItemPricecode",
                recordCap: 0
            );

            string uniqueEscapedPriceFormulasCommaDel = priceMatrixRecords.Items.Count > 0 ? string.Join(
                ",",
                priceMatrixRecords.Items.Select(record =>
                {
                    return "'" + utils.ParseIDOPropertyValue<string>(record.PropertyValues[priceMatrixRecords.PropertyKeys["Priceformula"]]) + "'";
                }).Distinct()
            ) : "''";



            /********************************************************************/
            /* LOAD THE PRICE FORMULA RECORDS FOR THE LOADED PRICE MATRICES
            /********************************************************************/

            LoadRecordsResponseData priceFormulasRecords = utils.LoadRecords(
                IDOName: "SLPriceformulas",
                properties: new List<string>() {
                    { "Priceformula" },
                    { "FirstDolPercent" },
                    { "FirstPrice" },
                    { "EffectDate" },
                    { "RecordDate" }
                },
                filter: "Priceformula IN ( " + uniqueEscapedPriceFormulasCommaDel + " )",
                orderBy: "Priceformula ASC, EffectDate DESC",
                recordCap: 0
            );

            Dictionary<string, IDOItem> activePriceFormulaLookupTable = new Dictionary<string, IDOItem>();
            priceFormulasRecords.Items.ForEach(record => {
                string priceFormula = utils.ParseIDOPropertyValue<string>(record.PropertyValues[priceFormulasRecords.PropertyKeys["Priceformula"]]);
                if (!activePriceFormulaLookupTable.ContainsKey(priceFormula))
                {
                    activePriceFormulaLookupTable[priceFormula] = record;
                }
            });



            /********************************************************************/
            /* USE THE PRICE FORMULA RECORDS TO GET THE MULTIPLIER OR FIXED 
            /* PRICE FOR EACH ITEM PRICE CODE
            /********************************************************************/

            Dictionary<string, ItemPriceCodePriceInfo> priceCalculatorLookupTable = new Dictionary<string, ItemPriceCodePriceInfo>();
            priceMatrixRecords.Items.ForEach(priceMatrixRecord => {

                string itemPricecode = utils.ParseIDOPropertyValue<string>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["ItemPricecode"]]);
                DateTime priceMatrixRecordDate = utils.ParseIDOPropertyValue<DateTime>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["RecordDate"]]);
                string priceFormula = utils.ParseIDOPropertyValue<string>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["Priceformula"]]);

                if (!priceCalculatorLookupTable.ContainsKey(itemPricecode))
                {

                    IDOItem priceFormulaRecord = activePriceFormulaLookupTable[priceFormula];

                    priceCalculatorLookupTable[itemPricecode] = new ItemPriceCodePriceInfo(
                        priceCode: itemPricecode,
                        priceMatrixRecordDate: priceMatrixRecordDate,
                        type: utils.ParseIDOPropertyValue<string>(priceFormulaRecord.PropertyValues[priceFormulasRecords.PropertyKeys["FirstDolPercent"]]),
                        value: utils.ParseIDOPropertyValue<decimal>(priceFormulaRecord.PropertyValues[priceFormulasRecords.PropertyKeys["FirstPrice"]]),
                        priceFormulaRecordDate: utils.ParseIDOPropertyValue<DateTime>(priceFormulaRecord.PropertyValues[priceFormulasRecords.PropertyKeys["RecordDate"]])
                    );

                }

            });



            /********************************************************************/
            /* QUERY ITEM PRICES TO GET BASE RECORDS
            /********************************************************************/

            LoadRecordsResponseData itemPriceRecords = utils.LoadRecords(
                IDOName: "SLItemprices",
                properties: new List<string>() {
                    { "Item" },
                    { "Pricecode" },
                    { "UnitPrice1" },
                    { "EffectDate" },
                    { "RecordDate" },
                    { "RowPointer" },
                },
                filter: utils.BuildFilterString(itempriceQueryFilters.Values.ToList()),
                orderBy: "Item ASC, EffectDate DESC",
                recordCap: 0
            );



            /********************************************************************/
            /* CREATE EMPTY TABLE
            /********************************************************************/

            DataTable outputTable = new DataTable("FullTable");
            Dictionary<string, int> itemIndices = new Dictionary<string, int>();
            DataRow outputRow;

            // ADD COLUMN STRUCTURE

            outputTable.Columns.Add("PriceCode", typeof(string));
            outputTable.Columns.Add("Item", typeof(string));
            outputTable.Columns.Add("ListPrice", typeof(decimal));
            outputTable.Columns.Add("CustomerPrice", typeof(decimal));
            outputTable.Columns.Add("EffectDate", typeof(DateTime));
            outputTable.Columns.Add("RecordDate", typeof(DateTime));
            outputTable.Columns.Add("RowPointer", typeof(string));

            outputTable.PrimaryKey = new DataColumn[] { outputTable.Columns[6] };


            /********************************************************************/
            /* LOOP THROUGH THE ITEM PRICE RECORDS AND FILL IN THE DATA TABLE
            /********************************************************************/

            if (priceMatrixRecords.Items.Count > 0)
            {

                foreach (IDOItem itemPriceRecord in itemPriceRecords.Items)
                {

                    // GRAB THE ITEM

                    string item = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["Item"]]);

                    if (item != null && !itemIndices.ContainsKey(item))
                    {

                        string itemPricecode = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["Pricecode"]]);
                        decimal listPrice = utils.ParseIDOPropertyValue<decimal>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["UnitPrice1"]]);
                        decimal customerPrice = listPrice;
                        DateTime effectDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["EffectDate"]]);
                        DateTime recordDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["RecordDate"]]);
                        string rowPointer = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["RowPointer"]]);

                        // IF THERE IS A PRICE CODE, WE NEED TO GET THE CALCULATED CUSTOMER PRICE AND THE HIGHEST RECORD DATE FROM THE ITEMPRICE, PRICE MATRIX, AND PRICE FORMULA RECORDS

                        if (itemPricecode != null && priceCalculatorLookupTable.ContainsKey(itemPricecode))
                        {
                            customerPrice = priceCalculatorLookupTable[itemPricecode].GetPrice(listPrice);
                            recordDate = (new List<DateTime>() { recordDate, priceCalculatorLookupTable[itemPricecode].PriceFormulaRecordDate, priceCalculatorLookupTable[itemPricecode].PriceMatrixRecordDate }).Max();
                        }

                        // SAVE INDEX

                        itemIndices[item] = outputTable.Rows.Count;

                        // CREATE OUTPUT ROW

                        outputRow = outputTable.NewRow();

                        // FILL IN OUTPUT ROW

                        outputRow["PriceCode"] = custPriceCode;
                        outputRow["Item"] = item;
                        outputRow["ListPrice"] = listPrice;
                        outputRow["CustomerPrice"] = customerPrice;
                        outputRow["EffectDate"] = effectDate;
                        outputRow["RecordDate"] = recordDate;
                        outputRow["RowPointer"] = rowPointer;

                        // ADD ROW TO OUTPUT

                        outputTable.Rows.Add(outputRow);

                    }

                }

            }

            /********************************************************************/
            /* APPLY POST-FILTERS AND SORTING
            /********************************************************************/

            if (flags.arePostFiltering)
            {
                outputTable = utils.ApplyPostFilters(
                    fullTable: outputTable,
                    userPostQueryFilterString: userPostQueryFilterString
                );
            }

            outputTable.DefaultView.Sort = userRequest.OrderBy;
            outputTable = outputTable.DefaultView.ToTable();

            /********************************************************************/
            /* APPLY RECORD CAPPING AND CREATE BOOKMARK
            /********************************************************************/

            outputTable = utils.ApplyPaging(
                filteredTable: outputTable,
                userRequest: userRequest
            );

            return outputTable;

        }



        /**********************************************************************************************************/
        /**********************************************************************************************************/
        /*
        /* Name:     Example_04A_LoadPricesForCustomer_Matrix
        /* Date:     2025-11-19
        /* Authors:  Andy Mercer
        /* Purpose:  This example loads the all prices for a specified customer, taking into account their pricecode
        /*           and any customer contract prices they have. It does NOT filter which products they are allowed
        /*           to purchase, which is something that would have to be taken into account for real use.
        /*
        /* Copyright 2025, Functional Devices, Inc
        /*
        /**********************************************************************************************************/
        /**********************************************************************************************************/

        [IDOMethod(MethodFlags.CustomLoad)]
        public DataTable Example_04A_LoadPricesForCustomer_Matrix(string sFilter = null, string sOrderBy = null, string sRecordCap = null, string sBookmark = null)
        {

            /********************************************************************/
            /* SET UP HELPER VARIABLES
            /********************************************************************/

            Utilities utils = new Utilities(
                commands: this.Context.Commands
            );

            (bool haveBookmark, bool arePostFiltering, bool orderingByRowPointer, bool areCappingResults) flags = (false, false, false, false);



            /********************************************************************/
            /* LOAD USER INPUT FROM THE REQUEST OBJECT AND PARAMETERS IF SET
            /********************************************************************/

            LoadRecordsRequestData userRequest = new LoadRecordsRequestData(
                contextRequest: this.Context.Request as LoadCollectionRequestData,
                filterOverride: sFilter,
                orderByOverride: sOrderBy,
                recordCapOverride: sRecordCap,
                bookmarkOverride: sBookmark
            );

            userRequest.OrderBy = userRequest.OrderBy == "" ? "Item ASC, EffectDate DESC" : userRequest.OrderBy;

            flags.haveBookmark = userRequest.Bookmark != "<B/>";
            flags.orderingByRowPointer = userRequest.OrderBy == "RowPointer" || userRequest.OrderBy == "RowPointer ASC";
            flags.areCappingResults = userRequest.RecordCap != 0;



            /********************************************************************/
            /* PARSE FILTERS
            /********************************************************************/

            string custNum = "C000001";
            string userFilterValue;
            string userFilterOperator;

            Dictionary<string, string> itempriceQueryFilters = new Dictionary<string, string>() {
                { "Item", "" },
                { "EffectDate", "" },
                { "ListPrice", "" },
                { "RecordDate", "" },
                { "RowPointer", "" }
            };

            Dictionary<string, string> customerQueryFilters = new Dictionary<string, string>() {
                { "CustNum", "CustNum = '" + custNum + "'" }
            };

            Dictionary<string, string> priceMatrixQueryFilters = new Dictionary<string, string>() {
                { "CustPricecode", "CustPricecode = ''" }
            };

            Dictionary<string, string> postQueryFilters = new Dictionary<string, string>() {
                { "CustomerPrice", "" }
            };

            userRequest.Filters.ForEach(userFilter =>
            {

                userFilter = utils.FixParenthesis(userFilter);
                userFilterValue = utils.ExtractValue(userFilter);
                userFilterOperator = utils.ExtractOperator(userFilter);

                if (userFilter.Contains("CustNum"))
                {
                    customerQueryFilters["CustNum"] = userFilter;
                    custNum = userFilterValue;
                }

                if (userFilter.Contains("Item"))
                {
                    itempriceQueryFilters["Item"] = userFilter;
                }

                if (userFilter.Contains("EffectDate"))
                {
                    itempriceQueryFilters["EffectDate"] = userFilter;
                }

                if (userFilter.Contains("ListPrice"))
                {
                    itempriceQueryFilters["ListPrice"] = userFilter;
                }

                if (userFilter.Contains("CustomerPrice"))
                {
                    postQueryFilters["CustomerPrice"] = userFilter.Replace(" null ", " '' ");
                }

                if (userFilter.Contains("RecordDate"))
                {
                    postQueryFilters["RecordDate"] = userFilter;
                }

                if (userFilter.Contains("RowPointer"))
                {
                    itempriceQueryFilters["RowPointer"] = userFilter;
                }

            });

            if (flags.orderingByRowPointer && flags.haveBookmark)
            {
                postQueryFilters["RowPointer"] = "RowPointer > '" + userRequest.Bookmark + "'";
            }

            string userPostQueryFilterString = utils.BuildFilterString(postQueryFilters.Values.ToList());
            flags.arePostFiltering = userPostQueryFilterString != "";



            /********************************************************************/
            /* LOAD THE CUSTOMER RECORDS
            /********************************************************************/

            LoadRecordsResponseData customerRecords = utils.LoadRecords(
                IDOName: "SLCustomers",
                properties: new List<string>() {
                    { "CustNum" },
                    { "Name" },
                    { "Pricecode" }
                },
                filter: utils.BuildFilterString(customerQueryFilters.Values.ToList()),
                orderBy: "CustNum",
                recordCap: 1
            );

            if (customerRecords.Items.Count == 1)
            {
                string queriedCustNum = utils.ParseIDOPropertyValue<string>(customerRecords.Items[0].PropertyValues[customerRecords.PropertyKeys["CustNum"]]);
                string custPriceCode = utils.ParseIDOPropertyValue<string>(customerRecords.Items[0].PropertyValues[customerRecords.PropertyKeys["Pricecode"]]);
                if (queriedCustNum == custNum)
                {
                    priceMatrixQueryFilters["CustPricecode"] = "CustPricecode = '" + custPriceCode + "'";
                }
            }



            /********************************************************************/
            /* LOAD THE PRICE MATRIX RECORDS
            /********************************************************************/

            LoadRecordsResponseData priceMatrixRecords = utils.LoadRecords(
                IDOName: "SLPricematrixs",
                properties: new List<string>() {
                    { "CustPricecode" },
                    { "ItemPricecode" },
                    { "Priceformula" },
                    { "RecordDate" }
                },
                filter: utils.BuildFilterString(priceMatrixQueryFilters.Values.ToList()),
                orderBy: "CustPricecode, ItemPricecode"
            );

            string uniqueEscapedPriceFormulasCommaDel = priceMatrixRecords.Items.Count > 0 ? string.Join(
                ",",
                priceMatrixRecords.Items.Select(record =>
                {
                    return "'" + utils.ParseIDOPropertyValue<string>(record.PropertyValues[priceMatrixRecords.PropertyKeys["Priceformula"]]) + "'";
                }).Distinct()
            ) : "''";



            /********************************************************************/
            /* LOAD THE PRICE FORMULA RECORDS FOR THE LOADED PRICE MATRICES
            /********************************************************************/

            LoadRecordsResponseData priceFormulasRecords = utils.LoadRecords(
                IDOName: "SLPriceformulas",
                properties: new List<string>() {
                    { "Priceformula" },
                    { "FirstDolPercent" },
                    { "FirstPrice" },
                    { "EffectDate" },
                    { "RecordDate" }
                },
                filter: "Priceformula IN ( " + uniqueEscapedPriceFormulasCommaDel + " )",
                orderBy: "Priceformula ASC, EffectDate DESC"
            );

            Dictionary<string, IDOItem> activePriceFormulaLookupTable = new Dictionary<string, IDOItem>();
            priceFormulasRecords.Items.ForEach(record => {
                string priceFormula = utils.ParseIDOPropertyValue<string>(record.PropertyValues[priceFormulasRecords.PropertyKeys["Priceformula"]]);
                if (!activePriceFormulaLookupTable.ContainsKey(priceFormula))
                {
                    activePriceFormulaLookupTable[priceFormula] = record;
                }
            });



            /********************************************************************/
            /* USE THE PRICE FORMULA RECORDS TO GET THE MULTIPLIER OR FIXED 
            /* PRICE FOR EACH ITEM PRICE CODE
            /********************************************************************/

            Dictionary<string, ItemPriceCodePriceInfo> priceCalculatorLookupTable = new Dictionary<string, ItemPriceCodePriceInfo>();
            priceMatrixRecords.Items.ForEach(priceMatrixRecord => {

                string itemPricecode = utils.ParseIDOPropertyValue<string>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["ItemPricecode"]]);
                DateTime priceMatrixRecordDate = utils.ParseIDOPropertyValue<DateTime>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["RecordDate"]]);
                string priceFormula = utils.ParseIDOPropertyValue<string>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["Priceformula"]]);

                if (!priceCalculatorLookupTable.ContainsKey(itemPricecode))
                {

                    IDOItem priceFormulaRecord = activePriceFormulaLookupTable[priceFormula];

                    priceCalculatorLookupTable[itemPricecode] = new ItemPriceCodePriceInfo(
                        priceCode: itemPricecode,
                        priceMatrixRecordDate: priceMatrixRecordDate,
                        type: utils.ParseIDOPropertyValue<string>(priceFormulaRecord.PropertyValues[priceFormulasRecords.PropertyKeys["FirstDolPercent"]]),
                        value: utils.ParseIDOPropertyValue<decimal>(priceFormulaRecord.PropertyValues[priceFormulasRecords.PropertyKeys["FirstPrice"]]),
                        priceFormulaRecordDate: utils.ParseIDOPropertyValue<DateTime>(priceFormulaRecord.PropertyValues[priceFormulasRecords.PropertyKeys["RecordDate"]])
                    );

                }

            });



            /********************************************************************/
            /* QUERY ITEM PRICES TO GET BASE RECORDS
            /********************************************************************/

            LoadRecordsResponseData itemPriceRecords = utils.LoadRecords(
                IDOName: "SLItemprices",
                properties: new List<string>() {
                    { "Item" },
                    { "Pricecode" },
                    { "UnitPrice1" },
                    { "EffectDate" },
                    { "RecordDate" },
                    { "RowPointer" },
                },
                filter: utils.BuildFilterString(itempriceQueryFilters.Values.ToList()),
                orderBy: "Item ASC, EffectDate DESC",
                recordCap: 0
            );



            /********************************************************************/
            /* CREATE EMPTY TABLE
            /********************************************************************/

            DataTable outputTable = new DataTable("FullTable");
            Dictionary<string, int> itemIndices = new Dictionary<string, int>();
            DataRow outputRow;

            // ADD COLUMN STRUCTURE

            outputTable.Columns.Add("CustNum", typeof(string));
            outputTable.Columns.Add("Item", typeof(string));
            outputTable.Columns.Add("ListPrice", typeof(decimal));
            outputTable.Columns.Add("CustomerPrice", typeof(decimal));
            outputTable.Columns.Add("PriceType", typeof(string));
            outputTable.Columns.Add("EffectDate", typeof(DateTime));
            outputTable.Columns.Add("RecordDate", typeof(DateTime));
            outputTable.Columns.Add("RowPointer", typeof(string));



            /********************************************************************/
            /* LOOP THROUGH THE ITEM PRICE RECORDS AND FILL IN THE DATA TABLE
            /********************************************************************/

            if (customerRecords.Items.Count > 0 && priceMatrixRecords.Items.Count > 0)
            {

                foreach (IDOItem itemPriceRecord in itemPriceRecords.Items)
                {

                    // GRAB THE ITEM

                    string item = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["Item"]]);

                    if (item != null && !itemIndices.ContainsKey(item))
                    {

                        string itemPricecode = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["Pricecode"]]);
                        decimal listPrice = utils.ParseIDOPropertyValue<decimal>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["UnitPrice1"]]);
                        decimal customerPrice = listPrice;
                        string priceType = "List";
                        DateTime effectDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["EffectDate"]]);
                        DateTime recordDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["RecordDate"]]);
                        string rowPointer = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["RowPointer"]]);

                        // IF THERE IS A PRICE CODE, WE NEED TO GET THE CALCULATED CUSTOMER PRICE AND THE HIGHEST RECORD DATE FROM THE ITEMPRICE, PRICE MATRIX, AND PRICE FORMULA RECORDS

                        if (itemPricecode != null && priceCalculatorLookupTable.ContainsKey(itemPricecode))
                        {
                            customerPrice = priceCalculatorLookupTable[itemPricecode].GetPrice(listPrice);
                            recordDate = (new List<DateTime>() { recordDate, priceCalculatorLookupTable[itemPricecode].PriceFormulaRecordDate, priceCalculatorLookupTable[itemPricecode].PriceMatrixRecordDate }).Max();
                            priceType = "Matrix";
                        }

                        // SAVE INDEX

                        itemIndices[item] = outputTable.Rows.Count;

                        // CREATE OUTPUT ROW

                        outputRow = outputTable.NewRow();

                        // FILL IN OUTPUT ROW

                        outputRow["CustNum"] = custNum;
                        outputRow["Item"] = item;
                        outputRow["ListPrice"] = listPrice;
                        outputRow["CustomerPrice"] = customerPrice;
                        outputRow["PriceType"] = priceType;
                        outputRow["EffectDate"] = effectDate;
                        outputRow["RecordDate"] = recordDate;
                        outputRow["RowPointer"] = rowPointer;

                        // ADD ROW TO OUTPUT

                        outputTable.Rows.Add(outputRow);

                    }

                }

            }

            /********************************************************************/
            /* APPLY POST-FILTERS AND SORTING
            /********************************************************************/

            if (flags.arePostFiltering)
            {
                outputTable = utils.ApplyPostFilters(
                    fullTable: outputTable,
                    userPostQueryFilterString: userPostQueryFilterString
                );
            }

            outputTable.DefaultView.Sort = userRequest.OrderBy;
            outputTable = outputTable.DefaultView.ToTable();



            /********************************************************************/
            /* APPLY RECORD CAPPING AND CREATE BOOKMARK
            /********************************************************************/

            outputTable = utils.ApplyPaging(
                filteredTable: outputTable,
                userRequest: userRequest
            );

            return outputTable;

        }



        /**********************************************************************************************************/
        /**********************************************************************************************************/
        /*
        /* Name:     Example_04B_LoadPricesForCustomer_MatrixAndContract
        /* Date:     2025-11-19
        /* Authors:  Andy Mercer
        /* Purpose:  This example loads the all prices for a specified customer, taking into account their pricecode
        /*           and any customer contract prices they have. It does NOT filter which products they are allowed
        /*           to purchase, which is something that would have to be taken into account for real use.
        /*
        /* Copyright 2025, Functional Devices, Inc
        /*
        /**********************************************************************************************************/
        /**********************************************************************************************************/

        [IDOMethod(MethodFlags.CustomLoad)]
        public DataTable Example_04B_LoadPricesForCustomer_MatrixAndContract(string sFilter = null, string sOrderBy = null, string sRecordCap = null, string sBookmark = null)
        {

            /********************************************************************/
            /* SET UP HELPER VARIABLES
            /********************************************************************/

            Utilities utils = new Utilities(
                commands: this.Context.Commands
            );

            (bool haveBookmark, bool arePostFiltering, bool orderingByRowPointer, bool areCappingResults) flags = (false, false, false, false);



            /********************************************************************/
            /* LOAD USER INPUT FROM THE REQUEST OBJECT AND PARAMETERS IF SET
            /********************************************************************/

            LoadRecordsRequestData userRequest = new LoadRecordsRequestData(
                contextRequest: this.Context.Request as LoadCollectionRequestData,
                filterOverride: sFilter,
                orderByOverride: sOrderBy,
                recordCapOverride: sRecordCap,
                bookmarkOverride: sBookmark
            );

            userRequest.OrderBy = userRequest.OrderBy == "" ? "Item ASC, EffectDate DESC" : userRequest.OrderBy;

            flags.haveBookmark = userRequest.Bookmark != "<B/>";
            flags.orderingByRowPointer = userRequest.OrderBy == "RowPointer" || userRequest.OrderBy == "RowPointer ASC";
            flags.areCappingResults = userRequest.RecordCap != 0;



            /********************************************************************/
            /* PARSE FILTERS
            /********************************************************************/

            string custNum = "C000001";
            string userFilterValue;
            string userFilterOperator;

            List<string> userFilters = new List<string>();
            if (sFilter != null)
            {
                userFilters = sFilter.Split(
                    new string[] { "AND" }, StringSplitOptions.None
                ).Select(
                    sPropertyFilter =>
                        "( "
                        + sPropertyFilter
                            .Trim()
                            .Replace(" <> N", " <> ")
                            .Replace(" = N", " = ")
                            .Replace(" like N", " like ")
                            .Replace("DATEPART( yyyy, ", "YEAR( ")
                            .Replace("DATEPART( mm, ", "MONTH( ")
                            .Replace("DATEPART( dd, ", "DAY( ")
                        + " )"
                ).ToList();
            }

            Dictionary<string, string> itempriceQueryFilters = new Dictionary<string, string>() {
                { "Item", "" },
                { "EffectDate", "" },
                { "ListPrice", "" },
                { "RecordDate", "" },
                { "RowPointer", "" }
            };

            Dictionary<string, string> customerQueryFilters = new Dictionary<string, string>() {
                { "CustNum", "CustNum = '" + custNum + "'" }
            };

            Dictionary<string, string> priceMatrixQueryFilters = new Dictionary<string, string>() {
                { "CustPricecode", "CustPricecode = ''" }
            };

            Dictionary<string, string> itemcustpricesQueryFilters = new Dictionary<string, string>() {
                { "CustNum", "CustNum = '" + custNum + "'" },
                { "EffectDate", "" }
            };

            Dictionary<string, string> postQueryFilters = new Dictionary<string, string>() {
                { "CustomerPrice", "" },
                { "PriceType", ""}
            };

            userFilters.ForEach(userFilter =>
            {

                userFilter = utils.FixParenthesis(userFilter);
                userFilterValue = utils.ExtractValue(userFilter);
                userFilterOperator = utils.ExtractOperator(userFilter);

                if (userFilter.Contains("CustNum"))
                {
                    customerQueryFilters["CustNum"] = userFilter;
                    custNum = userFilterValue;
                    itemcustpricesQueryFilters["CustNum"] = userFilter;
                }

                if (userFilter.Contains("Item"))
                {
                    itempriceQueryFilters["Item"] = userFilter;
                }

                if (userFilter.Contains("EffectDate"))
                {
                    itempriceQueryFilters["EffectDate"] = userFilter;
                    itemcustpricesQueryFilters["EffectDate"] = userFilter;
                }

                if (userFilter.Contains("ListPrice"))
                {
                    itempriceQueryFilters["ListPrice"] = userFilter;
                }

                if (userFilter.Contains("CustomerPrice"))
                {
                    postQueryFilters["CustomerPrice"] = userFilter.Replace(" null ", " '' ");
                }

                if (userFilter.Contains("PriceType"))
                {
                    postQueryFilters["PriceType"] = userFilter;
                }

                if (userFilter.Contains("RecordDate"))
                {
                    postQueryFilters["RecordDate"] = userFilter;
                }

                if (userFilter.Contains("RowPointer"))
                {
                    itempriceQueryFilters["RowPointer"] = userFilter;
                }

            });

            if (flags.orderingByRowPointer && flags.haveBookmark)
            {
                postQueryFilters["RowPointer"] = "RowPointer > '" + userRequest.Bookmark + "'";
            }

            string userPostQueryFilterString = utils.BuildFilterString(postQueryFilters.Values.ToList());
            flags.arePostFiltering = userPostQueryFilterString != "";



            /********************************************************************/
            /* LOAD THE CUSTOMER RECORDS
            /********************************************************************/

            LoadRecordsResponseData customerRecords = utils.LoadRecords(
                IDOName: "SLCustomers",
                properties: new List<string>() {
                    { "CustNum" },
                    { "Pricecode" }
                },
                filter: utils.BuildFilterString(customerQueryFilters.Values.ToList()),
                orderBy: "CustNum",
                recordCap: 1
            );

            if (customerRecords.Items.Count == 1)
            {
                string queriedCustNum = utils.ParseIDOPropertyValue<string>(customerRecords.Items[0].PropertyValues[customerRecords.PropertyKeys["CustNum"]]);
                string custPriceCode = utils.ParseIDOPropertyValue<string>(customerRecords.Items[0].PropertyValues[customerRecords.PropertyKeys["Pricecode"]]);
                if (queriedCustNum == custNum)
                {
                    priceMatrixQueryFilters["CustPricecode"] = "CustPricecode = '" + custPriceCode + "'";
                }
            }



            /********************************************************************/
            /* LOAD THE PRICE MATRIX RECORDS
            /********************************************************************/

            LoadRecordsResponseData priceMatrixRecords = utils.LoadRecords(
                IDOName: "SLPricematrixs",
                properties: new List<string>() {
                    { "CustPricecode" },
                    { "ItemPricecode" },
                    { "Priceformula" },
                    { "RecordDate" }
                },
                filter: utils.BuildFilterString(priceMatrixQueryFilters.Values.ToList()),
                orderBy: "CustPricecode, ItemPricecode"
            );

            string uniqueEscapedPriceFormulasCommaDel = priceMatrixRecords.Items.Count > 0 ? string.Join(
                ",",
                priceMatrixRecords.Items.Select(record =>
                {
                    return "'" + utils.ParseIDOPropertyValue<string>(record.PropertyValues[priceMatrixRecords.PropertyKeys["Priceformula"]]) + "'";
                }).Distinct()
            ) : "''";



            /********************************************************************/
            /* LOAD THE PRICE FORMULA RECORDS FOR THE LOADED PRICE MATRICES
            /********************************************************************/

            LoadRecordsResponseData priceFormulasRecords = utils.LoadRecords(
                IDOName: "SLPriceformulas",
                properties: new List<string>() {
                    { "Priceformula" },
                    { "FirstDolPercent" },
                    { "FirstPrice" },
                    { "EffectDate" },
                    { "RecordDate" }
                },
                filter: "Priceformula IN ( " + uniqueEscapedPriceFormulasCommaDel + " )",
                orderBy: "Priceformula ASC, EffectDate DESC"
            );

            Dictionary<string, IDOItem> activePriceFormulaLookupTable = new Dictionary<string, IDOItem>();
            priceFormulasRecords.Items.ForEach(record => {
                string priceFormula = utils.ParseIDOPropertyValue<string>(record.PropertyValues[priceFormulasRecords.PropertyKeys["Priceformula"]]);
                if (!activePriceFormulaLookupTable.ContainsKey(priceFormula))
                {
                    activePriceFormulaLookupTable[priceFormula] = record;
                }
            });



            /********************************************************************/
            /* QUERY CUSTOMER CONTRACT PRICES TO GET PRICE AND DATE OVERRIDES
            /********************************************************************/

            LoadRecordsResponseData customerContractPriceRecords = utils.LoadRecords(
                IDOName: "SLItemCustPrices",
                properties: new List<string>() {
                    { "CustNum" },
                    { "Item" },
                    { "ContPrice" },
                    { "EffectDate" },
                    { "RecordDate"}
                },
                filter: utils.BuildFilterString(itemcustpricesQueryFilters.Values.ToList()),
                orderBy: "Item ASC, EffectDate DESC"
            );

            Dictionary<string, IDOItem> customerContractPriceIndexLookupTable = new Dictionary<string, IDOItem>();
            customerContractPriceRecords.Items.ForEach(customerContractPriceRecord => {
                string item = utils.ParseIDOPropertyValue<string>(customerContractPriceRecord.PropertyValues[customerContractPriceRecords.PropertyKeys["Item"]]);
                customerContractPriceIndexLookupTable[item] = customerContractPriceRecord;
            });



            /********************************************************************/
            /* USE THE PRICE FORMULA RECORDS TO GET THE MULTIPLIER OR FIXED 
            /* PRICE FOR EACH ITEM PRICE CODE
            /********************************************************************/

            Dictionary<string, ItemPriceCodePriceInfo> priceCalculatorLookupTable = new Dictionary<string, ItemPriceCodePriceInfo>();
            priceMatrixRecords.Items.ForEach(priceMatrixRecord => {

                string itemPricecode = utils.ParseIDOPropertyValue<string>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["ItemPricecode"]]);
                DateTime priceMatrixRecordDate = utils.ParseIDOPropertyValue<DateTime>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["RecordDate"]]);
                string priceFormula = utils.ParseIDOPropertyValue<string>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["Priceformula"]]);

                if (!priceCalculatorLookupTable.ContainsKey(itemPricecode))
                {

                    IDOItem priceFormulaRecord = activePriceFormulaLookupTable[priceFormula];

                    priceCalculatorLookupTable[itemPricecode] = new ItemPriceCodePriceInfo(
                        priceCode: itemPricecode,
                        priceMatrixRecordDate: priceMatrixRecordDate,
                        type: utils.ParseIDOPropertyValue<string>(priceFormulaRecord.PropertyValues[priceFormulasRecords.PropertyKeys["FirstDolPercent"]]),
                        value: utils.ParseIDOPropertyValue<decimal>(priceFormulaRecord.PropertyValues[priceFormulasRecords.PropertyKeys["FirstPrice"]]),
                        priceFormulaRecordDate: utils.ParseIDOPropertyValue<DateTime>(priceFormulaRecord.PropertyValues[priceFormulasRecords.PropertyKeys["RecordDate"]])
                    );

                }

            });



            /********************************************************************/
            /* QUERY ITEM PRICES TO GET BASE RECORDS
            /********************************************************************/

            LoadRecordsResponseData itemPriceRecords = utils.LoadRecords(
                IDOName: "SLItemprices",
                properties: new List<string>() {
                    { "Item" },
                    { "Pricecode" },
                    { "UnitPrice1" },
                    { "EffectDate" },
                    { "RecordDate" },
                    { "RowPointer" },
                },
                filter: utils.BuildFilterString(itempriceQueryFilters.Values.ToList()),
                orderBy: "Item ASC, EffectDate DESC",
                recordCap: 0
            );



            /********************************************************************/
            /* CREATE EMPTY TABLE
            /********************************************************************/

            DataTable outputTable = new DataTable("FullTable");
            Dictionary<string, int> itemIndices = new Dictionary<string, int>();
            DataRow outputRow;

            // ADD COLUMN STRUCTURE

            outputTable.Columns.Add("CustNum", typeof(string));
            outputTable.Columns.Add("Item", typeof(string));
            outputTable.Columns.Add("ListPrice", typeof(decimal));
            outputTable.Columns.Add("CustomerPrice", typeof(decimal));
            outputTable.Columns.Add("PriceType", typeof(string));
            outputTable.Columns.Add("EffectDate", typeof(DateTime));
            outputTable.Columns.Add("RecordDate", typeof(DateTime));
            outputTable.Columns.Add("RowPointer", typeof(string));



            /********************************************************************/
            /* LOOP THROUGH THE ITEM PRICE RECORDS AND FILL IN THE DATA TABLE
            /********************************************************************/

            foreach (IDOItem itemPriceRecord in itemPriceRecords.Items)
            {

                // GRAB THE ITEM

                string item = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["Item"]]);

                if (item != null && !itemIndices.ContainsKey(item))
                {

                    // LOAD DATA FROM THE ITEM PRICE RECORD

                    string itemPricecode = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["Pricecode"]]);
                    decimal listPrice = utils.ParseIDOPropertyValue<decimal>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["UnitPrice1"]]);
                    decimal customerPrice = listPrice;
                    string priceType = "List";
                    DateTime effectDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["EffectDate"]]);
                    DateTime recordDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["RecordDate"]]);
                    string rowPointer = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["RowPointer"]]);

                    // IF THERE IS A MATRIX PRICE FOR THIS ITEM'S PRICECODE, WE NEED TO GET THE DATA FROM IT

                    if (itemPricecode != null && priceCalculatorLookupTable.ContainsKey(itemPricecode))
                    {
                        customerPrice = priceCalculatorLookupTable[itemPricecode].GetPrice(listPrice);
                        priceType = "Matrix";
                        recordDate = (new List<DateTime>() {
                            recordDate,
                            priceCalculatorLookupTable[itemPricecode].PriceFormulaRecordDate,
                            priceCalculatorLookupTable[itemPricecode].PriceMatrixRecordDate
                        }).Max();
                    }

                    // IF THERE IS A CUSTOMER CONTRACT PRICE FOR THIS ITEM, WE NEED TO GET DATA FROM IT

                    if (item != null && customerContractPriceIndexLookupTable.ContainsKey(item))
                    {
                        IDOItem customerContractPriceRecord = customerContractPriceIndexLookupTable[item];
                        customerPrice = utils.ParseIDOPropertyValue<decimal>(customerContractPriceRecord.PropertyValues[customerContractPriceRecords.PropertyKeys["ContPrice"]]);
                        effectDate = utils.ParseIDOPropertyValue<DateTime>(customerContractPriceRecord.PropertyValues[customerContractPriceRecords.PropertyKeys["EffectDate"]]);
                        priceType = "Contract";
                        recordDate = (new List<DateTime>() {
                            recordDate,
                            utils.ParseIDOPropertyValue<DateTime>(customerContractPriceRecord.PropertyValues[customerContractPriceRecords.PropertyKeys["RecordDate"]])
                        }).Max();
                    }

                    // SAVE INDEX

                    itemIndices[item] = outputTable.Rows.Count;

                    // CREATE OUTPUT ROW

                    outputRow = outputTable.NewRow();

                    // FILL IN OUTPUT ROW

                    outputRow["CustNum"] = custNum;
                    outputRow["Item"] = item;
                    outputRow["ListPrice"] = listPrice;
                    outputRow["CustomerPrice"] = customerPrice;
                    outputRow["PriceType"] = priceType;
                    outputRow["EffectDate"] = effectDate;
                    outputRow["RecordDate"] = recordDate;
                    outputRow["RowPointer"] = rowPointer;

                    // ADD ROW TO OUTPUT

                    outputTable.Rows.Add(outputRow);

                }

            }



            /********************************************************************/
            /* APPLY POST-FILTERS AND SORTING
            /********************************************************************/

            if (flags.arePostFiltering)
            {
                outputTable = utils.ApplyPostFilters(
                    fullTable: outputTable,
                    userPostQueryFilterString: userPostQueryFilterString
                );
            }

            outputTable.DefaultView.Sort = userRequest.OrderBy;
            outputTable = outputTable.DefaultView.ToTable();



            /********************************************************************/
            /* APPLY RECORD CAPPING AND CREATE BOOKMARK
            /********************************************************************/

            outputTable = utils.ApplyPaging(
                filteredTable: outputTable,
                userRequest: userRequest
            );

            return outputTable;

        }

    }

}