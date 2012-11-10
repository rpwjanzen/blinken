using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Argotic.Common;
using Argotic.Syndication;
using Blinken;
using Blinken.Font;

namespace RssLedSignController
{
    class Program
    {
        private static RssFeed m_feed;

        private static List<string> m_forecastFeedLines = new List<string>();
        private static List<string> m_allFeedLines = new List<string>();

        private static Timer m_reloadTimer;

        static void Main(string[] args)
        {
            var lcdNotifier = new LcdNotifier();
            //LedFont font = LedFont.LoadFromFile(@"Font\somybmp01_7.txt");
            LedFont font = LedFont.LoadFromFile(@"Font\Tinier.txt");

            m_reloadTimer = new Timer();
            m_reloadTimer.Elapsed += (s, e) =>
            {
                Console.WriteLine("Reloading RSS Feed....");
                m_feed.LoadAsync(new Uri("http://feeds.huffingtonpost.com/HP/MostPopular"), null);
            };
            m_reloadTimer.Interval = TimeSpan.FromSeconds(15).TotalMilliseconds;


            m_feed = new RssFeed();
            m_feed.Loaded += feed_Loaded;
            m_reloadTimer.Start();
            Console.WriteLine("Reloading RSS Feed....");
            m_feed.LoadAsync(new Uri("http://feeds.huffingtonpost.com/HP/MostPopular"), null);

            int i = 0;
            while (true)
            {
                lock (m_allFeedLines)
                {
                    if (m_allFeedLines.Any())
                    {
                        if (i >= m_allFeedLines.Count)
                            i = 0;
                        lcdNotifier.Text = m_allFeedLines[i];
                    }
                }

                lcdNotifier.ScrollText(font, TimeSpan.FromMilliseconds(20));
                i++;

                // end of messages break
                //System.Threading.Thread.Sleep(1000);
            }
        }

        private static void feed_Loaded(object sender, SyndicationResourceLoadedEventArgs e)
        {
            Console.WriteLine("RSS Feed loaded.");
            List<string[]> allLines = new List<string[]>();
            foreach (var rssItem in m_feed.Channel.Items)
            {
                string title = rssItem.Title;
                title = RemoveHtml(title);
                allLines.Add(new string [] { title });
            }

            lock (m_forecastFeedLines)
            {
                m_forecastFeedLines.Clear();
                m_forecastFeedLines.AddRange(allLines.SelectMany(s => s));
            }

            UpdateAllFeedLines();
        }

        private static string RemoveHtml(string line)
        {
            foreach (var s in new[] { "<b>", "</b>", "<br/>" })
            {
                line = line.Replace(s, "");
            }

            line = line.Replace("&deg;", "*");
            line = line.Replace("&lt;", "<");
            line = line.Replace("&gt;", ">");
            line = line.Replace("&amp;", "&");

            // huffington post feed
            line = line.Replace("![CDATA[", "");
            line = line.Replace("]]", "");

            line = line.Replace("<&>", "&");
            line = line.Replace("PHOTOS", "");
            line = line.Replace("PHOTO", "");
            line = line.Replace("UPDATE", "");
            line = line.Replace("AUDIO", "");
            line = line.Replace("LIVE UPDATES", "");
            line = line.Replace("VIDEO", "");
            line = line.Replace("POLL", "");
            line = line.Replace("()", "");
            line = line.Replace("(, )", "");
            line = line.Replace("(, , )", "");

            return line;
        }

        private static void UpdateAllFeedLines()
        {
            List<string> linesCopy;
            lock (m_allFeedLines)
            {
                lock (m_forecastFeedLines)
                {
                    m_allFeedLines.Clear();
                    m_allFeedLines.AddRange(m_forecastFeedLines);
                    linesCopy = m_allFeedLines.ToList();
                }
            }

            Console.WriteLine("Begin updated RSS feed...");
            foreach (var s in linesCopy)
                Console.WriteLine(s);
            Console.WriteLine("End updated RSS feed.");

        }
    }
}
