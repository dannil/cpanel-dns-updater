using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPanelDnsUpdater.Utility
{
    public class JsonUtility
    {
        public static JToken FindRecord(JArray records, String record)
        {
            foreach (var tempRecord in records)
            {
                String name = (tempRecord["name"] != null) ? tempRecord["name"].Value<String>() : "";
                if (record.Equals(name))
                {
                    return tempRecord;
                }
            }
            return null;
        }
    }
}
