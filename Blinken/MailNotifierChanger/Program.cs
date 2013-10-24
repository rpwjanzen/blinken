using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Drawing;
using MailNotifierClient.MailNotifierService;

namespace MailNotifierClient
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                const string uriText = "net.pipe://localhost/mailnotifier/sign";

                NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);

                EndpointAddress endpointAddress = new EndpointAddress(uriText);

                MailNotifierServiceClient client = new MailNotifierServiceClient(binding, endpointAddress);

                if (args.Length == 1 && args[0] == "rainbow")
                {
                    var colors = new Color []
                    {
                        Color.Red,
                        Color.Yellow,
                        Color.Green,
                        Color.Blue,
                        Color.Purple
                    };
                    var colorBytes = colors
                        .SelectMany(c => new byte[] { c.R, c.G, c.B })
                        .ToArray();

                    Console.WriteLine("Rainbow...");
                    client.FadeToMultiRgb(colorBytes);
                }
                else if (args.Length == 3)
                {
                    byte red = byte.Parse(args[0]);
                    byte green = byte.Parse(args[1]);
                    byte blue = byte.Parse(args[2]);

                    Color color = Color.FromArgb(red, green, blue);
                    client.SetColorRgb(color.R, color.G, color.B);

                    Console.WriteLine("Color set to " + color.Name);

                }
                else if (args.Length == 4 && args[0] == "fade")
                {
                    byte red = byte.Parse(args[1]);
                    byte green = byte.Parse(args[2]);
                    byte blue = byte.Parse(args[3]);

                    Console.WriteLine("Fading...");
                    client.FadeToRgb(red, green, blue);
                }

                else if (args.Length != 1)
                {
                    return;
                }
                else
                {
                    Color color = Color.FromName(args[0]);
                    byte red = RoundToNotifierColorValue(color.R);
                    byte green = RoundToNotifierColorValue(color.G);
                    byte blue = RoundToNotifierColorValue(color.B);

                    client.SetColorRgb(red, green, blue);

                    var notifierColor = Color.FromArgb(red, green, blue);
                    Console.WriteLine("Color set to " + notifierColor.Name);
                }
                
                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("She broke. :(" + e);
                Console.ReadLine();
            }
        }

        private static byte RoundToNotifierColorValue(byte b)
        {
            byte newValue;
            if (b == 0)
                newValue = 0;
            else if (b > 255)
                newValue = 64;
            else
                newValue = b;

            return newValue;
        }
    }
}
