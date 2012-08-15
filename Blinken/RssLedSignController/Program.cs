using System;
using System.Collections.Generic;
using System.Linq;
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

        static void Main(string[] args)
        {
            var lcdNotifier = new LcdNotifier();
            LedFont font = LedFont.LoadFromFile(@"Font\somybmp01_7.txt");

            m_feed = new RssFeed();
            m_feed.Loaded += feed_Loaded;
            m_feed.LoadAsync(new Uri("http://feeds.huffingtonpost.com/HP/MostPopular"), null);

            while (true)
            {
                lock (m_allFeedLines)
                {
                    for (int i = 0; i < m_allFeedLines.Count; i++)
                    {
                        lcdNotifier.Text = m_allFeedLines[i];
                        lcdNotifier.ScrollText(font);
                    }
                }

                System.Threading.Thread.Sleep(1000);
            }
        }

        private static void feed_Loaded(object sender, Argotic.Common.SyndicationResourceLoadedEventArgs e)
        {
            List<string[]> allLines = new List<string[]>();
            foreach (var rssItem in m_feed.Channel.Items)
            {
                //string[] descriptionLines = rssItem.Description.Split('\n');
                //for (int i = 0; i < descriptionLines.Length; i++)
                //{
                //    string line = descriptionLines[i];
                //    line = RemoveHtml(line);

                //    descriptionLines[i] = line.Trim();
                //}

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
