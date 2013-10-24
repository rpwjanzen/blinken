using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using SystemTimer = System.Timers.Timer;
using Thread = System.Threading.Thread;

namespace Timer
{
    class Program
    {
        private static readonly object m_lock = new object();

        private static SystemTimer m_updateTimerTextTimer;
        
        private static DateTime m_targetTime;
        private static TimeSpan m_warningTime;

        private static bool m_isFlashing = false;

        private static Thread m_updateSignTextThread;
        private static bool m_isStopRequested;

        private static string m_signText;

        static void Main(string[] args)
        {
            if (args.Length != 2)
                return;

            m_targetTime = DateTime.Parse(args[0]);
            m_warningTime = TimeSpan.Parse(args[1]);

            m_updateSignTextThread = new Thread(new System.Threading.ThreadStart(() =>
            {
                while (!m_isStopRequested)
                {
                    lock (m_lock)
                    {
                        RenderToLedSign(m_signText);
                        System.Threading.Thread.Sleep(250);
                        RenderToLedSign(m_signText);
                        System.Threading.Thread.Sleep(250);
                    }

                    if (m_isFlashing)
                    {
                        RenderToLedSign(" ");
                        System.Threading.Thread.Sleep(250);
                        RenderToLedSign(" ");
                        System.Threading.Thread.Sleep(250);
                    }
                }
            }));
            

            m_updateTimerTextTimer = new SystemTimer();
            m_updateTimerTextTimer.Interval = TimeSpan.FromMilliseconds(500).TotalMilliseconds;
            m_updateTimerTextTimer.Elapsed += (s, e) => UpdateSignText();


            RenderToLedSign("Hello");
            System.Threading.Thread.Sleep(5000);
            UpdateSignText();
            m_updateSignTextThread.Start();
            m_updateTimerTextTimer.Start();

            Console.WriteLine("Press <Enter> to exit.");
            Console.ReadLine();

            m_updateTimerTextTimer.Stop();
            m_updateTimerTextTimer.Dispose();
            m_isStopRequested = true;
        }

        private static void UpdateSignText()
        {
            TimeSpan timeTillTargetTime = m_targetTime - DateTime.Now;
            if (timeTillTargetTime < TimeSpan.Zero)
                timeTillTargetTime = TimeSpan.Zero;

            bool isFlashRequested = timeTillTargetTime <= m_warningTime;

            string timeText;
            if (timeTillTargetTime.TotalDays > 1)
            {
                double totalHours = timeTillTargetTime.TotalHours;
                string totalHoursText = string.Format("{00:0}", Math.Floor(totalHours));
                timeText = totalHoursText + timeTillTargetTime.ToString(@"\:mm");                
            }
            else if (timeTillTargetTime.TotalMinutes > 60)
            {
                timeText = timeTillTargetTime.ToString(@"hh\:mm");
            }
            else if (timeTillTargetTime.TotalSeconds > 60)
            {
                if (m_updateTimerTextTimer.Interval > TimeSpan.FromSeconds(1).TotalMilliseconds)
                    m_updateTimerTextTimer.Interval = TimeSpan.FromSeconds(1).TotalMilliseconds;

                timeText = timeTillTargetTime.ToString(@"mm\:ss");
            }
            else
                timeText = timeTillTargetTime.ToString(@"mm\:ss");

            lock (m_lock)
            {
                m_signText = timeText;
            }
            if (m_isFlashing != isFlashRequested)
                m_isFlashing = isFlashRequested;
        }
        
        private static void RenderToLedSign(string text)
        {
            const string uriText = "net.pipe://localhost/ledsign/sign";

            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);

            EndpointAddress endpointAddress = new EndpointAddress(uriText);
            SignService.SignServiceClient client = new SignService.SignServiceClient(binding, endpointAddress);
            client.SetText(text);
            client.Close();
        }

    }
}
