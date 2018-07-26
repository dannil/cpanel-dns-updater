using cPanelSharp;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.IO;
using CPanelDnsUpdater.Utility;
using System.Text.RegularExpressions;
using System.Text;
using log4net;

namespace CPanelDnsUpdater
{
    public class CPanelCommunicator
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CPanelCommunicator));

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
            log.Info("Fetching current external IP...");

            String currentIp = NetworkUtility.GetCurrentIpAddress();
            if (currentIp != null)
            {
                // Normalize string
                currentIp = Regex.Replace(currentIp, @"\r\n?|\n", "");
                currentIp = currentIp.Trim();
            }
            if (NetworkUtility.IsValidIpAddress(currentIp))
            {
                UpdateDomain(zone, record, currentIp);
            }
            else
            {
                log.ErrorFormat("Address {0} is not a valid IP address", currentIp);
            }
        }

        public void UpdateDomain(String zone, String record, String address)
        {
            log.InfoFormat("Updating record {0} with address {1}", record, address);

            String lastRunFile = String.Format("Status/{0}_last_run.txt", record);
            if (!File.Exists(lastRunFile))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(lastRunFile));
                using (FileStream fs = File.Create(lastRunFile))
                {
                    byte[] placeholderAddress = Encoding.UTF8.GetBytes("0.0.0.0");
                    fs.Write(placeholderAddress, 0, placeholderAddress.Length);
                }
            }

            String lastRunAddress = File.ReadAllText(lastRunFile);
            if (address.Equals(lastRunAddress))
            {
                log.InfoFormat("New address is identical to the address saved from the last run ({0}), not calling API", lastRunAddress);
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

                    log.InfoFormat("Updated address for record {0} to {1}", recordName, address);
                }
                else
                {
                    log.InfoFormat("Old and new addresses are identical ({0}), not updating", address);
                }
            }
            else
            {
                log.WarnFormat("Record {0} doesn't exist", record);
            }
        }
    }
}
