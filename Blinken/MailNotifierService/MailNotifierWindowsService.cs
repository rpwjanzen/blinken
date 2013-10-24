using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceProcess;

namespace MailNotifierService
{
    public sealed class MailNotifierWindowsService : ServiceBase
    {
        private ServiceHost m_serviceHost;
        private NotifierService m_mailNotifierService;

        public MailNotifierWindowsService()
        {
        }

        public void StartInConsoleMode()
        {
            OnStart(null);
        }

        //protected override void OnStart(string[] args)
        //{
        //    //Uri baseAddress = new Uri("http://localhost:8002/mailnotifier/service");
        //    string address = "net.pipe://localhost/mailnotifier/sign";

        //    m_mailNotifierService = new NotifierService();
        //    m_serviceHost = new ServiceHost(m_mailNotifierService, new Uri(address));

        //    NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
        //    m_serviceHost.AddServiceEndpoint(typeof(IMailNotifierService), binding, address);

        //    long maxBufferPoolSize = binding.MaxBufferPoolSize;
        //    int maxBufferSize = binding.MaxBufferSize;
        //    int maxConnections = binding.MaxConnections;
        //    long maxReceivedMessageSize = binding.MaxReceivedMessageSize;
        //    NetNamedPipeSecurity security = binding.Security;
            
        //    string scheme = binding.Scheme;
        //    var bCollection = binding.CreateBindingElements();

        //    HostNameComparisonMode hostNameComparisonMode = binding.HostNameComparisonMode;

        //    bool TransactionFlow = binding.TransactionFlow;
        //    TransactionProtocol transactionProtocol = binding.TransactionProtocol;
        //    EnvelopeVersion envelopeVersion = binding.EnvelopeVersion;
        //    TransferMode transferMode = binding.TransferMode;

        //    m_serviceHost.Open();

        //    base.OnStart(args);
        //}

        protected override void OnStart(string[] args)
        {
            Uri baseAddress = new Uri("http://localhost:8002/mailnotifier/service");
            string address = "net.pipe://localhost/mailnotifier/sign";

            m_mailNotifierService = new NotifierService();
            m_serviceHost = new ServiceHost(m_mailNotifierService, baseAddress);

            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            m_serviceHost.AddServiceEndpoint(typeof(IMailNotifierService), binding, address);

            // Add a mex endpoint
            ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
            smb.HttpGetEnabled = true;
            smb.HttpGetUrl = new Uri("http://localhost:8003/mailnotifier");
            m_serviceHost.Description.Behaviors.Add(smb);

            long maxBufferPoolSize = binding.MaxBufferPoolSize;

            int maxBufferSize = binding.MaxBufferSize;

            int maxConnections = binding.MaxConnections;

            long maxReceivedMessageSize =
                binding.MaxReceivedMessageSize;

            NetNamedPipeSecurity security = binding.Security;

            string scheme = binding.Scheme;
            var bCollection = binding.CreateBindingElements();

            HostNameComparisonMode hostNameComparisonMode =
                binding.HostNameComparisonMode;

            bool TransactionFlow = binding.TransactionFlow;

            TransactionProtocol transactionProtocol = binding.TransactionProtocol;
            EnvelopeVersion envelopeVersion = binding.EnvelopeVersion;
            TransferMode transferMode = binding.TransferMode;

            m_serviceHost.Open();

            base.OnStart(args);
        }

        protected override void OnStop()
        {
            m_serviceHost.Close();
            IDisposable disposable = m_serviceHost;
            disposable.Dispose();

            base.OnStop();
        }
    }
}
