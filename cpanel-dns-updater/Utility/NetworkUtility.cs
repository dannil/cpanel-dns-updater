using System;
using System.Net;
using System.Net.Sockets;

namespace CPanelDnsUpdater.Utility
{
    public class NetworkUtility
    {
        public static String GetCurrentIpAddress()
        {
            WebClient c = new WebClient();
            return c.DownloadString("http://myip.cpanel.net");
        }

        public static Boolean IsValidIpAddress(String address)
        {
            if (IPAddress.TryParse(address, out IPAddress ipAddress))
            {
                switch (ipAddress.AddressFamily)
                {
                    case AddressFamily.InterNetwork:
                        return true;
                    case AddressFamily.InterNetworkV6:
                        return true;
                    default:
                        return false;
                }
            }
            return false;
        }
    }
}
