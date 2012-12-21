using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using MailNotifierTimer.MailNotifierService;

namespace MailNotifierTimer
{
    class Program
    {
        static bool m_isFlashEnabled = false;
        static readonly object m_lock = new object();
        static Color m_currentColor;
        static bool m_stopRequested;
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

            m_currentColor = normalColor;

            System.Timers.Timer warningTimer = new System.Timers.Timer();
            warningTimer.Interval = timeToWarning.TotalMilliseconds;
            warningTimer.Elapsed += (s, e) =>
            {
                warningTimer.Stop();
                lock (m_lock)
                {
                    m_currentColor = warningColor;
                }
            };
            warningTimer.AutoReset = false;
            warningTimer.Start();

            System.Timers.Timer dangerTimer = new System.Timers.Timer();
            dangerTimer.Interval = timeToDanger.TotalMilliseconds;
            dangerTimer.Elapsed += (s, e) =>
            {
                dangerTimer.Stop();
                lock (m_lock)
                {
                    m_isFlashEnabled = true;
                    m_currentColor = dangerColor;
                }
            };
            dangerTimer.AutoReset = false;
            dangerTimer.Start();

            System.Threading.Thread flashNotifierThread = new System.Threading.Thread(() =>
            {
                const string uriText = "net.pipe://localhost/mailnotifier/sign";
                NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
                EndpointAddress endpointAddress = new EndpointAddress(uriText);
                MailNotifierServiceClient client = new MailNotifierServiceClient(binding, endpointAddress);
                Color? previousColor = null;

                while (!m_stopRequested)
                {
                    bool isFlashEnabled;
                    Color currentColor;
                    lock (m_lock)
                    {
                        isFlashEnabled = m_isFlashEnabled;
                        currentColor = m_currentColor;
                    }

                    if (isFlashEnabled)
                    {
                        client.SetColor(0, 0, 0);
                        System.Threading.Thread.Sleep(250);

                        byte red = currentColor.R;
                        byte green = currentColor.G;
                        byte blue = currentColor.B;
                        client.SetColor(red, green, blue);

                        System.Threading.Thread.Sleep(250);
                    }
                    else
                    {
                        if (previousColor != currentColor)
                        {
                            byte red = currentColor.R;
                            byte green = currentColor.G;
                            byte blue = currentColor.B;
                            client.SetColor(red, green, blue);

                            previousColor = currentColor;
                        }
                    }
                }

                client.Close();
            });
            flashNotifierThread.Start();

            while (true)
            {
                Console.WriteLine("Press <Enter> to restart.");
                Console.WriteLine("Press enter 'quit' to quit.");
                string result = Console.ReadLine();
                if (result == "quit")
                    break;

                m_currentColor = normalColor;
                m_isFlashEnabled = false;
                System.Threading.Thread.Sleep(500);

                warningTimer.Start();
                dangerTimer.Start();
            }

            warningTimer.Dispose();
            dangerTimer.Dispose();

            m_currentColor = normalColor;
            m_isFlashEnabled = false;
            System.Threading.Thread.Sleep(500);

            m_stopRequested = true;
            flashNotifierThread.Join();
        }
    }
}
