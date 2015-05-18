using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ManInTheMiddle
{
    class InfoManager
    {
        private System.Threading.Timer timer;
        private string lastGateway;

        public InfoManager()
        {
            lastGateway = GetGatewayMAC();
        }

        public void StartListener()
        {
            // check whether MAC entry has changed every 5 seconds
            timer = new System.Threading.Timer(_ => OnCallBack(), null, 5000, Timeout.Infinite);
        }

        private void OnCallBack()
        {
            timer.Dispose();

            if (!(GetGatewayMAC() == lastGateway))
            {
                Console.WriteLine("Possible ARP cache poison detected.");
            }
            else
            {
                lastGateway = GetGatewayMAC();
            }

            // todo: is it really necessary with an additional call to this?
            timer = new Timer(_ => OnCallBack(), null, 5000, Timeout.Infinite);
        }

        private string GetDefaultGateway()
        {
            // todo: make dynamic and not hardcoded
            return "172.25.18.1";
        }

        private string GetArpTable()
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = @"C:\Windows\System32\arp.exe";
            start.Arguments = "-a";
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;

            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private string GetGatewayMAC()
        {
            string routerIP = GetDefaultGateway();

            string regx = String.Format(@"({0} [\W]*) ([a-z0-9-]*)", routerIP);
            Regex regex = new Regex(@regx);
            Match matches = regex.Match(GetArpTable());

            // todo: make sure this is the correct gateway every time
            return matches.Groups[2].ToString();
        }
    }

}
