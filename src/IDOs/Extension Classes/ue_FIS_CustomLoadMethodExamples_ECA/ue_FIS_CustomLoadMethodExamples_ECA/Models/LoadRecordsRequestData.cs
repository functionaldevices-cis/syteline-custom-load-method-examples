using Mongoose.IDO.Metadata;
using Mongoose.IDO.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using ue_FIS_CustomLoadMethodExamples_ECA.Helpers;

namespace ue_FIS_CustomLoadMethodExamples_ECA.Models
{

    public class LoadRecordsRequestData
    {

        public LoadCollectionRequestData ContextRequest { get; set; }

        public string Filter
        {
            get
            {
                return this.ContextRequest.Filter;
            }
            set
            {
                this.ContextRequest.Filter = value;
            }
        }

        public string OrderBy
        {
            get
            {
                return this.ContextRequest.OrderBy;
            }
            set
            {
                this.ContextRequest.OrderBy = value;
            }
        }

        public int RecordCap
        {
            get
            {
                return this.ContextRequest.RecordCap;
            }
            set
            {
                this.ContextRequest.RecordCap = value;
            }
        }

        public string Bookmark
        {
            get
            {
                return this.ContextRequest.Bookmark;
            }
            set
            {
                this.ContextRequest.Bookmark = value;
            }
        }

        public List<(string originalString, string propertyName, string operatorName, string value)> Filters => this.Filter != "" ? this.Filter.Replace("and", "AND").Split(
            new string[] { "AND" }, StringSplitOptions.None
        ).Select(
            filterString =>
            {
                filterString = FixParenthesis("( " + filterString.Trim().Replace(" <> N", " <> ").Replace(" = N", " = ").Replace(" like N", " like ").Replace("DATEPART( yyyy, ", "YEAR( ").Replace("DATEPART( mm, ", "MONTH( ").Replace("DATEPART( dd, ", "DAY( ") + " )");

                string filterOperator = FilterStringParser.ExtractOperator(filterString);
                string filterValue = FilterStringParser.ExtractValue(filterString, filterOperator);
                string filterPropertyName = FilterStringParser.ExtractPropertyName(filterString, filterOperator, filterValue);

                return (filterString, filterPropertyName, filterOperator, filterValue);
            }
        ).ToList() : new List<(string originalString, string propertyName, string operatorName, string value)>();


        public LoadRecordsRequestData(LoadCollectionRequestData contextRequest, string filterOverride = null, string orderByOverride = null, string recordCapOverride = null, string bookmarkOverride = null)
        {

            // SAVE THE CONTEXT REQUEST

            this.ContextRequest = contextRequest;

            // APPLY FILTER OVERRIDE IF THERE IS ONE

            if (filterOverride != null)
            {
                this.ContextRequest.Filter = filterOverride;
            }

            this.ContextRequest.Filter = this.ContextRequest.Filter ?? "";

            // APPLY ORDERBY OVERRIDE IF THERE IS ONE

            if (orderByOverride != null)
            {
                this.ContextRequest.OrderBy = orderByOverride;
            }

            this.ContextRequest.OrderBy = this.ContextRequest.OrderBy ?? "";

            // APPLY RECORD CAP OVERRIDE IF THERE IS ONE

            if (recordCapOverride != null && recordCapOverride != "")
            {
                int parsedRecordCap = 0;
                int.TryParse(recordCapOverride, out parsedRecordCap);
                this.ContextRequest.RecordCap = parsedRecordCap;
            }

            // APPLY BOOKMARK OVERRIDE IF THERE IS ONE

            if (bookmarkOverride != null)
            {
                this.ContextRequest.Bookmark = bookmarkOverride;
            }

            if (this.ContextRequest.Bookmark == null || this.ContextRequest.Bookmark == "" )
            {
                this.ContextRequest.Bookmark = "<B/>";
            }

        }

        private static string FixParenthesis(string input)
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

    }

}
