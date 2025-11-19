using Mongoose.IDO;
using Mongoose.IDO.Metadata;
using Mongoose.IDO.Protocol;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
        /* Purpose:  This example loads 10 records from the SLPricecodes IDO. It includes the bound properties of
        /*           Item, UnitPrice1, and EffectDate, and a calculated property called UnitPriceDoubled1, which
        /*           is just UnitPrice1 * 2. The three parameters are unused because the goal of example 01 is to
        /*           keep things as simple as possible.
        /*
        /* Copyright 2025, Functional Devices, Inc
        /*
        /**********************************************************************************************************/
        /**********************************************************************************************************/

        [IDOMethod(MethodFlags.CustomLoad)]
        public DataTable Example_01A_LoadItemPrices_Base(string sFilter = null, string sOrderBy = null, string sRecordCap = null)
        {

            /********************************************************************/
            /* SET UP HELPER VARIABLES
            /********************************************************************/

            ue_FDI_Utilities utils = new ue_FDI_Utilities()
            {
                IDOCommands = this.Context.Commands
            };



            /********************************************************************/
            /* QUERY ITEM PRICES TO GET BASE RECORDS
            /********************************************************************/

            LoadRecordsResponseData itemPriceRecords = utils.LoadRecords(
                IDOName: "SLItemprices",
                properties: new List<string>() {
                    { "Item" },
                    { "UnitPrice1" },
                    { "EffectDate" },
                },
                filter: "",
                orderBy: "Item ASC, EffectDate DESC",
                recordCap: 10
            );



            /********************************************************************/
            /* CREATE EMPTY TABLE
            /********************************************************************/

            DataTable fullTable = new DataTable("FullTable");
            DataRow outputRow;

            // ADD COLUMN STRUCTURE

            fullTable.Columns.Add("Item", typeof(string));
            fullTable.Columns.Add("UnitPrice1", typeof(decimal));
            fullTable.Columns.Add("UnitPriceDoubled1", typeof(decimal));
            fullTable.Columns.Add("EffectDate", typeof(DateTime));



            /********************************************************************/
            /* LOOP THROUGH THE ITEM PRICE RECORDS AND FILL IN THE DATA TABLE
            /********************************************************************/

            itemPriceRecords.Items.ForEach(itemPriceRecord =>
            {

                // GRAB THE ITEM

                string item = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["Item"]]);
                decimal price = utils.ParseIDOPropertyValue<decimal>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["UnitPrice1"]]);
                DateTime effectDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["EffectDate"]]);

                // CREATE OUTPUT ROW

                outputRow = fullTable.NewRow();

                // FILL IN OUTPUT ROW

                outputRow["Item"] = item;
                outputRow["UnitPrice1"] = price;
                outputRow["UnitPriceDoubled1"] = price * 2;
                outputRow["EffectDate"] = effectDate;

                // ADD ROW TO OUTPUT
                fullTable.Rows.Add(outputRow);


            });

            // FILTER THE TABLE

            return fullTable;

        }



        /**********************************************************************************************************/
        /**********************************************************************************************************/
        /*
        /* Name:     Example_01B_LoadItemPrices_QueryFiltering
        /* Date:     2025-11-14
        /* Authors:  Andy Mercer
        /* Purpose:  This example loads 10 records from the SLPricecodes IDO. It includes the bound properties of
        /*           Item, UnitPrice1, and EffectDate, and a calculated property called UnitPriceDoubled1, which
        /*           is just UnitPrice1 * 2. This example introduces standard query filtering, so the three bound
        /*           properties can now be filtered.
        /*
        /* Copyright 2025, Functional Devices, Inc
        /*
        /**********************************************************************************************************/
        /**********************************************************************************************************/

        [IDOMethod(MethodFlags.CustomLoad)]
        public DataTable Example_01B_LoadItemPrices_QueryFiltering(string sFilter = null, string sOrderBy = null, string sRecordCap = null)
        {

            /********************************************************************/
            /* SET UP HELPER VARIABLES
            /********************************************************************/

            ue_FDI_Utilities utils = new ue_FDI_Utilities()
            {
                IDOCommands = this.Context.Commands
            };



            /********************************************************************/
            /* PARSE FILTERS
            /********************************************************************/

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

            Dictionary<string, string> itempricesQueryFilters = new Dictionary<string, string>() {
                { "Item", "" },
                { "EffectDate", "" },
                { "UnitPrice1", "" }
            };

            userFilters.ForEach(userFilter =>
            {

                userFilter = utils.FixParenthesis(userFilter);
                userFilterValue = utils.ExtractValue(userFilter);
                userFilterOperator = utils.ExtractOperator(userFilter);

                if (userFilter.Contains("Item"))
                {
                    itempricesQueryFilters["Item"] = userFilter;
                }

                if (userFilter.Contains("EffectDate"))
                {
                    itempricesQueryFilters["EffectDate"] = userFilter;
                }

                if (userFilter.Contains("UnitPrice1"))
                {
                    itempricesQueryFilters["UnitPrice1"] = userFilter;
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
                    { "EffectDate" },
                },
                filter: utils.BuildFilterString(itempricesQueryFilters.Values.ToList()),
                orderBy: "Item ASC, EffectDate DESC",
                recordCap: 10
            );



            /********************************************************************/
            /* CREATE EMPTY TABLE
            /********************************************************************/

            DataTable fullTable = new DataTable("FullTable");
            DataTable dtFilteredTable = new DataTable("PostFilteredTable");
            DataRow outputRow;

            // ADD COLUMN STRUCTURE

            fullTable.Columns.Add("Item", typeof(string));
            fullTable.Columns.Add("UnitPrice1", typeof(decimal));
            fullTable.Columns.Add("UnitPriceDoubled1", typeof(decimal));
            fullTable.Columns.Add("EffectDate", typeof(DateTime));



            /********************************************************************/
            /* LOOP THROUGH THE ITEM PRICE RECORDS AND FILL IN THE DATA TABLE
            /********************************************************************/

            itemPriceRecords.Items.ForEach(itemPriceRecord =>
            {

                // GRAB THE ITEM

                string item = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["Item"]]);
                decimal price = utils.ParseIDOPropertyValue<decimal>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["UnitPrice1"]]);
                DateTime effectDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["EffectDate"]]);

                // CREATE OUTPUT ROW

                outputRow = fullTable.NewRow();

                // FILL IN OUTPUT ROW

                outputRow["Item"] = item;
                outputRow["UnitPrice1"] = price;
                outputRow["UnitPriceDoubled1"] = price * 2;
                outputRow["EffectDate"] = effectDate;

                // ADD ROW TO OUTPUT
                fullTable.Rows.Add(outputRow);


            });

            return fullTable;

        }



        /**********************************************************************************************************/
        /**********************************************************************************************************/
        /*
        /* Name:     Example_01C_LoadItemPrices_PostFiltering
        /* Date:     2025-11-14
        /* Authors:  Andy Mercer
        /* Purpose:  This example loads 10 records from the SLPricecodes IDO. It includes the bound properties of
        /*           Item, UnitPrice1, and EffectDate, and a calculated property called UnitPriceDoubled1, which
        /*           is just UnitPrice1 * 2. This example introduces post-filtering, so all properties including
        /*           the calculated property can now be filtered.
        /*
        /* Copyright 2025, Functional Devices, Inc
        /*
        /**********************************************************************************************************/
        /**********************************************************************************************************/

        [IDOMethod(MethodFlags.CustomLoad)]
        public DataTable Example_01C_LoadItemPrices_PostFiltering(string sFilter = null, string sOrderBy = null, string sRecordCap = null)
        {

            /********************************************************************/
            /* SET UP HELPER VARIABLES
            /********************************************************************/

            ue_FDI_Utilities utils = new ue_FDI_Utilities()
            {
                IDOCommands = this.Context.Commands
            };



            /********************************************************************/
            /* PARSE FILTERS
            /********************************************************************/

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

            Dictionary<string, string> itempricesQueryFilters = new Dictionary<string, string>() {
                { "Item", "" },
                { "EffectDate", "" },
                { "UnitPrice1", "" }
            };

            Dictionary<string, string> postQueryFilters = new Dictionary<string, string>() {
                { "UnitPriceDoubled1", "" }
            };

            userFilters.ForEach(userFilter =>
            {

                userFilter = utils.FixParenthesis(userFilter);
                userFilterValue = utils.ExtractValue(userFilter);
                userFilterOperator = utils.ExtractOperator(userFilter);

                if (userFilter.Contains("Item"))
                {
                    itempricesQueryFilters["Item"] = userFilter;
                }

                if (userFilter.Contains("EffectDate"))
                {
                    itempricesQueryFilters["EffectDate"] = userFilter;
                }

                if (userFilter.Contains("UnitPrice1"))
                {
                    itempricesQueryFilters["UnitPrice1"] = userFilter;
                }

                if (userFilter.Contains("UnitPriceDoubled1"))
                {
                    postQueryFilters["UnitPriceDoubled1"] = userFilter.Replace(" null ", " '' ");
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
                    { "EffectDate" },
                },
                filter: utils.BuildFilterString(itempricesQueryFilters.Values.ToList()),
                orderBy: "Item ASC, EffectDate DESC",
                recordCap: 10
            );



            /********************************************************************/
            /* CREATE EMPTY TABLE
            /********************************************************************/

            DataTable fullTable = new DataTable("FullTable");
            DataTable dtFilteredTable = new DataTable("PostFilteredTable");
            DataRow outputRow;

            // ADD COLUMN STRUCTURE

            fullTable.Columns.Add("Item", typeof(string));
            fullTable.Columns.Add("UnitPrice1", typeof(decimal));
            fullTable.Columns.Add("UnitPriceDoubled1", typeof(decimal));
            fullTable.Columns.Add("EffectDate", typeof(DateTime));



            /********************************************************************/
            /* LOOP THROUGH THE ITEM PRICE RECORDS AND FILL IN THE DATA TABLE
            /********************************************************************/

            itemPriceRecords.Items.ForEach(itemPriceRecord =>
            {

                // GRAB THE ITEM

                string item = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["Item"]]);
                decimal price = utils.ParseIDOPropertyValue<decimal>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["UnitPrice1"]]);
                DateTime effectDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["EffectDate"]]);

                // CREATE OUTPUT ROW

                outputRow = fullTable.NewRow();

                // FILL IN OUTPUT ROW

                outputRow["Item"] = item;
                outputRow["UnitPrice1"] = price;
                outputRow["UnitPriceDoubled1"] = price * 2;
                outputRow["EffectDate"] = effectDate;

                // ADD ROW TO OUTPUT

                fullTable.Rows.Add(outputRow);

            });



            /********************************************************************/
            /* APPLY POST-FILTERS
            /********************************************************************/

            string userPostQueryFilterString = utils.BuildFilterString(postQueryFilters.Values.ToList());

            if (userPostQueryFilterString != "")
            {
                dtFilteredTable = fullTable.Clone();
                DataRow[] filteredRows = fullTable.Select(userPostQueryFilterString);
                foreach (DataRow row in filteredRows)
                {
                    dtFilteredTable.ImportRow(row);
                }
            }
            else
            {
                dtFilteredTable = fullTable;
            }

            return dtFilteredTable.DefaultView.ToTable();

        }

        
        
        /**********************************************************************************************************/
        /**********************************************************************************************************/
        /*
        /* Name:     Example_01D_LoadItemPrices_FilteringOrderingAndCapping
        /* Date:     2025-11-14
        /* Authors:  Andy Mercer
        /* Purpose:  This example loads 10 records from the SLPricecodes IDO. It includes the bound properties of
        /*           Item, UnitPrice1, and EffectDate, and a calculated property called UnitPriceDoubled1, which
        /*           is just UnitPrice1 * 2. This example adds support for the second and third method parameters
        /*           and allows the caller (whether it is a Mongoose form or API) to override the default order by
        /*           and record cap. The matching Mongoose form reverses the default ordering and sets the cap to 20.
        /*
        /* Copyright 2025, Functional Devices, Inc
        /*
        /**********************************************************************************************************/
        /**********************************************************************************************************/

        [IDOMethod(MethodFlags.CustomLoad)]
        public DataTable Example_01D_LoadItemPrices_FilteringOrderingAndCapping(string sFilter = null, string sOrderBy = null, string sRecordCap = null)
        {

            /********************************************************************/
            /* SET UP HELPER VARIABLES
            /********************************************************************/

            ue_FDI_Utilities utils = new ue_FDI_Utilities()
            {
                IDOCommands = this.Context.Commands
            };



            /********************************************************************/
            /* PARSE FILTERS
            /********************************************************************/

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

            Dictionary<string, string> itempricesQueryFilters = new Dictionary<string, string>() {
                { "Item", "" },
                { "EffectDate", "" },
                { "UnitPrice1", "" }
            };

            Dictionary<string, string> postQueryFilters = new Dictionary<string, string>() {
                { "UnitPriceDoubled1", "" }
            };

            userFilters.ForEach(userFilter =>
            {

                userFilter = utils.FixParenthesis(userFilter);
                userFilterValue = utils.ExtractValue(userFilter);
                userFilterOperator = utils.ExtractOperator(userFilter);

                if (userFilter.Contains("Item"))
                {
                    itempricesQueryFilters["Item"] = userFilter;
                }

                if (userFilter.Contains("EffectDate"))
                {
                    itempricesQueryFilters["EffectDate"] = userFilter;
                }

                if (userFilter.Contains("UnitPrice1"))
                {
                    itempricesQueryFilters["UnitPrice1"] = userFilter;
                }

                if (userFilter.Contains("UnitPriceDoubled1"))
                {
                    postQueryFilters["UnitPriceDoubled1"] = userFilter.Replace(" null ", " '' ");
                }

            });



            /********************************************************************/
            /* PARSE ORDER BY
            /********************************************************************/

            string parsedOrderBy = sOrderBy ?? "Item ASC, EffectDate DESC";



            /********************************************************************/
            /* PARSE RECORD CAP
            /********************************************************************/


            int parsedRecordCap = 10;
            if (sRecordCap != null && sRecordCap != "")
            {
                int.TryParse(sRecordCap, out parsedRecordCap);
            }



            /********************************************************************/
            /* QUERY ITEM PRICES TO GET BASE RECORDS
            /********************************************************************/

            LoadRecordsResponseData itemPriceRecords = utils.LoadRecords(
                IDOName: "SLItemprices",
                properties: new List<string>() {
                    { "Item" },
                    { "UnitPrice1" },
                    { "EffectDate" },
                },
                filter: utils.BuildFilterString(itempricesQueryFilters.Values.ToList()),
                orderBy: parsedOrderBy,
                recordCap: parsedRecordCap
            );



            /********************************************************************/
            /* CREATE EMPTY TABLE
            /********************************************************************/

            DataTable fullTable = new DataTable("FullTable");
            DataTable dtFilteredTable = new DataTable("PostFilteredTable");
            DataRow outputRow;

            // ADD COLUMN STRUCTURE

            fullTable.Columns.Add("Item", typeof(string));
            fullTable.Columns.Add("UnitPrice1", typeof(decimal));
            fullTable.Columns.Add("UnitPriceDoubled1", typeof(decimal));
            fullTable.Columns.Add("EffectDate", typeof(DateTime));



            /********************************************************************/
            /* LOOP THROUGH THE ITEM PRICE RECORDS AND FILL IN THE DATA TABLE
            /********************************************************************/

            itemPriceRecords.Items.ForEach(itemPriceRecord =>
            {

                // GRAB THE ITEM

                string item = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["Item"]]);
                decimal price = utils.ParseIDOPropertyValue<decimal>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["UnitPrice1"]]);
                DateTime effectDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["EffectDate"]]);

                // CREATE OUTPUT ROW

                outputRow = fullTable.NewRow();

                // FILL IN OUTPUT ROW

                outputRow["Item"] = item;
                outputRow["UnitPrice1"] = price;
                outputRow["UnitPriceDoubled1"] = price * 2;
                outputRow["EffectDate"] = effectDate;

                // ADD ROW TO OUTPUT
                fullTable.Rows.Add(outputRow);


            });



            /********************************************************************/
            /* APPLY POST-FILTERS
            /********************************************************************/

            string userPostQueryFilterString = utils.BuildFilterString(postQueryFilters.Values.ToList());

            if (userPostQueryFilterString != "")
            {
                dtFilteredTable = fullTable.Clone();
                DataRow[] filteredRows = fullTable.Select(userPostQueryFilterString);
                foreach (DataRow row in filteredRows)
                {
                    dtFilteredTable.ImportRow(row);
                }
            }
            else
            {
                dtFilteredTable = fullTable;
            }

            return dtFilteredTable.DefaultView.ToTable();

        }



        /**********************************************************************************************************/
        /**********************************************************************************************************/
        /*
        /* Name:     Example_02_LoadCurrentItemPrices
        /* Date:     2025-11-14
        /* Authors:  Andy Mercer
        /* Purpose:  This example loads all current records from the SLPricecodes IDO, meaning the record with the
        /*           highest non-future effective date for each unique Item value.
        /*
        /* Copyright 2025, Functional Devices, Inc
        /*
        /**********************************************************************************************************/
        /**********************************************************************************************************/

        [IDOMethod(MethodFlags.CustomLoad)]
        public DataTable Example_02_LoadCurrentItemPrices(string sFilter = null, string sOrderBy = null, string sRecordCap = null)
        {

            /********************************************************************/
            /* SET UP HELPER VARIABLES
            /********************************************************************/

            ue_FDI_Utilities utils = new ue_FDI_Utilities()
            {
                IDOCommands = this.Context.Commands
            };



            /********************************************************************/
            /* PARSE FILTERS
            /********************************************************************/

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

            Dictionary<string, string> itempricesQueryFilters = new Dictionary<string, string>() {
                { "Item", "" },
                { "EffectDate", "" },
                { "UnitPrice1", "" }
            };

            Dictionary<string, string> postQueryFilters = new Dictionary<string, string>() {
                { "UnitPriceDoubled1", "" }
            };

            userFilters.ForEach(userFilter =>
            {

                userFilter = utils.FixParenthesis(userFilter);
                userFilterValue = utils.ExtractValue(userFilter);
                userFilterOperator = utils.ExtractOperator(userFilter);

                if (userFilter.Contains("Item"))
                {
                    itempricesQueryFilters["Item"] = userFilter;
                }

                if (userFilter.Contains("EffectDate"))
                {
                    itempricesQueryFilters["EffectDate"] = userFilter;
                }

                if (userFilter.Contains("UnitPrice1"))
                {
                    itempricesQueryFilters["UnitPrice1"] = userFilter;
                }

                if (userFilter.Contains("UnitPriceDoubled1"))
                {
                    postQueryFilters["UnitPriceDoubled1"] = userFilter.Replace(" null ", " '' ");
                }

            });



            /********************************************************************/
            /* PARSE ORDER BY
            /********************************************************************/

            string parsedOrderBy = sOrderBy ?? "Item ASC, EffectDate DESC";



            /********************************************************************/
            /* PARSE RECORD CAP
            /********************************************************************/


            int parsedRecordCap = 0;
            if (sRecordCap != null && sRecordCap != "")
            {
                int.TryParse(sRecordCap, out parsedRecordCap);
            }



            /********************************************************************/
            /* QUERY ITEM PRICES TO GET BASE RECORDS
            /********************************************************************/

            LoadRecordsResponseData itemPriceRecords = utils.LoadRecords(
                IDOName: "SLItemprices",
                properties: new List<string>() {
                    { "Item" },
                    { "UnitPrice1" },
                    { "EffectDate" },
                },
                filter: utils.BuildFilterString(itempricesQueryFilters.Values.ToList()),
                orderBy: "Item ASC, EffectDate DESC", // THIS WILL BE HARDCODED SO THAT THE FIRST RECORD FOR EACH ITEM WILL ALWAYS BE THE HIGHEST EFFECT DATE
                recordCap: parsedRecordCap
            );



            /********************************************************************/
            /* CREATE EMPTY TABLE
            /********************************************************************/

            DataTable fullTable = new DataTable("FullTable");
            DataTable filteredTable = new DataTable("PostFilteredTable");
            Dictionary<string, int> itemIndices = new Dictionary<string, int>();
            DataRow outputRow;

            // ADD COLUMN STRUCTURE
            
            fullTable.Columns.Add("Item", typeof(string));
            fullTable.Columns.Add("UnitPrice1", typeof(decimal));
            fullTable.Columns.Add("UnitPriceDoubled1", typeof(decimal));
            fullTable.Columns.Add("EffectDate", typeof(DateTime));



            /********************************************************************/
            /* LOOP THROUGH THE ITEM PRICE RECORDS AND FILL IN THE DATA TABLE
            /********************************************************************/

            itemPriceRecords.Items.ForEach(itemPriceRecord =>
            {

                // GRAB THE ITEM

                string item = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["Item"]]);
                decimal price = utils.ParseIDOPropertyValue<decimal>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["UnitPrice1"]]);
                DateTime effectDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["EffectDate"]]);

                if (!itemIndices.ContainsKey(item))
                {

                    // SAVE INDEX

                    itemIndices[item] = fullTable.Rows.Count;

                    // CREATE OUTPUT ROW

                    outputRow = fullTable.NewRow();

                    // FILL IN OUTPUT ROW

                    outputRow["Item"] = item;
                    outputRow["UnitPrice1"] = price;
                    outputRow["UnitPriceDoubled1"] = price * 2;
                    outputRow["EffectDate"] = effectDate;

                    // ADD ROW TO OUTPUT

                    fullTable.Rows.Add(outputRow);

                }

            });



            /********************************************************************/
            /* APPLY POST-FILTERS AND USER-SPECIFIED ORDER BY
            /********************************************************************/

            string userPostQueryFilterString = utils.BuildFilterString(postQueryFilters.Values.ToList());

            if (userPostQueryFilterString != "")
            {
                filteredTable = fullTable.Clone();
                DataRow[] filteredRows = fullTable.Select(userPostQueryFilterString);
                foreach (DataRow row in filteredRows)
                {
                    filteredTable.ImportRow(row);
                }
            }
            else
            {
                filteredTable = fullTable;
            }

            filteredTable.DefaultView.Sort = parsedOrderBy;

            return filteredTable.DefaultView.ToTable();

        }



        /**********************************************************************************************************/
        /**********************************************************************************************************/
        /*
        /* Name:     Example_03A_LoadPricesForPricecode_Base
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
        public DataTable Example_03A_LoadPricesForPricecode_Base(string sFilter = null, string sOrderBy = null, string sRecordCap = null)
        {

            /********************************************************************/
            /* SET UP HELPER VARIABLES
            /********************************************************************/

            ue_FDI_Utilities utils = new ue_FDI_Utilities()
            {
                IDOCommands = this.Context.Commands
            };



            /********************************************************************/
            /* PARSE FILTERS
            /********************************************************************/

            string customerPriceCode = "000"; // !! CHANGE THIS TO WHATEVER YOUR BASE LIST PRICE CODE IS !!
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
            };

            Dictionary<string, string> priceMatrixQueryFilters = new Dictionary<string, string>() {
                { "CustPricecode", "CustPricecode = '" + customerPriceCode + "'" }
            };

            Dictionary<string, string> postQueryFilters = new Dictionary<string, string>() {
                { "CustomerPrice", "" }
            };

            userFilters.ForEach(userFilter =>
            {

                userFilter = utils.FixParenthesis(userFilter);
                userFilterValue = utils.ExtractValue(userFilter);
                userFilterOperator = utils.ExtractOperator(userFilter);

                if (userFilter.Contains("PriceCode"))
                {
                    priceMatrixQueryFilters["CustPricecode"] = userFilter.Replace("PriceCode", "CustPricecode");
                    customerPriceCode = userFilterValue;
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

            });



            /********************************************************************/
            /* PARSE ORDER BY
            /********************************************************************/

            string parsedOrderBy = sOrderBy ?? "Item ASC, EffectDate DESC";



            /********************************************************************/
            /* PARSE RECORD CAP
            /********************************************************************/


            int parsedRecordCap = 0;
            if (sRecordCap != null && sRecordCap != "")
            {
                int.TryParse(sRecordCap, out parsedRecordCap);
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
                },
                filter: utils.BuildFilterString(priceMatrixQueryFilters.Values.ToList()),
                orderBy: "CustPricecode, ItemPricecode"
            );

            string uniqueEscapedPriceFormulasCommaDel = string.Join(
                ",",
                priceMatrixRecords.Items.Select(record => {
                    return "'" + utils.ParseIDOPropertyValue<string>(record.PropertyValues[priceMatrixRecords.PropertyKeys["Priceformula"]]) + "'";
                }).Distinct()
            );



            /********************************************************************/
            /* LOAD THE PRICE FORMULA RECORDS FOR THE LOADED PRICE MATRICES
            /********************************************************************/

            LoadRecordsResponseData priceFormulasRecords = utils.LoadRecords(
                IDOName: "SLPriceformulas",
                properties: new List<string>() {
                    { "Priceformula" },
                    { "FirstDolPercent" },
                    { "FirstPrice" },
                    { "EffectDate" }
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
                string priceFormula = utils.ParseIDOPropertyValue<string>(priceMatrixRecord.PropertyValues[priceMatrixRecords.PropertyKeys["Priceformula"]]);

                if (!priceCalculatorLookupTable.ContainsKey(itemPricecode))
                {
                    IDOItem priceFormulaRecord = activePriceFormulaLookupTable[priceFormula];
                    priceCalculatorLookupTable[itemPricecode] = new ItemPriceCodePriceInfo(
                        priceCode: itemPricecode,
                        type: utils.ParseIDOPropertyValue<string>(priceFormulaRecord.PropertyValues[priceFormulasRecords.PropertyKeys["FirstDolPercent"]]),
                        value: utils.ParseIDOPropertyValue<decimal>(priceFormulaRecord.PropertyValues[priceFormulasRecords.PropertyKeys["FirstPrice"]])
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
                },
                filter: utils.BuildFilterString(itempriceQueryFilters.Values.ToList()),
                orderBy: "Item ASC, EffectDate DESC", // THIS WILL BE HARDCODED SO THAT THE FIRST RECORD FOR EACH ITEM WILL ALWAYS BE THE HIGHEST EFFECT DATE
                recordCap: 0
            );



            /********************************************************************/
            /* CREATE EMPTY TABLE
            /********************************************************************/

            DataTable fullTable = new DataTable("FullTable");
            DataTable filteredTable = new DataTable("PostFilteredTable");
            Dictionary<string, int> itemIndices = new Dictionary<string, int>();
            DataRow outputRow;

            // ADD COLUMN STRUCTURE

            fullTable.Columns.Add("PriceCode", typeof(string));
            fullTable.Columns.Add("Item", typeof(string));
            fullTable.Columns.Add("ListPrice", typeof(decimal));
            fullTable.Columns.Add("CustomerPrice", typeof(decimal));
            fullTable.Columns.Add("EffectDate", typeof(DateTime));



            /********************************************************************/
            /* LOOP THROUGH THE ITEM PRICE RECORDS AND FILL IN THE DATA TABLE
            /********************************************************************/

            foreach(IDOItem itemPriceRecord in itemPriceRecords.Items)
            {

                // GRAB THE ITEM

                string item = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["Item"]]);
                string itemPricecode = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["Pricecode"]]);
                decimal listPrice = utils.ParseIDOPropertyValue<decimal>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["UnitPrice1"]]);
                decimal customerPrice = listPrice;
                DateTime effectDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["EffectDate"]]);

                if (priceCalculatorLookupTable.ContainsKey(itemPricecode))
                {
                    customerPrice = priceCalculatorLookupTable[itemPricecode].GetPrice(listPrice);
                }

                if (!itemIndices.ContainsKey(item))
                {

                    // SAVE INDEX

                    itemIndices[item] = fullTable.Rows.Count;

                    // CREATE OUTPUT ROW

                    outputRow = fullTable.NewRow();

                    // FILL IN OUTPUT ROW

                    outputRow["PriceCode"] = customerPriceCode;
                    outputRow["Item"] = item;
                    outputRow["ListPrice"] = listPrice;
                    outputRow["CustomerPrice"] = customerPrice;
                    outputRow["EffectDate"] = effectDate;

                    // ADD ROW TO OUTPUT

                    fullTable.Rows.Add(outputRow);

                }

            }



            /********************************************************************/
            /* APPLY POST-FILTERS
            /********************************************************************/

            string userPostQueryFilterString = utils.BuildFilterString(postQueryFilters.Values.ToList());

            if (userPostQueryFilterString != "")
            {
                filteredTable = fullTable.Clone();
                DataRow[] filteredRows = fullTable.Select(userPostQueryFilterString);
                foreach (DataRow row in filteredRows)
                {
                    filteredTable.ImportRow(row);
                }
            }
            else
            {
                filteredTable = fullTable;
            }

            filteredTable.DefaultView.Sort = parsedOrderBy;
            filteredTable = filteredTable.DefaultView.ToTable();
            if (parsedRecordCap > 0)
            {
                filteredTable = filteredTable.AsEnumerable().Take(parsedRecordCap).CopyToDataTable();
            }

            return filteredTable;

        }



        /**********************************************************************************************************/
        /**********************************************************************************************************/
        /*
        /* Name:     Example_03B_LoadPricesForPricecode_PagingAndIncremental
        /* Date:     2025-11-14
        /* Authors:  Andy Mercer
        /* Purpose:  This example loads the same set of prices as 3A, but adds support for RowPointer-based
        /*           pagination and modified timestamp-based incremental loading.
        /*           
        /*
        /* Copyright 2025, Functional Devices, Inc
        /*
        /**********************************************************************************************************/
        /**********************************************************************************************************/

        [IDOMethod(MethodFlags.CustomLoad)]
        public DataTable Example_03B_LoadPricesForPricecode_PagingAndIncremental(string sFilter = null, string sOrderBy = null, string sRecordCap = null)
        {

            /********************************************************************/
            /* SET UP HELPER VARIABLES
            /********************************************************************/

            ue_FDI_Utilities utils = new ue_FDI_Utilities()
            {
                IDOCommands = this.Context.Commands
            };



            /********************************************************************/
            /* PARSE FILTERS
            /********************************************************************/

            string custPriceCode = "000"; // !! CHANGE THIS TO WHATEVER YOUR BASE LIST PRICE CODE IS !!
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

            Dictionary<string, string> priceMatrixQueryFilters = new Dictionary<string, string>() {
                { "CustPricecode", "CustPricecode = '" + custPriceCode + "'" }
            };

            Dictionary<string, string> postQueryFilters = new Dictionary<string, string>() {
                { "CustomerPrice", "" }
            };

            userFilters.ForEach(userFilter =>
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



            /********************************************************************/
            /* PARSE ORDER BY
            /********************************************************************/

            string parsedOrderBy = sOrderBy ?? "Item ASC, EffectDate DESC";



            /********************************************************************/
            /* PARSE RECORD CAP
            /********************************************************************/


            int parsedRecordCap = 0;
            if (sRecordCap != null && sRecordCap != "")
            {
                int.TryParse(sRecordCap, out parsedRecordCap);
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

            string uniqueEscapedPriceFormulasCommaDel = string.Join(
                ",",
                priceMatrixRecords.Items.Select(record => {
                    return "'" + utils.ParseIDOPropertyValue<string>(record.PropertyValues[priceMatrixRecords.PropertyKeys["Priceformula"]]) + "'";
                }).Distinct()
            );



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
                orderBy: "Item ASC, EffectDate DESC", // THIS WILL BE HARDCODED SO THAT THE FIRST RECORD FOR EACH ITEM WILL ALWAYS BE THE HIGHEST EFFECT DATE
                recordCap: 0
            );



            /********************************************************************/
            /* CREATE EMPTY TABLE
            /********************************************************************/

            DataTable fullTable = new DataTable("FullTable");
            DataTable filteredTable = new DataTable("PostFilteredTable");
            Dictionary<string, int> itemIndices = new Dictionary<string, int>();
            DataRow outputRow;

            // ADD COLUMN STRUCTURE

            fullTable.Columns.Add("PriceCode", typeof(string));
            fullTable.Columns.Add("Item", typeof(string));
            fullTable.Columns.Add("ListPrice", typeof(decimal));
            fullTable.Columns.Add("CustomerPrice", typeof(decimal));
            fullTable.Columns.Add("EffectDate", typeof(DateTime));
            fullTable.Columns.Add("RecordDate", typeof(DateTime));
            fullTable.Columns.Add("RowPointer", typeof(string));



            /********************************************************************/
            /* LOOP THROUGH THE ITEM PRICE RECORDS AND FILL IN THE DATA TABLE
            /********************************************************************/

            foreach (IDOItem itemPriceRecord in itemPriceRecords.Items)
            {

                // GRAB THE ITEM

                string item = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["Item"]]);
                string itemPricecode = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["Pricecode"]]);
                decimal listPrice = utils.ParseIDOPropertyValue<decimal>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["UnitPrice1"]]);
                decimal customerPrice = listPrice;
                DateTime effectDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["EffectDate"]]);
                DateTime recordDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["RecordDate"]]);
                string rowPointer = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["RowPointer"]]);

                // IF THERE IS A PRICE CODE, WE NEED TO GET THE CALCULATED CUSTOMER PRICE AND THE HIGHEST RECORD DATE FROM THE ITEMPRICE, PRICE MATRIX, AND PRICE FORMULA RECORDS

                if (priceCalculatorLookupTable.ContainsKey(itemPricecode))
                {
                    customerPrice = priceCalculatorLookupTable[itemPricecode].GetPrice(listPrice);
                    recordDate = (new List<DateTime>() { recordDate, priceCalculatorLookupTable[itemPricecode].PriceFormulaRecordDate, priceCalculatorLookupTable[itemPricecode].PriceMatrixRecordDate }).Max();
                }

                if (!itemIndices.ContainsKey(item))
                {

                    // SAVE INDEX

                    itemIndices[item] = fullTable.Rows.Count;

                    // CREATE OUTPUT ROW

                    outputRow = fullTable.NewRow();

                    // FILL IN OUTPUT ROW

                    outputRow["PriceCode"] = custPriceCode;
                    outputRow["Item"] = item;
                    outputRow["ListPrice"] = listPrice;
                    outputRow["CustomerPrice"] = customerPrice;
                    outputRow["EffectDate"] = effectDate;
                    outputRow["RecordDate"] = recordDate;
                    outputRow["RowPointer"] = rowPointer;

                    // ADD ROW TO OUTPUT

                    fullTable.Rows.Add(outputRow);

                }

            }



            /********************************************************************/
            /* APPLY POST-FILTERS
            /********************************************************************/

            string userPostQueryFilterString = utils.BuildFilterString(postQueryFilters.Values.ToList());

            if (userPostQueryFilterString != "")
            {
                filteredTable = fullTable.Clone();
                DataRow[] filteredRows = fullTable.Select(userPostQueryFilterString);
                foreach (DataRow row in filteredRows)
                {
                    filteredTable.ImportRow(row);
                }
            }
            else
            {
                filteredTable = fullTable;
            }

            filteredTable.DefaultView.Sort = parsedOrderBy;
            filteredTable = filteredTable.DefaultView.ToTable();
            if (parsedRecordCap > 0)
            {
                filteredTable = filteredTable.AsEnumerable().Take(parsedRecordCap).CopyToDataTable();
            }

            return filteredTable;

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
        public DataTable Example_04A_LoadPricesForCustomer_Matrix(string sFilter = null, string sOrderBy = null, string sRecordCap = null)
        {

            /********************************************************************/
            /* SET UP HELPER VARIABLES
            /********************************************************************/

            ue_FDI_Utilities utils = new ue_FDI_Utilities()
            {
                IDOCommands = this.Context.Commands
            };



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

            Dictionary<string, string> postQueryFilters = new Dictionary<string, string>() {
                { "CustomerPrice", "" }
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



            /********************************************************************/
            /* PARSE ORDER BY
            /********************************************************************/

            string parsedOrderBy = sOrderBy ?? "Item ASC, EffectDate DESC";



            /********************************************************************/
            /* PARSE RECORD CAP
            /********************************************************************/


            int parsedRecordCap = 0;
            if (sRecordCap != null && sRecordCap != "")
            {
                int.TryParse(sRecordCap, out parsedRecordCap);
            }



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

            string uniqueEscapedPriceFormulasCommaDel = string.Join(
                ",",
                priceMatrixRecords.Items.Select(record => {
                    return "'" + utils.ParseIDOPropertyValue<string>(record.PropertyValues[priceMatrixRecords.PropertyKeys["Priceformula"]]) + "'";
                }).Distinct()
            );



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
                orderBy: "Item ASC, EffectDate DESC", // THIS WILL BE HARDCODED SO THAT THE FIRST RECORD FOR EACH ITEM WILL ALWAYS BE THE HIGHEST EFFECT DATE
                recordCap: 0
            );



            /********************************************************************/
            /* CREATE EMPTY TABLE
            /********************************************************************/

            DataTable fullTable = new DataTable("FullTable");
            DataTable filteredTable = new DataTable("PostFilteredTable");
            Dictionary<string, int> itemIndices = new Dictionary<string, int>();
            DataRow outputRow;

            // ADD COLUMN STRUCTURE

            fullTable.Columns.Add("PriceCode", typeof(string));
            fullTable.Columns.Add("Item", typeof(string));
            fullTable.Columns.Add("ListPrice", typeof(decimal));
            fullTable.Columns.Add("CustomerPrice", typeof(decimal));
            fullTable.Columns.Add("PriceType", typeof(string));
            fullTable.Columns.Add("EffectDate", typeof(DateTime));
            fullTable.Columns.Add("RecordDate", typeof(DateTime));
            fullTable.Columns.Add("RowPointer", typeof(string));



            /********************************************************************/
            /* LOOP THROUGH THE ITEM PRICE RECORDS AND FILL IN THE DATA TABLE
            /********************************************************************/

            foreach (IDOItem itemPriceRecord in itemPriceRecords.Items)
            {

                // GRAB THE ITEM

                string item = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["Item"]]);
                string itemPricecode = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["Pricecode"]]);
                decimal listPrice = utils.ParseIDOPropertyValue<decimal>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["UnitPrice1"]]);
                decimal customerPrice = listPrice;
                string priceType = "List";
                DateTime effectDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["EffectDate"]]);
                DateTime recordDate = utils.ParseIDOPropertyValue<DateTime>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["RecordDate"]]);
                string rowPointer = utils.ParseIDOPropertyValue<string>(itemPriceRecord.PropertyValues[itemPriceRecords.PropertyKeys["RowPointer"]]);

                // IF THERE IS A PRICE CODE, WE NEED TO GET THE CALCULATED CUSTOMER PRICE AND THE HIGHEST RECORD DATE FROM THE ITEMPRICE, PRICE MATRIX, AND PRICE FORMULA RECORDS

                if (priceCalculatorLookupTable.ContainsKey(itemPricecode))
                {
                    customerPrice = priceCalculatorLookupTable[itemPricecode].GetPrice(listPrice);
                    recordDate = (new List<DateTime>() { recordDate, priceCalculatorLookupTable[itemPricecode].PriceFormulaRecordDate, priceCalculatorLookupTable[itemPricecode].PriceMatrixRecordDate }).Max();
                    priceType = "Matrix";
                }

                if (!itemIndices.ContainsKey(item))
                {

                    // SAVE INDEX

                    itemIndices[item] = fullTable.Rows.Count;

                    // CREATE OUTPUT ROW

                    outputRow = fullTable.NewRow();

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

                    fullTable.Rows.Add(outputRow);

                }

            }



            /********************************************************************/
            /* APPLY POST-FILTERS
            /********************************************************************/

            string userPostQueryFilterString = utils.BuildFilterString(postQueryFilters.Values.ToList());

            if (userPostQueryFilterString != "")
            {
                filteredTable = fullTable.Clone();
                DataRow[] filteredRows = fullTable.Select(userPostQueryFilterString);
                foreach (DataRow row in filteredRows)
                {
                    filteredTable.ImportRow(row);
                }
            }
            else
            {
                filteredTable = fullTable;
            }

            filteredTable.DefaultView.Sort = parsedOrderBy;
            filteredTable = filteredTable.DefaultView.ToTable();
            if (parsedRecordCap > 0)
            {
                filteredTable = filteredTable.AsEnumerable().Take(parsedRecordCap).CopyToDataTable();
            }

            return filteredTable;

        }

    }

}