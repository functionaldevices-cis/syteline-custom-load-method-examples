using Mongoose.IDO.Metadata;
using Mongoose.IDO.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ue_FIS_CustomLoadMethodExamples_ECA.Models
{

    public class LoadRecordsResponseData
    {
        public Dictionary<string, int> PropertyKeys { get; set; } = new Dictionary<string, int>();

        public List<IDOItem> Items { get { return this.LoadCollectionResponseData.Items; } }

        public LoadCollectionResponseData LoadCollectionResponseData { get; set; }

        public LoadCollectionRequestData LoadCollectionRequestData { get; set; }

        public LoadRecordsResponseData(LoadCollectionResponseData loadCollectionResponseData, LoadCollectionRequestData loadCollectionRequestData, string queryIDOName, string queryFilter, string queryOrderBy, List<string> queryProperties, int queryRecordCap = 0)
        {

            this.LoadCollectionRequestData = loadCollectionRequestData;
            this.LoadCollectionResponseData = loadCollectionResponseData;

            queryProperties = queryProperties ?? new List<string>();

            this.PropertyKeys = Enumerable.Range(0, queryProperties.Count).ToDictionary(
                i => queryProperties[i],
                i => i
            );

        }

    }

}
