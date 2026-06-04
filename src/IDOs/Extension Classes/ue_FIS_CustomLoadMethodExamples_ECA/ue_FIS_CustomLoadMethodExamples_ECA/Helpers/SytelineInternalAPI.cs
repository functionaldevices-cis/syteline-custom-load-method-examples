using Infor.DocumentManagement.ICP;
using Mongoose.Core.Extensions;
using Mongoose.IDO;
using Mongoose.IDO.DataAccess;
using Mongoose.IDO.Metadata;
using Mongoose.IDO.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ue_FIS_CustomLoadMethodExamples_ECA.Models.SytelineAPI;

namespace ue_FIS_CustomLoadMethodExamples_ECA.Helpers {

    public class SytelineInternalAPI {

        public IIDOCommands IDOCommands { get; set; }

        public SytelineInternalAPI(IIDOCommands IDOCommands)
        {
            this.IDOCommands = IDOCommands;
        }

        public T ParseIDOPropertyValue<T>(IDOPropertyValue value)
        {

            if (!value.IsNull)
            {
                return value.GetValue<T>();
            }

            return default;

        }

        public string BuildFilterString(List<string> filters, string joiner = "AND")
        {

            string joinedFilter = string.Join(" " + joiner + " ", filters.Where(filter => filter != "").Select(filter => "(" + filter + ")"));

            return joinedFilter != "" ? $"(" + joinedFilter + ")" : "";

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

        public IDOUpdateItem CreateRecord(string IDOName, IDOUpdateItem record)
        {

            if (this.IDOCommands != null)
            {

                // CREATE THE UPDATE REQUEST WRAPPER

                UpdateCollectionRequestData oUpdateRequest = new UpdateCollectionRequestData(IDOName)
                {
                    RefreshAfterUpdate = true
                };
                oUpdateRequest.Items.Add(record);

                // SEND THE UPDATE REQUEST

                UpdateCollectionResponseData response = this.IDOCommands.UpdateCollection(oUpdateRequest);
                if (response.Items != null && response.Items.Count == 1)
                {
                    return response.Items[0];
                }

            }

            return null;

        }

        public IDOUpdateItems CreateRecords(string IDOName, List<IDOUpdateItem> records)
        {

            if (this.IDOCommands != null)
            {

                // CREATE THE UPDATE REQUEST WRAPPER

                UpdateCollectionRequestData oUpdateRequest = new UpdateCollectionRequestData(IDOName)
                {
                    RefreshAfterUpdate = true
                };
                oUpdateRequest.Items.AddRange(records);

                // SEND THE UPDATE REQUEST

                try
                {
                    UpdateCollectionResponseData response = this.IDOCommands.UpdateCollection(oUpdateRequest);
                    if (response.Items != null && response.Items.Count == records.Count)
                    {
                        return response.Items;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }

            }

            return null;

        }

        public IDOUpdateItem UpdateRecord(string IDOName, IDOUpdateItem record)
        {

            if (this.IDOCommands != null)
            {

                // CREATE THE UPDATE REQUEST WRAPPER

                UpdateCollectionRequestData oUpdateRequest = new UpdateCollectionRequestData(IDOName)
                {
                    RefreshAfterUpdate = true
                };
                oUpdateRequest.Items.Add(record);

                // SEND THE UPDATE REQUEST

                try
                {
                    UpdateCollectionResponseData response = this.IDOCommands.UpdateCollection(oUpdateRequest);
                    if (response.Items != null && response.Items.Count == 1)
                    {
                        return response.Items[0];
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }


            }

            return null;

        }

        public IDOUpdateItems UpdateRecords(string IDOName, List<IDOUpdateItem> records)
        {

            if (this.IDOCommands != null)
            {

                // CREATE THE UPDATE REQUEST WRAPPER

                UpdateCollectionRequestData oUpdateRequest = new UpdateCollectionRequestData(IDOName)
                {
                    RefreshAfterUpdate = true
                };
                oUpdateRequest.Items.AddRange(records);

                // SEND THE UPDATE REQUEST

                try
                {
                    UpdateCollectionResponseData response = this.IDOCommands.UpdateCollection(oUpdateRequest);
                    if (response.Items != null && response.Items.Count == records.Count)
                    {
                        return response.Items;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }

            }

            return null;

        }

        public IDOUpdateItem DeleteRecord(string IDOName, IDOUpdateItem record)
        {

            if (this.IDOCommands != null)
            {

                // CREATE THE UPDATE REQUEST WRAPPER

                UpdateCollectionRequestData oUpdateRequest = new UpdateCollectionRequestData(IDOName)
                {
                    RefreshAfterUpdate = true
                };
                oUpdateRequest.Items.Add(record);

                // SEND THE UPDATE REQUEST

                try
                {
                    UpdateCollectionResponseData response = this.IDOCommands.UpdateCollection(oUpdateRequest);
                    if (response.Items != null && response.Items.Count == 1)
                    {
                        return response.Items[0];
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }


            }

            return null;

        }

        public IDOUpdateItems DeleteRecords(string IDOName, List<IDOUpdateItem> records)
        {

            if (this.IDOCommands != null)
            {

                // CREATE THE UPDATE REQUEST WRAPPER

                UpdateCollectionRequestData oUpdateRequest = new UpdateCollectionRequestData(IDOName)
                {
                    RefreshAfterUpdate = true
                };
                oUpdateRequest.Items.AddRange(records);

                // SEND THE UPDATE REQUEST

                try
                {
                    UpdateCollectionResponseData response = this.IDOCommands.UpdateCollection(oUpdateRequest);
                    return response.Items;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }


            }

            return null;

        }

        public (bool success, T value) LoadHighestValue<T>(string IDOName, string property, string filter = null)
        {

            LoadRecordsResponseData records = this.LoadRecords(
                IDOName: IDOName,
                filter: filter ?? "",
                properties: new List<string>() { property },
                orderBy: property + " DESC",
                recordCap: 1
            );

            if (records.Items.Count > 0)
            {
                return (true, this.ParseIDOPropertyValue<T>(records.Items[0].PropertyValues[records.PropertyKeys[property]]));
            }

            return (false, default);

        }

        public (bool success, T value) LoadLowestValue<T>(string IDOName, string property, string filter = null)
        {

            LoadRecordsResponseData records = this.LoadRecords(
                IDOName: IDOName,
                filter: filter ?? "",
                properties: new List<string>() { property },
                orderBy: property + " ASC",
                recordCap: 1
            );

            if (records.Items.Count > 0)
            {
                return (true, this.ParseIDOPropertyValue<T>(records.Items[0].PropertyValues[records.PropertyKeys[property]]));
            }

            return (false, default);

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

            if (propertyUpdates != null)
            {

                foreach (IDOUpdateProperty propertyUpdate in propertyUpdates)
                {

                    // IF WE HAVE A VALUE IN THE VALUE PROP, USE THAT. OTHERWISE, USE WHAT IS IN THE LOAD COLLECTION.

                    if (propertyUpdate.Value != null)
                    {

                        oUpdateItem.Properties.Add(propertyUpdate.Name, propertyUpdate.Value, propertyUpdate.Modified);

                    }

                }

            }

            return oUpdateItem;

        }

        public IDOUpdateItem BuildDeleteItem(string itemID)
        {

            // CREATE A DELETE ITEM OBJECT

            IDOUpdateItem oUpdateItem = new IDOUpdateItem(UpdateAction.Delete, itemID)
            {
                ItemID = itemID
            };

            return oUpdateItem;

        }

    }

}