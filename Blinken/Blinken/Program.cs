using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HidLibrary;
using System.Drawing;

namespace Blinken
{
    class Program
    {
        static void Main(string[] args)
        {
            new LcdNotifier();
            var mailNotifier = new MailNotifier();
            mailNotifier.SetColor(Color.Peru);
            mailNotifier.DoRainbow();

            Console.ReadKey();
        }
    }
}
