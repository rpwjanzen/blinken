using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ServiceModel;
using System.Threading;

namespace MinecraftLedSign
{
    class Program
    {
        const string MonitoredDirectory = @"C:\Users\Ryan\Desktop\Tekkit_Server_3.1.2\";
        const string Path = @"C:\Users\Ryan\Desktop\Tekkit_Server_3.1.2\logged_on_players.txt";
        private static bool g_stopRequested;

        static void Main(string[] args)
        {
            Thread updateSignThread = new Thread(() =>
                {
                    while (!g_stopRequested)
                    {
                        try
                        {
                            string[] onlinePlayers = File.ReadAllLines(Path);

                            string text = string.Empty;
                            for (int i = 0; i < onlinePlayers.Length; i++)
                            {
                                if (i != 0)
                                    text += ", ";

                                text += onlinePlayers[i];
                            }

                            const string uriText = "net.pipe://localhost/ledsign/sign";

                            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
                            EndpointAddress endpointAddress = new EndpointAddress(uriText);

                            using (SignService.SignServiceClient client = new SignService.SignServiceClient(binding, endpointAddress))
                            {
                                client.ScrollText(text);
                                client.Close();
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("She broke. :(" + e);
                        }

                        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(30));
                    }
                });
            updateSignThread.IsBackground = true;
            updateSignThread.Start();
            
            Console.WriteLine("Monitoring " + Path + " ...");
            Console.ReadLine();
            g_stopRequested = true;
        }
    }
}
