using cPanelSharp;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CPanelDnsUpdater
{
    public class CPanelCommunicator
    {
        private String username;
        private String password;
        private String host;

        private CPanelCommunicator()
        {

        }

        public CPanelCommunicator(String username, String password, String host) : this()
        {
            this.username = username;
            this.password = password;
            this.host = host;
        }

        public void UpdateDomain(String zone, String record)
        {
            Console.WriteLine("Fetching current external IP...");
            String currentIp = GetCurrentIpAddress();
            UpdateDomain(zone, record, currentIp);
        }

        public void UpdateDomain(String zone, String record, String address)
        {
            Console.WriteLine("Updating record {0} with address {1}", record, address);

            String recordName = record;
            String recordNameWithDot = record += '.'; // Add a trailing dot

            // Hack to handle non-trusted SSL certificates
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            cPanelClient client = new cPanelClient(username, host, password: password, cpanel: true);
            String jsonResponse = client.Api2("ZoneEdit", "fetchzone", null, new { domain = zone });

            JObject jsonObject = JObject.Parse(jsonResponse);
            JToken jsonData = jsonObject["cpanelresult"]["data"][0];
            JArray records = jsonData["record"] as JArray;
            JToken jsonRecord = FindRecord(records, recordNameWithDot);

            if (jsonRecord != null)
            {
                String oldAddress = jsonRecord["address"].Value<String>();
                if (!address.Equals(oldAddress))
                {
                    // Line is "primary key" for the record
                    String line = jsonRecord["line"].Value<String>();
                    String response = client.Api2("ZoneEdit", "edit_zone_record", null, new { domain = zone, line, address });
                    Console.WriteLine("Updated address for record {0} to {1}", recordName, address);
                }
                else
                {
                    Console.WriteLine("Old and new addresses are identical ({0} == {1}), not updating", oldAddress, address);
                }
            }
            else
            {
                Console.WriteLine("Record {0} doesn't exist", record);
            }
        }

        private JToken FindRecord(JArray records, String record)
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

        private String GetCurrentIpAddress()
        {
            WebClient c = new WebClient();
            return c.DownloadString("http://myip.cpanel.net");
        }
    }
}
