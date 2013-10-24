using System;
using System.Drawing;
using System.ServiceModel;
using MailNotifierTimer.MailNotifierService;

namespace MailNotifierTimer
{
    class Program
    {
        static void Main(string[] args)
        {
            Color normalColor = Color.Green;
            Color warningColor = Color.Yellow;
            Color dangerColor = Color.Red;

            if (args.Length != 2)
            {
                return;
            }

            TimeSpan timeToWarning;
            if (!TimeSpan.TryParse(args[0], out timeToWarning))
            {
                return;
            }

            TimeSpan timeToDanger;
            if (!TimeSpan.TryParse(args[1], out timeToDanger))
            {
                return;
            }

            System.Timers.Timer warningTimer = new System.Timers.Timer();
            warningTimer.Interval = timeToWarning.TotalMilliseconds;
            warningTimer.Elapsed += (s, e) =>
            {
                warningTimer.Stop();
                TrySetColor(warningColor);
            };
            warningTimer.AutoReset = false;
            warningTimer.Start();

            System.Timers.Timer dangerTimer = new System.Timers.Timer();
            dangerTimer.Interval = timeToDanger.TotalMilliseconds;
            dangerTimer.Elapsed += (s, e) =>
            {
                dangerTimer.Stop();
                TrySetColor(dangerColor);
            };
            dangerTimer.AutoReset = false;
            dangerTimer.Start();

            while (true)
            {
                TrySetColor(normalColor);

                Console.WriteLine("Press <Enter> to restart.");
                Console.WriteLine("Press enter 'quit' to quit.");
                string result = Console.ReadLine();

                if (result == "quit")
                {
                    break;
                }

                warningTimer.Start();
                dangerTimer.Start();
            }

            warningTimer.Dispose();
            dangerTimer.Dispose();

            System.Threading.Thread.Sleep(500);
        }

        private static bool TrySetColor(Color color)
        {
            try
            {
                const string uriText = "net.pipe://localhost/mailnotifier/sign";
                NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
                EndpointAddress endpointAddress = new EndpointAddress(uriText);
                using (MailNotifierServiceClient client = new MailNotifierServiceClient(binding, endpointAddress))
                {
                    byte red = color.R;
                    byte green = color.G;
                    byte blue = color.B;

                    client.SetColor(red, green, blue);

                    client.Close();
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to set color to " + color + ": " + e.Message);
                return false;
            }
        }
    }
}
