using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ue_FIS_CustomLoadMethodExamples_ECA.Models
{
    public class ItemPriceCodePriceInfo
    {

        public string PriceCode { get; set; }

        public string Type { get; set; } // "A" or "P"

        private decimal Value { get; set; }

        public DateTime PriceMatrixRecordDate { get; set; }

        public DateTime PriceFormulaRecordDate { get; set; }

        public ItemPriceCodePriceInfo(string priceCode, string type, decimal value, DateTime priceMatrixRecordDate = default, DateTime priceFormulaRecordDate = default)
        {

            this.PriceCode = priceCode;
            this.Type = type;
            this.Value = value;
            this.PriceMatrixRecordDate = priceMatrixRecordDate;
            this.PriceFormulaRecordDate = priceFormulaRecordDate;
        }

        public decimal GetPrice(decimal listPrice)
        {

            if (this.Type == "A")
            {
                return this.Value;
            }
            else if (this.Type == "P")
            {
                return Math.Round(listPrice * (100m + this.Value) / 100m, 2, MidpointRounding.AwayFromZero);
            }
            else
            {
                return listPrice;
            }

        }

    }
}
