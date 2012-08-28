using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceProcess;

namespace MailNotifierService
{
    class Program
    {
        private static MailNotifierWindowsService m_service;

        static void Main(string[] args)
        {
            m_service = new MailNotifierWindowsService();

            if (Environment.UserInteractive)
            {
                m_service.StartInConsoleMode();

                Console.WriteLine("Mail notifier service started.");
                Console.WriteLine("Press Enter to terminate service.");

                Console.ReadLine();
            }
            else
            {
                ServiceBase.Run(m_service);
            }
        }
    }
}
