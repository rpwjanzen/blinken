using System;
using System.Drawing;
using Blinken;

namespace MailNotifierController
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 3)
            {
                byte red = byte.Parse(args[0]);
                byte green = byte.Parse(args[1]);
                byte blue = byte.Parse(args[2]);

                Color color = Color.FromArgb(red, green, blue);
                var mailNotifier = new MailNotifier();
                mailNotifier.SetColor(color);

                Console.WriteLine("Color set to " + color.Name);

            }
            else if (args.Length != 1)
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
                byte red = RoundToNotifierColorValue(color.R);
                byte green = RoundToNotifierColorValue(color.G);
                byte blue = RoundToNotifierColorValue(color.B);

                var notifierColor = Color.FromArgb(red, green, blue);

                var mailNotifier = new MailNotifier();
                mailNotifier.SetColor(notifierColor);

                Console.WriteLine("Color set to " + notifierColor.Name);
            }
        }

        private static byte RoundToNotifierColorValue(byte b)
        {
            byte newValue;
            if (b == 0)
                newValue = 0;
            else if (b > 64)
                newValue = 64;
            else
                newValue = b;

            return newValue;
        }
    }
}
