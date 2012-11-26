using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using Blinken;
using Blinken.Font;

namespace SignService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    internal sealed class SignService : ISignService
    {
        private readonly LcdNotifier m_lcdNotifier;
        private bool m_isScrollText = true;

        public SignService()
        {
            // Test Patterns
            string upperAlphabet = @"ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string lowerAlphabet = @" abcdefghijklmnopqrstuvwxyz";
            string numbers = @"01234567890 +-=";
            string symbols = @"/\|<>,.)(*&^%$#@!~`';:[]{}?";

            string testPattern = upperAlphabet + " " +
                lowerAlphabet + " " +
                numbers + " " +
                symbols;

            m_lcdNotifier = new LcdNotifier();

            List<string> paths = new List<string>()
            {
                //@"Font\MyTiny.txt",
                @"Font\somybmp01_7.txt",
                @"Font\somybmp02_7.txt",
                @"Font\somybmp04_7.txt",
                @"Font\Tinier.txt",
                @"Font\Tiny.txt",
            };

            List<LedFont> fonts = new List<LedFont>();
            foreach (var path in paths)
            {
                var font = LedFont.LoadFromFile(path);
                fonts.Add(font);
            }

            var displayFont = fonts[0];

            m_lcdNotifier.Text = string.Empty; //testPattern;

            Thread thread = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    lock (m_lcdNotifier)
                    {
                        if (m_lcdNotifier.Text != string.Empty)
                        {
                            if (m_isScrollText)
                                m_lcdNotifier.ScrollText(displayFont, TimeSpan.FromMilliseconds(40));
                            else
                                m_lcdNotifier.ShowText(displayFont);
                        }
                        else if (m_lcdNotifier.Image != null)
                            m_lcdNotifier.ScrollImage(TimeSpan.FromMilliseconds(40));
                    }
                }
            }));
            thread.IsBackground = true;
            thread.Start();
        }

        #region ISignService Members

        [OperationBehavior(ReleaseInstanceMode = ReleaseInstanceMode.None)]
        public void ScrollText(string text)
        {
            lock (m_lcdNotifier)
            {
                m_isScrollText = true;
                m_lcdNotifier.Image = null;
                m_lcdNotifier.Text = text;
            }
        }

        [OperationBehavior(ReleaseInstanceMode = ReleaseInstanceMode.None)]
        public void ScrollImage(bool[] imageData)
        {
            int height = 7;
            lock (m_lcdNotifier)
            {
                m_lcdNotifier.Text = string.Empty;
                int width = imageData.Length / height;
                bool[,] newImage = new bool[width, height];
                for (int column = 0; column < width; column++)
                {
                    for (int row = 0; row < height; row++)
                    {
                        newImage[column, height - (row + 1)] = imageData[row * width + column];
                    }
                }
                m_lcdNotifier.Image = newImage;
            }
        }

        [OperationBehavior(ReleaseInstanceMode = ReleaseInstanceMode.None)]
        public void SetText(string text)
        {
            lock (m_lcdNotifier)
            {
                m_isScrollText = false;
                m_lcdNotifier.Image = null;
                m_lcdNotifier.Text = text;
            }
        }

        #endregion
    }
}
