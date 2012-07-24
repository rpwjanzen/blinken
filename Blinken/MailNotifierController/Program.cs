using System;
using System.Drawing;
using Blinken;

namespace MailNotifierController
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                return;
            }
            else if (args[0] == "rainbow")
            {
                var mailNotifier = new MailNotifier();
                Console.WriteLine("Rainbow!");
                mailNotifier.DoRainbow();
            }
            else if (args[0] == "fade")
            {
                Console.WriteLine("Fading...");
                var mailNotifier = new MailNotifier();
                while (true)
                {
                    mailNotifier.DoFadeRed();
                    System.Threading.Thread.Sleep(150);
                    mailNotifier.DoFadeGreen();
                    System.Threading.Thread.Sleep(150);
                    mailNotifier.DoFadeBlue();
                    System.Threading.Thread.Sleep(150);
                }
            }
            else
            {
                Color color = Color.FromName(args[0]);
                var mailNotifier = new MailNotifier();
                mailNotifier.SetColor(color);

                Console.WriteLine("Color set to " + color.Name);
            }
        }
    }
}
