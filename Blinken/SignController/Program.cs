using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blinken;
using System.IO;

namespace SignController
{
    class Program
    {
        static void Main(string[] args)
        {
            var lcdNotifier = new LcdNotifier();
            string[] lines = 
            {
                "Who said you could eat my coo- kies!",

                "First I’m gonna use you as a human shield , then I'm gonna kill this guard over there, with the Pat- ter- son trocar on the table. Then I was thinking about breaking your neck",
                "Well, you see, this is the problem with ter- ror- ists. They’re really in- con- si- der- ate when it comes to peo- ple’s sche- dules",
                "I know what this is ... This is an es- presso ma- chine ... No, no wait. It’s a snow cone maker . Is it a water heat- er ?",
                "Why am I wast- ing my time with a silly putz like you when I could be doing some- thing more dan- ger- ous - like re- arra- nging my sock draw- er ? And how exac- tly are you going to snap your fin- gers, after I rip off both of your thum- bs?",
                "To be or not to be? Not to beee- eeee- eeee!",
                "Some- one’s in my house . Eating my birthday cake. With my fam- ily",
                "It's not a tu- mor!",
                "I'll be back.",
                "Hasta la vista, baby",
                "To crush your ene- mies, see them dri- ven before you, and to hear the la- men- ta- tion of their wo- men!",
                "Do it!",
                "Re- mem- ber when I said I’d kill you last ... ... I lied!",
                "I live to see you eat that con- tract, but I hope you leave e- nough room for my fist be- cause I’m going to ram it into your sto- mach and break your spine!",
                "Let off some steam, Bennett.",
                "Get to da Choppa!",
                "If it bleeds, we can kill it.",
                "I eat Green Berets for breakfast.",

                "Who is your daddy, and what does he do?",
                
                
                "Come with me if you want to live.",
                "Oh no! Not the view loca- tor!",
            };

            while (true)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    lcdNotifier.Text = lines[i].ToUpper();
                    lcdNotifier.DrawText();
                    System.Threading.Thread.Sleep(600);
                    
                    lcdNotifier.Text = "";
                    lcdNotifier.DrawText();
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }
    }
}
