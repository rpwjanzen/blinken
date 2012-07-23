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
                return;

            Color color = Color.FromName(args[0]);

            var mailNotifier = new MailNotifier();
            mailNotifier.SetColor(color);

            Console.WriteLine("Color set to " + color.Name);
        }
    }
}
