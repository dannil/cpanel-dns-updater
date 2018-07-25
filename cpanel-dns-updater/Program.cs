using log4net;
using System;
using System.Net;

namespace CPanelDnsUpdater
{
    public class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CPanelCommunicator));

        public static void Main(string[] args)
        {
            try
            {
                String username = args[0];
                String password = args[1];
                String host = args[2];

                CPanelCommunicator s = new CPanelCommunicator(username, password, host);

                String zone = args[3];
                String record = args[4];

                String mode = args[5];
                String modeValue = args[6];
                if ("auto".Equals(modeValue))
                {
                    s.UpdateDomain(args[3], args[4]);
                }
                else if ("random".Equals(modeValue))
                {
                    byte[] data = new byte[4];
                    new Random().NextBytes(data);
                    IPAddress ip = new IPAddress(data);
                    s.UpdateDomain(args[3], args[4], ip.ToString());
                }
                else
                {
                    s.UpdateDomain(args[3], args[4], args[6]);
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }
        }
    }
}
