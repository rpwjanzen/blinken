using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Clock
{
    class Program
    {
        private static Timer m_clockTimer;

        static void Main(string[] args)
        {
            RenderToLedSign(DateTime.Now.ToString("HH:mm"));

            m_clockTimer = new Timer();
            m_clockTimer.Interval = TimeSpan.FromSeconds(30).TotalMilliseconds;
            m_clockTimer.Elapsed += (s,e) => RenderToLedSign(DateTime.Now.ToString("HH:mm"));
            m_clockTimer.Start();

            Console.WriteLine("Press <Enter> to exit.");
            Console.ReadLine();

            m_clockTimer.Stop();
            m_clockTimer.Dispose();
        }

        private static void RenderToLedSign(string text)
        {
            const string uriText = "net.pipe://localhost/ledsign/sign";

            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);

            EndpointAddress endpointAddress = new EndpointAddress(uriText);
            SignServiceReference.SignServiceClient client = new SignServiceReference.SignServiceClient(binding, endpointAddress);
            client.SetText(text);
            client.Close();
        }
    }
}
