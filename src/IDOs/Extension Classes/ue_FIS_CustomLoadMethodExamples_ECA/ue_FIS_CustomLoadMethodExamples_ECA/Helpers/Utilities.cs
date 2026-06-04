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

            if ((debugLevel < 0) || (2 < debugLevel))
            {
                debugLevel = 0;
            }

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

        public string BuildNumberDecimalMask(int precision)
        {
            string precisionMask = (precision > 0 ? "." : "");
            for (int counter = 0; counter < precision; counter++)
            {
                precisionMask += "f";
            }
            return precisionMask;
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


    }

}