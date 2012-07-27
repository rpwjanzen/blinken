using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blinken;
using System.IO;
using System.Threading;
using Blinken.Font;

namespace SignController
{
    class Program
    {
        static void Main(string[] args)
        {
            var lcdNotifier = new LcdNotifier();
            LedFont tinierFony = LedFont.LoadFromFile(@"Font\Tinier.txt");
            LedFont somyFont = LedFont.LoadFromFile(@"Font\somybmp04_7.txt");

            string[] lines = 
            {
                "Remember when I said I'd kill you last ...... I lied!",

                "Who said you could eat my cookies!?",
                "First I'm gonna use you as a human shield, then I'm gonna kill this guard over there, with the Patterson trocar on the table. Then I was thinking about breaking your neck.",
                "Well, you see, this is the problem with terrorists. They're really inconsiderate when it comes to people's schedules",
                "I know what this is... This is an espresso machine... No, no wait... It's a snow cone maker... Is it a water heater?",
                "Why am I wasting my time with a silly putz like you when I could be doing something more dangerous like rearranging my sock drawer? And how exactly are you going to snap your fingers, after I rip off both of your thumbs?",
                "To be or not to be? Not to beeeeeeeeeee!",
                "Someone's in my house. Eating my birthday cake. With my family!",
                "It's not a tumor!",
                "I'll be back.",
                "Hasta la vista, baby.",
                "To crush your enemies, see them driven before you, and to hear the lamentation of their women.",
                "Do it!",
                "I'd love to see you eat that contract, but I hope you leave enough room for my fist, because I'm going to ram it into your stomach, and break your spine!",
                "Let off some steam, Bennett.",
                "Get to da choppa!",
                "If it bleeds, we can kill it.",
                "I eat Green Berets for breakfast.",
                "Who is your daddy, and what does he do?",
                "Come with me if you want to live.",
                "Oh no! Not the view locator!",
            };

            while (true)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    lcdNotifier.Text = lines[i].ToUpper();
                    if (i % 2 == 0)
                        lcdNotifier.DrawText(tinierFony);
                    else
                        lcdNotifier.DrawText(somyFont);
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }
    }
}
