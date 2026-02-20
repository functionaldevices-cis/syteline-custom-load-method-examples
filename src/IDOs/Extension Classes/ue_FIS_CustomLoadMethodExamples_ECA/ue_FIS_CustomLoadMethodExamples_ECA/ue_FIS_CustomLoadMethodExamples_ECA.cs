using Mongoose.IDO;
using Mongoose.IDO.DataAccess;
using Mongoose.IDO.Metadata;
using Mongoose.IDO.Protocol;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
    /* Free to use under MIT License, Copyright (c) 2025 FDI Information Systems. See full license at
    /* https://github.com/functionaldevices-cis/syteline-custom-load-method-examples?tab=MIT-1-ov-file#readme
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
        /* Purpose:  This example loads 10 records from the SLItemPrices IDO. It returns the bound properties of Item,
        /*           UnitPrice1, UnitPrice2, EffectDate, RecordDate, and RowPointer,, plus calculated properties of
        /*           ItemReversed, UnitPriceDoubled1, UnitPriceDoubled2, EffectDateMinus1Day, EffectDateIsWeekday, and
        /*           EffectDateIsWeekend. The calculated properties range in type, to demonstrate different possibilities
        /*           that could be calculated.
        /*
        /*           Version A has hardcoded sort order and record cap, and no filtering.
        /*
        /* Free to use under MIT License, Copyright (c) 2025 FDI Information Systems. See full license at
        /* https://github.com/functionaldevices-cis/syteline-custom-load-method-examples?tab=MIT-1-ov-file#readme
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
        /* Free to use under MIT License, Copyright (c) 2025 FDI Information Systems. See full license at
        /* https://github.com/functionaldevices-cis/syteline-custom-load-method-examples?tab=MIT-1-ov-file#readme
        /*
        /**********************************************************************************************************/
        /**********************************************************************************************************/

        [IDOMethod(MethodFlags.CustomLoad)]
        public DataTable Example_01B_LoadItemPrices_Pagination(string sFilter = null, string sOrderBy = null, string sRecordCap = null, string sBookmark = null)
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
        /* Free to use under MIT License, Copyright (c) 2025 FDI Information Systems. See full license at
        /* https://github.com/functionaldevices-cis/syteline-custom-load-method-examples?tab=MIT-1-ov-file#readme
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

            Dictionary<string, IIDOPropertyFilterSet> itempriceQueryFilters = new List<IIDOPropertyFilterSet>() {
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "Item"
                ),
                new IDOPropertyFilterSet<decimal>(
                    outputPropertyName: "UnitPrice1"
                ),
                new IDOPropertyFilterSet<decimal>(
                    outputPropertyName: "UnitPrice2"
                ),
                new IDOPropertyFilterSet<DateTime>(
                    outputPropertyName: "EffectDate"
                ),
                new IDOPropertyFilterSet<DateTime>(
                    outputPropertyName: "RecordDate"
                ),
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "RowPointer"
                )
            }.ToDictionary(f => f.OutputPropertyName);

            userRequest.Filters.ForEach(userFilter =>
            {

                if (itempriceQueryFilters.Keys.Contains(userFilter.propertyName))
                {
                    itempriceQueryFilters[userFilter.propertyName].AddFilter(
                        filter: userFilter
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
        /* Free to use under MIT License, Copyright (c) 2025 FDI Information Systems. See full license at
        /* https://github.com/functionaldevices-cis/syteline-custom-load-method-examples?tab=MIT-1-ov-file#readme
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

            Dictionary<string, IIDOPropertyFilterSet> itempriceQueryFilters = new List<IIDOPropertyFilterSet>() {
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "Item"
                ),
                new IDOPropertyFilterSet<decimal>(
                    outputPropertyName: "UnitPrice1"
                ),
                new IDOPropertyFilterSet<decimal>(
                    outputPropertyName: "UnitPrice2"
                ),
                new IDOPropertyFilterSet<DateTime>(
                    outputPropertyName: "EffectDate"
                ),
                new IDOPropertyFilterSet<DateTime>(
                    outputPropertyName: "RecordDate"
                ),
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "RowPointer"
                )
            }.ToDictionary(f => f.OutputPropertyName);

            Dictionary<string, IIDOPropertyFilterSet> inlineFilters = new List<IIDOPropertyFilterSet>() {
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "ItemReversed"
                ),
                new IDOPropertyFilterSet<decimal>(
                    outputPropertyName: "UnitPriceDoubled1"
                ),
                new IDOPropertyFilterSet<decimal>(
                    outputPropertyName: "UnitPriceDoubled2"
                ),
                new IDOPropertyFilterSet<DateTime>(
                    outputPropertyName: "EffectDateMinus1Day"
                ),
                new IDOPropertyFilterSet<int>(
                    outputPropertyName: "EffectDateIsWeekday"
                ),
                new IDOPropertyFilterSet<int>(
                    outputPropertyName: "EffectDateIsWeekend"
                )
            }.ToDictionary(f => f.OutputPropertyName);

            userRequest.Filters.ForEach(userFilter =>
            {

                if (itempriceQueryFilters.Keys.Contains(userFilter.propertyName))
                {
                    itempriceQueryFilters[userFilter.propertyName].AddFilter(
                        filter: userFilter
                    );
                }

                if (inlineFilters.Keys.Contains(userFilter.propertyName))
                {
                    inlineFilters[userFilter.propertyName].AddFilter(
                        filter: userFilter
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
        /*           highest non-future effective date for each unique Item value. Nearly all filters have to be
        /*           inline filters, because we are loading multiple item price records for the same item, and only
        /*           keeping the current one.
        /*
        /* Free to use under MIT License, Copyright (c) 2025 FDI Information Systems. See full license at
        /* https://github.com/functionaldevices-cis/syteline-custom-load-method-examples?tab=MIT-1-ov-file#readme
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
            DateTime tomorrow = DateTime.Now.AddDays(1).Date;
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

            Dictionary<string, IIDOPropertyFilterSet> itempriceQueryFilters = new List<IIDOPropertyFilterSet>() {
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "Item"
                ),
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "EffectDateHardcoded", // THIS MUST BE APPLIED REGARDLESS OF WHAT THE USER CHOOSES AS EFFECTIVE DATE FILTER, SO IT'S NAME MATCHES NOTHING. THEREFORE IT CANNOT BE OVERRIDDEN
                    defaultFilter: $"EffectDate < '{tomorrow.ToString("yyyyMMdd HH:mm:ss.fff")}'"
                )
            }.ToDictionary(f => f.OutputPropertyName);

            Dictionary<string, IIDOPropertyFilterSet> inlineFilters = new List<IIDOPropertyFilterSet>() {
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "ItemReversed"
                ),
                new IDOPropertyFilterSet<decimal>(
                    outputPropertyName: "UnitPrice1"
                ),
                new IDOPropertyFilterSet<decimal>(
                    outputPropertyName: "UnitPriceDoubled1"
                ),
                new IDOPropertyFilterSet<decimal>(
                    outputPropertyName: "UnitPrice2"
                ),
                new IDOPropertyFilterSet<decimal>(
                    outputPropertyName: "UnitPriceDoubled2"
                ),
                new IDOPropertyFilterSet<DateTime>(
                    outputPropertyName: "EffectDate"
                ),
                new IDOPropertyFilterSet<DateTime>(
                    outputPropertyName: "EffectDateMinus1Day"
                ),
                new IDOPropertyFilterSet<int>(
                    outputPropertyName: "EffectDateIsWeekday"
                ),
                new IDOPropertyFilterSet<int>(
                    outputPropertyName: "EffectDateIsWeekend"
                ),
                new IDOPropertyFilterSet<DateTime>(
                    outputPropertyName: "RecordDate"
                ),
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "RowPointer"
                )
            }.ToDictionary(f => f.OutputPropertyName);

            userRequest.Filters.ForEach(userFilter =>
            {

                if (itempriceQueryFilters.Keys.Contains(userFilter.propertyName))
                {
                    itempriceQueryFilters[userFilter.propertyName].AddFilter(
                        filter: userFilter
                    );
                }

                if (inlineFilters.Keys.Contains(userFilter.propertyName))
                {
                    inlineFilters[userFilter.propertyName].AddFilter(
                        filter: userFilter
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
        /* Free to use under MIT License, Copyright (c) 2025 FDI Information Systems. See full license at
        /* https://github.com/functionaldevices-cis/syteline-custom-load-method-examples?tab=MIT-1-ov-file#readme
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
            /* LOAD USER INPUT FROM THE REQUEST OBJECT AND PARAMETERS IF SET
            /********************************************************************/

            (bool haveBookmark, bool areCappingResults) flags = (false, false);
            
            int iStartingCounterItems = 0;
            int iCounterItems = 0;
            DateTime tomorrow = DateTime.Now.AddDays(1).Date;
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

            string custPriceCode = "Y00"; // !! CHANGE THIS TO WHATEVER YOUR BASE LIST PRICE CODE IS !!


            Dictionary<string, IIDOPropertyFilterSet> itempriceQueryFilters = new List<IIDOPropertyFilterSet>() {
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "Item"
                ),
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "EffectDateHardcoded", // THIS MUST BE APPLIED REGARDLESS OF WHAT THE USER CHOOSES AS EFFECTIVE DATE FILTER, SO IT'S NAME MATCHES NOTHING. THEREFORE IT CANNOT BE OVERRIDDEN
                    defaultFilter: $"EffectDate < '{tomorrow.ToString("yyyyMMdd HH:mm:ss.fff")}'"
                )
            }.ToDictionary(f => f.OutputPropertyName);

            Dictionary<string, IIDOPropertyFilterSet> priceMatrixQueryFilters = new List<IIDOPropertyFilterSet>() {
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "PriceCode",
                    sourcePropertyName: "CustPricecode",
                    defaultFilter: "CustPricecode = '" + custPriceCode + "'"
                )
            }.ToDictionary(f => f.OutputPropertyName);

            Dictionary<string, IIDOPropertyFilterSet> inlineFilters = new List<IIDOPropertyFilterSet>() {
                new IDOPropertyFilterSet<decimal>(
                    outputPropertyName: "CustomerPrice"
                ),
                new IDOPropertyFilterSet<decimal>(
                    outputPropertyName: "ListPrice"
                ),
                new IDOPropertyFilterSet<DateTime>(
                    outputPropertyName: "EffectDate"
                ),
                new IDOPropertyFilterSet<DateTime>(
                    outputPropertyName: "RecordDate"
                ),
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "RowPointer"
                )
            }.ToDictionary(f => f.OutputPropertyName);

            userRequest.Filters.ForEach(userFilter =>
            {

                if (itempriceQueryFilters.Keys.Contains(userFilter.propertyName))
                {
                    itempriceQueryFilters[userFilter.propertyName].AddFilter(
                        filter: userFilter
                    );
                }

                if (priceMatrixQueryFilters.Keys.Contains(userFilter.propertyName))
                {

                    priceMatrixQueryFilters[userFilter.propertyName].AddFilter(
                        filter: userFilter
                    );
                }

                if (inlineFilters.Keys.Contains(userFilter.propertyName))
                {
                    inlineFilters[userFilter.propertyName].AddFilter(
                        filter: userFilter
                    );
                }

                if (userFilter.propertyName == "PriceCode")
                {
                    custPriceCode = userFilter.value;
                }

            });



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
                filter: "",
                orderBy: "Priceformula ASC, EffectDate DESC"
            );

            Dictionary<string, IDOItem> activePriceFormulaLookupTable = new Dictionary<string, IDOItem>();
            priceFormulasRecords.Items.ForEach(record => {
                string priceformula = utils.ParseIDOPropertyValue<string>(record.PropertyValues[priceFormulasRecords.PropertyKeys["Priceformula"]]);
                if (!activePriceFormulaLookupTable.ContainsKey(priceformula))
                {
                    activePriceFormulaLookupTable[priceformula] = record;
                }
            });



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
                filter: utils.BuildFilterString(priceMatrixQueryFilters.Values.Select(filter => filter.GetFilterString()).ToList()),
                orderBy: "CustPricecode, ItemPricecode"
            );

            priceMatrixRecords.AddProperty("FirstDolPercent");
            priceMatrixRecords.AddProperty("FirstPrice");
            priceMatrixRecords.AddProperty("EffectDate");
            priceMatrixRecords.AddProperty("PriceFormulaRecordDate");

            Dictionary<string, IDOItem> priceMatrixLookupTable = new Dictionary<string, IDOItem>();
            priceMatrixRecords.Items.ForEach(priceMatrixRecord =>
            {

                string itemPricecode = utils.ParseIDOPropertyValue<string>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["ItemPricecode"]]);
                string priceformula = utils.ParseIDOPropertyValue<string>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["Priceformula"]]);

                if (!priceMatrixLookupTable.ContainsKey(itemPricecode))
                {

                    string firstDolPercent = null;
                    decimal? firstPrice = null;
                    DateTime? effectDate = null;
                    DateTime? recordDate = null;

                    if (activePriceFormulaLookupTable.ContainsKey(priceformula))
                    {
                        IDOItem priceformulaRecord = activePriceFormulaLookupTable[priceformula];
                        firstDolPercent = utils.ParseIDOPropertyValue<string>(priceformulaRecord.PropertyValues[priceFormulasRecords.PropertyKeys["FirstDolPercent"]]);
                        firstPrice = utils.ParseIDOPropertyValue<decimal?>(priceformulaRecord.PropertyValues[priceFormulasRecords.PropertyKeys["FirstPrice"]]);
                        effectDate = utils.ParseIDOPropertyValue<DateTime?>(priceformulaRecord.PropertyValues[priceFormulasRecords.PropertyKeys["EffectDate"]]);
                        recordDate = utils.ParseIDOPropertyValue<DateTime?>(priceformulaRecord.PropertyValues[priceFormulasRecords.PropertyKeys["RecordDate"]]);
                    }

                    priceMatrixRecord.PropertyValues.Add(firstDolPercent);
                    priceMatrixRecord.PropertyValues.Add(firstPrice);
                    priceMatrixRecord.PropertyValues.Add(effectDate);
                    priceMatrixRecord.PropertyValues.Add(recordDate);

                    priceMatrixLookupTable[itemPricecode] = priceMatrixRecord;

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

                    string itemPricecode = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["Pricecode"]]);
                    decimal listPrice = utils.ParseIDOPropertyValue<decimal>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["UnitPrice1"]]);
                    decimal customerPrice = listPrice;
                    DateTime effectDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["EffectDate"]]);
                    DateTime recordDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["RecordDate"]]);
                    string rowPointer = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["RowPointer"]]);

                    // IF THERE IS A PRICE CODE, WE NEED TO GET THE CALCULATED CUSTOMER PRICE AND THE HIGHEST RECORD DATE FROM THE ITEMPRICE, PRICE MATRIX, AND PRICE FORMULA RECORDS

                    if (itemPricecode != null && priceMatrixLookupTable.ContainsKey(itemPricecode))
                    {
                        IDOItem priceMatrixRecord = priceMatrixLookupTable[itemPricecode];

                        string matrixType = utils.ParseIDOPropertyValue<string>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["FirstDolPercent"]]);
                        decimal matrixValue = utils.ParseIDOPropertyValue<decimal>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["FirstPrice"]]);

                        if (matrixType == "A")
                        {
                            customerPrice = matrixValue;
                        }
                        else if (matrixType == "P")
                        {
                            customerPrice = Math.Round(listPrice * (100m + matrixValue) / 100m, 2, MidpointRounding.AwayFromZero);
                        }

                        recordDate = (new List<DateTime>() {
                            recordDate,
                            utils.ParseIDOPropertyValue<DateTime>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["RecordDate"]]),
                            utils.ParseIDOPropertyValue<DateTime>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["PriceFormulaRecordDate"]])
                        }).Max();
                    }

                    // RUN ALL INLINE FILTERS

                    bool passesInlineFilters = true;

                    if (inlineFilters["ListPrice"].ValueFails(listPrice))
                    {
                        passesInlineFilters = false;
                    }

                    if (inlineFilters["CustomerPrice"].ValueFails(customerPrice))
                    {
                        passesInlineFilters = false;
                    }

                    if (inlineFilters["EffectDate"].ValueFails(effectDate))
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
        /* Name:     Example_04A_LoadPricesForCustomer_Matrix
        /* Date:     2025-11-19
        /* Authors:  Andy Mercer
        /* Purpose:  This example loads the all prices for a specified customer, taking into account their pricecode
        /*           and any customer contract prices they have. It does NOT filter which products they are allowed
        /*           to purchase, which is something that would have to be taken into account for real use.
        /*
        /* Free to use under MIT License, Copyright (c) 2025 FDI Information Systems. See full license at
        /* https://github.com/functionaldevices-cis/syteline-custom-load-method-examples?tab=MIT-1-ov-file#readme
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
            /* LOAD USER INPUT FROM THE REQUEST OBJECT AND PARAMETERS IF SET
            /********************************************************************/

            (bool haveBookmark, bool areCappingResults) flags = (false, false);

            int iStartingCounterItems = 0;
            int iCounterItems = 0;
            DateTime tomorrow = DateTime.Now.AddDays(1).Date;
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

            string custNum = "C000001";

            Dictionary<string, IIDOPropertyFilterSet> itempriceQueryFilters = new List<IIDOPropertyFilterSet>() {
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "Item"
                ),
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "EffectDateHardcoded", // THIS MUST BE APPLIED REGARDLESS OF WHAT THE USER CHOOSES AS EFFECTIVE DATE FILTER, SO IT'S NAME MATCHES NOTHING. THEREFORE IT CANNOT BE OVERRIDDEN
                    defaultFilter: $"EffectDate < '{tomorrow.ToString("yyyyMMdd HH:mm:ss.fff")}'"
                )
            }.ToDictionary(f => f.OutputPropertyName);

            Dictionary<string, IIDOPropertyFilterSet> customerQueryFilters = new List<IIDOPropertyFilterSet>() {
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "CustNum",
                    defaultFilter: "CustNum = '" + custNum + "'"
                )
            }.ToDictionary(f => f.OutputPropertyName);

            Dictionary<string, IIDOPropertyFilterSet> priceMatrixQueryFilters = new List<IIDOPropertyFilterSet>() {
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "PriceCode",
                    sourcePropertyName: "CustPricecode",
                    defaultFilter: "CustPricecode = ''"
                )
            }.ToDictionary(f => f.OutputPropertyName);

            Dictionary<string, IIDOPropertyFilterSet> inlineFilters = new List<IIDOPropertyFilterSet>() {
                new IDOPropertyFilterSet<decimal>(
                    outputPropertyName: "CustomerPrice"
                ),
                new IDOPropertyFilterSet<decimal>(
                    outputPropertyName: "ListPrice"
                ),
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "PriceType"
                ),
                new IDOPropertyFilterSet<DateTime>(
                    outputPropertyName: "EffectDate"
                ),
                new IDOPropertyFilterSet<DateTime>(
                    outputPropertyName: "RecordDate"
                ),
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "RowPointer"
                )
            }.ToDictionary(f => f.OutputPropertyName);

            userRequest.Filters.ForEach(userFilter =>
            {

                if (itempriceQueryFilters.Keys.Contains(userFilter.propertyName))
                {
                    itempriceQueryFilters[userFilter.propertyName].AddFilter(
                        filter: userFilter
                    );
                }

                if (customerQueryFilters.Keys.Contains(userFilter.propertyName))
                {

                    customerQueryFilters[userFilter.propertyName].AddFilter(
                        filter: userFilter
                    );
                }

                if (priceMatrixQueryFilters.Keys.Contains(userFilter.propertyName))
                {

                    itempriceQueryFilters[userFilter.propertyName].AddFilter(
                        filter: userFilter
                    );
                }

                if (inlineFilters.Keys.Contains(userFilter.propertyName))
                {
                    inlineFilters[userFilter.propertyName].AddFilter(
                        filter: userFilter
                    );
                }

                if (userFilter.propertyName == "CustNum")
                {
                    custNum = userFilter.value;
                }

            });





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
                filter: utils.BuildFilterString(customerQueryFilters.Values.Select(filter => filter.GetFilterString()).ToList()),
                orderBy: "CustNum",
                recordCap: 1
            );

            if (customerRecords.Items.Count == 1)
            {
                string queriedCustNum = utils.ParseIDOPropertyValue<string>(customerRecords.Items[0].PropertyValues[customerRecords.PropertyKeys["CustNum"]]);
                string custPriceCode = utils.ParseIDOPropertyValue<string>(customerRecords.Items[0].PropertyValues[customerRecords.PropertyKeys["Pricecode"]]);
                if (queriedCustNum == custNum)
                {
                    priceMatrixQueryFilters["PriceCode"].OverwriteFilter("CustPricecode = '" + custPriceCode + "'");
                }
            }



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
                filter: "",
                orderBy: "Priceformula ASC, EffectDate DESC"
            );

            Dictionary<string, IDOItem> activePriceFormulaLookupTable = new Dictionary<string, IDOItem>();
            priceFormulasRecords.Items.ForEach(record => {
                string priceformula = utils.ParseIDOPropertyValue<string>(record.PropertyValues[priceFormulasRecords.PropertyKeys["Priceformula"]]);
                if (!activePriceFormulaLookupTable.ContainsKey(priceformula))
                {
                    activePriceFormulaLookupTable[priceformula] = record;
                }
            });



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
                filter: utils.BuildFilterString(priceMatrixQueryFilters.Values.Select(filter => filter.GetFilterString()).ToList()),
                orderBy: "CustPricecode, ItemPricecode"
            );

            priceMatrixRecords.AddProperty("FirstDolPercent");
            priceMatrixRecords.AddProperty("FirstPrice");
            priceMatrixRecords.AddProperty("EffectDate");
            priceMatrixRecords.AddProperty("PriceFormulaRecordDate");

            Dictionary<string, IDOItem> priceMatrixLookupTable = new Dictionary<string, IDOItem>();
            priceMatrixRecords.Items.ForEach(priceMatrixRecord =>
            {

                string itemPricecode = utils.ParseIDOPropertyValue<string>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["ItemPricecode"]]);
                string priceformula = utils.ParseIDOPropertyValue<string>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["Priceformula"]]);

                if (!priceMatrixLookupTable.ContainsKey(itemPricecode))
                {

                    string firstDolPercent = null;
                    decimal? firstPrice = null;
                    DateTime? effectDate = null;
                    DateTime? recordDate = null;

                    if (activePriceFormulaLookupTable.ContainsKey(priceformula))
                    {
                        IDOItem priceformulaRecord = activePriceFormulaLookupTable[priceformula];
                        firstDolPercent = utils.ParseIDOPropertyValue<string>(priceformulaRecord.PropertyValues[priceFormulasRecords.PropertyKeys["FirstDolPercent"]]);
                        firstPrice = utils.ParseIDOPropertyValue<decimal?>(priceformulaRecord.PropertyValues[priceFormulasRecords.PropertyKeys["FirstPrice"]]);
                        effectDate = utils.ParseIDOPropertyValue<DateTime?>(priceformulaRecord.PropertyValues[priceFormulasRecords.PropertyKeys["EffectDate"]]);
                        recordDate = utils.ParseIDOPropertyValue<DateTime?>(priceformulaRecord.PropertyValues[priceFormulasRecords.PropertyKeys["RecordDate"]]);
                    }

                    priceMatrixRecord.PropertyValues.Add(firstDolPercent);
                    priceMatrixRecord.PropertyValues.Add(firstPrice);
                    priceMatrixRecord.PropertyValues.Add(effectDate);
                    priceMatrixRecord.PropertyValues.Add(recordDate);

                    priceMatrixLookupTable[itemPricecode] = priceMatrixRecord;

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

                    string itemPricecode = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["Pricecode"]]);
                    decimal listPrice = utils.ParseIDOPropertyValue<decimal>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["UnitPrice1"]]);
                    decimal customerPrice = listPrice;
                    string priceType = "List";
                    DateTime effectDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["EffectDate"]]);
                    DateTime recordDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["RecordDate"]]);
                    string rowPointer = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["RowPointer"]]);

                    // IF THERE IS A PRICE CODE, WE NEED TO GET THE CALCULATED CUSTOMER PRICE AND THE HIGHEST RECORD DATE FROM THE ITEMPRICE, PRICE MATRIX, AND PRICE FORMULA RECORDS

                    if (itemPricecode != null && priceMatrixLookupTable.ContainsKey(itemPricecode))
                    {
                        IDOItem priceMatrixRecord = priceMatrixLookupTable[itemPricecode];

                        string matrixType = utils.ParseIDOPropertyValue<string>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["FirstDolPercent"]]);
                        decimal matrixValue = utils.ParseIDOPropertyValue<decimal>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["FirstPrice"]]);

                        if (matrixType == "A")
                        {
                            customerPrice = matrixValue;
                        }
                        else if (matrixType == "P")
                        {
                            customerPrice = Math.Round(listPrice * (100m + matrixValue) / 100m, 2, MidpointRounding.AwayFromZero);
                        }

                        priceType = "Matrix";
                        recordDate = (new List<DateTime>() {
                            recordDate,
                            utils.ParseIDOPropertyValue<DateTime>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["RecordDate"]]),
                            utils.ParseIDOPropertyValue<DateTime>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["PriceFormulaRecordDate"]])
                        }).Max();
                    }

                    // RUN ALL INLINE FILTERS

                    bool passesInlineFilters = true;

                    if (inlineFilters["ListPrice"].ValueFails(listPrice))
                    {
                        passesInlineFilters = false;
                    }

                    if (inlineFilters["CustomerPrice"].ValueFails(customerPrice))
                    {
                        passesInlineFilters = false;
                    }

                    if (inlineFilters["PriceType"].ValueFails(priceType))
                    {
                        passesInlineFilters = false;
                    }

                    if (inlineFilters["EffectDate"].ValueFails(effectDate))
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
        /* Name:     Example_04B_LoadPricesForCustomer_MatrixAndContract
        /* Date:     2025-11-19
        /* Authors:  Andy Mercer
        /* Purpose:  This example loads the all prices for a specified customer, taking into account their pricecode
        /*           and any customer contract prices they have. It does NOT filter which products they are allowed
        /*           to purchase, which is something that would have to be taken into account for real use.
        /*
        /* Free to use under MIT License, Copyright (c) 2025 FDI Information Systems. See full license at
        /* https://github.com/functionaldevices-cis/syteline-custom-load-method-examples?tab=MIT-1-ov-file#readme
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
            /* LOAD USER INPUT FROM THE REQUEST OBJECT AND PARAMETERS IF SET
            /********************************************************************/

            (bool haveBookmark, bool areCappingResults) flags = (false, false);

            int iStartingCounterItems = 0;
            int iCounterItems = 0;
            DateTime tomorrow = DateTime.Now.AddDays(1).Date;
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

            string custNum = "C000001";

            Dictionary<string, IIDOPropertyFilterSet> itempriceQueryFilters = new List<IIDOPropertyFilterSet>() {
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "Item"
                ),
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "EffectDateHardcoded", // THIS MUST BE APPLIED REGARDLESS OF WHAT THE USER CHOOSES AS EFFECTIVE DATE FILTER, SO IT'S NAME MATCHES NOTHING. THEREFORE IT CANNOT BE OVERRIDDEN
                    defaultFilter: $"EffectDate < '{tomorrow.ToString("yyyyMMdd HH:mm:ss.fff")}'"
                )
            }.ToDictionary(f => f.OutputPropertyName);

            Dictionary<string, IIDOPropertyFilterSet> customerQueryFilters = new List<IIDOPropertyFilterSet>() {
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "CustNum",
                    defaultFilter: "CustNum = '" + custNum + "'"
                )
            }.ToDictionary(f => f.OutputPropertyName);

            Dictionary<string, IIDOPropertyFilterSet> itemcustpricesQueryFilters = new List<IIDOPropertyFilterSet>() {
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "CustNum",
                    defaultFilter: "CustNum = '" + custNum + "'"
                ),
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "EffectDateHardcoded", // THIS MUST BE APPLIED REGARDLESS OF WHAT THE USER CHOOSES AS EFFECTIVE DATE FILTER, SO IT'S NAME MATCHES NOTHING. THEREFORE IT CANNOT BE OVERRIDDEN
                    defaultFilter: $"EffectDate < '{tomorrow.ToString("yyyyMMdd HH:mm:ss.fff")}'"
                )
            }.ToDictionary(f => f.OutputPropertyName);

            Dictionary<string, IIDOPropertyFilterSet> priceMatrixQueryFilters = new List<IIDOPropertyFilterSet>() {
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "PriceCode",
                    sourcePropertyName: "CustPricecode",
                    defaultFilter: "CustPricecode = ''"
                )
            }.ToDictionary(f => f.OutputPropertyName);

            Dictionary<string, IIDOPropertyFilterSet> inlineFilters = new List<IIDOPropertyFilterSet>() {
                new IDOPropertyFilterSet<decimal>(
                    outputPropertyName: "CustomerPrice"
                ),
                new IDOPropertyFilterSet<decimal>(
                    outputPropertyName: "ListPrice"
                ),
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "PriceType"
                ),
                new IDOPropertyFilterSet<DateTime>(
                    outputPropertyName: "EffectDate"
                ),
                new IDOPropertyFilterSet<DateTime>(
                    outputPropertyName: "RecordDate"
                ),
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "RowPointer"
                )
            }.ToDictionary(f => f.OutputPropertyName);

            userRequest.Filters.ForEach(userFilter =>
            {

                if (itempriceQueryFilters.Keys.Contains(userFilter.propertyName))
                {
                    itempriceQueryFilters[userFilter.propertyName].AddFilter(
                        filter: userFilter
                    );
                }

                if (customerQueryFilters.Keys.Contains(userFilter.propertyName))
                {
                    customerQueryFilters[userFilter.propertyName].AddFilter(
                        filter: userFilter
                    );
                }

                if (itemcustpricesQueryFilters.Keys.Contains(userFilter.propertyName))
                {
                    itemcustpricesQueryFilters[userFilter.propertyName].AddFilter(
                        filter: userFilter
                    );
                }

                if (priceMatrixQueryFilters.Keys.Contains(userFilter.propertyName))
                {
                    priceMatrixQueryFilters[userFilter.propertyName].AddFilter(
                        filter: userFilter
                    );
                }

                if (inlineFilters.Keys.Contains(userFilter.propertyName))
                {
                    inlineFilters[userFilter.propertyName].AddFilter(
                        filter: userFilter
                    );
                }

                if (userFilter.propertyName == "CustNum")
                {
                    custNum = userFilter.value;
                }

            });



            /********************************************************************/
            /* LOAD THE CUSTOMER RECORDS
            /********************************************************************/

            LoadRecordsResponseData customerRecords = utils.LoadRecords(
                IDOName: "SLCustomers",
                properties: new List<string>() {
                    { "CustNum" },
                    { "Pricecode" }
                },
                filter: utils.BuildFilterString(customerQueryFilters.Values.Select(filter => filter.GetFilterString()).ToList()),
                orderBy: "CustNum",
                recordCap: 1
            );

            if (customerRecords.Items.Count == 1)
            {
                string queriedCustNum = utils.ParseIDOPropertyValue<string>(customerRecords.Items[0].PropertyValues[customerRecords.PropertyKeys["CustNum"]]);
                string custPriceCode = utils.ParseIDOPropertyValue<string>(customerRecords.Items[0].PropertyValues[customerRecords.PropertyKeys["Pricecode"]]);
                if (queriedCustNum == custNum)
                {
                    priceMatrixQueryFilters["PriceCode"].OverwriteFilter("CustPricecode = '" + custPriceCode + "'");
                }
            }



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
                filter: "",
                orderBy: "Priceformula ASC, EffectDate DESC"
            );

            Dictionary<string, IDOItem> activePriceFormulaLookupTable = new Dictionary<string, IDOItem>();
            priceFormulasRecords.Items.ForEach(record => {
                string priceformula = utils.ParseIDOPropertyValue<string>(record.PropertyValues[priceFormulasRecords.PropertyKeys["Priceformula"]]);
                if (!activePriceFormulaLookupTable.ContainsKey(priceformula))
                {
                    activePriceFormulaLookupTable[priceformula] = record;
                }
            });



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
                filter: utils.BuildFilterString(priceMatrixQueryFilters.Values.Select(filter => filter.GetFilterString()).ToList()),
                orderBy: "CustPricecode, ItemPricecode"
            );

            priceMatrixRecords.AddProperty("FirstDolPercent");
            priceMatrixRecords.AddProperty("FirstPrice");
            priceMatrixRecords.AddProperty("EffectDate");
            priceMatrixRecords.AddProperty("PriceFormulaRecordDate");

            Dictionary<string, IDOItem> priceMatrixLookupTable = new Dictionary<string, IDOItem>();
            priceMatrixRecords.Items.ForEach(priceMatrixRecord =>
            {
                string itemPricecode = utils.ParseIDOPropertyValue<string>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["ItemPricecode"]]);
                string priceformula = utils.ParseIDOPropertyValue<string>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["Priceformula"]]);

                if (!priceMatrixLookupTable.ContainsKey(itemPricecode))
                {

                    string firstDolPercent = null;
                    decimal? firstPrice = null;
                    DateTime? effectDate = null;
                    DateTime? recordDate = null;

                    if (activePriceFormulaLookupTable.ContainsKey(priceformula))
                    {
                        IDOItem priceformulaRecord = activePriceFormulaLookupTable[priceformula];
                        firstDolPercent = utils.ParseIDOPropertyValue<string>(priceformulaRecord.PropertyValues[priceFormulasRecords.PropertyKeys["FirstDolPercent"]]);
                        firstPrice = utils.ParseIDOPropertyValue<decimal?>(priceformulaRecord.PropertyValues[priceFormulasRecords.PropertyKeys["FirstPrice"]]);
                        effectDate = utils.ParseIDOPropertyValue<DateTime?>(priceformulaRecord.PropertyValues[priceFormulasRecords.PropertyKeys["EffectDate"]]);
                        recordDate = utils.ParseIDOPropertyValue<DateTime?>(priceformulaRecord.PropertyValues[priceFormulasRecords.PropertyKeys["RecordDate"]]);
                    }

                    priceMatrixRecord.PropertyValues.Add(firstDolPercent);
                    priceMatrixRecord.PropertyValues.Add(firstPrice);
                    priceMatrixRecord.PropertyValues.Add(effectDate);
                    priceMatrixRecord.PropertyValues.Add(recordDate);

                    priceMatrixLookupTable[itemPricecode] = priceMatrixRecord;

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
                filter: utils.BuildFilterString(itemcustpricesQueryFilters.Values.Select(filter => filter.GetFilterString()).ToList()),
                orderBy: "Item ASC, EffectDate DESC"
            );

            Dictionary<string, IDOItem> customerContractPriceIndexLookupTable = new Dictionary<string, IDOItem>();
            customerContractPriceRecords.Items.ForEach(customerContractPriceRecord => {
                string item = utils.ParseIDOPropertyValue<string>(customerContractPriceRecord.PropertyValues[customerContractPriceRecords.PropertyKeys["Item"]]);
                customerContractPriceIndexLookupTable[item] = customerContractPriceRecord;
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

                    // LOAD DATA FROM THE ITEM PRICE RECORD

                    string itemPricecode = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["Pricecode"]]);
                    decimal listPrice = utils.ParseIDOPropertyValue<decimal>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["UnitPrice1"]]);
                    decimal customerPrice = listPrice;
                    string priceType = "List";
                    DateTime effectDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["EffectDate"]]);
                    DateTime recordDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["RecordDate"]]);
                    string rowPointer = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["RowPointer"]]);

                    // IF THERE IS A MATRIX PRICE FOR THIS ITEM'S PRICECODE, WE NEED TO GET THE DATA FROM IT

                    if (itemPricecode != null && priceMatrixLookupTable.ContainsKey(itemPricecode))
                    {
                        IDOItem priceMatrixRecord = priceMatrixLookupTable[itemPricecode];

                        string matrixType = utils.ParseIDOPropertyValue<string>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["FirstDolPercent"]]);
                        decimal matrixValue = utils.ParseIDOPropertyValue<decimal>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["FirstPrice"]]);

                        if (matrixType == "A")
                        {
                            customerPrice = matrixValue;
                        }
                        else if (matrixType == "P")
                        {
                            customerPrice = Math.Round(listPrice * (100m + matrixValue) / 100m, 2, MidpointRounding.AwayFromZero);
                        }

                        priceType = "Matrix";
                        recordDate = (new List<DateTime>() {
                            recordDate,
                            utils.ParseIDOPropertyValue<DateTime>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["RecordDate"]]),
                            utils.ParseIDOPropertyValue<DateTime>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["PriceFormulaRecordDate"]])
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

                    // RUN ALL INLINE FILTERS

                    bool passesInlineFilters = true;

                    if (inlineFilters["ListPrice"].ValueFails(listPrice))
                    {
                        passesInlineFilters = false;
                    }

                    if (inlineFilters["CustomerPrice"].ValueFails(customerPrice))
                    {
                        passesInlineFilters = false;
                    }

                    if (inlineFilters["PriceType"].ValueFails(priceType))
                    {
                        passesInlineFilters = false;
                    }

                    if (inlineFilters["EffectDate"].ValueFails(effectDate))
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
        /* Name:     Example_04B_LoadPricesForCustomer_MatrixAndContract
        /* Date:     2025-11-19
        /* Authors:  Andy Mercer
        /* Purpose:  This example loads the all prices for a specified customer, taking into account their pricecode
        /*           and any customer contract prices they have. It does NOT filter which products they are allowed
        /*           to purchase, which is something that would have to be taken into account for real use.
        /*
        /* Free to use under MIT License, Copyright (c) 2025 FDI Information Systems. See full license at
        /* https://github.com/functionaldevices-cis/syteline-custom-load-method-examples?tab=MIT-1-ov-file#readme
        /*
        /**********************************************************************************************************/
        /**********************************************************************************************************/

        [IDOMethod(MethodFlags.CustomLoad)]
        public DataTable Example_04C_LoadPricesForCustomer_All(string sFilter = null, string sOrderBy = null, string sRecordCap = null, string sBookmark = null)
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
            /* LOAD USER INPUT FROM THE REQUEST OBJECT AND PARAMETERS IF SET
            /********************************************************************/

            (bool haveBookmark, bool areCappingResults) flags = (false, false);

            int iStartingCounterItems = 0;
            int iStartingCounterCustomers = 0;
            int iCounterItems = 0;
            int iCounterCustomers = 0;
            DateTime tomorrow = DateTime.Now.AddDays(1).Date;
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

            Dictionary<string, IIDOPropertyFilterSet> itempriceQueryFilters = new List<IIDOPropertyFilterSet>() {
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "Item"
                ),
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "EffectDateHardcoded", // THIS MUST BE APPLIED REGARDLESS OF WHAT THE USER CHOOSES AS EFFECTIVE DATE FILTER, SO IT'S NAME MATCHES NOTHING. THEREFORE IT CANNOT BE OVERRIDDEN
                    defaultFilter: $"EffectDate < '{tomorrow.ToString("yyyyMMdd HH:mm:ss.fff")}'"
                )
            }.ToDictionary(f => f.OutputPropertyName);

            Dictionary<string, IIDOPropertyFilterSet> customerQueryFilters = new List<IIDOPropertyFilterSet>() {
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "CustNum"
                ),
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "CustSeqHardcoded",  // THIS MUST BE APPLIED REGARDLESS OF WHAT THE USER CHOOSES AS EFFECTIVE DATE FILTER, SO IT'S NAME MATCHES NOTHING. THEREFORE IT CANNOT BE OVERRIDDEN
                    defaultFilter: "CustSeq = 0"
                )
            }.ToDictionary(f => f.OutputPropertyName);

            Dictionary<string, IIDOPropertyFilterSet> itemcustpricesQueryFilters = new List<IIDOPropertyFilterSet>() {
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "CustNum"
                ),
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "EffectDateHardcoded", // THIS MUST BE APPLIED REGARDLESS OF WHAT THE USER CHOOSES AS EFFECTIVE DATE FILTER, SO IT'S NAME MATCHES NOTHING. THEREFORE IT CANNOT BE OVERRIDDEN
                    defaultFilter: $"EffectDate < '{tomorrow.ToString("yyyyMMdd HH:mm:ss.fff")}'"
                )
            }.ToDictionary(f => f.OutputPropertyName);

            Dictionary<string, IIDOPropertyFilterSet> priceMatrixQueryFilters = new List<IIDOPropertyFilterSet>() {
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "CustPricecode"
                )
            }.ToDictionary(f => f.OutputPropertyName);

            Dictionary<string, IIDOPropertyFilterSet> inlineFilters = new List<IIDOPropertyFilterSet>() {
                new IDOPropertyFilterSet<decimal>(
                    outputPropertyName: "CustomerPrice"
                ),
                new IDOPropertyFilterSet<decimal>(
                    outputPropertyName: "ListPrice"
                ),
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "PriceType"
                ),
                new IDOPropertyFilterSet<DateTime>(
                    outputPropertyName: "EffectDate"
                ),
                new IDOPropertyFilterSet<DateTime>(
                    outputPropertyName: "RecordDate"
                ),
                new IDOPropertyFilterSet<string>(
                    outputPropertyName: "RowPointer"
                )
            }.ToDictionary(f => f.OutputPropertyName);

            userRequest.Filters.ForEach(userFilter =>
            {

                if (itempriceQueryFilters.Keys.Contains(userFilter.propertyName))
                {
                    itempriceQueryFilters[userFilter.propertyName].AddFilter(
                        filter: userFilter
                    );
                }

                if (customerQueryFilters.Keys.Contains(userFilter.propertyName))
                {
                    customerQueryFilters[userFilter.propertyName].AddFilter(
                        filter: userFilter
                    );
                }

                if (itemcustpricesQueryFilters.Keys.Contains(userFilter.propertyName))
                {
                    itemcustpricesQueryFilters[userFilter.propertyName].AddFilter(
                        filter: userFilter
                    );
                }

                if (inlineFilters.Keys.Contains(userFilter.propertyName))
                {
                    inlineFilters[userFilter.propertyName].AddFilter(
                        filter: userFilter
                    );
                }

            });



            /********************************************************************/
            /* LOAD THE CUSTOMER RECORDS
            /********************************************************************/

            LoadRecordsResponseData customerRecords = utils.LoadRecords(
                IDOName: "SLCustomers",
                properties: new List<string>() {
                    { "CustNum" },
                    { "Pricecode" }
                },
                filter: utils.BuildFilterString(customerQueryFilters.Values.Select(filter => filter.GetFilterString()).ToList()),
                orderBy: "CustNum",
                recordCap: 0
            );

            Dictionary<string, string> customerPricecodeLookupTable = new Dictionary<string, string>();
            customerRecords.Items.ForEach(record =>
            {
                string custNum = utils.ParseIDOPropertyValue<string>(record.PropertyValues[customerRecords.PropertyKeys["CustNum"]]);
                string priceCode = utils.ParseIDOPropertyValue<string>(record.PropertyValues[customerRecords.PropertyKeys["Pricecode"]]);
                if (!customerPricecodeLookupTable.ContainsKey(custNum))
                {
                    customerPricecodeLookupTable[custNum] = priceCode;
                }
            });
            if (customerRecords.Items.Count > 0)
            {
                priceMatrixQueryFilters["CustPricecode"].OverwriteFilter("CustPricecode IN (" + string.Join(",", customerPricecodeLookupTable.Values.Distinct().Select(custNum => "'" + custNum + "'").ToList()) + ")");
            }



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
                filter: "",
                orderBy: "Priceformula ASC, EffectDate DESC"
            );

            Dictionary<string, IDOItem> activePriceFormulaLookupTable = new Dictionary<string, IDOItem>();
            priceFormulasRecords.Items.ForEach(record => {
                string priceformula = utils.ParseIDOPropertyValue<string>(record.PropertyValues[priceFormulasRecords.PropertyKeys["Priceformula"]]);
                if (!activePriceFormulaLookupTable.ContainsKey(priceformula))
                {
                    activePriceFormulaLookupTable[priceformula] = record;
                }
            });



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
                filter: utils.BuildFilterString(priceMatrixQueryFilters.Values.Select(filter => filter.GetFilterString()).ToList()),
                orderBy: "CustPricecode, ItemPricecode"
            );

            priceMatrixRecords.AddProperty("FirstDolPercent");
            priceMatrixRecords.AddProperty("FirstPrice");
            priceMatrixRecords.AddProperty("EffectDate");
            priceMatrixRecords.AddProperty("PriceFormulaRecordDate");

            Dictionary<string, IDOItem> priceMatrixLookupTable = new Dictionary<string, IDOItem>();
            priceMatrixRecords.Items.ForEach(priceMatrixRecord =>
            {

                string custPricecode = utils.ParseIDOPropertyValue<string>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["CustPricecode"]]);
                string itemPricecode = utils.ParseIDOPropertyValue<string>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["ItemPricecode"]]);
                string priceformula = utils.ParseIDOPropertyValue<string>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["Priceformula"]]);

                if (!priceMatrixLookupTable.ContainsKey(custPricecode + "-" + itemPricecode))
                {

                    string firstDolPercent = null;
                    decimal? firstPrice = null;
                    DateTime? effectDate = null;
                    DateTime? recordDate = null;

                    if (activePriceFormulaLookupTable.ContainsKey(priceformula))
                    {
                        IDOItem priceformulaRecord = activePriceFormulaLookupTable[priceformula];
                        firstDolPercent = utils.ParseIDOPropertyValue<string>(priceformulaRecord.PropertyValues[priceFormulasRecords.PropertyKeys["FirstDolPercent"]]);
                        firstPrice = utils.ParseIDOPropertyValue<decimal?>(priceformulaRecord.PropertyValues[priceFormulasRecords.PropertyKeys["FirstPrice"]]);
                        effectDate = utils.ParseIDOPropertyValue<DateTime?>(priceformulaRecord.PropertyValues[priceFormulasRecords.PropertyKeys["EffectDate"]]);
                        recordDate = utils.ParseIDOPropertyValue<DateTime?>(priceformulaRecord.PropertyValues[priceFormulasRecords.PropertyKeys["RecordDate"]]);
                    }

                    priceMatrixRecord.PropertyValues.Add(firstDolPercent);
                    priceMatrixRecord.PropertyValues.Add(firstPrice);
                    priceMatrixRecord.PropertyValues.Add(effectDate);
                    priceMatrixRecord.PropertyValues.Add(recordDate);

                    priceMatrixLookupTable[custPricecode + "-" + itemPricecode] = priceMatrixRecord;

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
                filter: utils.BuildFilterString(itemcustpricesQueryFilters.Values.Select(filter => filter.GetFilterString()).ToList()),
                orderBy: "Item ASC, EffectDate DESC"
            );

            Dictionary<string, IDOItem> customerContractPriceIndexLookupTable = new Dictionary<string, IDOItem>();
            customerContractPriceRecords.Items.ForEach(customerContractPriceRecord => {
                string custNum = utils.ParseIDOPropertyValue<string>(customerContractPriceRecord.PropertyValues[customerContractPriceRecords.PropertyKeys["CustNum"]]);
                string item = utils.ParseIDOPropertyValue<string>(customerContractPriceRecord.PropertyValues[customerContractPriceRecords.PropertyKeys["Item"]]);
                customerContractPriceIndexLookupTable[custNum + "-" + item] = customerContractPriceRecord;
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
                filter: utils.BuildFilterString(itempriceQueryFilters.Values.Select(filter => filter.GetFilterString()).ToList()),
                orderBy: "Item ASC, EffectDate DESC",
                recordCap: 0
            );



            /********************************************************************/
            /* PARSE BOOKMARK TO DETERMINE WHERE TO START
            /********************************************************************/

            if (flags.haveBookmark)
            {

                string startingCustNum = userRequest.Bookmark.Substring(0, userRequest.Bookmark.IndexOf(','));
                string startingItem = userRequest.Bookmark.Substring(userRequest.Bookmark.IndexOf(',') + 1);

                iStartingCounterCustomers = customerRecords.Items.FindIndex(record => utils.ParseIDOPropertyValue<string>(record.PropertyValues[customerRecords.PropertyKeys["CustNum"]]) == startingCustNum);
                iStartingCounterItems = itemPriceRecords.Items.FindIndex(record => utils.ParseIDOPropertyValue<string>(record.PropertyValues[itemPriceRecords.PropertyKeys["Item"]]) == startingItem);

                if (iStartingCounterCustomers == -1)
                {
                    throw new Exception("Error: The provided bookmark refers to a CustNum ('" + iStartingCounterCustomers + "') that doesn't exist in the queried record set.");
                }
                if (iStartingCounterItems == -1)
                {
                    throw new Exception("Error: The provided bookmark refers to a Item ('" + startingItem + "') that doesn't exist in the queried record set.");
                }

                iStartingCounterItems++;

                if (iStartingCounterItems >= itemPriceRecords.Items.Count)
                {
                    iStartingCounterCustomers++;
                    iStartingCounterItems = 0;
                }

            }



            /********************************************************************/
            /* LOOP THROUGH THE ITEM PRICE RECORDS AND FILL IN THE DATA TABLE
            /********************************************************************/

            for (iCounterCustomers = iStartingCounterCustomers; iCounterCustomers < customerRecords.Items.Count; iCounterCustomers++)
            {

                // GRAB THE CUSTOMER

                IDOItem customerRecord = customerRecords.Items[iCounterCustomers];
                string custNum = utils.ParseIDOPropertyValue<string>(customerRecord.PropertyValues[customerRecords.PropertyKeys["CustNum"]]);
                string custPricecode = utils.ParseIDOPropertyValue<string>(customerRecord.PropertyValues[customerRecords.PropertyKeys["Pricecode"]]);
                itemIndices = new Dictionary<string, int>();

                for (iCounterItems = iStartingCounterItems; iCounterItems < itemPriceRecords.Items.Count; iCounterItems++)
                {

                    // GRAB THE ITEM

                    IDOItem itemPriceRecord = itemPriceRecords.Items[iCounterItems];
                    string item = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["Item"]]);

                    if (item != null && !itemIndices.ContainsKey(item))
                    {

                        // SAVE INDEX

                        itemIndices[item] = outputTable.Rows.Count;

                        // LOAD DATA FROM THE ITEM PRICE RECORD

                        string itemPricecode = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["Pricecode"]]);
                        decimal listPrice = utils.ParseIDOPropertyValue<decimal>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["UnitPrice1"]]);
                        decimal customerPrice = listPrice;
                        string priceType = "List";
                        DateTime effectDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["EffectDate"]]);
                        DateTime recordDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["RecordDate"]]);
                        string rowPointer = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["RowPointer"]]);

                        // IF THERE IS A MATRIX PRICE FOR THIS ITEM'S PRICECODE, WE NEED TO GET THE DATA FROM IT

                        if (itemPricecode != null && priceMatrixLookupTable.ContainsKey(custPricecode + "-" + itemPricecode))
                        {
                            IDOItem priceMatrixRecord = priceMatrixLookupTable[custPricecode + "-" + itemPricecode];

                            string matrixType = utils.ParseIDOPropertyValue<string>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["FirstDolPercent"]]);
                            decimal matrixValue = utils.ParseIDOPropertyValue<decimal>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["FirstPrice"]]);

                            if (matrixType == "A")
                            {
                                customerPrice = matrixValue;
                            }
                            else if (matrixType == "P")
                            {
                                customerPrice = Math.Round(listPrice * (100m + matrixValue) / 100m, 2, MidpointRounding.AwayFromZero);
                            }

                            priceType = "Matrix";
                            recordDate = (new List<DateTime>() {
                                recordDate,
                                utils.ParseIDOPropertyValue<DateTime>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["RecordDate"]]),
                                utils.ParseIDOPropertyValue<DateTime>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["PriceFormulaRecordDate"]])
                            }).Max();
                        }

                        // IF THERE IS A CUSTOMER CONTRACT PRICE FOR THIS ITEM, WE NEED TO GET DATA FROM IT

                        if (item != null && customerContractPriceIndexLookupTable.ContainsKey(custNum + "-" + item))
                        {
                            IDOItem customerContractPriceRecord = customerContractPriceIndexLookupTable[custNum + "-" + item];
                            customerPrice = utils.ParseIDOPropertyValue<decimal>(customerContractPriceRecord.PropertyValues[customerContractPriceRecords.PropertyKeys["ContPrice"]]);
                            effectDate = utils.ParseIDOPropertyValue<DateTime>(customerContractPriceRecord.PropertyValues[customerContractPriceRecords.PropertyKeys["EffectDate"]]);
                            priceType = "Contract";
                            recordDate = (new List<DateTime>() {
                                recordDate,
                                utils.ParseIDOPropertyValue<DateTime>(customerContractPriceRecord.PropertyValues[customerContractPriceRecords.PropertyKeys["RecordDate"]])
                            }).Max();
                        }

                        // RUN ALL INLINE FILTERS

                        bool passesInlineFilters = true;

                        if (inlineFilters["ListPrice"].ValueFails(listPrice))
                        {
                            passesInlineFilters = false;
                        }

                        if (inlineFilters["CustomerPrice"].ValueFails(customerPrice))
                        {
                            passesInlineFilters = false;
                        }

                        if (inlineFilters["PriceType"].ValueFails(priceType))
                        {
                            passesInlineFilters = false;
                        }

                        if (inlineFilters["EffectDate"].ValueFails(effectDate))
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

                    if (userRequest.RecordCap > 0 && outputTable.Rows.Count == userRequest.RecordCap + 1)
                    {
                        iCounterItems = itemPriceRecords.Items.Count;
                    }

                }

                if (userRequest.RecordCap > 0 && outputTable.Rows.Count == userRequest.RecordCap + 1)
                {
                    iCounterCustomers = customerRecords.Items.Count;
                }

                iStartingCounterItems = 0;

            }
            
            if (outputTable.Rows.Count > 0)
            {
                int bookmarkRowIndex = outputTable.Rows.Count > userRequest.RecordCap ? outputTable.Rows.Count - 2 : outputTable.Rows.Count - 1;
                userRequest.Bookmark = outputTable.Rows[bookmarkRowIndex]["CustNum"] + "," + outputTable.Rows[bookmarkRowIndex]["Item"];
                if (debug1 != "")
                {
                    outputTable.Rows[0]["CustNum"] = debug1;
                }
                if (debug2 != "")
                {
                    outputTable.Rows[0]["Item"] = debug2;
                }
            }

            return outputTable;

        }

    }

}