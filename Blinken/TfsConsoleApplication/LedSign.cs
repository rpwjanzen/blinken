using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace TfsConsoleApplication
{
    public sealed class LedSign
    {
        public static void RenderImageToLedSign(bool[] image)
        {
            const string uriText = "net.pipe://localhost/ledsign/sign";

            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);

            EndpointAddress endpointAddress = new EndpointAddress(uriText);
            SignService.SignServiceClient client = new SignService.SignServiceClient(binding, endpointAddress);
            client.ScrollImage(image);
            client.Close();
        }
    }
}
