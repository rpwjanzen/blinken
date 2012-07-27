using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using Blinken;
using System.Threading;

namespace SignService
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                Uri baseAddress = new Uri("http://localhost:8000/ledsign/service");
                ServiceHost serviceHost = new ServiceHost(new SignService(), baseAddress);
                try
                {
                    // full service url is: http://localhost:8000/ledsign/service/ledsign
                    serviceHost.AddServiceEndpoint(typeof(ISignService), new WSHttpBinding(SecurityMode.None), "ledsign");

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
                Uri baseAddress = new Uri("http://localhost:8000/ledsign/service");
                string address = "net.pipe://localhost/ledsign/sign";

                // Create a ServiceHost for the CalculatorService type and provide the base address.
                SignService signService = new SignService();
                using (ServiceHost serviceHost = new ServiceHost(signService, baseAddress))
                {
                    NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
                    serviceHost.AddServiceEndpoint(typeof(ISignService), binding, address);

                    // Add a mex endpoint
                    ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                    smb.HttpGetEnabled = true;
                    smb.HttpGetUrl = new Uri("http://localhost:8001/ledsign");
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

    [ServiceContract]
    public interface ISignService
    {
        [OperationContract]
        void SetText(string text);
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    internal sealed class SignService : ISignService
    {
        private readonly LcdNotifier m_lcdNotifier;

        public SignService()
        {
            m_lcdNotifier = new LcdNotifier();
            m_lcdNotifier.Text = "GTG!";

            Thread thread = new Thread(new ThreadStart(() => 
            {
                while (true)
                {
                    lock (m_lcdNotifier)
                        m_lcdNotifier.DrawText(null);

                    System.Threading.Thread.Sleep(1000);
                }
            }));
            thread.IsBackground = true;
            thread.Start();
        }

        #region ISignService Members

        [OperationBehavior(ReleaseInstanceMode = ReleaseInstanceMode.None)]
        public void SetText(string text)
        {
            var host = OperationContext.Current.Host;
            
            lock (m_lcdNotifier)
                m_lcdNotifier.Text = (text ?? string.Empty).ToUpper();
        }

        #endregion
    }
}
