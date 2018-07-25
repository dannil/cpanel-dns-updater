using cPanelSharp;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.IO;
using CPanelDnsUpdater.Utility;
using System.Text.RegularExpressions;
using System.Text;

namespace CPanelDnsUpdater
{
    public class CPanelCommunicator
    {
        private String username;
        private String password;
        private String host;

        private cPanelClient client;

        private CPanelCommunicator()
        {

        }

        public CPanelCommunicator(String username, String password, String host) : this()
        {
            this.username = username;
            this.password = password;
            this.host = host;

            client = new cPanelClient(username, host, password: password, cpanel: true);
        }

        public void UpdateDomain(String zone, String record)
        {
            Console.WriteLine("Fetching current external IP...");

            String currentIp = NetworkUtility.GetCurrentIpAddress();
            if (currentIp != null)
            {
                // Normalize string
                currentIp = Regex.Replace(currentIp, @"\r\n?|\n", " ");
                currentIp = currentIp.Trim();
            }
            if (NetworkUtility.IsValidIpAddress(currentIp))
            {
                UpdateDomain(zone, record, currentIp);
            }
            else
            {
                Console.WriteLine("Address {0} is not a valid IP address", currentIp);
            }
        }

        public void UpdateDomain(String zone, String record, String address)
        {
            Console.WriteLine("Updating record {0} with address {1}", record, address);

            String lastRunFile = String.Format("status/{0}_last_run.txt", record);
            if (!File.Exists(lastRunFile))
            {
                Directory.CreateDirectory("status");
                using (FileStream fs = File.Create(lastRunFile))
                {
                    byte[] placeholderAddress = Encoding.UTF8.GetBytes("0.0.0.0");
                    fs.Write(placeholderAddress, 0, placeholderAddress.Length);
                }
            }

            String lastRunAddress = File.ReadAllText(lastRunFile);
            if (address.Equals(lastRunAddress))
            {
                Console.WriteLine("New address is identical to the address saved from the last run ({0}), not calling API", lastRunAddress);
                return;
            }

            // Hack to handle non-trusted SSL certificates
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            String recordName = record;
            String recordNameWithDot = record += '.'; // Add a trailing dot
            
            String jsonResponse = client.Api2("ZoneEdit", "fetchzone", null, new { domain = zone });

            JObject jsonObject = JObject.Parse(jsonResponse);
            JToken jsonData = jsonObject["cpanelresult"]["data"][0];
            JArray records = jsonData["record"] as JArray;
            JToken jsonRecord = JsonUtility.FindRecord(records, recordNameWithDot);

            if (jsonRecord != null)
            {
                String oldAddress = jsonRecord["address"].Value<String>();
                if (!address.Equals(oldAddress))
                {
                    // Line is "primary key" for the record
                    String line = jsonRecord["line"].Value<String>();
                    String response = client.Api2("ZoneEdit", "edit_zone_record", null, new { domain = zone, line, address });

                    File.WriteAllText(lastRunFile, address);

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
    }
}
