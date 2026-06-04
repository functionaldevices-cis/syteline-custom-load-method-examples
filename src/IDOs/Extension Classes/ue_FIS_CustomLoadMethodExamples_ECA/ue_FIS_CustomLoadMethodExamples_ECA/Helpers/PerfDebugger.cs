using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ue_FIS_CustomLoadMethodExamples_ECA.Helpers
{
    public class PerfDebugger
    {
        public List<DateTime> Timestamps { get; set; } = new List<DateTime>();

        public List<string> Durations
        {
            get
            {
                List<string> list = new List<string>();
                for (int i = 1; i < Timestamps.Count; i++)
                {
                    list.Add((Timestamps[i] - Timestamps[i - 1]).ToString());
                }
                return list;
            }
        }

        public void RecordTimestamp()
        {
            Timestamps.Add(DateTime.Now);
        }
    }

}
