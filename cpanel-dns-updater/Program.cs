using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CPanelDnsUpdater
{
    public class Program
    {
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
                else
                {
                    s.UpdateDomain(args[3], args[4], args[6]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
