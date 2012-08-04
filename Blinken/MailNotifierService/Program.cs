using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace MailNotifierService
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                Uri baseAddress = new Uri("http://localhost:8002/mailnotifier/service");
                ServiceHost serviceHost = new ServiceHost(new NotifierService(), baseAddress);
                try
                {
                    // full service url is: http://localhost:8002/mailnotifier/service/mailnotifier
                    serviceHost.AddServiceEndpoint(typeof(IMailNotifierService), new WSHttpBinding(SecurityMode.None), "mailnotifier");

                    // create a mex behavior
                    ServiceMetadataBehavior smb = new ServiceMetadataBehavior()
                    {
                        HttpGetEnabled = true,
                    };
                    serviceHost.Description.Behaviors.Add(smb);

                    serviceHost.Open();

                    Console.WriteLine("The service is ready.");
                    Console.ReadLine();

                    serviceHost.Close();
                }
                catch (CommunicationException e)
                {
                    Console.WriteLine("An exception occurred " + e.Message);
                    serviceHost.Abort();
                    Console.ReadLine();
                }
            }
            else
            {
                Uri baseAddress = new Uri("http://localhost:8002/mailnotifier/service");
                string address = "net.pipe://localhost/mailnotifier/sign";

                NotifierService mailNotifierService = new NotifierService();
                using (ServiceHost serviceHost = new ServiceHost(mailNotifierService, baseAddress))
                {
                    NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
                    serviceHost.AddServiceEndpoint(typeof(IMailNotifierService), binding, address);

                    // Add a mex endpoint
                    ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                    smb.HttpGetEnabled = true;
                    smb.HttpGetUrl = new Uri("http://localhost:8003/mailnotifier");
                    serviceHost.Description.Behaviors.Add(smb);

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

                    serviceHost.Open();

                    Console.WriteLine("The service is ready.");
                    Console.WriteLine("Press <ENTER> to terminate service.");
                    Console.WriteLine();
                    Console.ReadLine();

                    serviceHost.Close();
                }
            }
        }
    }
}
