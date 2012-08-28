using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace MailNotifierService
{
    [RunInstaller(true)]
    public sealed class MailNotifierServiceInstaller : Installer
    {
        ServiceProcessInstaller m_serviceProcessInstaller;
        ServiceInstaller m_serviceInstaller;


        public MailNotifierServiceInstaller()
        {
            m_serviceProcessInstaller = new ServiceProcessInstaller();
            m_serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            m_serviceProcessInstaller.Password = null;
            m_serviceProcessInstaller.Username = null;

            m_serviceInstaller = new ServiceInstaller();
            m_serviceInstaller.DisplayName = "Mail Notifier Service";
            m_serviceInstaller.ServiceName = "MailNotifierService";

            // TODO: Change to on demand when the Mail Notifier is plugged in
            m_serviceInstaller.StartType = ServiceStartMode.Automatic;

            Installers.AddRange(new Installer [] { m_serviceProcessInstaller, m_serviceInstaller });
        }

    }
}
