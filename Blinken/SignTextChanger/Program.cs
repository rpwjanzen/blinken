using System.ServiceModel;
using System;

namespace SignTextClient
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
                return;
            try
            {
                const string uriText = "net.pipe://localhost/ledsign/sign";

                NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);

                EndpointAddress endpointAddress = new EndpointAddress(uriText);
                SignService.SignServiceClient client = new SignService.SignServiceClient(binding, endpointAddress);
                client.SetText(args[0]);
                client.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("She broke. :(");
            }
        }
    }
}
